namespace Product.UI.Models;

public record ProductResponse(int Id, string Name, decimal Price, DateTime LastUpdated);

public record ProductDetailResponse(string Name, List<PriceHistoryEntryResponse> PriceHistory);

public record PriceHistoryEntryResponse(decimal Price, DateTime Date);

public record DiscountedProductResponse(int Id, string Name, decimal OriginalPrice, decimal DiscountedPrice);

public record UpdatePriceResponse(int Id, string Name, decimal NewPrice, DateTime LastUpdated);
