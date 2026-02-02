using Domain.Entities.Enum;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controller
{
    [ApiController]
    [Route("api/metadata")]
    public class MetadataController : ControllerBase
    {
        [HttpGet("sizes")]
        public IActionResult GetProductSizes()
        {
            var sizes = Enum.GetValues(typeof(ProductSize))
                .Cast<ProductSize>()
                .Select(s => new
                {
                    Id = (int)s,
                    Name = s.ToString()
                });

            return Ok(sizes);
        }

        [HttpGet("status")]
        public IActionResult GetProductStatus()
        {
            var status = Enum.GetValues(typeof(ProductStatus))
                .Cast<ProductStatus>()
                .Select(s => new
                {
                    Id = (int)s,
                    Name = s.ToString()
                });

            return Ok(status);
        }


        [HttpGet("order-status")]
        public IActionResult GetOrderStatus()
        {
            var status = Enum.GetValues(typeof(OrderStatus))
                .Cast<OrderStatus>()
                .Select(s => new
                {
                    Id = (int)s,
                    Name = s.ToString()
                });

            return Ok(status);
        }
    }

}
