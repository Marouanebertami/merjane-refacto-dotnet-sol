using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Entities;

namespace Refacto.DotNet.Controllers.Services.Strategies.Impl;

public class ExpirableProductStockProcessor : ProductStockProcessorBase
{
    public ExpirableProductStockProcessor(INotificationService notificationService, AppDbContext ctx)
        : base(notificationService, ctx)
    {
    }

    public override ProductType SupportedType => ProductType.Expirable;

    public override void Process(Product product)
    {
        if (product.Available > 0 && product.ExpiryDate > DateTime.Now.Date)
        {
            product.Available -= 1;
            _ = Ctx.SaveChanges();
        }
        else
        {
            HandleExpiredProduct(product);
        }
    }

    private void HandleExpiredProduct(Product product)
    {
        if (product.Available > 0 && product.ExpiryDate > DateTime.Now)
        {
            product.Available -= 1;
            _ = Ctx.SaveChanges();
        }
        else
        {
            NotificationService.SendExpirationNotification(product.Name ?? "", (DateTime)product.ExpiryDate);
            product.Available = 0;
            _ = Ctx.SaveChanges();
        }
    }
}
