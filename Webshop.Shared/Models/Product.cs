namespace Webshop.Shared.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public int InStock { get; set; }
        public int MinStock { get; set; }
        public decimal BasePrice { get; set; }

        public List<ProductOption> Options { get; set; } = new();
    }

}
