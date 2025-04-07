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
        
    }

}
