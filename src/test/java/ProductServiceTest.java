    import java.util.ArrayList;
    import java.util.List;


    // Class ProductService để test
    class ProductService {

        List<String> products = new ArrayList<>();

        // Chức năng 1: Thêm sản phẩm
        void addProduct(String product){
            products.add(product);
        }

        // Chức năng 2: Lấy danh sách sản phẩm
        List<String> getProducts(){
            return products;
        }

        // Chức năng 3: Tính giá sản phẩm
        double calculatePrice(double price, int quantity){
            return price * quantity;
        }
    }


    public class ProductServiceTest {

        private int passed = 0;
        private int failed = 0;

        public static void main(String[] args) {
            ProductServiceTest test = new ProductServiceTest();
            test.runAllTests();
            System.out.println("\n=== KET QUA ===");
            System.out.println("Passed: " + test.passed + "/" + (test.passed + test.failed));
            System.out.println("Failed: " + test.failed + "/" + (test.passed + test.failed));
        }

        void runAllTests() {
            System.out.println("=== CHAY TEST ===\n");
            
            // BA01
            testBA01();
            // BA02
            testBA02();
            // BA03
            testBA03();
            // BA04
            testBA04();
            // BA05
            testBA05();
            // BA06
            testBA06();
            // BA07
            testBA07();
            // BA08
            testBA08();
            // BA09
            testBA09();
        }

        void assertEquals(int expected, int actual) {
            if (expected == actual) {
                passed++;
                System.out.println("  [PASS]");
            } else {
                failed++;
                System.out.println("  [FAIL] Expected: " + expected + ", Actual: " + actual);
            }
        }

        void assertEquals(String expected, String actual) {
            if (expected.equals(actual)) {
                passed++;
                System.out.println("  [PASS]");
            } else {
                failed++;
                System.out.println("  [FAIL] Expected: " + expected + ", Actual: " + actual);
            }
        }

        void assertEquals(double expected, double actual, double delta) {
            if (Math.abs(expected - actual) < delta) {
                passed++;
                System.out.println("  [PASS]");
            } else {
                failed++;
                System.out.println("  [FAIL] Expected: " + expected + ", Actual: " + actual);
            }
        }

        void assertTrue(boolean condition) {
            if (condition) {
                passed++;
                System.out.println("  [PASS]");
            } else {
                failed++;
                System.out.println("  [FAIL] Expected: true");
            }
        }

        // ==================================
        // TEST CHỨC NĂNG 1: THÊM SẢN PHẨM
        // ==================================

        // BA01 - Thêm sản phẩm hợp lệ
        void testBA01() {
            System.out.print("BA01 - Them san pham hop le: ");
            ProductService service = new ProductService();
            service.addProduct("Laptop");
            assertEquals(1, service.getProducts().size());
        }

        // BA02 - Thêm nhiều sản phẩm
        void testBA02() {
            System.out.print("BA02 - Them nhieu san pham: ");
            ProductService service = new ProductService();
            service.addProduct("Laptop");
            service.addProduct("Phone");
            assertEquals(2, service.getProducts().size());
        }

        // BA03 - Thêm sản phẩm rỗng
        void testBA03() {
            System.out.print("BA03 - Them san pham rong: ");
            ProductService service = new ProductService();
            service.addProduct("");
            assertEquals(1, service.getProducts().size());
        }


        // ==================================
        // TEST CHỨC NĂNG 2: LẤY DANH SÁCH
        // ==================================

        // BA04 - Lấy danh sách khi có sản phẩm
        void testBA04() {
            System.out.print("BA04 - Lay danh sach khi co san pham: ");
            ProductService service = new ProductService();
            service.addProduct("Laptop");
            assertEquals(1, service.getProducts().size());
        }

        // BA05 - Lấy danh sách khi rỗng
        void testBA05() {
            System.out.print("BA05 - Lay danh sach khi rong: ");
            ProductService service = new ProductService();
            assertTrue(service.getProducts().isEmpty());
        }

        // BA06 - Kiểm tra dữ liệu đúng
        void testBA06() {
            System.out.print("BA06 - Kiem tra du lieu dung: ");
            ProductService service = new ProductService();
            service.addProduct("Tablet");
            assertEquals("Tablet", service.getProducts().get(0));
        }



        // ==================================
        // TEST CHỨC NĂNG 3: TÍNH GIÁ SẢN PHẨM
        // ==================================

        // BA07 - Tính giá bình thường
        void testBA07() {
            System.out.print("BA07 - Tinh gia binh thuong: ");
            ProductService service = new ProductService();
            double result = service.calculatePrice(100, 2);
            assertEquals(200.0, result, 0.01);
        }

        // BA08 - Số lượng = 0
        void testBA08() {
            System.out.print("BA08 - So luong = 0: ");
            ProductService service = new ProductService();
            double result = service.calculatePrice(100, 0);
            assertEquals(0.0, result, 0.01);
        }

        // BA09 - Giá = 0
        void testBA09() {
            System.out.print("BA09 - Gia = 0: ");
            ProductService service = new ProductService();
            double result = service.calculatePrice(0, 5);
            assertEquals(0.0, result, 0.01);
        }
    }

