using eShop.WebApp.Components.Catalog;

namespace eShop.WebApp.Services;

public class CatalogService(HttpClient httpClient)
{
    private readonly string remoteServiceBaseUrl = "api/catalog/";
    const string ApiVersion = "api-version=1.0";

    public async Task<IEnumerable<CatalogBrand>> GetBrands()
    {
        var uri = $"{remoteServiceBaseUrl}catalogBrands?{ApiVersion}";
        var result = await httpClient.GetFromJsonAsync<CatalogBrand[]>(uri);
        return result ?? [];
    }
    
    public async Task<IEnumerable<CatalogItemType>> GetTypes()
    {
        var uri = $"{remoteServiceBaseUrl}catalogTypes?{ApiVersion}";
        var result = await httpClient.GetFromJsonAsync<CatalogItemType[]>(uri);
        return result ?? [];
    }

    public async Task<CatalogResult> GetCatalogItems(int pageIndex, int pageSize, int? brand, int? type)
    {
        var uri = GetAllCatalogItemsUri(remoteServiceBaseUrl, pageIndex, pageSize, brand, type); 
        var result = await httpClient.GetFromJsonAsync<CatalogResult>(uri);

        return result ?? new(0, 0, 0, []);
    }

    public async Task<List<CatalogItem>> GetCatalogItems(IEnumerable<int> ids)
    {
        var uri = $"{remoteServiceBaseUrl}items/by?ids={string.Join("&ids=", ids)}&{ApiVersion}";
        var result = await httpClient.GetFromJsonAsync<List<CatalogItem>>(uri);

        return result ?? [];
    }

    public Task<CatalogItem?> GetCatalogItem(int itemId)
    {
        var uri = $"{remoteServiceBaseUrl}items/{itemId}?{ApiVersion}"; 
        return httpClient.GetFromJsonAsync<CatalogItem>(uri);
    }

    private static string GetAllCatalogItemsUri(string baseUri, int pageIndex, int pageSize, int? brand, int? type)
    {
        // Build URLs like:
        //   [base]/items
        //   [base]/items/type/all
        //   [base]/items/type/123/brand/456
        //   [base]/items/type/123/brand/456?pageSize=9&pageIndex=2
        string filterPath;

        if (type.HasValue)
        {
            var brandPath = brand.HasValue ? brand.Value.ToString() : string.Empty;
            filterPath = $"/type/{type.Value}/brand/{brandPath}";

        }
        else if (brand.HasValue)
        {
            var brandPath = brand.HasValue ? brand.Value.ToString() : string.Empty;
            filterPath = $"/type/all/brand/{brandPath}";
        }
        else
        {
            filterPath = string.Empty;
        }

        return $"{baseUri}items{filterPath}?pageIndex={pageIndex}&pageSize={pageSize}&{ApiVersion}";
    }

}