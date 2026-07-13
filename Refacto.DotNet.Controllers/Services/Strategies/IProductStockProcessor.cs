using Refacto.DotNet.Controllers.Entities;

namespace Refacto.DotNet.Controllers.Services.Strategies
{
    public interface IProductStockProcessor
    {
        ProductType SupportedType { get; }
        void Process(Product product);
    }
}
