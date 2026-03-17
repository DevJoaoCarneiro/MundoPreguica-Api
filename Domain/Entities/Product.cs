using Domain.Entities.Enum;

namespace Domain.Entities
{
    public class Product
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        
        public decimal Price { get; set; }

        public bool IsPromotion { get; set; }

        public decimal? OldPrice { get; set; }

        public string ImageUrL { get; set; } = string.Empty;

        public int Stock { get; set; }

        public int Gender { get; set; }
        public ProductSize Size { get; set; }

        public ProductStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public Category Category { get; set; } = null!;

        public int CategoryId { get;  set; }
    }
}
