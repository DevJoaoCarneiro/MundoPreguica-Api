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

        public ProductController(IProductServices productServices)
        {
            _productServices = productServices;
        }

        [HttpPost]
        public async Task<IActionResult> createProduct([FromBody] ProductRequestDto productRequestDto)
        {
            try
            {
                var result = await _productServices.createNewProduct(productRequestDto);

                return result.Status switch
                {
                    "invalid_argument" => BadRequest(500),
                    "not_found" => NotFound(404),
                    "internal_error" => StatusCode(500),
                    "error" => StatusCode(500),
                    _ => Ok(result)
                };
            }
            catch (Exception)
            {

                return StatusCode(500);
            }
        }
    }
}
