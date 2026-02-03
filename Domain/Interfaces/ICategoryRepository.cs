using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllCategoryNames();

        Task<bool> CategoryExistsAsync(string name);
        Task AddCategoryAsync(Category category);

        Task<Category?> GetByIdAsync(int id);

        void Delete(Category category);
    }
}
