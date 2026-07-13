using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Entities;

namespace Refacto.DotNet.Controllers.Services.Strategies.Impl
{
    public class NormalProductStockProcessor : ProductStockProcessorBase
    {
        public NormalProductStockProcessor(INotificationService notificationService, AppDbContext ctx)
            : base(notificationService, ctx)
        {
        }

        public override ProductType SupportedType => ProductType.Normal;

        public override void Process(Product product)
        {
            if (product.Available > 0)
            {
                product.Available -= 1;
                _ = Ctx.SaveChanges();
            }
            else if (product.LeadTime > 0)
            {
                NotifyDelay(product);
            }
        }
    }
}
