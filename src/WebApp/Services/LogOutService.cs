using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace eShop.WebApp.Services;

public class LogOutService
{
    public async Task LogOutAsync(HttpContext httpContext)
    {
        await Task.CompletedTask;
    }
}
