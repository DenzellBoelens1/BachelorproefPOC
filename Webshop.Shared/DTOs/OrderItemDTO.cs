using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Shared.DTOs
{
    public class OrderItemDTO
    {
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public List<OrderItemOptionDTO> Options { get; set; } = new();
    }
}
