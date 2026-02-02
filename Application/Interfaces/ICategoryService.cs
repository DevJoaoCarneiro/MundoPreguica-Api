using Application.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface ICategoryService
    {
        Task<CategoryResponseDto> GetAllCategoriesAsync();

        Task<CategoryResponseDto> CreateCategoryAsync(string name);

        Task<CategoryResponseDto> DeleteCategoryAsync(int id);
    }
}
