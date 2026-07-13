using Microsoft.AspNetCore.Mvc;
using Refacto.DotNet.Controllers.Dtos.Product;
using Refacto.DotNet.Controllers.Entities;
using Refacto.DotNet.Controllers.Services;

namespace Refacto.DotNet.Controllers.Controllers;

[ApiController]
[Route("orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderProcessingService _orderProcessingService;

    public OrdersController(IOrderProcessingService orderProcessingService)
    {
        _orderProcessingService = orderProcessingService;
    }

    [HttpPost("{orderId}/processOrder")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public ActionResult<ProcessOrderResponse> ProcessOrder(long orderId)
    {
        Order? order = _orderProcessingService.ProcessOrder(orderId);

        // si l'order n'exist pas return 404
        if (order is null) return NotFound();

        return new ProcessOrderResponse(order.Id);
    }
}
