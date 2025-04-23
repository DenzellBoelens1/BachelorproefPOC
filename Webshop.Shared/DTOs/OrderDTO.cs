using HotChocolate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Webshop.Shared.DTOs.OrderItemDTO;

namespace Webshop.Shared.DTOs
{
    public class OrderDTO
    {
        [GraphQLName("OrderIndex")]
        public class Index
        {
            public int OrderID { get; set; }
            public DateTime OrderDate { get; set; }
            public decimal TotalPrice { get; set; }
            public List<OrderItemDTO.Index> Items { get; set; } = new();
        }

        [GraphQLName("OrderCreateInput")]
        public class Create
        {
            public int UserID { get; set; }  // Voeg UserID toe
            public DateTime OrderDate { get; set; }
            public List<OrderItemCreate> Items { get; set; } = new();
        }

        [GraphQLName("OrderCreated")]
        public class Created
        {
            public int OrderID { get; set; }
        }

    }
}
