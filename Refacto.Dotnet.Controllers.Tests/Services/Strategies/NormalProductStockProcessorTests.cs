using Moq;
using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Entities;
using Refacto.DotNet.Controllers.Services;
using Refacto.DotNet.Controllers.Services.Strategies.Impl;

namespace Refacto.DotNet.Controllers.Tests.Services.Strategies;

public class NormalProductStockProcessorTests
{
    private readonly Mock<INotificationService> _mockNotificationService;
    private readonly Mock<AppDbContext> _mockDbContext;
    private readonly NormalProductStockProcessor _processor;

    public NormalProductStockProcessorTests()
    {
        _mockNotificationService = new Mock<INotificationService>();
        _mockDbContext = new Mock<AppDbContext>();
        _processor = new NormalProductStockProcessor(_mockNotificationService.Object, _mockDbContext.Object);
    }

    [Fact]
    public void Process_WhenAvailable_DecrementsAndSaves()
    {
        Product product = new() { LeadTime = 15, Available = 5, Type = "NORMAL", Name = "USB Cable" };

        _processor.Process(product);

        Assert.Equal(4, product.Available);
        _mockDbContext.Verify(ctx => ctx.SaveChanges(), Times.Once());
        _mockNotificationService.Verify(s => s.SendDelayNotification(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
    }

    [Fact]
    public void Process_WhenOutOfStockAndLeadTimePositive_NotifiesDelay()
    {
        Product product = new() { LeadTime = 10, Available = 0, Type = "NORMAL", Name = "USB Dongle" };

        _processor.Process(product);

        Assert.Equal(0, product.Available);
        Assert.Equal(10, product.LeadTime);
        _mockDbContext.Verify(ctx => ctx.SaveChanges(), Times.Once());
        _mockNotificationService.Verify(s => s.SendDelayNotification(10, product.Name), Times.Once());
    }

    [Fact]
    public void Process_WhenOutOfStockAndLeadTimeZeroOrLess_DoesNothing()
    {
        Product product = new() { LeadTime = 0, Available = 0, Type = "NORMAL", Name = "Discontinued Widget" };

        _processor.Process(product);

        Assert.Equal(0, product.Available);
        _mockDbContext.Verify(ctx => ctx.SaveChanges(), Times.Never());
        _mockNotificationService.Verify(s => s.SendDelayNotification(It.IsAny<int>(), It.IsAny<string>()), Times.Never());
        _mockNotificationService.Verify(s => s.SendOutOfStockNotification(It.IsAny<string>()), Times.Never());
        _mockNotificationService.Verify(s => s.SendExpirationNotification(It.IsAny<string>(), It.IsAny<DateTime>()), Times.Never());
    }
}
