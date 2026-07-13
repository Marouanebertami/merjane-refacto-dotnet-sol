using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Entities;

namespace Refacto.DotNet.Controllers.Services.Strategies.Impl
{
    public class SeasonalProductStockProcessor : ProductStockProcessorBase
    {
        public SeasonalProductStockProcessor(INotificationService notificationService, AppDbContext ctx)
            : base(notificationService, ctx)
        {
        }

        public override ProductType SupportedType => ProductType.Seasonal;

        public override void Process(Product product)
        {
            if (DateTime.Now.Date > product.SeasonStartDate && DateTime.Now.Date < product.SeasonEndDate && product.Available > 0)
            {
                product.Available -= 1;
                _ = Ctx.SaveChanges();
            }
            else
            {
                HandleSeasonalProduct(product);
            }
        }

        private void HandleSeasonalProduct(Product product)
        {
            if (DateTime.Now.AddDays(product.LeadTime) > product.SeasonEndDate)
            {
                NotificationService.SendOutOfStockNotification(product.Name ?? "");
                product.Available = 0;
                _ = Ctx.SaveChanges();
            }
            else if (product.SeasonStartDate > DateTime.Now)
            {
                NotificationService.SendOutOfStockNotification(product.Name ?? "");
                _ = Ctx.SaveChanges();
            }
            else
            {
                NotifyDelay(product);
            }
        }
    }
}
