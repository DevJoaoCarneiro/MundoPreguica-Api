using Domain.Entities.Enum;
using System.Drawing;

namespace Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        
        public decimal Price { get; set; }

        public string ImageUrL { get; set; } = string.Empty;

        public ProductSize Size { get; set; }

        public ProductStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Category Category { get; set; }

        public int CategoryId { get;  set; }
    }
}
