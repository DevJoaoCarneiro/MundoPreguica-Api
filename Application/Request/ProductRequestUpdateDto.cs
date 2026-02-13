using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Request
{
    public class ProductRequestUpdateDto
    {
        public string Name { get; set; } = string.Empty;

        public int CategoryId { get; set; }

        public string? Price { get; set; }

        public int Gender { get; set; }
        public IFormFile? Image { get; set; }
    }
}
