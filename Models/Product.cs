namespace _1298_DUYHUNG.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Khởi tạo giá trị mặc định là chuỗi rỗng
        public decimal Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty; // Khởi tạo giá trị mặc định là chuỗi rỗng
        public string Description { get; set; } = string.Empty; // Khởi tạo giá trị mặc định là chuỗi rỗng
        public int CategoryId { get; set; }
        public Category? Category { get; set; } // Khai báo là nullable vì một sản phẩm có thể chưa thuộc danh mục
    }
}