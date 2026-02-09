using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly ILogger<OrderService> _logger;
        private readonly IClientRepository _clientRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(
            ILogger<OrderService> logger,
            IClientRepository clientRepository,
            IProductRepository productRepository,
            IOrderRepository orderRepository,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _clientRepository = clientRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<OrderResponseDto> createNewOrderAsync(OrderRequestDto orderRequestDto)
        {
            _logger.LogInformation("Iniciando transação para novo pedido.");
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (orderRequestDto == null || orderRequestDto.ProductInformation == null || !orderRequestDto.ProductInformation.Any())
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new OrderResponseDto {
                        Message = "O pedido deve conter ao menos um item.",
                        Status = "invalid_argument" 
                    };
                }

                var client = await _clientRepository.GetByPhoneAsync(orderRequestDto.ClientInformation.Phone);
                if (client == null)
                {
                    client = new Client
                    {
                        clientId = Guid.NewGuid(),
                        clientName = orderRequestDto.ClientInformation.Name,
                        clientPhone = orderRequestDto.ClientInformation.Phone
                    };
                    await _clientRepository.AddAsync(client);
                }

                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    ClientId = client.clientId,
                    TypeOrder = orderRequestDto.OrderType,
                    OrderDate = DateTime.UtcNow,
                    OrderStatus = OrderStatus.Pending,
                    TotalValue = 0
                };

                var orderItems = new List<OrderItem>();
                decimal totalOrderValue = 0;

                foreach (var itemDto in orderRequestDto.ProductInformation)
                {
                    var product = await _productRepository.GetByIdAsync(itemDto.ProductId);

                    if (product == null || product.Stock < itemDto.Amount)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return new OrderResponseDto {
                            Message = "Estoque insuficiente ou produto inexistente.",
                            Status = "out_of_stock" 
                        };
                    }

                    var orderItem = new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        ProductId = product.Id,
                        Quantity = itemDto.Amount,
                        UnitPrice = product.Price
                    };

                    totalOrderValue += (orderItem.UnitPrice * orderItem.Quantity);
                    product.Stock -= itemDto.Amount;
                    await _productRepository.UpdateAsync(product);
                    orderItems.Add(orderItem);
                }

                order.TotalValue = totalOrderValue;
                order.Items = orderItems;

                await _orderRepository.AddAsync(order);
                await _unitOfWork.CommitAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new OrderResponseDto
                {
                    Message = "Pedido finalizado com sucesso!",
                    Status = "success",
                    Order = MapToProductOrderDto(order)
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Erro crítico ao processar o pedido.");
                return new OrderResponseDto {
                    Message = "Erro interno: " + ex.Message,
                    Status = "error" };
            }
        }

        public async Task<OrderResponseListDto> GetAllOrdersAsync(int page)
        {
            const int FixedPageSize = 10;
            try
            {
                int currentPage = page > 0 ? page : 1;
                var (orders, totalItems) = await _orderRepository.GetAllPagedAsync(currentPage, FixedPageSize);

                return new OrderResponseListDto
                {
                    Message = "Lista recuperada com sucesso.",
                    Status = "success",
                    TotalItems = totalItems,
                    CurrentPage = currentPage,
                    PageSize = FixedPageSize,
                    TotalPages = (int)Math.Ceiling((double)totalItems / FixedPageSize),
                    Orders = orders.Select(MapToProductOrderDto).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar pedidos.");
                return new OrderResponseListDto {
                    Message = "Erro interno.",
                    Status = "error" 
                };
            }
        }



        public async Task<OrderResponseDto> GetOrderByIdAsync(Guid orderId)
        {
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null) return new OrderResponseDto { Message = "Pedido não encontrado.", Status = "not_found" };

                return new OrderResponseDto {
                    Message = "Sucesso.",
                    Status = "success", Order = MapToProductOrderDto(order) 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar pedido.");
                return new OrderResponseDto {
                    Message = "Erro interno.",
                    Status = "error"
                };
            }
        }

        public async Task<OrderResponseDto> SettleConsignmentAsync(Guid orderId, SettleConsignmentRequestDto request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var order = await _orderRepository.GetByIdAsync(orderId);
                if (order == null || order.OrderStatus != OrderStatus.Delivered )
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return new OrderResponseDto {
                        Message = "Pedido inválido para liquidação.",
                        Status = "invalid_operation" 
                    };
                }

                decimal finalTotalValue = 0;

                foreach (var settlement in request.ItemsSettlement)
                {
                    var orderItem = order.Items.FirstOrDefault(i => i.ProductId == settlement.ProductId);
                    if (orderItem == null) continue;

                    if (settlement.SoldAmount + settlement.ReturnedAmount != orderItem.Quantity)
                    {
                        await _unitOfWork.RollbackTransactionAsync();
                        return new OrderResponseDto {
                            Status = "invalid_argument",
                            Message = "As quantidades não batem." 
                        };
                    }

                    if (settlement.ReturnedAmount > 0)
                    {
                        var product = await _productRepository.GetByIdAsync(settlement.ProductId);
                        if (product != null)
                        {
                            product.Stock += settlement.ReturnedAmount;
                            await _productRepository.UpdateAsync(product);
                        }
                    }

                    orderItem.Quantity = settlement.SoldAmount;
                    finalTotalValue += (orderItem.UnitPrice * settlement.SoldAmount);
                }

                order.TotalValue = finalTotalValue;
                order.OrderStatus = OrderStatus.Finish;

                await _orderRepository.UpdateAsync(order);
                await _unitOfWork.CommitAsync();
                await _unitOfWork.CommitTransactionAsync();

                return new OrderResponseDto
                {
                    Message = "Liquidação concluída.",
                    Status = "success",
                    Order = MapToProductOrderDto(order)
                };
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Erro ao liquidar pedido.");
                return new OrderResponseDto {
                    Message = "Erro interno.",
                    Status = "error" 
                };
            }
        }

        public async Task<OrderResponseDto> UpdateOrderStatusAsync(Guid orderId)
        {
            _unitOfWork.BeginTransactionAsync();
            try
            {
                if (orderId == Guid.Empty)
                {
                    _logger.LogInformation("ID do pedido é obrigatório para atualização de status.");
                    return new OrderResponseDto
                    {
                        Message = "ID do pedido é obrigatório.",
                        Status = "invalid_argument"
                    };
                }

                var order = await _orderRepository.GetByIdAsync(orderId);

                if(order == null)
                {
                    _logger.LogInformation("Pedido com ID {OrderId} não encontrado.", orderId);
                    return new OrderResponseDto
                    {
                        Message = "Pedido não encontrado.",
                        Status = "not_found"
                    };
                }

                order.OrderStatus = order.OrderStatus switch
                {
                    OrderStatus.Pending => OrderStatus.Delivered,
                    OrderStatus.Delivered => OrderStatus.Pending,
                    _ => order.OrderStatus
                };

                await _orderRepository.UpdateAsync(order);
                await _unitOfWork.CommitAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Status do pedido {OrderId} atualizado para Pendente.", orderId);
                return new OrderResponseDto
                {
                    Message = "Status do pedido atualizado para Pendente.",
                    Status = "success",
                    Order = MapToProductOrderDto(order)
                };

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status do pedido.");
                _unitOfWork.RollbackTransactionAsync();
                return new OrderResponseDto
                {
                    Message = "Erro interno: " + ex.Message,
                    Status = "error"
                };
            }
            



        }

        private ProductOrderDto MapToProductOrderDto(Order order)
        {
            return new ProductOrderDto
            {
                OrderId = order.Id,
                CustomerName = order.Client?.clientName ?? "Cliente não identificado",
                CustomerPhone = order.Client?.clientPhone ?? "N/A",
                TotalValue = order.TotalValue,
                Date = order.OrderDate,
                OrderStatus = order.OrderStatus.ToString(),
                Items = order.Items?.Select(i => new OrderItemSummaryDto
                {
                    ProductName = i.Product?.Name ?? "Produto Removido",
                    Size = i.Product?.Size.ToString() ?? "-",
                    Quantity = i.Quantity
                }).ToList() ?? new List<OrderItemSummaryDto>()
            };
        }
    }
}