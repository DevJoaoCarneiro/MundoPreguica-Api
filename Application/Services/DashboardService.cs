using Application.Interfaces;
using Application.Response;
using Domain.Entities.Report;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ILogger<DashboardService> _logger;
        private readonly IOrderRepository _orderRepository;

        public DashboardService(ILogger<DashboardService> logger, IOrderRepository orderRepository)
        {
            _logger = logger;
            _orderRepository = orderRepository;
        }

        public async Task<DashboardResponseDto> GetMonthlyDashboardAsync(int? year, int? startMonth, int? endMonth)
        {
            try
            {
                var resolvedYear = year ?? DateTime.UtcNow.Year;
                var resolvedStartMonth = startMonth ?? endMonth ?? DateTime.UtcNow.Month;
                var resolvedEndMonth = endMonth ?? startMonth ?? DateTime.UtcNow.Month;

                if (resolvedStartMonth < 1 || resolvedStartMonth > 12 || resolvedEndMonth < 1 || resolvedEndMonth > 12 || resolvedStartMonth > resolvedEndMonth)
                {
                    return new DashboardResponseDto
                    {
                        Message = "Intervalo de meses invalido.",
                        Status = "invalid_argument"
                    };
                }

                var summaries = await _orderRepository.GetMonthlySummaryAsync(resolvedYear, resolvedStartMonth, resolvedEndMonth);
                var summaryByMonth = summaries.ToDictionary(s => s.Month, s => s);

                var months = new List<DashboardMonthlySummaryDto>();

                for (int month = resolvedStartMonth; month <= resolvedEndMonth; month++)
                {
                    if (summaryByMonth.TryGetValue(month, out var summary) && summary is not null)
                    {
                        months.Add(new DashboardMonthlySummaryDto
                        {
                            Year = summary.Year,
                            Month = summary.Month,
                            TotalRevenue = summary.TotalRevenue,
                            AverageOrderValue = summary.AverageOrderValue,
                            TotalOrders = summary.TotalOrders,
                            SalesCount = summary.SalesCount,
                            ConsignmentCount = summary.ConsignmentCount
                        });
                    }
                    else
                    {
                        months.Add(new DashboardMonthlySummaryDto
                        {
                            Year = resolvedYear,
                            Month = month,
                            TotalRevenue = 0,
                            AverageOrderValue = 0,
                            TotalOrders = 0,
                            SalesCount = 0,
                            ConsignmentCount = 0
                        });
                    }
                }

                return new DashboardResponseDto
                {
                    Message = "Dashboard carregado com sucesso.",
                    Status = "success",
                    TotalRevenue = months.Sum(m => m.TotalRevenue),
                    Months = months
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar dados do dashboard.");
                return new DashboardResponseDto
                {
                    Message = "Erro interno.",
                    Status = "error"
                };
            }
        }
    }
}
