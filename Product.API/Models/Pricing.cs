namespace Product.Models;

public class Pricing
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal CurrentPrice { get; set; }
    public List<PriceHistoryEntry> PriceHistory { get; set; } = [];
    public DateTime LastUpdated { get; set; }
}

public record PriceHistoryEntry(decimal Price, DateTime Date);