using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Entities;

namespace Refacto.DotNet.Controllers.Services.Strategies;

public abstract class ProductStockProcessorBase : IProductStockProcessor
{
    protected readonly INotificationService NotificationService;
    protected readonly AppDbContext Ctx;

    protected ProductStockProcessorBase(INotificationService notificationService, AppDbContext ctx)
    {
        NotificationService = notificationService;
        Ctx = ctx;
    }

    public abstract ProductType SupportedType { get; }
    public abstract void Process(Product product);

    protected void NotifyDelay(Product product)
    {
        _ = Ctx.SaveChanges();
        NotificationService.SendDelayNotification(product.LeadTime, product.Name ?? "");
    }
}
