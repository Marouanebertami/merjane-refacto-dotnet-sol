using Refacto.DotNet.Controllers.Entities;

namespace Refacto.DotNet.Controllers.Services
{
    public interface IOrderProcessingService
    {
        Order? ProcessOrder(long orderId);
    }
}
