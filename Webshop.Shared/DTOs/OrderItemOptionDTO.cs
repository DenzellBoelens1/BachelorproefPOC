using HotChocolate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Shared.DTOs
{
    public class OrderItemOptionDTO
    {
        [GraphQLName("OrderItemOptionIndex")]
        public class Index
        {
            public int OptionID { get; set; }
            public string OptionType { get; set; } = string.Empty;
            public string OptionValue { get; set; } = string.Empty;
        }

        [GraphQLName("OrderItemOptionCreateInput")]
        public class OrderItemOptionCreate
        {
            public int OptionID { get; set; }  // Voeg OptionID toe
            public string Key { get; set; }  // Voeg Key toe
            public string Value { get; set; }  // Voeg Value toe
        }

    }
}
