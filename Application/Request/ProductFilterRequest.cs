using Domain.Entities.Enum;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Request
{
    public class ProductFilterRequest
    {
        public string? Name { get; set; }
        public int? CategoryId { get; set; }
        public ProductStatus? Status { get; set; }

        public ProductSize? Size { get; set; }

        public int? gender { get; set; }
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 10;

        
    }
}
