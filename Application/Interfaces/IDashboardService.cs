using Application.Response;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardResponseDto> GetMonthlyDashboardAsync(int? year, int? startMonth, int? endMonth);
    }
}
