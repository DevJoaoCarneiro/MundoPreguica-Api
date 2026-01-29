using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controller
{
    [ApiController]
    [Route("api/order")]
    public class OrderController : ControllerBase
    {
        public readonly ILogger<OrderController> _logger;
        public readonly IOrderService _orderService;

        public OrderController(ILogger<OrderController> logger, IOrderService orderService)
        {
            _logger = logger;
            _orderService = orderService;
        }

        [HttpPost]
        public async Task<IActionResult> createNewOrder(OrderRequestDto orderRequestDto)
        {
            try
            {
                _logger.LogInformation("Recebendo requisição para novo pedido. Cliente: {CustomerName}",
                    orderRequestDto?.ClientInformation?.Name);

                var result = await _orderService.createNewOrderAsync(orderRequestDto);

                return result.Status switch
                {
                    "success" => Ok(result),
                    "invalid_argument" => BadRequest(result),
                    "out_of_stock" => UnprocessableEntity(result),
                    "not_found" => NotFound(result),
                    "error" => StatusCode(500, result),
                    _ => StatusCode(500, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no endpoint de criação de pedido.");
                return StatusCode(500, new
                {
                    Message = "Ocorreu um erro inesperado no servidor.",
                    Status = "error"
                });
            }

        }
    }
}
