using HotChocolate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Webshop.Shared.DTOs.OrderItemOptionDTO;

namespace Webshop.Shared.DTOs
{
    public class OrderItemDTO
    {
        [GraphQLName("OrderItemIndex")]
        public class Index
        {
            public int ProductID { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public List<OrderItemOptionDTO.Index> Options { get; set; } = new();
        }

        [GraphQLName("OrderItemCreateInput")]
        public class OrderItemCreate
        {
            public int ProductID { get; set; }  // Voeg ProductID toe
            public int Quantity { get; set; }   // Voeg Quantity toe
            public decimal UnitPrice { get; set; }  // Voeg UnitPrice toe
            public List<OrderItemOptionCreate> Options { get; set; } = new();  // Voeg Options toe
        }

    }
}
