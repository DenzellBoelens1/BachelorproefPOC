namespace Webshop.Shared.DTOs
{
    public static class ProductDTO
    {
        public class Index
        {
            public int ProductID { get; set; }
            public string Name { get; set; } = null!;
            public int InStock { get; set; }
        }

        public class Details
        {
            public int ProductID { get; set; }
            public string Name { get; set; } = null!;
            public string? Description { get; set; }
            public decimal BasePrice { get; set; }
            public int InStock { get; set; }
            public List<OptionGroup> Options { get; set; } = new();
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
