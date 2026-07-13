using Microsoft.EntityFrameworkCore;
using Refacto.DotNet.Controllers.Database.Context;
using Refacto.DotNet.Controllers.Entities;
using Refacto.DotNet.Controllers.Services.Strategies;

namespace Refacto.DotNet.Controllers.Services.Impl;

public class OrderProcessingService : IOrderProcessingService
{
    private readonly AppDbContext _ctx;
    private readonly IReadOnlyDictionary<ProductType, IProductStockProcessor> _processors;

    public OrderProcessingService(AppDbContext ctx, IEnumerable<IProductStockProcessor> processors)
    {
        _ctx = ctx;
        _processors = processors.ToDictionary(p => p.SupportedType);
    }

    public Order? ProcessOrder(long orderId)
    {
        Order? order = _ctx.Orders.Include(o => o.Items).SingleOrDefault(o => o.Id == orderId);
        
        if(order is not null)
        {
            foreach (Product product in order.Items ?? Enumerable.Empty<Product>())
            {
                if (ProductTypeMapper.TryMap(product.Type, out ProductType type)
                    && _processors.TryGetValue(type, out IProductStockProcessor? processor))
                {
                    processor.Process(product);
                }
            }
        }
        
        return order;
    }
}
