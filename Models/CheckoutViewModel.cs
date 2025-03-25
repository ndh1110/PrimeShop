namespace _1298_DUYHUNG.Models
{
    public class CheckoutViewModel
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public List<CartItem> SelectedItems { get; set; } = new List<CartItem>();
        public decimal TotalAmount => SelectedItems.Sum(item => item.Total);
    }
}