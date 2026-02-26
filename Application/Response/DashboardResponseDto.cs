using System;
using System.Collections.Generic;

namespace Application.Response
{
    public class DashboardResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public List<DashboardMonthlySummaryDto> Months { get; set; } = new List<DashboardMonthlySummaryDto>();
    }

    public class DashboardMonthlySummaryDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalOrders { get; set; }
        public int SalesCount { get; set; }
        public int ConsignmentCount { get; set; }
    }
}
