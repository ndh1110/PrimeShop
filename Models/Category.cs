namespace _1298_DUYHUNG.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Khởi tạo giá trị mặc định là chuỗi rỗng
        public List<Product> Products { get; set; } = new List<Product>(); // Khởi tạo danh sách rỗng
    }
}