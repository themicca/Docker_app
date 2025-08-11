using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Model
{
    public class CreateOrderRequest
    {
        public Guid CustomerId { get; set; }
        public Guid ResellerId { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }
}
