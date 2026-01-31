using Domain.Entities.Enum;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Request
{
    public class OrderRequestDto
    {
        public OrderType OrderType { get; set; }

        public ClientRequest ClientInformation { get; set; } = new ClientRequest();

        public List<ProductInformation> ProductInformation { get; set; } = new List<ProductInformation>();
    }

    public class ClientRequest
    {
        public string Name { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
    }

    public class ProductInformation
    {
        public Guid ProductId { get; set; }

        public int Amount { get; set; }
    }

}
