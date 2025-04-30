using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Shared.DTOs
{
    public class PriceCalculationRequestDTO
    {
        public int Quantity { get; set; }
        public List<int> SelectedOptionIds { get; set; } = new();
        public Dictionary<int, string> OptionValues { get; set; } = new();
        public string? CustomText { get; set; }
    }
}
