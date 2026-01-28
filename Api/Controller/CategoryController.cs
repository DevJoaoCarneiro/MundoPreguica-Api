
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controller
{

    [ApiController]
    [Route("api/category")]
    public class CategoryController : ControllerBase
    {

        private readonly ILogger<CategoryController> _logger;
        private readonly ICategoryService _categoryService;

        public CategoryController(ILogger<CategoryController> logger, ICategoryService categoryService)
        {
            _logger = logger;
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> getAllCategory()
        {
            try
            {
                _logger.LogInformation("Começando a consulta de produto..");
                var result = await _categoryService.GetAllCategoriesAsync();

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
