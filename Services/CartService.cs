using _1298_DUYHUNG.Models;

namespace _1298_DUYHUNG.Services
{
    public class CartService
    {
        public List<CartItem> Items { get; private set; } = new List<CartItem>();

        public void AddItem(int productId, string productName, decimal price, int quantity, string? imageUrl = null)
        {
            if (quantity <= 0) throw new ArgumentException("Số lượng phải lớn hơn 0");
            if (price < 0) throw new ArgumentException("Giá không được âm");

            var item = Items.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                item.Quantity += quantity;
                if (imageUrl != null) item.ImageUrl = imageUrl;
            }
            else
            {
                Items.Add(new CartItem
                {
                    ProductId = productId,
                    ProductName = productName,
                    Price = price,
                    Quantity = quantity,
                    ImageUrl = imageUrl ?? string.Empty
                });
            }
        }

        public bool RemoveItem(int productId)
        {
            var item = Items.FirstOrDefault(c => c.ProductId == productId);
            if (item != null)
            {
                Items.Remove(item);
                return true;
            }
            return false;
        }

        public decimal GetTotal() => Items.Sum(i => i.Price * i.Quantity);

        public void Clear() => Items.Clear();
    }
}