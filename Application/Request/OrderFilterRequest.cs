using Domain.Entities.Enum;
using System;

namespace Application.Request
{
    public class OrderFilterRequest
    {
        public string? Phone { get; set; }
        public OrderStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
