using Application.Interfaces;
using Application.Request;
using Microsoft.AspNetCore.Authorization;
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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> getAllOrder([FromQuery] OrderFilterRequest filter)
        {
            try
            {
                _logger.LogInformation("Recebendo requisição para consulta de pedidos");
                var result = await _orderService.GetAllOrdersAsync(filter);
                return result.Status switch
                {
                    "success" => Ok(result),
                    "not_found" => NotFound(result),
                    "error" => StatusCode(500, result),
                    _ => StatusCode(500, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no endpoint de consulta de pedidos.");
                return StatusCode(500, new
                {
                    Message = "Ocorreu um erro inesperado no servidor.",
                    Status = "error"
                });
            }
        }
        [Authorize]
        [HttpGet]
        [Route("{orderId}")]
        public async Task<IActionResult> getOrderById([FromRoute] Guid orderId)
        {
            try
            {
                _logger.LogInformation("Consultando pedido ID: {OrderId}", orderId);
                var result = await _orderService.GetOrderByIdAsync(orderId);

                return result.Status switch
                {
                    "success" => Ok(result),
                    "not_found" => NotFound(result),
                    "error" => StatusCode(500, result),
                    _ => StatusCode(500, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na consulta do pedido ID: {OrderId}", orderId);
                return StatusCode(500, new { Message = "Erro interno no servidor.", Status = "error" });
            }
        }

        [Authorize]
        [HttpPatch]
        [Route("{orderId}/status")]
        public async Task<IActionResult> updateOrderStatus([FromRoute] Guid orderId)
        {
            try
            {
                _logger.LogInformation("Atualizando status do pedido ID: {OrderId}", orderId);
                var result = await _orderService.UpdateOrderStatusAsync(orderId);
                return result.Status switch
                {
                    "success" => Ok(result),
                    "not_found" => NotFound(result),
                    "invalid_argument" => BadRequest(result),
                    "error" => StatusCode(500, result),
                    _ => StatusCode(500, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status do pedido ID: {OrderId}", orderId);
                return StatusCode(500, new { Message = "Erro interno no servidor.", Status = "error" });
            }
        }

        [Authorize]
        [HttpPut]
        [Route("{orderId}/return")]
        public async Task<IActionResult> updateConsignedOrder([FromRoute] Guid orderId, [FromBody] SettleConsignmentRequestDto request)
        {
            try
            {
                _logger.LogInformation("Iniciando liquidação de consignado para o pedido: {Id}", orderId);

                if (request == null)
                {
                    return BadRequest(new { Message = "Requisição inválida.", Status = "invalid_argument" });
                }

                var result = await _orderService.SettleConsignmentAsync(orderId, request);

                return result.Status switch
                {
                    "success" => Ok(result),
                    "not_found" => NotFound(result),
                    "invalid_argument" => BadRequest(result),
                    _ => StatusCode(500, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no endpoint de liquidação de consignado.");
                return StatusCode(500, new { Message = "Ocorreu um erro inesperado no servidor.", Status = "error" });
            }
        }

        [Authorize]
        [HttpPatch]
        [Route("{orderId}/cancel")]
        public async Task<IActionResult> cancelOrder([FromRoute] Guid orderId)
        {
            try
            {
                _logger.LogInformation("Cancelando pedido ID: {OrderId}", orderId);
                var result = await _orderService.CancelOrderAsync(orderId);

                return result.Status switch
                {
                    "success" => Ok(result),
                    "not_found" => NotFound(result),
                    "invalid_argument" => BadRequest(result),
                    "invalid_operation" => UnprocessableEntity(result),
                    "error" => StatusCode(500, result),
                    _ => StatusCode(500, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar pedido ID: {OrderId}", orderId);
                return StatusCode(500, new { Message = "Erro interno no servidor.", Status = "error" });
            }
        }
    }

}
