using Application.Interfaces;
using Application.Request;
using Application.Service;
using Domain.Entities.Enum;
using Microsoft.AspNetCore.Mvc;

namespace Api.controller
{
    [ApiController]
    [Route("api/product")]
    public class ProductController : ControllerBase
    {

        private readonly IProductServices _productServices;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IProductServices productServices, ILogger<ProductController> logger)
        {
            _productServices = productServices;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> createProduct([FromForm] ProductRequestDto productRequestDto)
        {
            try
            {
                _logger.LogInformation("Começando o cadastro de produto..", productRequestDto.Name);

                var result = await _productServices.CreateNewProduct(productRequestDto);

                return result.Status switch
                {
                    "invalid_argument" => BadRequest(result),
                    "not_found" => NotFound(result),
                    "error" => StatusCode(500, result),
                    "internal_error" => StatusCode(500, result),
                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro no cadastro de produto..", productRequestDto.Name);
                return StatusCode(500, new
                {
                    Message = "Ocorreu um erro inesperado ao criar o produto: " + ex.Message,
                    Status = "error"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> getFilterProduct([FromQuery] ProductFilterRequest productFilterRequest)
        {

            try
            {
                _logger.LogInformation("Começando a consulta de produto..");
                var result = await _productServices.GetByFiltersAsync(productFilterRequest);

                return result.Status switch
                {
                    "not_found" => NotFound(404),
                    "error" => StatusCode(500),
                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro na consulta de produto..");
                return StatusCode(500, new
                {
                    Message = "Ocorreu um erro inesperado ao criar o produto: " + ex.Message,
                    Status = "error"
                });
            }

        }

        [HttpGet]
        [Route("{productId}")]
        public async Task<IActionResult> getProductById([FromRoute] Guid productId)
        {
            try
            {
                _logger.LogInformation("Começando a consulta de produto por Id..", productId);
                var result = await _productServices.GetByIdAsync(productId);
                return result.Status switch
                {
                    "not_found" => NotFound(404),
                    "error" => StatusCode(500),
                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro na consulta de produto por Id..", productId);
                return StatusCode(500, new
                {
                    Message = "Ocorreu um erro inesperado ao criar o produto: " + ex.Message,
                    Status = "error"
                });
            }

        }

        [HttpPut]
        [Route("{productId}")]
        public async Task<IActionResult> updateProduct([FromRoute] Guid productId, [FromForm] ProductRequestDto productRequestDto)
        {
            try
            {
                _logger.LogInformation("Começando a edição do produto..", productRequestDto.Name);

                var result = await _productServices.updateProductById(productId, productRequestDto);

                return result.Status switch
                {
                    "invalid_argument" => BadRequest(result),
                    "not_found" => NotFound(result),
                    "error" => StatusCode(500, result),
                    "internal_error" => StatusCode(500, result),
                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro ao editar o produto..", ex.Message);
                return StatusCode(500, new
                {
                    Message = "Ocorreu um erro inesperado ao editar o produto: " + ex.Message,
                    Status = "error"
                });
            }
        }

        [HttpPatch]
        [Route("{productId}/status")]
        public async Task<IActionResult> updateProductStatus([FromRoute] Guid productId, [FromBody] int newStatus)
        {
            try
            {
                _logger.LogInformation("Iniciando alteração de status para o produto ID: {ProductId}", productId);

                var statusEnum = (ProductStatus)newStatus;

                var result = await _productServices.UpdateStatusAsync(productId, statusEnum);

                return result.Status switch
                {
                    "not_found" => NotFound(result),
                    "error" => StatusCode(500, result),
                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao alterar status do produto ID: {ProductId}", productId);
                return StatusCode(500, new
                {
                    Message = "Erro ao processar alteração de status: " + ex.Message,
                    Status = "error"
                });
            }
        }
    }
}
