using Duende.IdentityServer;
using Duende.IdentityServer.Models;

namespace eShop.Identity.API.Configuration;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        new IdentityResource[]
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        };

    public static IEnumerable<ApiScope> ApiScopes =>
        new ApiScope[]
        {
            new ApiScope("orders", "Orders Service"),
            new ApiScope("basket", "Basket Service"),
        };

    public static IEnumerable<ApiResource> Apis =>
        new ApiResource[]
        {
            new ApiResource("orders", "Orders Service"),
            new ApiResource("basket", "Basket Service"),
        };

    public static IEnumerable<Client> GetClients(IConfiguration configuration) =>
        new Client[]
        {
            new Client
            {
                ClientId = "webapp",
                ClientName = "WebApp Client",
                ClientSecrets = new List<Secret>
                {
                    new Secret("secret".Sha256())
                },
                ClientUri = $"{configuration["WebAppClient"]}",                             // public uri of the client
                AllowedGrantTypes = GrantTypes.Code,
                AllowAccessTokensViaBrowser = false,
                RequireConsent = false,
                AllowOfflineAccess = true,
                AlwaysIncludeUserClaimsInIdToken = true,
                RequirePkce = false,
                RedirectUris = new List<string>
                {
                    $"{configuration["WebAppClient"]}/signin-oidc"
                },
                PostLogoutRedirectUris = new List<string>
                {
                    $"{configuration["WebAppClient"]}/signout-callback-oidc"
                },
                AllowedScopes = new List<string>
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.OfflineAccess,
                    "orders",
                    "basket",
                },
                AccessTokenLifetime = 60*60*2, // 2 hours
                IdentityTokenLifetime= 60*60*2 // 2 hours
            },
            new Client
            {
                ClientId = "orderingswaggerui",
                ClientName = "Ordering Swagger UI",
                AllowedGrantTypes = GrantTypes.Implicit,
                AllowAccessTokensViaBrowser = true,

                RedirectUris = { $"{configuration["OrderingApiClient"]}/swagger/oauth2-redirect.html", $"{configuration["OrderingApiClientHttps"]}/swagger/oauth2-redirect.html" },
                PostLogoutRedirectUris = { $"{configuration["OrderingApiClient"]}/swagger/", $"{configuration["OrderingApiClientHttps"]}/swagger/" },

                AllowedScopes =
                {
                    "orders"
                }
            },
        };
}
