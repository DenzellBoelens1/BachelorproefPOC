using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Webshop.Shared.Models
{
    public class ProductOption
    {
        [Key]
        public int OptionID { get; set; }
        public int ProductID { get; set; }
        public string OptionType { get; set; } = string.Empty;
        public string OptionValue { get; set; } = string.Empty;

        [ForeignKey(nameof(ProductID))]
        public Product Product { get; set; } = null!;
    }
}
