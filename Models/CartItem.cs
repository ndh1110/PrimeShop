namespace _1298_DUYHUNG.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; } // Thêm thuộc tính ImageUrl
        public decimal Total => Price * Quantity;
    }
}