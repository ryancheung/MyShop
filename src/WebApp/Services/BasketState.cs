using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using eShop.WebApp.Components.Catalog;

namespace eShop.WebApp.Services;

public class BasketState(
	BasketService basketService,
	CatalogService catalogService,
	OrderingService orderingService,
	AuthenticationStateProvider authenticationStateProvider
)
{
	Task<IReadOnlyCollection<BasketItem>>? _cachedBasket;
	readonly HashSet<BasketStateChangedSubscription> _changeSubscriptions = [];

	public Task DeleteBasketAsync()
        => basketService.DeleteBasketAsync();

	public async Task<IReadOnlyCollection<BasketItem>> GetBasketItemsAsync()
	      => (await GetUserAsync()).Identity?.IsAuthenticated == true
        ? await FetchBasketItemsAsync()
        : [];

	public IDisposable NotifyOnChange(EventCallback callback)
	{
		var subscription = new BasketStateChangedSubscription(this, callback);
		_changeSubscriptions.Add(subscription);
		return subscription;
	}

	public async Task AddAsync(CatalogItem item)
	{
        var items = (await FetchBasketItemsAsync()).Select(i => new BasketQuantity(i.ProductId, i.Quantity)).ToList();
        bool found = false;
        for (var i = 0; i < items.Count; i++)
        {
            var existing = items[i];
            if (existing.ProductId == item.Id)
            {
                items[i] = existing with { Quantity = existing.Quantity + 1 };
                found = true;
                break;
            }
        }

        if (!found)
        {
            items.Add(new BasketQuantity(item.Id, 1));
        }

        _cachedBasket = null;
        await basketService.UpdateBasketAsync(items);
        await NotifyChangeSubscribersAsync();
	}

    public async Task SetQuantityAsync(int productId, int quantity)
    {
        var existingItems = (await FetchBasketItemsAsync()).ToList();
        if (existingItems.FirstOrDefault(row => row.ProductId == productId) is { } row)
        {
            if (quantity > 0)
            {
                row.Quantity = quantity;
            }
            else
            {
                existingItems.Remove(row);
            }

            _cachedBasket = null;
            await basketService.UpdateBasketAsync(existingItems.Select(i => new BasketQuantity(i.ProductId, i.Quantity)).ToList());
            await NotifyChangeSubscribersAsync();
        }
    }

    public async Task CheckoutAsync(BasketCheckoutInfo checkoutInfo)
    {
        if (checkoutInfo.RequestId == default)
        {
            checkoutInfo.RequestId = Guid.NewGuid();
        }

        var userName = await authenticationStateProvider.GetUserNameAsync() ?? throw new InvalidOperationException("User does not have a user name");

        // Get details for the items in the basket
        var orderItems = await FetchBasketItemsAsync();

        // Call into Ordering.API to create the order using those details
        var request = new CreateOrderRequest(
            UserName: userName,
            City: checkoutInfo.City!,
            Street: checkoutInfo.Street!,
            State: checkoutInfo.State!,
            Country: checkoutInfo.Country!,
            ZipCode: checkoutInfo.ZipCode!,
            CardNumber: checkoutInfo.CardNumber!,
            CardHolderName: checkoutInfo.CardHolderName!,
            CardExpiration: checkoutInfo.CardExpiration!.Value, 
            CardSecurityNumber: checkoutInfo.CardSecurityNumber!,
            CardTypeId: checkoutInfo.CardTypeId,
            Items: [.. orderItems]);
        
        await orderingService.CreateOrder(request, checkoutInfo.RequestId);

        // Delete the basket
        await DeleteBasketAsync();
    }

	Task NotifyChangeSubscribersAsync()
		=> Task.WhenAll(_changeSubscriptions.Select(s => s.NotifyAsync()));

	async Task<ClaimsPrincipal> GetUserAsync()
	        => (await authenticationStateProvider.GetAuthenticationStateAsync()).User;

	Task<IReadOnlyCollection<BasketItem>> FetchBasketItemsAsync()
	{
		return _cachedBasket ??= FetchCoreAsync();

		async Task<IReadOnlyCollection<BasketItem>> FetchCoreAsync()
		{
			var quantities = await basketService.GetBasketAsync();
			if (quantities.Count == 0)
				return [];

			// Get details for the items in the basket
			var basketItems = new List<BasketItem>();
			var productIds = quantities.Select(row => row.ProductId);
			var catalogItems = (await catalogService.GetCatalogItems(productIds)).ToDictionary(k => k.Id, v => v);
			foreach (var item in quantities)
			{
				var catalogItem = catalogItems[item.ProductId];
				var orderItem = new BasketItem
				{
					Id = Guid.NewGuid().ToString(),
					ProductId = catalogItem.Id,
					ProductName = catalogItem.Name,
					UnitPrice = catalogItem.Price,
					Quantity = item.Quantity,
				};
				basketItems.Add(orderItem);
			}

			return basketItems;
		}
	}

	private class BasketStateChangedSubscription(BasketState Owner, EventCallback Callback) : IDisposable
	{
		public Task NotifyAsync() => Callback.InvokeAsync();
		public void Dispose() => Owner._changeSubscriptions.Remove(this);
	}
}

public record CreateOrderRequest(
    string UserName,
    string City,
    string Street,
    string State,
    string Country,
    string ZipCode,
    string CardNumber,
    string CardHolderName,
    DateTime CardExpiration,
    string CardSecurityNumber,
    int CardTypeId,
    List<BasketItem> Items);
