namespace Webshop.Shared.DTOs
{
    public class ProductDTO
    {
        public int ProductID { get; set; }
        public string Name { get; set; } = null!;
        public int InStock { get; set; }
    }

}
