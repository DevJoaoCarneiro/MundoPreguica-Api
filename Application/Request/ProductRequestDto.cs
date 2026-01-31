using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;


namespace Application.Request
{
    public class ProductRequestDto
    {

        public string Name { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public IFormFile? Image { get; set; }

        public List<ProductVariantRequest> Variant { get; set; } = new();


    }

    public class ProductVariantRequest
    {
        public int Size { get; set; }
        public int Stock { get; set; }
    }
}
