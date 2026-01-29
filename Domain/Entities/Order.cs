using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class Order
    {
        public Guid Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public int TypeOrder { get; set; }
        public decimal TotalValue { get; set; }

        public Guid ClientId { get; set; }
        public Client Client { get; set; } = null!;
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
