using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Extensions;
using Restaurant.Models;
using Restaurant.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant.Controllers
{
    public class OrderController : Controller
    {
        private readonly RmsContext _context;

        public OrderController(RmsContext context)
        {
            _context = context;
        }

        // ✅ Add to cart
        [HttpPost]
        public IActionResult AddToCart(int itemId, int quantity = 1)
        {
            if (HttpContext.Session.GetInt32("CustomerId") == null)
                return RedirectToAction("Login", "Account");

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var existing = cart.FirstOrDefault(i => i.ItemId == itemId);

            if (existing != null)
                existing.Quantity += quantity;
            else
                cart.Add(new CartItem { ItemId = itemId, Quantity = quantity });

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            TempData["Message"] = "Item added to cart!";
            return RedirectToAction("Index", "Menu");
        }

        // ✅ Increase quantity
        [HttpPost]
        public IActionResult IncreaseQuantity(int itemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(i => i.ItemId == itemId);

            if (item != null)
                item.Quantity++;

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("ViewCart");
        }

        // ✅ Decrease quantity
        [HttpPost]
        public IActionResult DecreaseQuantity(int itemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(i => i.ItemId == itemId);

            if (item != null)
            {
                item.Quantity--;
                if (item.Quantity <= 0)
                    cart.Remove(item);
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("ViewCart");
        }

        // ✅ Remove from cart
        [HttpPost]
        public IActionResult RemoveFromCart(int itemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            cart.RemoveAll(i => i.ItemId == itemId);

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("ViewCart");
        }

        // ✅ View Cart
        public async Task<IActionResult> ViewCart()
        {
            var customerId = HttpContext.Session.GetInt32("CustomerId");
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartItems = new List<CartItemViewModel>();

            foreach (var item in cart)
            {
                var menuItem = await _context.MenuItems.FindAsync(item.ItemId);
                if (menuItem != null)
                {
                    cartItems.Add(new CartItemViewModel
                    {
                        ItemId = menuItem.ItemId,
                        Name = menuItem.Name,
                        Price = menuItem.Price ?? 0,
                        Quantity = item.Quantity
                    });
                }
            }

            if (customerId != null)
            {
                var addresses = await _context.Addresses
                    .Where(a => a.CustomerId == customerId.Value)
                    .ToListAsync();

                ViewBag.Addresses = addresses;
            }

            return View(cartItems);
        }

        // ✅ Select address for checkout
        [HttpPost]
        public IActionResult SelectAddressAndCheckout(int? addressId)
        {
            if (addressId == null || addressId == 0)
            {
                TempData["Error"] = "Please select a delivery address.";
                return RedirectToAction("ViewCart");
            }

            HttpContext.Session.SetInt32("SelectedAddressId", addressId.Value);
            return RedirectToAction("ViewCart");
        }

        // ✅ Place order and create records
        // ✅ Place order from ViewCart with address + payment method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrderFromCart(int AddressId, string PaymentMethod)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            if (AddressId == 0)
            {
                TempData["Error"] = "Please select a delivery address.";
                return RedirectToAction("ViewCart");
            }

            var address = _context.Addresses
                .FirstOrDefault(a => a.AddressID == AddressId && a.CustomerId == customerId.Value);

            if (address == null)
            {
                TempData["Error"] = "Invalid address selected.";
                return RedirectToAction("ViewCart");
            }

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Your cart is empty.";
                return RedirectToAction("ViewCart");
            }

            decimal totalAmount = 0;
            foreach (var item in cart)
            {
                var menuItem = _context.MenuItems.FirstOrDefault(m => m.ItemId == item.ItemId);
                if (menuItem != null && menuItem.Price.HasValue)
                {
                    totalAmount += menuItem.Price.Value * item.Quantity;
                }
            }

            var order = new Order
            {
                CustomerId = customerId.Value,
                OrderDate = DateTime.Now,
                Status = PaymentMethod == "Online" ? "Pending" : "Confirmed",
                Source = "Online",
                TotalAmount = totalAmount,
                InvoiceId = GenerateInvoiceId(),
                PaymentMethod = PaymentMethod,
                AddressID = AddressId
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var item in cart)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.OrderId,
                    ItemId = item.ItemId,
                    Quantity = item.Quantity
                });
            }

            _context.Payments.Add(new Payment
            {
                OrderId = order.OrderId,
                Amount = totalAmount,
                PaymentMethod = PaymentMethod,
                PaidAt = DateTime.Now,
                Status = PaymentMethod == "Online" ? "Processing" : "Paid"
            });

            _context.SaveChanges();
            HttpContext.Session.Remove("Cart");

            if (PaymentMethod == "Online")
            {
                return RedirectToAction("FakePay", "FakePayment", new { orderId = order.OrderId });
            }

            return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
        }


        // ✅ Order confirmation page
        public IActionResult OrderConfirmation(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null) return NotFound();

            if (order.PaymentMethod == "Online" && order.Status != "Paid")
            {
                return RedirectToAction("Process", "FakePayment", new { orderId = orderId, result = 1 });
            }

            return View(order);
        }

        // ✅ My Orders list
        public async Task<IActionResult> MyOrders()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            var orders = await _context.Orders
                .Where(o => o.CustomerId == customerId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // ✅ Order Details
        public async Task<IActionResult> Details(int id)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.CustomerId == customerId);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // ✅ Floating Cart Partial View
        public async Task<IActionResult> CartPartial()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartItems = new List<CartItemViewModel>();

            foreach (var item in cart)
            {
                var menuItem = await _context.MenuItems.FindAsync(item.ItemId);
                if (menuItem != null)
                {
                    cartItems.Add(new CartItemViewModel
                    {
                        ItemId = menuItem.ItemId,
                        Name = menuItem.Name,
                        Price = menuItem.Price ?? 0,
                        Quantity = item.Quantity
                    });
                }
            }

            return PartialView("_CartPartial", cartItems);
        }

        // ✅ Invoice helper
        private string GenerateInvoiceId()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var random = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            return $"INV-{date}-{random}";
        }
    }
}
