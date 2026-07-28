# RedisCommerce

A full-stack e-commerce learning project built to master Redis by integrating it into a production-style
ASP.NET Core + React application.

> **Phase 1: Redis Fundamentals + Product Cache Implementation** ✅
> Clean Architecture, EF Core + SQL Server persistence, the Redis **cache-aside pattern** (String), and a complete
> Product CRUD flow across the API and the React frontend.
>
> **Phase 2: Redis Data Structures** ✅ (this phase)
> Four business features, each backed by a different Redis data structure: a **Hash**-backed shopping cart, a
> **List**-backed order processing queue with a background worker, a **Set**-backed favorites list, and a
> **Sorted Set**-backed product popularity leaderboard.
>
> **Not implemented yet** (future phases): Redis Pub/Sub, Redis Streams, Redis Cluster, Redis Replication, Lua
> Scripts, RedisJSON, RediSearch, Bitmap, HyperLogLog.

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
                                   Services/    ProductService, CartService, FavoriteService,
                                                ProductPopularityService, OrderService
                                   Interfaces/  IProductRepository, ICartRepository, IFavoriteRepository,
                                                IProductPopularityRepository, IOrderRepository,
                                                IOrderQueueRepository, plus the generic IHashService/
                                                IListService/ISetService/ISortedSetService
                                   Caching/     CacheKeys (single source of truth for Redis key strings)
  RedisCommerce.Infrastructure  EF Core DbContext + migrations, SQL repositories (ProductRepository,
                                OrderRepository), Redis-backed repositories (RedisCartRepository,
                                RedisFavoriteRepository, RedisProductPopularityRepository,
                                RedisOrderQueueRepository), generic Redis services (Caching/: RedisCacheService,
                                HashService, ListService, SetService, SortedSetService — all StackExchange.Redis),
                                Workers/OrderProcessingWorker (BackgroundService)
  RedisCommerce.API             Controllers (Products, Cart, Favorites, Orders), middleware, DI composition, Swagger
  RedisCommerce.Tests           xUnit tests per service/repository layer (Moq — no live SQL Server or Redis needed)
```

Every Redis-backed feature follows the same shape: a **generic data-structure service** (`Infrastructure/Caching`,
one per Redis type — String/Hash/List/Set/Sorted Set) wrapped by a **feature-specific repository**
(`Infrastructure/Repositories`) that knows the key naming and serialization for that feature, consumed by an
**Application-layer service** that owns the business rules and logging. Controllers stay thin — no Redis logic ever
lives in a controller.

Frontend follows a **feature-based** structure:

```
frontend/rediscommerce-web/src/
  app/            App shell, router, entry point
  core/           Axios client, query client, API route constants, CURRENT_USER_ID (no auth phase yet)
  features/
    products/     ProductCard/ProductForm/DeleteConfirmDialog, List/Details/Edit/Popular pages,
                   productService.ts, hooks (useProducts, useProduct, usePopularProducts, ...)
    cart/          CartItemRow, CartPage, cartService.ts, hooks (useCart, useAddCartItem, useUpdateCartItem, ...)
    favorites/     FavoriteButton (optimistic toggle), FavoritesPage, favoriteService.ts, hooks
    orders/        OrderConfirmationPage, orderService.ts, useCheckout
  shared/         Reusable Button/Spinner/ErrorMessage, formatting utils, test utilities
  layouts/        MainLayout (nav: Popular / Favorites / Cart / Add Product)
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
56 tests across both phases):
- **Product cache**: cache hit/miss, 30-minute TTL, update/delete invalidation, popularity recorded on every view.
- **Generic Redis services** (`HashServiceTests`, `ListServiceTests`, `SetServiceTests`, `SortedSetServiceTests`):
  each wraps `IDatabase` with a mock and asserts the right Redis command is called with the right arguments.
- **Cart** (`CartServiceTests`): adding an existing product increases quantity; setting quantity to 0 removes the
  field instead of storing it; an empty cart returns successfully instead of 404ing.
- **Favorites** (`FavoriteServiceTests`): adding a favorite that doesn't exist as a product throws; re-adding an
  already-favorited product stays idempotent (no duplicate, no error).
- **Popularity** (`ProductPopularityServiceTests`): increments by one per view; top-N results are enriched with
  current product data and skip products that were since deleted.
- **Orders** (`OrderServiceTests`): checkout on an empty cart throws `EmptyCartException`; a valid checkout computes
  the total from current product prices, clears the cart, and enqueues the new order id.

### Frontend

```bash
cd frontend/rediscommerce-web
npm test
```

Covers `ProductCard` (including its new Add-to-Cart button), `ProductForm` validation/submission, `CartPage`
(empty-cart state and item rendering), and `FavoriteButton`'s **optimistic update** — the heart flips to favorited
immediately on click, before the network request resolves, and rolls back if the request fails.

## API Endpoints

| Method | Route                                       | Description |
|--------|----------------------------------------------|--------------|
| GET    | `/api/products`                               | List all products |
| GET    | `/api/products/popular`                       | Top 20 most-viewed products, descending |
| GET    | `/api/products/{id}`                          | Get a product by id (cache-aside, records a popularity view) |
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

## Architecture decisions (Phase 2)

- **No auth phase yet**: `userId` is an opaque int the client supplies; the frontend hardcodes
  `CURRENT_USER_ID = 1001` (`core/constants/currentUser.ts`), matching the spec's own `cart:1001` example. Swapping
  in real authentication later doesn't touch any Redis-facing code.
- **Repository Pattern applies to Redis too**: `ICartRepository`, `IFavoriteRepository`,
  `IProductPopularityRepository`, and `IOrderQueueRepository` sit next to the SQL-backed `IProductRepository`/
  `IOrderRepository` — a repository is a data-access abstraction regardless of whether the store is SQL Server or
  Redis.
- **Generic data-structure services stay dumb**: `HashService`/`ListService`/`SetService`/`SortedSetService` know
  nothing about carts or favorites — only Redis commands. All business meaning (key naming, TTL, "remove instead of
  storing zero") lives in the feature repository/service layer above them.
- **Checkout clears the cart and prices at checkout time**: the cart only ever stores `productId → quantity`, so
  `OrderItem.UnitPrice` is captured from the product's current price at the moment of checkout, not a price snapshot
  taken when the item was added to the cart.
- **`Cart Expired` logging (from the original spec) is not implemented**: observing passive Redis key expiration
  requires keyspace notifications, which are a Pub/Sub mechanism — explicitly out of scope for this phase. Only
  explicit mutations (`Cart Updated`, `Cart Item Removed`, `Cart Cleared`) are logged.
- **No dedicated worker test**: `OrderProcessingWorker`'s `BackgroundService` polling loop isn't itself unit-tested
  (low value relative to effort); its behavior is covered indirectly by `OrderServiceTests` (enqueueing) and verified
  manually end-to-end via `redis-cli LRANGE` + API logs.

## Next recommended phase

**Phase 3: Redis Pub/Sub** — e.g. broadcasting stock-level change notifications when `StockQuantity` is updated, and
publishing an event when `OrderProcessingWorker` completes an order, to learn Redis's publish/subscribe messaging
model on top of the data-structure foundation built here.
