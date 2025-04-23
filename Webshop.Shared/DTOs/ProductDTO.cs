using HotChocolate;

namespace Webshop.Shared.DTOs
{
    public static class ProductDTO
    {
        [GraphQLName("ProductIndex")]
        public class Index
        {
            public int ProductID { get; set; }
            public string Name { get; set; } = null!;
            public int InStock { get; set; }
        }

        [GraphQLName("ProductDetails")]
        public class Details
        {
            public int ProductID { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public decimal BasePrice { get; set; }
            public int InStock { get; set; }
            public List<OptionDetail> Options { get; set; } = new();
        }

        public class OptionDetail
        {
            public int OptionID { get; set; }
            public string OptionType { get; set; } = string.Empty;
            public string OptionValue { get; set; } = string.Empty;
        }

        public class OptionGroup
        {
            public string OptionType { get; set; } = null!;
            public List<string> Values { get; set; } = new();
        }


        public class UpdateStock
        {
            public int ProductID { get; set; }
            public int InStock { get; set; }
        }
    }

}
