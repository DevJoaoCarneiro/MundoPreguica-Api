using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Request
{
    public class ProductRequestDto
    {

        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string ImageUrL { get; set; } = string.Empty;

    }
}
