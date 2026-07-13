using Moq;
using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Entities;
using Refacto.DotNet.Controllers.Services;
using Refacto.DotNet.Controllers.Services.Strategies.Impl;

namespace Refacto.DotNet.Controllers.Tests.Services.Strategies;

public class SeasonalProductStockProcessorTests
{
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<AppDbContext> _mockDbContext;
    private readonly SeasonalProductStockProcessor _processor;

    public SeasonalProductStockProcessorTests()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _mockDbContext = new Mock<AppDbContext>();
        _processor = new SeasonalProductStockProcessor(_mockNotificationService.Object, _mockDbContext.Object);
    }

    [Fact]
    public void Process_WhenInSeasonAndAvailable_Decrements()
    {
        Product product = new()
        {
            LeadTime = 5,
            Available = 5,
            Type = "SEASONAL",
            Name = "Watermelon",
            SeasonStartDate = DateTime.Now.AddDays(-10),
            SeasonEndDate = DateTime.Now.AddDays(60)
        };

        _processor.Process(product);

        Assert.Equal(4, product.Available);
        _mockDbContext.Verify(ctx => ctx.SaveChanges(), Times.Once());
        _mockNotificationService.Verify(s => s.SendOutOfStockNotification(It.IsAny<string>()), Times.Never());
        _mockNotificationService.Verify(s => s.SendDelayNotification(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
    }

    [Fact]
    public void Process_WhenLeadTimePushesPastSeasonEnd_SendsOutOfStockAndZeroesAvailable()
    {
        Product product = new()
        {
            LeadTime = 30,
            Available = 0,
            Type = "SEASONAL",
            Name = "Pumpkin",
            SeasonStartDate = DateTime.Now.AddDays(-10),
            SeasonEndDate = DateTime.Now.AddDays(5)
        };

        _processor.Process(product);

        Assert.Equal(0, product.Available);
        _mockNotificationService.Verify(s => s.SendOutOfStockNotification(product.Name), Times.Once());
        _mockDbContext.Verify(ctx => ctx.SaveChanges(), Times.Once());
    }

    [Fact]
    public void Process_WhenSeasonNotYetStarted_SendsOutOfStockNotification_LeavesAvailableUnchanged()
    {
        Product product = new()
        {
            LeadTime = 5,
            Available = 5,
            Type = "SEASONAL",
            Name = "Grapes",
            SeasonStartDate = DateTime.Now.AddDays(10),
            SeasonEndDate = DateTime.Now.AddDays(60)
        };

        _processor.Process(product);

        Assert.Equal(5, product.Available);
        _mockNotificationService.Verify(s => s.SendOutOfStockNotification(product.Name), Times.Once());
        _mockDbContext.Verify(ctx => ctx.SaveChanges(), Times.Once());
    }

    [Fact]
    public void Process_WhenInSeasonWindowButUnavailable_NotifiesDelay()
    {
        Product product = new()
        {
            LeadTime = 7,
            Available = 0,
            Type = "SEASONAL",
            Name = "Strawberries",
            SeasonStartDate = DateTime.Now.AddDays(-10),
            SeasonEndDate = DateTime.Now.AddDays(60)
        };

        _processor.Process(product);

        Assert.Equal(0, product.Available);
        _mockNotificationService.Verify(s => s.SendDelayNotification(7, product.Name), Times.Once());
        _mockDbContext.Verify(ctx => ctx.SaveChanges(), Times.Once());
    }
}
