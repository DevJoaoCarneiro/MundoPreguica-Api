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

        public int Size { get; set; }
        public IFormFile? Image { get; set; }

    }
}
