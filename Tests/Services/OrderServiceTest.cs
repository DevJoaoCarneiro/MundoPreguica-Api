using Application.Request;
using Application.Services;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace Tests.Services
{
    public class OrderServiceTest
    {
        private readonly ILogger<OrderService> _logger = Substitute.For<ILogger<OrderService>>();
        private readonly IClientRepository _clientRepository = Substitute.For<IClientRepository>();
        private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
        private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
        private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
        private readonly OrderService _service;

        public OrderServiceTest()
        {
            _service = new OrderService(_logger, _clientRepository, _productRepository, _orderRepository, _unitOfWork);
        }

        [Fact]
        public async Task CreateNewOrder_WhenRequestHasNoItems_ReturnsInvalidArgument()
        {
            var request = new OrderRequestDto
            {
                OrderType = OrderType.Sale,
                ProductInformation = new List<ProductInformation>()
            };

            var result = await _service.createNewOrderAsync(request);

            Assert.Equal("invalid_argument", result.Status);
            Assert.Equal("O pedido deve conter ao menos um item.", result.Message);

            await _unitOfWork.Received(1).BeginTransactionAsync();
            await _unitOfWork.Received(1).RollbackTransactionAsync();
            await _unitOfWork.Received(0).CommitAsync();
        }

        [Fact]
        public async Task CreateNewOrder_WhenProductOutOfStock_ReturnsOutOfStock()
        {
            var request = new OrderRequestDto
            {
                OrderType = OrderType.Sale,
                ClientInformation = new ClientRequest { Name = "Ana", Phone = "11999999999" },
                ProductInformation = new List<ProductInformation>
                {
                    new ProductInformation { ProductId = Guid.NewGuid(), Amount = 2 }
                }
            };

            _clientRepository.GetByPhoneAsync(Arg.Any<string>()).Returns(new Client { clientId = Guid.NewGuid() });
            _productRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(new Product { Stock = 0, Price = 50 });

            var result = await _service.createNewOrderAsync(request);

            Assert.Equal("out_of_stock", result.Status);
            Assert.Equal("Estoque insuficiente ou produto inexistente.", result.Message);

            await _unitOfWork.Received(1).BeginTransactionAsync();
            await _unitOfWork.Received(1).RollbackTransactionAsync();
            await _unitOfWork.Received(0).CommitAsync();
        }

        [Fact]
        public async Task CreateNewOrder_WhenValidSale_ReturnsSuccess()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Stock = 5,
                Price = 100
            };

            var request = new OrderRequestDto
            {
                OrderType = OrderType.Sale,
                ClientInformation = new ClientRequest { Name = "Ana", Phone = "11999999999" },
                ProductInformation = new List<ProductInformation>
                {
                    new ProductInformation { ProductId = productId, Amount = 2 }
                }
            };

            _clientRepository.GetByPhoneAsync(Arg.Any<string>()).Returns(new Client { clientId = Guid.NewGuid() });
            _productRepository.GetByIdAsync(productId).Returns(product);

            var result = await _service.createNewOrderAsync(request);

            Assert.Equal("success", result.Status);
            Assert.Equal("Pedido finalizado com sucesso!", result.Message);
            Assert.Equal("Pendente", result.Order?.OrderStatus);
            Assert.Equal(200, result.Order?.TotalValue);

            await _productRepository.Received(1).UpdateAsync(Arg.Is<Product>(p => p.Stock == 3));
            await _orderRepository.Received(1).AddAsync(Arg.Any<Order>());
            await _unitOfWork.Received(1).CommitAsync();
            await _unitOfWork.Received(1).CommitTransactionAsync();
        }

        [Fact]
        public async Task CreateNewOrder_WhenValidConsignment_ReturnsDelivered()
        {
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Stock = 3,
                Price = 80
            };

            var request = new OrderRequestDto
            {
                OrderType = OrderType.Consignment,
                ClientInformation = new ClientRequest { Name = "Ana", Phone = "11999999999" },
                ProductInformation = new List<ProductInformation>
                {
                    new ProductInformation { ProductId = productId, Amount = 1 }
                }
            };

            _clientRepository.GetByPhoneAsync(Arg.Any<string>()).Returns(new Client { clientId = Guid.NewGuid() });
            _productRepository.GetByIdAsync(productId).Returns(product);

            var result = await _service.createNewOrderAsync(request);

            Assert.Equal("success", result.Status);
            Assert.Equal("Pendente", result.Order?.OrderStatus);
            Assert.Equal(80, result.Order?.TotalValue);
        }

        [Fact]
        public async Task SettleConsignment_WhenOrderNotDelivered_ReturnsInvalidOperation()
        {
            var orderId = Guid.NewGuid();
            _orderRepository.GetByIdAsync(orderId).Returns(new Order { OrderStatus = OrderStatus.Finish });

            var result = await _service.SettleConsignmentAsync(orderId, new SettleConsignmentRequestDto());

            Assert.Equal("invalid_operation", result.Status);
            Assert.Equal("Pedido inválido para liquidação.", result.Message);

            await _unitOfWork.Received(1).BeginTransactionAsync();
            await _unitOfWork.Received(1).RollbackTransactionAsync();
        }

        [Fact]
        public async Task SettleConsignment_WhenQuantitiesDontMatch_ReturnsInvalidArgument()
        {
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var order = new Order
            {
                Id = orderId,
                OrderStatus = OrderStatus.Delivered,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = productId, Quantity = 5, UnitPrice = 10 }
                }
            };

            _orderRepository.GetByIdAsync(orderId).Returns(order);

            var result = await _service.SettleConsignmentAsync(orderId, new SettleConsignmentRequestDto
            {
                ItemsSettlement = new List<ItemSettlementDto>
                {
                    new ItemSettlementDto { ProductId = productId, SoldAmount = 3, ReturnedAmount = 1 }
                }
            });

            Assert.Equal("invalid_argument", result.Status);
            Assert.Equal("As quantidades não batem.", result.Message);

            await _unitOfWork.Received(1).RollbackTransactionAsync();
        }

        [Fact]
        public async Task SettleConsignment_WhenValid_ReturnsSuccess()
        {
            var orderId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var order = new Order
            {
                Id = orderId,
                OrderStatus = OrderStatus.Delivered,
                Items = new List<OrderItem>
                {
                    new OrderItem { ProductId = productId, Quantity = 5, UnitPrice = 10 }
                }
            };

            _orderRepository.GetByIdAsync(orderId).Returns(order);
            _productRepository.GetByIdAsync(productId).Returns(new Product { Id = productId, Stock = 1 });

            var result = await _service.SettleConsignmentAsync(orderId, new SettleConsignmentRequestDto
            {
                ItemsSettlement = new List<ItemSettlementDto>
                {
                    new ItemSettlementDto { ProductId = productId, SoldAmount = 3, ReturnedAmount = 2 }
                }
            });

            Assert.Equal("success", result.Status);
            Assert.Equal("Liquidação concluída.", result.Message);
            Assert.Equal("Finalizado", result.Order?.OrderStatus);
            Assert.Equal(30, result.Order?.TotalValue);

            await _productRepository.Received(1).UpdateAsync(Arg.Is<Product>(p => p.Stock == 3));
            await _orderRepository.Received(1).UpdateAsync(Arg.Is<Order>(o => o.OrderStatus == OrderStatus.Finish));
            await _unitOfWork.Received(1).CommitAsync();
            await _unitOfWork.Received(1).CommitTransactionAsync();
        }
    }
}
