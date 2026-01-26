using Domain.Entities.Enum;

namespace Application.Response
{
    public class ProductStatusResponseDto
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DataStatus? Data { get; set; } = new DataStatus();
    }

    public class DataStatus
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public string ProductStatus { get; set; } = string.Empty;
}

}
