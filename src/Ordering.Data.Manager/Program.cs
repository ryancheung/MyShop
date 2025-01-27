using eShop.Ordering.Data;
using eShop.Ordering.Data.Manager;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddNpgsqlDbContext<OrderingDbContext>("OrderingDB", null, options =>
{
    options.UseNpgsql(npgsqlOption => npgsqlOption.MigrationsAssembly(typeof(Program).Assembly.GetName().Name));
});

builder.Services.AddMigration<OrderingDbContext, OrderingDbContextSeed>();

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();