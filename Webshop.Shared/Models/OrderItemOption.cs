using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Shared.Models
{
    public class OrderItemOption
    {
        public int OrderItemOptionID { get; set; }
        public int OrderItemID { get; set; }
        public int OptionID { get; set; }
    }
}
