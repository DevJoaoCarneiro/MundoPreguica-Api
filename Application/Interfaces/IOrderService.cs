using Application.Request;
using Application.Response;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces
{
    public interface IOrderService {

        Task<OrderResponseDto> createNewOrderAsync(OrderRequestDto orderRequestDto);
    }
}
