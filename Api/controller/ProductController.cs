using Application.Interfaces;
using Application.Request;
using Application.Service;
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

                var result = await _productServices.createNewProduct(productRequestDto);

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
    }
}
