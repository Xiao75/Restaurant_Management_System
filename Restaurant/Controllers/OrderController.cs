using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Extensions;
using Restaurant.Models;
using Restaurant.Models.ViewModels;

namespace Restaurant.Controllers
{
    public class OrderController : Controller
    {
        private readonly RmsContext _context;

        public OrderController(RmsContext context)
        {
            _context = context;
        }

        // ✅ Add item to cart
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

        //Increase Quantity

        [HttpPost]
        public IActionResult IncreaseQuantity(int itemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(i => i.ItemId == itemId);

            if (item != null)
            {
                item.Quantity++;
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("ViewCart");
        }


        // Decrease Quantity
        [HttpPost]
        public IActionResult DecreaseQuantity(int itemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(i => i.ItemId == itemId);

            if (item != null)
            {
                item.Quantity--;
                if (item.Quantity < 0)
                    cart.Remove(item);
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("ViewCart");
        }

        // Remove Item
        [HttpPost]
        public IActionResult RemoveFromCart(int itemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            cart.RemoveAll(i => i.ItemId == itemId);

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("ViewCart");
        }

        // ✅ View cart
        public async Task<IActionResult> ViewCart()
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

            return View(cartItems);
        }

        // ✅ Place order and redirect to payment
        [HttpPost]
        public IActionResult PlaceOrder(string source = "Online")
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Cart is empty.";
                return RedirectToAction("ViewCart");
            }

            decimal totalAmount = 0;

            foreach (var item in cart)
            {
                var menuItem = _context.MenuItems.FirstOrDefault(m => m.ItemId == item.ItemId);
                if (menuItem != null && menuItem.Price.HasValue)
                {
                    decimal itemTotal = (menuItem.Price ?? 0) * item.Quantity;
                    totalAmount += itemTotal;
                }
            }
            var order = new Order
            {
                CustomerId = customerId.Value,
                OrderDate = DateTime.Now,
                Status = "Pending",
                Source = source,
                TotalAmount = totalAmount,
                InvoiceId = GenerateInvoiceId()
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

            _context.SaveChanges();

            // Clear cart
            HttpContext.Session.Remove("Cart");

            // 🔁 Redirect to Payment page with orderId
            return RedirectToAction("Payment", "Payment", new { orderId = order.OrderId });
        }

        // ✅ Order confirmation page
        public IActionResult OrderConfirmation()
        {
            return View();
        }

        // ✅ View past orders
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

        // ✅ View order details
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

        // ✅ Generates a unique invoice ID
        private string GenerateInvoiceId()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var random = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            return $"INV-{date}-{random}";
        }
    }
}