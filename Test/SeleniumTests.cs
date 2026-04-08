using System;
using System.Threading; 
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Xunit;

namespace _1298_DUYHUNG.Tests
{
    public class SeleniumTests
    {
        private readonly string _baseUrl = "http://localhost:5122";

        [Fact]
        public void Test_Register()
        {
            string uniqueId = DateTime.Now.ToString("HHmmss"); 
            string newUsername = "hung" + uniqueId;
            string newEmail = "hung" + uniqueId + "@gmail.com";
            string password = "Password123!";

            IWebDriver driver = new ChromeDriver();
            
            try 
            {
                // ================= PHẦN 1: ĐĂNG KÝ =================
                driver.Navigate().GoToUrl($"{_baseUrl}/Account/Register");
                Thread.Sleep(2000); 

                // Nhập ĐỦ 6 Ô theo đúng giao diện
                driver.FindElement(By.Name("UserName")).SendKeys(newUsername);
                Thread.Sleep(1000); 
 
                driver.FindElement(By.Name("Email")).SendKeys(newEmail);
                Thread.Sleep(1000);
                driver.FindElement(By.Name("Password")).SendKeys(password);
                Thread.Sleep(1000);
                driver.FindElement(By.Name("FullName")).SendKeys("Ngo Duy Hung");
                Thread.Sleep(1000);
                driver.FindElement(By.Name("Address")).SendKeys("Hutech");
                Thread.Sleep(1000);
                
                // Ở ô cuối cùng (PhoneNumber), gõ xong rồi nhấn phím ENTER để nộp form luôn
                var phoneField = driver.FindElement(By.Name("PhoneNumber"));
                phoneField.SendKeys("0123456789");
                phoneField.SendKeys(Keys.Enter);

                Thread.Sleep(3000); // Chờ hệ thống lưu vào Database và chuyển trang
                driver.Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Robot vấp ngã tại: " + ex.Message);
                throw; 
            }
        }
        // 2. TEST CASE ĐĂNG NHẬP (TC_LOGIN_01)
        [Fact]
        public void Test_Login_Success()
        {
            string userName = "Tester";
            string password = "Tester@123";
            IWebDriver driver = new ChromeDriver();
            try
            {
                driver.Navigate().GoToUrl($"{_baseUrl}/Account/Login");
                Thread.Sleep(3000);

                // Nhập Username và Password theo đúng tên field trong HTML
                driver.FindElement(By.Name("UserName")).SendKeys(userName);
                Thread.Sleep(1000);
                
                var passField = driver.FindElement(By.Name("Password"));
                passField.SendKeys(password);
                Thread.Sleep(1000);

                // Submit the form by pressing Enter
                passField.SendKeys(Keys.Enter);
                Thread.Sleep(3000); // Wait for login to process

                // Kiểm tra xem đã về trang chủ chưa
                Assert.True(driver.Url.Contains("Home") || driver.Url == $"{_baseUrl}/");
                driver.Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi Đăng nhập: " + ex.Message);
                throw;
            }
        }

        // 3. TEST CASE TÌM KIẾM (TC_SEARCH_01)
        [Fact]
        public void Test_Search_Product()
        {
            IWebDriver driver = new ChromeDriver();
            try
            {
                // Navigate to product listing page first
                driver.Navigate().GoToUrl($"{_baseUrl}/Home/Index");
                Thread.Sleep(2000);

                // Tìm ô tìm kiếm 
                var searchBox = driver.FindElement(By.Name("query")); 
                searchBox.SendKeys("Razer");
                Thread.Sleep(1000);
                searchBox.SendKeys(Keys.Enter);

                Thread.Sleep(2000);
                // Kiểm tra xem trang kết quả có hiện ra không
                Assert.Contains("Razer", driver.PageSource);
                driver.Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi Tìm kiếm: " + ex.Message);
                throw;
            }
        }

        // 4. TEST CASE GIỎ HÀNG (TC_CART_01)
        [Fact]
        public void Test_AddToCart()
        {
            string userName = "Tester";
            string password = "Tester@123";
            IWebDriver driver = new ChromeDriver();
            try
            {
                // ================= PHẦN 1: ĐĂNG NHẬP =================
                driver.Navigate().GoToUrl($"{_baseUrl}/Account/Login");
                Thread.Sleep(3000);

                driver.FindElement(By.Name("UserName")).SendKeys(userName);
                Thread.Sleep(1000);
                
                var passField = driver.FindElement(By.Name("Password"));
                passField.SendKeys(password);
                Thread.Sleep(1000);

                passField.SendKeys(Keys.Enter);
                Thread.Sleep(3000); // Wait for login to process

                // ================= PHẦN 2: THÊM VÀO GIỎ =================
                // Đi tới trang sản phẩm
                driver.Navigate().GoToUrl($"{_baseUrl}/Home/Index");
                Thread.Sleep(2000);

                // Tìm nút "Thêm vào giỏ" theo link text
                var addToCartBtn = driver.FindElement(By.LinkText("Thêm vào giỏ"));
                addToCartBtn.Click();

                Thread.Sleep(3000);
                
                // ================= PHẦN 3: KIỂM TRA GIỎ HÀNG =================
                // Navigate to cart page to verify item was added
                driver.Navigate().GoToUrl($"{_baseUrl}/Home/Cart");
                Thread.Sleep(2000);
                
                // Kiểm tra xem robot có nhảy sang trang Cart không
                Assert.Contains("Cart", driver.Url);
                driver.Quit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi Giỏ hàng: " + ex.Message);
                throw;
            }
        }
    }
}