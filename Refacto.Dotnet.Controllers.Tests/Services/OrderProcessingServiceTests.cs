using Microsoft.EntityFrameworkCore;
using Moq;
using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Entities;
using Refacto.DotNet.Controllers.Services.Impl;
using Refacto.DotNet.Controllers.Services.Strategies;

namespace Refacto.DotNet.Controllers.Tests.Services;

public class OrderProcessingServiceTests
{
    private static AppDbContext CreateContext()
    {
        DbContextOptions<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static Mock<IProductStockProcessor> CreateProcessorMock(ProductType type)
    {
        Mock<IProductStockProcessor> mock = new();
        _ = mock.Setup(p => p.SupportedType).Returns(type);
        return mock;
    }

    [Fact]
    public void ProcessOrder_WhenOrderMissing_ReturnsNull()
    {
        using AppDbContext ctx = CreateContext();
        OrderProcessingService service = new(ctx, Array.Empty<IProductStockProcessor>());

        Order? result = service.ProcessOrder(999);

        Assert.Null(result);
    }

    [Fact]
    public void ProcessOrder_DispatchesEachProductToItsMatchingProcessor()
    {
        using AppDbContext ctx = CreateContext();

        Product normal = new() { Type = "NORMAL", Name = "Normal Item", Available = 1, LeadTime = 1 };
        Product seasonal = new() { Type = "SEASONAL", Name = "Seasonal Item", Available = 1, LeadTime = 1 };
        Product expirable = new() { Type = "EXPIRABLE", Name = "Expirable Item", Available = 1, LeadTime = 1 };
        Order order = new() { Items = new List<Product> { normal, seasonal, expirable } };

        ctx.Products.AddRange(normal, seasonal, expirable);
        ctx.Orders.Add(order);
        ctx.SaveChanges();

        Mock<IProductStockProcessor> normalProcessor = CreateProcessorMock(ProductType.Normal);
        Mock<IProductStockProcessor> seasonalProcessor = CreateProcessorMock(ProductType.Seasonal);
        Mock<IProductStockProcessor> expirableProcessor = CreateProcessorMock(ProductType.Expirable);

        OrderProcessingService service = new(ctx, new[]
        {
            normalProcessor.Object, seasonalProcessor.Object, expirableProcessor.Object
        });

        Order? result = service.ProcessOrder(order.Id);

        Assert.NotNull(result);
        normalProcessor.Verify(p => p.Process(normal), Times.Once());
        seasonalProcessor.Verify(p => p.Process(seasonal), Times.Once());
        expirableProcessor.Verify(p => p.Process(expirable), Times.Once());
    }

    [Fact]
    public void ProcessOrder_WhenProductTypeUnrecognizedOrNull_SkipsWithoutThrowing()
    {
        using AppDbContext ctx = CreateContext();

        Product unknown = new() { Type = "UNKNOWN", Name = "Unknown Item", Available = 1 };
        Product noType = new() { Type = null, Name = "Untyped Item", Available = 1 };
        Order order = new() { Items = new List<Product> { unknown, noType } };

        ctx.Products.AddRange(unknown, noType);
        ctx.Orders.Add(order);
        ctx.SaveChanges();

        Mock<IProductStockProcessor> normalProcessor = CreateProcessorMock(ProductType.Normal);
        OrderProcessingService service = new(ctx, new[] { normalProcessor.Object });

        Order? result = service.ProcessOrder(order.Id);

        Assert.NotNull(result);
        normalProcessor.Verify(p => p.Process(It.IsAny<Product>()), Times.Never());
    }
}
