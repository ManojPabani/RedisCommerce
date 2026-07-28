# RedisCommerce

A full-stack e-commerce learning project built to master Redis by integrating it into a production-style
ASP.NET Core + React application.

> **Phase 1: Redis Fundamentals + Product Cache Implementation** ✅
> Clean Architecture, EF Core + SQL Server persistence, the Redis **cache-aside pattern** (String), and a complete
> Product CRUD flow across the API and the React frontend.
>
> **Phase 2: Redis Data Structures** ✅
> Four business features, each backed by a different Redis data structure: a **Hash**-backed shopping cart, a
> **List**-backed order processing queue with a background worker, a **Set**-backed favorites list, and a
> **Sorted Set**-backed product popularity leaderboard.
>
> **Phase 3: Enterprise Analytics** ✅ (this phase)
> Session management (String + **sliding TTL**), user activity tracking (**Bitmap**), unique-visitor analytics
> (**HyperLogLog**), a centralized **TTL policy provider**, **keyspace-notification**-driven expiration monitoring,
> and a React analytics dashboard with three more background workers.
>
> **Not implemented yet** (future phases): Redis Pub/Sub (general-purpose business events — see the caveat below),
> Redis Streams, Redis Cluster, Redis Replication, Lua Scripts, RedisJSON, RediSearch.

## Architecture

```
                    React Client
                         |
                         |
                    HTTP / REST
                         |
                         |
              ASP.NET Core Web API
                         |
        --------------------------------
        |                              |
    SQL Server                      Redis
 Permanent Storage              Cache Layer
```

Backend follows strict **Clean Architecture**:

```
backend/
  RedisCommerce.Domain          Entities (Product, Order, OrderItem), OrderStatus, exceptions — no dependencies
  RedisCommerce.Application     DTOs, validators, service interfaces, business logic:
                                   Services/       ProductService, CartService, FavoriteService,
                                                   ProductPopularityService, OrderService, SessionService,
                                                   ActivityTrackingService, VisitorAnalyticsService,
                                                   ExpirationNotificationService, TTLPolicyProvider
                                   Interfaces/     IProductRepository, ICartRepository, IFavoriteRepository,
                                                   IProductPopularityRepository, IOrderRepository,
                                                   IOrderQueueRepository, ISessionRepository,
                                                   IActivityTrackingRepository, IVisitorAnalyticsRepository,
                                                   plus the generic IHashService/IListService/ISetService/
                                                   ISortedSetService/IBitmapService/IHyperLogLogService
                                   Configuration/  RedisOptions (strongly typed, bound from RedisSettings)
                                   Caching/        CacheKeys (single source of truth for Redis key strings)
  RedisCommerce.Infrastructure  EF Core DbContext + migrations, SQL repositories (ProductRepository,
                                OrderRepository), Redis-backed repositories (RedisCartRepository,
                                RedisFavoriteRepository, RedisProductPopularityRepository,
                                RedisOrderQueueRepository, RedisSessionRepository, RedisActivityTrackingRepository,
                                RedisVisitorAnalyticsRepository), generic Redis services (Caching/:
                                RedisCacheService, HashService, ListService, SetService, SortedSetService,
                                BitmapService, HyperLogLogService — all StackExchange.Redis),
                                Workers/ (OrderProcessingWorker, RedisExpirationListener, SessionCleanupWorker,
                                DailyAnalyticsWorker — all BackgroundService)
  RedisCommerce.API             Controllers (Products, Cart, Favorites, Orders, Auth, Admin), SessionMiddleware,
                                DI composition, Swagger
  RedisCommerce.Tests           xUnit tests per service/repository layer (Moq — no live SQL Server or Redis needed)
```

Every Redis-backed feature follows the same shape: a **generic data-structure service** (`Infrastructure/Caching`,
one per Redis type — String/Hash/List/Set/Sorted Set/Bitmap/HyperLogLog) wrapped by a **feature-specific repository**
(`Infrastructure/Repositories`) that knows the key naming and serialization for that feature, consumed by an
**Application-layer service** that owns the business rules, TTL policy, and logging. Controllers stay thin — no
Redis logic ever lives in a controller.

Frontend follows a **feature-based** structure:

```
frontend/rediscommerce-web/src/
  app/            App shell, router, entry point, auto-login bootstrap
  core/           Axios client (attaches X-Session-Id / X-Visitor-Id), query client, API route constants,
                  CURRENT_USER_ID (no real auth phase yet), visitorId util (localStorage-persisted GUID)
  features/
    products/     ProductCard/ProductForm/DeleteConfirmDialog, List/Details/Edit/Popular pages,
                  productService.ts, hooks (useProducts, useProduct, usePopularProducts, ...)
    cart/         CartItemRow, CartPage, cartService.ts, hooks (useCart, useAddCartItem, useUpdateCartItem, ...)
    favorites/    FavoriteButton (optimistic toggle), FavoritesPage, favoriteService.ts, hooks
    orders/       OrderConfirmationPage, orderService.ts, useCheckout
    auth/         authService.ts (login/logout/session), useAutoLogin (establishes a demo session on load)
    admin/        StatCard widget, AnalyticsDashboardPage, UserSessionsPage, VisitorAnalyticsPage,
                  DailyActivityPage, adminService.ts, hooks (all auto-refresh every 30s)
  shared/         Reusable Button/Spinner/ErrorMessage, formatting utils, test utilities
  layouts/        MainLayout (nav: Popular / Favorites / Cart / Analytics / Add Product)
```

## How the Redis cache works (cache-aside pattern)

`GET /api/products/{id}`:

1. Check Redis for key `product:{id}`.
2. **Hit** → deserialize and return the cached JSON. Logged as `CACHE HIT: product:{id}`.
3. **Miss** → query SQL Server, cache the result in Redis with a **30 minute** expiration, return it. Logged as
   `CACHE MISS: product:{id}`.

On `PUT`/`DELETE /api/products/{id}`, SQL Server is updated/deleted first, then the `product:{id}` key is removed
from Redis (invalidate, don't repopulate — the next `GET` re-fills the cache). Logged as `CACHE UPDATED: product:{id}`
or `CACHE REMOVED: product:{id}`.

## Redis data structures (Phase 2)

Each feature below is chosen because its access pattern is a natural fit for that Redis type — not just to
demonstrate the command set.

### Shopping Cart — Hash

A cart is a small map of `productId → quantity`, which is exactly what a Redis **Hash** models — one key per user,
one field per product, no need to deserialize a whole blob to change one line item. `POST .../items` reads the
current quantity (`HGET`), adds the requested amount, and writes it back (`HSET`) — quantity is never allowed to go
to zero or below; instead the field is removed (`HDEL`). An empty cart is still a valid cart (a missing Redis key
just yields zero fields via `HGETALL`, no 404). Every write resets a **24-hour TTL** on the whole hash.

### Order Processing Queue — List

Checkout needs a durable, ordered, pop-once queue between the web request and a worker — a Redis **List** used as a
FIFO: `LPUSH` on enqueue (checkout), `RPOP` on dequeue (the worker), so items are processed in the order they
arrived. `OrderProcessingWorker` is an ASP.NET Core `BackgroundService` that polls the queue every 5 seconds,
moves a dequeued order from `Pending` → `Processing` → `Completed` in SQL Server, and logs the result. No TTL — the
list is a work queue, not a cache; items live until popped.

### Favorites — Set

Favorites need duplicate-free membership with fast add/remove/contains checks and no ordering requirement — a Redis
**Set** gives all of that natively (`SADD` is a no-op if the member is already present, so there's no separate
duplicate check in application code). No TTL — a favorite doesn't expire on its own.

### Product Popularity — Sorted Set

"Which products are viewed the most" is a ranking problem: a Redis **Sorted Set** keeps members ordered by score
automatically, so reading the top N is a single `O(log(N)+M)` range query instead of a manual sort. Every
`GET /api/products/{id}` call — hit or miss — increments that product's score by 1 (`ZINCRBY`), and
`GET /api/products/popular` reads the top 20 descending (`ZREVRANGE ... WITHSCORES`). No TTL — popularity is a
running total, not a cache entry.

### Key naming and TTL strategy

| Feature | Redis type | Key pattern | Example | TTL |
|---|---|---|---|---|
| Product cache | String | `product:{id}` | `product:1001` | 30 min, reset on every re-cache after a miss |
| Shopping cart | Hash | `cart:{userId}` | `cart:1001` | 24 h, **reset on every write** |
| Order queue | List | `order-processing` (single key) | `order-processing` | none — a work queue |
| Favorites | Set | `favorites:{userId}` | `favorites:1001` | none |
| Popularity | Sorted Set | `popular-products` (single key) | `popular-products` | none |

### redis-cli verification

```bash
podman exec -it rediscommerce-redis redis-cli

# Cart (Hash)
HGETALL cart:1001          # all productId -> quantity fields for user 1001
TTL cart:1001               # seconds remaining (<= 86400), resets on every cart write

# Order queue (List)
LRANGE order-processing 0 -1   # everything currently queued (empties out as the worker RPOPs)
LLEN order-processing

# Favorites (Set)
SMEMBERS favorites:1001     # every productId user 1001 has favorited
SISMEMBER favorites:1001 10

# Popularity (Sorted Set)
ZREVRANGE popular-products 0 9 WITHSCORES   # top 10 most-viewed products, highest first
ZSCORE popular-products 10                   # view count for product 10
```

## Enterprise analytics (Phase 3)

### Session management — String + sliding TTL

`POST /api/auth/login` (body `{ "userId": 1001 }`, no password — there's no auth/Users table to check one against)
creates a Redis String at `session:{sessionId}` holding a JSON blob (`UserId`, `LoginTime`, `LastActivity`,
`IpAddress`, `Browser`, `Device`) with a **30-minute TTL**, plus a `session:user:{userId}` pointer (same TTL) so a
second login for a user who's already got a live session **reuses and refreshes it instead of creating a duplicate**.
A `sessions:active` **Set** (reusing the existing `ISetService`) tracks which session ids are currently live, so the
admin dashboard can enumerate them without `KEYS`/`SCAN`.

`SessionMiddleware` reads the `X-Session-Id` request header on every request (opportunistically — if the header is
missing, the request proceeds exactly as in Phases 1–2, no endpoint requires a session) and calls
`RefreshSessionAsync`, which **only touches the TTL** (`EXPIRE`, via the new `IRedisCacheService.RefreshExpirationAsync`)
rather than re-fetching and rewriting the JSON — this is the sliding expiration: "whenever the user performs a
request, refresh the TTL" without re-serializing on every single call. The frontend sends this header via an Axios
request interceptor (`core/api/axiosClient.ts`); a one-time `useAutoLogin` hook establishes a session for the demo
user on first load since there's no login form to build (no real credentials to check).

### User activity tracking — Bitmap

One bit per user, one key per day (`activity:yyyyMMdd`) — instead of a row per (user, day, activity) in SQL, a
day's worth of activity for the entire user base lives in a few KB. `SETBIT activity:20260728 1001 1` marks user
1001 active today; product views, logins, checkouts, and searches all mark the same bit (a user is either active
that day or not — the bitmap doesn't distinguish which activity). "Active users over the last N days" is a single
`BITOP OR` across the N daily keys into a scratch key, `BITCOUNT`ed, then discarded — one round trip instead of N
separate counts or a SQL `GROUP BY`.

### Unique visitor analytics — HyperLogLog

Marketing wants approximate daily/weekly/monthly unique-visitor counts without storing a set of every visitor id
(which would grow unbounded) — a HyperLogLog answers "how many distinct visitors" in ~12KB regardless of whether
that's 100 or 100 million, with a standard error of ~0.81%. Every product view calls `PFADD` on the daily, weekly
(ISO week), and monthly key simultaneously; `PFCOUNT` reads the (approximate) cardinality of one key, or the union
of several keys at once (e.g. the dashboard's "merged last 7 days" figure needs no separate `PFMERGE` step —
`PFCOUNT` accepts multiple keys directly for an ad-hoc union).

### Centralized TTL policy

`ITTLPolicyProvider.GetTtl(RedisObjectType)` is the single place expiration durations are decided, backed by
strongly typed `RedisOptions` bound from `appsettings.json`'s `RedisSettings` section — nothing hardcodes a
`TimeSpan` anymore. `ProductService` and `RedisCartRepository` (both from earlier phases) were refactored to pull
their TTLs from this provider instead of a local constant.

| Object | TTL | Reasoning |
|---|---|---|
| Product cache | 30 min (`ProductTTLMinutes`) | Cache-aside — short enough to pick up catalog edits promptly |
| Cart | 24 h (`CartTTLHours`), reset on every write | Abandoned carts shouldn't live forever |
| Session | 30 min (`SessionTTLMinutes`), **sliding** | Reset on every request via `RefreshExpirationAsync` |
| Analytics daily-active-counts | 90 days (`AnalyticsTTLDays`) | See the per-key vs per-member gotcha below |
| Favorites / Popularity | none | Durable by design, not cache entries |

**Redis TTL is per-key, not per-member.** The `analytics:daily-active-counts` Sorted Set (member = date, score =
that day's active-user count, populated by `DailyAnalyticsWorker`) can't have individual old dates "expire" the way
a String key can — `EXPIRE` would delete the *entire* sorted set, not just old entries. So `DailyAnalyticsWorker`
explicitly prunes members older than `AnalyticsTTLDays` on every pass (`ZREM`) instead of relying on Redis TTL for
that rolling window.

### Expiration event monitoring — keyspace notifications

> **A note on Pub/Sub**: Redis keyspace notifications are delivered over Redis's Pub/Sub subscribe mechanism —
> there's no other way to observe key expiration. This is scoped narrowly to expiration monitoring here, distinct
> from general-purpose business-event Pub/Sub (e.g. broadcasting order/stock updates across services), which
> remains out of scope until a later phase.

`RedisExpirationListener` (a `BackgroundService`) best-effort enables `notify-keyspace-events Ex` on startup (wrapped
in try/catch — managed Redis providers often block `CONFIG SET`, in which case it logs a warning and relies on the
setting having been enabled out-of-band) and subscribes to the `__keyevent@*__:expired` pattern channel. When a
`session:{id}` key expires, it logs **`Session Expired`**, removes the id from the `sessions:active` Set, and
records the event; when a `cart:{userId}` key expires, it logs **`Cart Expired`** — finally closing the gap
documented (but not implementable without Pub/Sub) back in Phase 2. Events are kept in a capped `expiration-events`
**List** (`LTRIM`'d to the last 100) for the admin dashboard's "recent expirations" view.
`SessionCleanupWorker` is a defensive backstop that periodically reconciles `sessions:active` against actual live
sessions, in case a particular expiration was ever missed by the listener.

### Key naming and TTL strategy (all phases)

| Feature | Redis type | Key pattern | Example | TTL |
|---|---|---|---|---|
| Product cache | String | `product:{id}` | `product:1001` | 30 min, reset on every re-cache after a miss |
| Shopping cart | Hash | `cart:{userId}` | `cart:1001` | 24 h, reset on every write |
| Order queue | List | `order-processing` (single key) | `order-processing` | none — a work queue |
| Favorites | Set | `favorites:{userId}` | `favorites:1001` | none |
| Popularity | Sorted Set | `popular-products` (single key) | `popular-products` | none |
| Session | String | `session:{sessionId}` | `session:6fd41d2...` | 30 min, **sliding** |
| Session user pointer | String | `session:user:{userId}` | `session:user:1001` | 30 min, mirrors the session |
| Active sessions index | Set | `sessions:active` (single key) | `sessions:active` | none |
| Recent expirations | List | `expiration-events` (single key, capped at 100) | `expiration-events` | none |
| Daily activity | Bitmap | `activity:{yyyyMMdd}` | `activity:20260728` | none |
| Daily active-count history | Sorted Set | `analytics:daily-active-counts` (single key) | `analytics:daily-active-counts` | none (pruned by age instead) |
| Daily unique visitors | HyperLogLog | `visitors:daily:{yyyyMMdd}` | `visitors:daily:20260728` | none |
| Weekly unique visitors | HyperLogLog | `visitors:weekly:{isoYear}W{isoWeek}` | `visitors:weekly:2026W31` | none |
| Monthly unique visitors | HyperLogLog | `visitors:monthly:{yyyyMM}` | `visitors:monthly:202607` | none |

### redis-cli verification (Phase 3)

```bash
podman exec -it rediscommerce-redis redis-cli

# Sessions (String, sliding TTL)
GET session:abcd1234abcd1234abcd1234abcd1234
TTL session:abcd1234abcd1234abcd1234abcd1234   # watch this reset toward 1800 on each API request

# Activity (Bitmap)
SETBIT activity:20260728 1001 1
GETBIT activity:20260728 1001
BITCOUNT activity:20260728

# Visitors (HyperLogLog)
PFADD visitors:daily:20260728 visitor-abc-123
PFCOUNT visitors:daily:20260728

# Recent expiration events
LRANGE expiration-events 0 9
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) and npm
- SQL Server accessible via **Windows Authentication** — this project targets the `(localdb)\MSSQLLocalDB` instance
  that ships with Visual Studio / SQL Server tooling. No container or SQL login is needed for the database.
- [Podman Desktop](https://podman-desktop.io/) with a running Podman machine (for the Redis container only). On
  Windows this needs a WSL2 or Hyper-V backend — run `podman machine init` and `podman machine start` once that's
  set up (Podman Desktop's UI can do this for you too).

## Running locally

### 1. Start Redis

```bash
podman compose up -d
```

`docker-compose.yml` works as-is with Podman — `podman compose` is a drop-in for `docker compose`. If your Podman
version doesn't bundle the `compose` subcommand, install `podman-compose` (`pip install podman-compose`) and run
`podman-compose up -d` instead. This only starts Redis on `localhost:6379` — SQL Server is the local LocalDB
instance, not a container. The API and frontend run directly on your machine (not containerized in Phase 1).

### 2. Run the backend API

The API connects to SQL Server via `Trusted_Connection=True` (Windows Authentication — no password in
`appsettings.json`), and applies EF Core migrations and seeds sample data automatically on startup, so no manual
migration step is required. If you want to run migrations manually instead:

```bash
cd backend
dotnet tool install --global dotnet-ef   # first time only
dotnet ef database update --project RedisCommerce.Infrastructure --startup-project RedisCommerce.API
```

Run the API:

```bash
cd backend
dotnet run --project RedisCommerce.API
```

The API listens on `http://localhost:5281` (see `RedisCommerce.API/Properties/launchSettings.json`). Swagger UI is
available at `http://localhost:5281/swagger` in development.

### 3. Run the frontend

```bash
cd frontend/rediscommerce-web
npm install
npm run dev
```

The app runs at `http://localhost:5173` and talks to the API via `VITE_API_URL` (see `.env`).

## Podman commands

```bash
podman machine start        # start the Podman Linux VM (once per session/reboot)
podman compose up -d        # start SQL Server + Redis in the background
podman compose ps           # check container status
podman compose logs -f      # follow logs
podman compose down         # stop containers (add -v to also remove volumes/data)
```

## Redis verification

```bash
podman exec -it rediscommerce-redis redis-cli
KEYS *   # list every key across all features currently in Redis
```

Call `GET /api/products/1` twice via Swagger/curl — the first call logs `CACHE MISS`, the second logs `CACHE HIT`,
and `GET product:1` / `TTL product:1` will show the cached value with a TTL. See **redis-cli verification** above for
the Hash/List/Set/Sorted-Set commands covering Cart, Orders, Favorites, and Popularity.

## Testing

### Backend

```bash
cd backend
dotnet test
```

Covers, without needing a live SQL Server or Redis instance (everything mocked with Moq — `RedisCommerce.Tests` has
96 tests across all three phases):
- **Product cache**: cache hit/miss, TTL from the policy provider, update/delete invalidation, popularity + activity
  + visitor recorded on every view.
- **Generic Redis services** (`HashServiceTests`, `ListServiceTests`, `SetServiceTests`, `SortedSetServiceTests`,
  `BitmapServiceTests`, `HyperLogLogServiceTests`): each wraps `IDatabase` with a mock and asserts the right Redis
  command is called with the right arguments.
- **Cart** (`CartServiceTests`): adding an existing product increases quantity; setting quantity to 0 removes the
  field instead of storing it; an empty cart returns successfully instead of 404ing.
- **Favorites** (`FavoriteServiceTests`): adding a favorite that doesn't exist as a product throws; re-adding an
  already-favorited product stays idempotent (no duplicate, no error).
- **Popularity** (`ProductPopularityServiceTests`): increments by one per view; top-N results are enriched with
  current product data and skip products that were since deleted.
- **Orders** (`OrderServiceTests`): checkout on an empty cart throws `EmptyCartException`; a valid checkout computes
  the total from current product prices, clears the cart, and enqueues the new order id.
- **Sessions** (`SessionServiceTests`): re-login reuses an existing active session instead of duplicating it; a
  stale user→session pointer (previous session expired) creates a fresh one; `RefreshSessionAsync` touches only the
  TTL, never rewrites the stored value; logout removes both the session and the active-set entry.
- **TTL policy** (`TTLPolicyProviderTests`): each `RedisObjectType` resolves to its configured duration; Popularity
  and Favorites resolve to `null` (never expire).
- **Activity tracking** (`ActivityTrackingServiceTests`): marks the correct day's bit; unique-count queries pass the
  right number of dates to the range query; most-active-day reads the top of the sorted-set snapshot.
- **Visitor analytics** (`VisitorAnalyticsServiceTests`): a single visit is recorded against daily/weekly/monthly
  simultaneously; the dashboard summary assembles all four counts.
- **Expiration notifications** (`ExpirationNotificationServiceTests`): a `session:*` key logs `Session Expired` and
  clears the active-set entry; a `cart:*` key logs `Cart Expired`; the `session:user:*` pointer key and unrelated
  keys are correctly ignored so they don't pollute the event log.

No tests for the `BackgroundService` polling loops themselves (`OrderProcessingWorker`, `RedisExpirationListener`,
`SessionCleanupWorker`, `DailyAnalyticsWorker`) — low value relative to effort; their behavior is covered indirectly
through the services they call, and verified manually end-to-end (see below).

### Frontend

```bash
cd frontend/rediscommerce-web
npm test
```

Covers `ProductCard` (including its Add-to-Cart button), `ProductForm` validation/submission, `CartPage` (empty-cart
state and item rendering), `FavoriteButton`'s **optimistic update** (the heart flips immediately on click, before
the network request resolves, and rolls back on failure), `StatCard` (the reusable dashboard widget), and
`UserSessionsPage` (renders the sessions table and the empty state).

## API Endpoints

| Method | Route                                       | Description |
|--------|----------------------------------------------|--------------|
| GET    | `/api/products`                               | List all products |
| GET    | `/api/products/popular`                       | Top 20 most-viewed products, descending |
| GET    | `/api/products/search?query=`                 | Search products by name (tracks Search activity) |
| GET    | `/api/products/{id}`                          | Get a product by id (cache-aside; records popularity, activity, visitor) |
| POST   | `/api/products`                               | Create a product |
| PUT    | `/api/products/{id}`                          | Update a product (invalidates cache) |
| DELETE | `/api/products/{id}`                          | Delete a product (invalidates cache) |
| GET    | `/api/cart/{userId}`                          | Get a user's cart |
| POST   | `/api/cart/{userId}/items`                    | Add a product to the cart (increments quantity if already present) |
| PUT    | `/api/cart/{userId}/items/{productId}`        | Set a line item's quantity (0 removes it) |
| DELETE | `/api/cart/{userId}/items/{productId}`        | Remove a line item |
| DELETE | `/api/cart/{userId}`                          | Clear the whole cart |
| GET    | `/api/users/{userId}/favorites`               | List favorited products (enriched with name/price) |
| POST   | `/api/users/{userId}/favorites/{productId}`   | Add a favorite (idempotent) |
| DELETE | `/api/users/{userId}/favorites/{productId}`   | Remove a favorite |
| POST   | `/api/orders/checkout`                        | Validate the cart, create the order, enqueue it for processing |
| POST   | `/api/auth/login`                             | Create or refresh a session for `{ userId }` |
| POST   | `/api/auth/logout`                            | End the session named by `X-Session-Id` |
| GET    | `/api/auth/session`                           | Get the current session named by `X-Session-Id` |
| GET    | `/api/admin/sessions`                         | Active sessions + recent expiration events |
| GET    | `/api/admin/activity/today`                   | Today's active-user count |
| GET    | `/api/admin/activity/count?days={n}`          | Unique active users over the last N days |
| GET    | `/api/admin/activity/summary`                 | Today / yesterday / last 7 days / last 30 days in one call |
| GET    | `/api/admin/activity/most-active-day`         | The single highest active-count day on record |
| GET    | `/api/admin/activity/{userId}`                | Whether a specific user is active today |
| GET    | `/api/admin/visitors`                         | Daily / weekly / monthly / merged-7-day unique visitor counts |

## Architecture decisions (Phase 3)

- **Login is UserId-only**: `{ userId }`, no password — there's still no auth/Users table, consistent with how
  Cart/Favorites already treat `userId` as an opaque client-supplied value. The frontend auto-establishes a session
  for `CURRENT_USER_ID` on load (`useAutoLogin`) rather than building a login form with no real credentials to check.
- **Session transport is a header, not a cookie**: `X-Session-Id`, attached by an Axios request interceptor and
  stored in `localStorage` — simpler than cookie/CORS-credential plumbing for a SPA calling a separate API origin.
- **Keyspace notifications were implemented despite the Pub/Sub exclusion**: see the callout in "Expiration event
  monitoring" above — it's the only mechanism Redis offers for observing expiration, scoped narrowly to that,
  distinct from general business-event Pub/Sub which is still deferred.
- **Bitmap tracking needs a known user; HyperLogLog visitor tracking doesn't**: activity bits are keyed by `UserId`
  (from the resolved session), so only requests carrying a valid session record activity. Visitor tracking uses a
  client-generated anonymous `X-Visitor-Id` instead, so it works whether or not the caller is logged in.
- **A minimal product search endpoint was added**: `GET /api/products/search` didn't exist before this phase; it
  exists purely so "track Search activity" has something real to instrument, not as a search-feature deliverable.
- **`RedisExpirationListener` also fulfils "Expired Session Monitor"**: the spec named these as two separate hosted
  services, but they're the same responsibility (observe session expiration) — one class, not a duplicate.
- **TTL is per-key, not per-member**: see the dedicated callout above — the daily-active-counts Sorted Set is pruned
  by explicit age check rather than relying on Redis `EXPIRE`.
- **No dedicated worker tests**: none of the four `BackgroundService` polling loops are unit-tested (low value
  relative to effort, per the same decision made for `OrderProcessingWorker` in Phase 2); covered indirectly through
  the services they call and verified manually.

## Next recommended phase

**Phase 4: Redis Pub/Sub, Streams & Lua** — general-purpose business-event messaging (e.g. broadcasting stock-level
changes or order-completion events across services) now has a natural foundation in the keyspace-notification
subscriber plumbing built this phase; Streams for a durable, replayable event log (as an alternative to the List-
based order queue); and Lua scripts for atomic multi-command operations (e.g. an atomic "check stock and decrement"
that a plain `GET`+`SET` can't guarantee under concurrency).
