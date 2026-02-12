
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
                _logger.LogInformation("Começando a a busca de categorias..");
                var result = await _categoryService.GetAllCategoriesAsync();

                return result.Status switch
                {
                    "not_found" => NotFound(result),
                    "error" => StatusCode(500, result),
                    _ => Ok(result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro na busca de categorias.");
                return StatusCode(500, new
                {
                    Message = "Ocorreu um erro inesperado ao buscar categorias.",
                    Status = "error"
                });
            }

        }

        [HttpPost]
        public async Task<IActionResult> createCategory([FromBody] string name)
        {
            try
            {
                _logger.LogInformation("Começando a criação de categoria: {Name}", name);

                var result = await _categoryService.CreateCategoryAsync(name);

                return result.Status switch
                {
                    "success" => Ok(result),
                    "conflict" => Conflict(result),
                    "invalid_argument" => BadRequest(result),
                    "error" => StatusCode(500, result),
                    _ => StatusCode(500, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no Controller de Categoria");
                return StatusCode(500, new { Message = "Erro interno", Status = "error" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteCategory([FromRoute] int id)
        {
            try
            {
                _logger.LogInformation("Começando a exclusão de categoria: {id}", id);
                var result = await _categoryService.DeleteCategoryAsync(id);
                return result.Status switch
                {
                    "success" => Ok(result),
                    "conflict" => Conflict(result),
                    "not_found" => NotFound(result),
                    "error" => StatusCode(500, result),
                    _ => StatusCode(500, result)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado no Controller de Categoria");
                return StatusCode(500, new { Message = "Erro interno", Status = "error" });
            }
        }
    }
}
