using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Response
{

    public class OrderResponseListDto
    {
        public string Message { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public List<ProductOrderDto>? Orders { get; set; } = new List<ProductOrderDto>();

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }


    }

    public class ProductOrderDto
    {
        public Guid OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalValue { get; set; }

        public string CustomerPhone { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public DateTime Date { get; set; }

        public List<OrderItemSummaryDto> Items { get; set; } = new();
    }
    public class OrderItemSummaryDto
    {
        public string ProductName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
