using eShop.Basket.API.Storage;
using eShop.ServiceDefaults;

namespace Microsoft.Extensions.Hosting;

public static class HostingExtensions
{
    public static IHostApplicationBuilder AddApplicationServices(this IHostApplicationBuilder builder)
    {
        builder.AddDefaultAuthentication();

        builder.AddRedisClient("BasketStore");
        builder.Services.AddSingleton<RedisBasketStore>();

        return builder;
    }
}