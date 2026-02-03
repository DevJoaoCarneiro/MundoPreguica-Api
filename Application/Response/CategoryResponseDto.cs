using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Response
{
    public class CategoryResponseDto
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public IEnumerable<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
