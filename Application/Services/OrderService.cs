using Application.Interfaces;
using Application.Request;
using Application.Response;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        public readonly ILogger<OrderService> _logger;
        private readonly IClientRepository _clientRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;

        public OrderService(ILogger<OrderService> logger, IClientRepository clientRepository, IProductRepository productRepository, IOrderRepository orderRepository)
        {
            _logger = logger;
            _clientRepository = clientRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
        }

        public async Task<OrderResponseDto> createNewOrderAsync(OrderRequestDto orderRequestDto)
        {
            try
            {
                _logger.LogInformation("Iniciando processamento de novo pedido para o cliente: {CustomerName}", orderRequestDto?.ClientInformation.Name);

                if (orderRequestDto == null || orderRequestDto.ProductInformation == null || !orderRequestDto.ProductInformation.Any())
                {
                    _logger.LogWarning("Tentativa de criar pedido com dados nulos ou sem itens.");
                    return new OrderResponseDto
                    {
                        Message = "O pedido deve conter ao menos um item.",
                        Status = "invalid_argument"
                    };
                }

                var client = await _clientRepository.GetByPhoneAsync(orderRequestDto.ClientInformation.Phone);
                if (client == null)
                {
                    _logger.LogInformation("Cliente não encontrado. Criando novo cadastro para: {Phone}", orderRequestDto.ClientInformation.Phone);
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
                    TotalValue = 0
                };

                var orderItems = new List<OrderItem>();
                decimal totalOrderValue = 0;

                foreach (var itemDto in orderRequestDto.ProductInformation)
                {
                    var product = await _productRepository.GetByIdAsync(itemDto.ProductId);

                    if (product == null || product.Stock < itemDto.Amount)
                    {
                        _logger.LogWarning("Falha no estoque ou produto inexistente. ID: {ProductId}", itemDto.ProductId);
                        return new OrderResponseDto
                        {
                            Message = $"Estoque insuficiente ou produto não encontrado para o ID: {itemDto.ProductId}",
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

                _logger.LogInformation("Salvando pedido no banco. Total: {Total}", totalOrderValue);
                var savedOrder = await _orderRepository.AddAsync(order);

                return new OrderResponseDto
                {
                    Message = "Pedido finalizado com sucesso!",
                    Status = "success"
                    //Adicionar mais detalhes se necessário
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro crítico ao processar o pedido.");
                return new OrderResponseDto
                {
                    Message = "Erro interno ao processar pedido: " + ex.Message,
                    Status = "error"
                };
            }
        
        }
    }
}
