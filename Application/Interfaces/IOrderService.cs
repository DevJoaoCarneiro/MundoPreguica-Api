using Application.Request;
using Application.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IOrderService {

        Task<OrderResponseDto> createNewOrderAsync(OrderRequestDto orderRequestDto);

        Task<OrderResponseListDto> GetAllOrdersAsync(int currentPage);

        Task<OrderResponseDto> GetOrderByIdAsync(Guid orderId);

        Task<OrderResponseDto> UpdateOrderStatusAsync (Guid orderId);

        Task<OrderResponseDto> SettleConsignmentAsync(Guid orderId, SettleConsignmentRequestDto request);
    }
}
