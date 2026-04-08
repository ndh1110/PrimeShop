using System;
using _1298_DUYHUNG.Models;
using _1298_DUYHUNG.Services;
using Xunit;

namespace _1298_DUYHUNG.Tests
{
    public class CartServiceTests
    {
        private readonly CartService _cart = new();

        [Fact]
        public void AddItem_NewProduct_AddsToCart()
        {
            _cart.AddItem(1, "Laptop", 15000000m, quantity: 2);

            Assert.Single(_cart.Items);
            var item = _cart.Items[0];
            Assert.Equal(1, item.ProductId);
            Assert.Equal("Laptop", item.ProductName);
            Assert.Equal(15000000m, item.Price);
            Assert.Equal(2, item.Quantity);
        }

        [Fact]
        public void AddItem_ExistingProduct_IncrementsQuantity()
        {
            _cart.AddItem(1, "Laptop", 15000000m, quantity: 1);
            _cart.AddItem(1, "Laptop", 15000000m, quantity: 2);

            var item = Assert.Single(_cart.Items);
            Assert.Equal(3, item.Quantity);
        }

        [Fact]
        public void GetTotal_ReturnsSumOfAllItems()
        {
            _cart.AddItem(1, "Laptop", 15000000m, quantity: 1);
            _cart.AddItem(2, "Mouse", 500000m, quantity: 3);

            var total = _cart.GetTotal();

            Assert.Equal(15000000m + 500000m * 3, total);
        }

        [Fact]
        public void RemoveItem_RemovesByProductId()
        {
            _cart.AddItem(1, "Laptop", 15000000m, quantity: 1);
            _cart.AddItem(2, "Mouse", 500000m, quantity: 1);

            var removed = _cart.RemoveItem(1);

            Assert.True(removed);
            Assert.Single(_cart.Items);
            Assert.Equal(2, _cart.Items[0].ProductId);
        }

        [Fact]
        public void AddItem_WithInvalidData_Throws()
        {
            Assert.Throws<ArgumentException>(() => _cart.AddItem(1, "Laptop", 15000000m, quantity: 0));
            Assert.Throws<ArgumentException>(() => _cart.AddItem(1, "Laptop", -1m, quantity: 1));
        }

        [Fact]
        public void AddItem_WithZeroPrice_AllowsAndTotalsZero()
        {
            _cart.AddItem(1, "Gift", 0m, quantity: 2);

            var item = Assert.Single(_cart.Items);
            Assert.Equal(0m, item.Price);
            Assert.Equal(0m, _cart.GetTotal());
        }

        [Fact]
        public void AddItem_UpdatesImageUrlWhenProvided()
        {
            _cart.AddItem(1, "Laptop", 15000000m, quantity: 1, imageUrl: "old.jpg");
            _cart.AddItem(1, "Laptop", 15000000m, quantity: 1, imageUrl: "new.jpg");

            var item = Assert.Single(_cart.Items);
            Assert.Equal("new.jpg", item.ImageUrl);
            Assert.Equal(2, item.Quantity);
        }

        [Fact]
        public void GetTotal_WhenEmpty_ReturnsZero()
        {
            var total = _cart.GetTotal();
            Assert.Equal(0m, total);
        }

        [Fact]
        public void RemoveItem_WhenNotFound_ReturnsFalseAndNoChange()
        {
            _cart.AddItem(1, "Laptop", 15000000m, quantity: 1);

            var removed = _cart.RemoveItem(999);

            Assert.False(removed);
            Assert.Single(_cart.Items);
        }

        [Fact]
        public void Clear_RemovesAllItems()
        {
            _cart.AddItem(1, "Laptop", 15000000m, quantity: 1);
            _cart.AddItem(2, "Mouse", 500000m, quantity: 1);

            _cart.Clear();

            Assert.Empty(_cart.Items);
            Assert.Equal(0m, _cart.GetTotal());
        }
    }
}
