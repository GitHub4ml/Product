using Microsoft.AspNetCore.Mvc;
using Product.DTOs;
using Product.Models;

namespace Product.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private static readonly List<Pricing> _products =
    [
        new()
        {
            Id = 1, Name = "Pokemon Small Tin", CurrentPrice = 9.99m, LastUpdated = DateTime.UtcNow,
            PriceHistory =
            [
                new(7.99m, DateTime.UtcNow.AddMonths(-3)),
                new(8.99m, DateTime.UtcNow.AddMonths(-2)),
                new(9.99m, DateTime.UtcNow.AddMonths(-1)),
            ]
        },
        new()
        {
            Id = 2, Name = "Pokemon Tin", CurrentPrice = 19.99m, LastUpdated = DateTime.UtcNow,
            PriceHistory =
            [
                new(15.99m, DateTime.UtcNow.AddMonths(-3)),
                new(17.99m, DateTime.UtcNow.AddMonths(-2)),
                new(19.99m, DateTime.UtcNow.AddMonths(-1)),
            ]
        },
        new()
        {
            Id = 3, Name = "Pokemon Pack", CurrentPrice = 4.30m, LastUpdated = DateTime.UtcNow,
            PriceHistory =
            [
                new(3.50m, DateTime.UtcNow.AddMonths(-3)),
                new(3.99m, DateTime.UtcNow.AddMonths(-2)),
                new(4.30m, DateTime.UtcNow.AddMonths(-1)),
            ]
        },
    ];

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<ProductResponse>> GetProducts()
    {
        var response = _products.Select(p => new ProductResponse(p.Id, p.Name, p.CurrentPrice, p.LastUpdated));
        return Ok(response);
    }


    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ProductDetailResponse> GetProductById(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);

        if (product is null)
            return NotFound();

        var response = new ProductDetailResponse(
            product.Name,
            product.PriceHistory.Select(h => new DTOs.PriceHistoryEntryResponse(h.Price, h.Date)).ToList()
        );

        return Ok(response);
    }

    [HttpPost("{id:int}/discount")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<DiscountedProductResponse> ApplyDiscount(int id, int discountPercentage)
    {
        if (discountPercentage is <= 0 or >= 100)
            return BadRequest("Discount percentage must be between 1 and 99.");

        var product = _products.FirstOrDefault(p => p.Id == id);

        if (product is null)
            return NotFound();

        var originalPrice = product.CurrentPrice;
        var discountedPrice = Math.Round(originalPrice * (1 - discountPercentage / 100m), 2);

        return Ok(new DiscountedProductResponse(product.Id, product.Name, originalPrice, discountedPrice));
    }

    [HttpPut("{id:int}/price")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<UpdatePriceResponse> UpdatePrice(int id, decimal newPrice)
    {
        if (newPrice <= 0)
            return BadRequest("Price must be greater than 0.");

        var product = _products.FirstOrDefault(p => p.Id == id);

        if (product is null)
            return NotFound();

        product.PriceHistory.Add(new(product.CurrentPrice, DateTime.UtcNow));
        product.CurrentPrice = newPrice;
        product.LastUpdated = DateTime.UtcNow;

        return Ok(new UpdatePriceResponse(product.Id, product.Name, product.CurrentPrice, product.LastUpdated));
    }
}
