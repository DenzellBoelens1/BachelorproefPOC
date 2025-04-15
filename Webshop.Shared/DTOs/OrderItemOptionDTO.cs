using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Shared.DTOs
{
    public class OrderItemOptionDTO
    {
        public int OptionID { get; set; }
        public string OptionType { get; set; } = string.Empty;
        public string OptionValue { get; set; } = string.Empty;
    }
}
