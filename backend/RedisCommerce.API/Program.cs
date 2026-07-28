using Asp.Versioning;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RedisCommerce.API.Middleware;
using RedisCommerce.Application.Configuration;
using RedisCommerce.Application.Interfaces;
using RedisCommerce.Application.Services;
using RedisCommerce.Application.Validators;
using RedisCommerce.Infrastructure.Caching;
using RedisCommerce.Infrastructure.Data;
using RedisCommerce.Infrastructure.Repositories;
using RedisCommerce.Infrastructure.Workers;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<RedisCommerceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection(RedisOptions.SectionName));

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(sp.GetRequiredService<IOptions<RedisOptions>>().Value.ConnectionString));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IRedisCacheService, RedisCacheService>();

// Generic Redis data-structure services (Phase 2).
builder.Services.AddScoped<IHashService, HashService>();
builder.Services.AddScoped<IListService, ListService>();
builder.Services.AddScoped<ISetService, SetService>();
builder.Services.AddScoped<ISortedSetService, SortedSetService>();

// Centralized TTL policy (Phase 3).
builder.Services.AddScoped<ITTLPolicyProvider, TTLPolicyProvider>();

// Shopping cart (Redis Hash).
builder.Services.AddScoped<ICartRepository, RedisCartRepository>();
builder.Services.AddScoped<ICartService, CartService>();

// Favorites (Redis Set).
builder.Services.AddScoped<IFavoriteRepository, RedisFavoriteRepository>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();

// Product popularity (Redis Sorted Set).
builder.Services.AddScoped<IProductPopularityRepository, RedisProductPopularityRepository>();
builder.Services.AddScoped<IProductPopularityService, ProductPopularityService>();

// Order processing queue (Redis List) + checkout.
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderQueueRepository, RedisOrderQueueRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddHostedService<OrderProcessingWorker>();

builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddValidatorsFromAssemblyContaining<CreateProductRequestValidator>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    })
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var corsAllowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(corsAllowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Apply migrations and seed data at startup.
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RedisCommerceDbContext>();
    await context.Database.MigrateAsync();
    await DbInitializer.SeedAsync(context);
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAuthorization();

app.MapControllers();

app.Run();
