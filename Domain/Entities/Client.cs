using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Client
    {
        public Guid clientId { get; set; }

        public string clientName { get; set; } = string.Empty;

        public string clientPhone { get; set; } = string.Empty;

        public ICollection<Order> Orders { get; set; } = new List<Order>();


    }
}
