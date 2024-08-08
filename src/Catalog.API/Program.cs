using eShop.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.AddApplicationServices();
builder.Services.AddProblemDetails();

var withApiVersioning = builder.Services.AddApiVersioning();

builder.AddDefaultOpenApi(withApiVersioning);

var app = builder.Build();

app.UseDefaultOpenApi();

app.MapDefaultEndpoints();

app.NewVersionedApi("Catalog")
    .MapCatalogApiV1();

app.Run();
