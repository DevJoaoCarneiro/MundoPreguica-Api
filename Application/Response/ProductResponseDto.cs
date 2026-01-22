using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Response
{
    public class ProductResponseDto
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public Data? Data { get; set; } = new Data();

    }

    public class Data
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Category { get; set; }
        public decimal Price { get; set; }
        public string ImageUrL { get; set; } = string.Empty;
    }
}
