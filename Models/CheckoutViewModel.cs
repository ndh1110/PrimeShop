namespace _1298_DUYHUNG.Models
{
    public class CheckoutViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public List<CartItem> SelectedItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount => SelectedItems.Sum(item => item.Total);
    }
}