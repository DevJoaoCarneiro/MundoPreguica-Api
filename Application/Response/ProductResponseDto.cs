namespace Application.Response
{
    public class ProductResponseDto
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public Data? Data { get; set; }

    }

    public class Data
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Category { get; set; }

        public int Gender { get; set; }
        public decimal Price { get; set; }
        public bool IsPromotion { get; set; }
        public decimal? OldPrice { get; set; }
        public string ImageUrL { get; set; } = string.Empty;

        public List<VariantInfoResponse> Variants { get; set; } = new List<VariantInfoResponse>();
    }

    public class VariantInfoResponse
    {
        public Guid Id { get; set; }
        public string Size { get; set; } = string.Empty;
        public int Stock { get; set; }
    }

}
