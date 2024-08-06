using System.Security.Claims;
using eShop.ServiceDefaults;

namespace Grpc.Core;

internal static class GrpcExtensions
{
    public static string? GetUserIdentity(this ServerCallContext context) => context.GetHttpContext().User.GetUserId();
}