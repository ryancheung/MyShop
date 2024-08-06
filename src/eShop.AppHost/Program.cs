var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres").WithPgAdmin();
var catalogDb = postgres.AddDatabase("CatalogDB");

var identityDb = postgres.AddDatabase("IdentityDB");

var identityApi = builder.AddProject<Projects.Identity_API>("identity-api")
    .WithExternalHttpEndpoints()
    .WithReference(identityDb);

var identityEndpoint = identityApi.GetEndpoint("https");

builder.AddProject<Projects.Catalog_Data_Manager>("catalog-db-mgr").WithReference(catalogDb);
var catalogApi = builder.AddProject<Projects.Catalog_API>("catalog-api").WithReference(catalogDb);

var webApp = builder.AddProject<Projects.WebApp>("webapp")
    .WithReference(catalogApi)
    .WithEnvironment("IdentityUrl", identityEndpoint);

// Wire up the callback urls (self referencing)
webApp.WithEnvironment("CallBackUrl", webApp.GetEndpoint("https"));

identityApi.WithEnvironment("WebAppClient", webApp.GetEndpoint("https"));

builder.Build().Run();
