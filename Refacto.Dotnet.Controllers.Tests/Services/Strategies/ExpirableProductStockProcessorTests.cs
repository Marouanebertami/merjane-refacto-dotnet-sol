using Moq;
using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Entities;
using Refacto.DotNet.Controllers.Services;
using Refacto.DotNet.Controllers.Services.Strategies.Impl;

namespace Refacto.DotNet.Controllers.Tests.Services.Strategies;

public class ExpirableProductStockProcessorTests
{
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<AppDbContext> _mockDbContext;
    private readonly ExpirableProductStockProcessor _processor;

    public ExpirableProductStockProcessorTests()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _mockDbContext = new Mock<AppDbContext>();
        _processor = new ExpirableProductStockProcessor(_mockNotificationService.Object, _mockDbContext.Object);
    }

    [Fact]
    public void Process_WhenAvailableAndNotExpired_Decrements()
    {
        Product product = new()
        {
            LeadTime = 15,
            Available = 5,
            Type = "EXPIRABLE",
            Name = "Butter",
            ExpiryDate = DateTime.Now.AddDays(10)
        };

        _processor.Process(product);

        Assert.Equal(4, product.Available);
        _mockDbContext.Verify(ctx => ctx.SaveChanges(), Times.Once());
        _mockNotificationService.Verify(s => s.SendExpirationNotification(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never());
    }

    [Fact]
    public void Process_WhenExpired_SendsExpirationNotificationAndZeroesAvailable()
    {
        DateTime expiryDate = DateTime.Now.AddDays(-2);
        Product product = new()
        {
            LeadTime = 90,
            Available = 5,
            Type = "EXPIRABLE",
            Name = "Milk",
            ExpiryDate = expiryDate
        };

        _processor.Process(product);

        Assert.Equal(0, product.Available);
        _mockNotificationService.Verify(s => s.SendExpirationNotification(product.Name, expiryDate), Times.Once());
        _mockDbContext.Verify(ctx => ctx.SaveChanges(), Times.Once());
    }

    [Fact]
    public void Process_WhenAvailableIsZero_SendsExpirationNotificationAndZeroesAvailable()
    {
        DateTime expiryDate = DateTime.Now.AddDays(10);
        Product product = new()
        {
            LeadTime = 15,
            Available = 0,
            Type = "EXPIRABLE",
            Name = "Cheese",
            ExpiryDate = expiryDate
        };

        _processor.Process(product);

        Assert.Equal(0, product.Available);
        _mockNotificationService.Verify(s => s.SendExpirationNotification(product.Name, expiryDate), Times.Once());
        _mockDbContext.Verify(ctx => ctx.SaveChanges(), Times.Once());
    }
}
