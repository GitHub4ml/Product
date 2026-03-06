using Product.UI.Models;
using System.Net;

namespace Product.UI.Services;

public interface IProductApiClient
{
    Task<List<ProductResponse>> GetProductsAsync();
    Task<ProductDetailResponse?> GetProductByIdAsync(int id);
    Task<DiscountedProductResponse?> ApplyDiscountAsync(int id, int discountPercentage);
    Task<UpdatePriceResponse?> UpdatePriceAsync(int id, decimal newPrice);
}

public class ProductApiClient(HttpClient httpClient) : IProductApiClient
{
    public async Task<List<ProductResponse>> GetProductsAsync()
    {
        var result = await httpClient.GetFromJsonAsync<List<ProductResponse>>("api/product");
        return result ?? [];
    }

    public async Task<ProductDetailResponse?> GetProductByIdAsync(int id)
    {
        var response = await httpClient.GetAsync($"api/product/{id}");
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        return await ReadOrThrowAsync<ProductDetailResponse>(response);
    }

    public async Task<DiscountedProductResponse?> ApplyDiscountAsync(int id, int discountPercentage)
    {
        var response = await httpClient.PostAsync(
            $"api/product/{id}/discount?discountPercentage={discountPercentage}", null);

        return await ReadOrThrowAsync<DiscountedProductResponse>(response);
    }

    public async Task<UpdatePriceResponse?> UpdatePriceAsync(int id, decimal newPrice)
    {
        var response = await httpClient.PutAsync(
            $"api/product/{id}/price?newPrice={newPrice}", null);

        return await ReadOrThrowAsync<UpdatePriceResponse>(response);
    }
    private static async Task<T?> ReadOrThrowAsync<T>(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<T>();

        var body = await response.Content.ReadAsStringAsync();
        var message = string.IsNullOrWhiteSpace(body)
            ? $"Request failed ({(int)response.StatusCode})"
            : body.Trim('"');

        throw new HttpRequestException(message, inner: null, response.StatusCode);
    }
}