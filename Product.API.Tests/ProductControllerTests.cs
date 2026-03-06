using Microsoft.AspNetCore.Mvc;
using Product.Controllers;
using Product.DTOs;

namespace Product.API.Tests.ProductControllerTests;

[TestFixture]
public class ProductControllerTests
{
    private ProductController _controller;

    [SetUp]
    public void Setup()
    {
        _controller = new ProductController();
    }

    [Test]
    public void GetProducts_ReturnsOk_WithAllProducts()
    {
        var result = _controller.GetProducts();
        var okResult = result.Result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        var products = okResult!.Value as IEnumerable<ProductResponse>;
        Assert.That(products?.Count(), Is.EqualTo(3));
    }

    [Test]
    public void GetProductById_ValidId_ReturnsMatchingProduct()
    {
        var result = _controller.GetProductById(1);
        var okResult = result.Result as OkObjectResult;

        Assert.That(okResult, Is.Not.Null);
        var product = okResult!.Value as ProductDetailResponse;
        Assert.That(product?.Name, Is.EqualTo("Pokemon Small Tin"));
    }

    [Test]
    public void GetProductById_ValidId_ReturnsPriceHistory()
    {
        var result = _controller.GetProductById(1);
        var okResult = result.Result as OkObjectResult;
        var product = okResult!.Value as ProductDetailResponse;

        Assert.That(product?.PriceHistory, Is.Not.Null);
        Assert.That(product!.PriceHistory, Is.Not.Empty);
    }

    [Test]
    public void GetProductById_InvalidId_ReturnsNotFound()
    {
        var result = _controller.GetProductById(999);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public void ApplyDiscount_ValidPercentage_ReturnsCorrectDiscountedPrice()
    {
        var result = _controller.ApplyDiscount(1, 10);
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as DiscountedProductResponse;

        Assert.That(response?.OriginalPrice, Is.EqualTo(9.99m));
        Assert.That(response?.DiscountedPrice, Is.EqualTo(8.99m));
    }

    [Test]
    public void ApplyDiscount_ValidPercentage_ResponseContainsProductDetails()
    {
        var result = _controller.ApplyDiscount(1, 10);
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as DiscountedProductResponse;

        Assert.That(response?.Id, Is.EqualTo(1));
        Assert.That(response?.Name, Is.EqualTo("Pokemon Small Tin"));
    }

    [Test]
    public void ApplyDiscount_DoesNotMutateCurrentPrice()
    {
        var before = _controller.GetProducts().Result as OkObjectResult;
        var priceBefore = (before!.Value as IEnumerable<ProductResponse>)!
            .First(p => p.Id == 1).Price;

        _controller.ApplyDiscount(1, 20);

        var after = _controller.GetProducts().Result as OkObjectResult;
        var priceAfter = (after!.Value as IEnumerable<ProductResponse>)!
            .First(p => p.Id == 1).Price;

        Assert.That(priceAfter, Is.EqualTo(priceBefore));
    }

    [Test]
    [TestCase(0)]
    [TestCase(-5)]
    [TestCase(100)]
    [TestCase(150)]
    public void ApplyDiscount_InvalidPercentage_ReturnsBadRequest(int percentage)
    {
        var result = _controller.ApplyDiscount(1, percentage);

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void ApplyDiscount_InvalidProductId_ReturnsNotFound()
    {
        var result = _controller.ApplyDiscount(999, 10);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }

    [Test]
    public void UpdatePrice_ValidPrice_ReturnsUpdatedPrice()
    {
        var result = _controller.UpdatePrice(2, 25.00m);
        var okResult = result.Result as OkObjectResult;
        var response = okResult!.Value as UpdatePriceResponse;

        Assert.That(response?.NewPrice, Is.EqualTo(25.00m));
        Assert.That(response?.Name, Is.EqualTo("Pokemon Tin"));
    }

    [Test]
    public void UpdatePrice_ValidPrice_UpdatesLastUpdated()
    {
        var before = DateTime.UtcNow;
        var result = _controller.UpdatePrice(2, 22.00m);
        var response = (result.Result as OkObjectResult)!.Value as UpdatePriceResponse;

        Assert.That(response?.LastUpdated, Is.GreaterThanOrEqualTo(before));
    }

    [Test]
    public void UpdatePrice_ValidPrice_AddsPreviousPriceToHistory()
    {
        var historyBefore = (_controller.GetProductById(3).Result as OkObjectResult)!
            .Value as ProductDetailResponse;
        var countBefore = historyBefore!.PriceHistory.Count;

        _controller.UpdatePrice(3, 5.99m);

        var historyAfter = (_controller.GetProductById(3).Result as OkObjectResult)!
            .Value as ProductDetailResponse;

        Assert.That(historyAfter?.PriceHistory.Count, Is.EqualTo(countBefore + 1));
    }

    [Test]
    [TestCase(0)]
    [TestCase(-1)]
    public void UpdatePrice_InvalidPrice_ReturnsBadRequest(decimal price)
    {
        var result = _controller.UpdatePrice(1, price);

        Assert.That(result.Result, Is.InstanceOf<BadRequestObjectResult>());
    }

    [Test]
    public void UpdatePrice_InvalidProductId_ReturnsNotFound()
    {
        var result = _controller.UpdatePrice(999, 10.00m);

        Assert.That(result.Result, Is.InstanceOf<NotFoundResult>());
    }
}