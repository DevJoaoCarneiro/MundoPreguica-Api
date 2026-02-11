using System;

namespace Domain.Entities.Report
{
    public class DashboardMonthlySummary
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int TotalOrders { get; set; }
        public int SalesCount { get; set; }
        public int ConsignmentCount { get; set; }
    }
}
