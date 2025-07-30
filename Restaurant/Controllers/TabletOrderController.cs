using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Restaurant.Data;
using Restaurant.Extensions;
using Restaurant.Models;
using Restaurant.Models.ViewModels;

namespace Restaurant.Controllers
{
    public class TabletOrderController : Controller
    {
        private readonly RmsContext _context;
       // private readonly int TableNumber = 5; // Set this uniquely per tablet

        public TabletOrderController(RmsContext context)
        {
            _context = context;
        }

        // Show Menu Items for Tablet (like Menu/Index)
        public IActionResult Index(string category)
        {
            var menuItemsQuery = _context.MenuItems
                .Where(m => m.Available);

            if (!string.IsNullOrEmpty(category))
            {
                menuItemsQuery = menuItemsQuery.Where(m => m.Category == category);
            }

            var menuItems = menuItemsQuery
                .OrderBy(m => m.Category)
                .ToList();

            var categories = _context.MenuItems
                .Where(m => m.Available && !string.IsNullOrEmpty(m.Category))
                .Select(m => m.Category)
                .Distinct()
                .ToList();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;
            ViewBag.TableNumber = HttpContext.Session.GetInt32("TableNumber") ?? 0;

            return View(menuItems);
        }
        

        // Add item to cart
        [HttpPost]
        public IActionResult AddToCart(int itemId, int quantity = 1)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("TabletCart") ?? new List<CartItem>();

            var existing = cart.FirstOrDefault(i => i.ItemId == itemId);
            if (existing != null)
                existing.Quantity += quantity;
            else
                cart.Add(new CartItem { ItemId = itemId, Quantity = quantity });

            HttpContext.Session.SetObjectAsJson("TabletCart", cart);

            TempData["Message"] = "Item added to cart!";
            return RedirectToAction("Index", "TabletOrder");
        }

        // View cart
        public async Task<IActionResult> ViewCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("TabletCart") ?? new List<CartItem>();
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

        // Increase quantity
        [HttpPost]
        public IActionResult IncreaseQuantity(int itemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("TabletCart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(i => i.ItemId == itemId);

            if (item != null)
                item.Quantity++;

            HttpContext.Session.SetObjectAsJson("TabletCart", cart);
            return RedirectToAction("ViewCart");
        }

        // Decrease quantity
        [HttpPost]
        public IActionResult DecreaseQuantity(int itemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("TabletCart") ?? new List<CartItem>();
            var item = cart.FirstOrDefault(i => i.ItemId == itemId);

            if (item != null)
            {
                item.Quantity--;
                if (item.Quantity <= 0)
                    cart.Remove(item);
            }

            HttpContext.Session.SetObjectAsJson("TabletCart", cart);
            return RedirectToAction("ViewCart");
        }

        // Remove item
        [HttpPost]
        public IActionResult RemoveFromCart(int itemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("TabletCart") ?? new List<CartItem>();
            cart.RemoveAll(i => i.ItemId == itemId);

            HttpContext.Session.SetObjectAsJson("TabletCart", cart);
            return RedirectToAction("ViewCart");
        }

        // Place order from tablet
        [HttpPost]
        public IActionResult PlaceOrder(int tableNumber)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("TabletCart");
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
                    totalAmount += menuItem.Price.Value * item.Quantity;
                }
            }

            var order = new Order
            {
                TableNumber = tableNumber,
                OrderDate = DateTime.Now,
                Status = "Pending",
                Source = "Offline", // Always Offline for tablet
                TotalAmount = totalAmount,
                InvoiceId = GenerateInvoiceId(),
                //AddressID = null
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

            // Clear tablet cart
            HttpContext.Session.Remove("TabletCart");

            return RedirectToAction("OrderConfirmation");
        }

        //partial tablet cart
        // Helper – builds view-models from the tablet cart
        private async Task<List<CartItemViewModel>> BuildTabletCartViewModelsAsync()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("TabletCart")
                       ?? new List<CartItem>();
            var result = new List<CartItemViewModel>();

            foreach (var ci in cart)
            {
                var menuItem = await _context.MenuItems.FindAsync(ci.ItemId);
                if (menuItem != null)
                {
                    result.Add(new CartItemViewModel
                    {
                        ItemId = menuItem.ItemId,
                        Name = menuItem.Name,
                        Price = menuItem.Price ?? 0,
                        Quantity = ci.Quantity
                    });
                }
            }
            return result;
        }

        // Action that returns the partial (for AJAX or <partial> tag)
        public async Task<IActionResult> CartPartial()
        {
            var vm = await BuildTabletCartViewModelsAsync();
            return PartialView("_CartPartial", vm);
        }

        // Action that returns the tablet-specific partial view you already created
        public async Task<IActionResult> GetTabletCart()
        {
            var vm = await BuildTabletCartViewModelsAsync();
            return PartialView("~/Views/TabletOrder/TabletPartialCart.cshtml", vm);
        }


        // Confirmation page
        public IActionResult OrderConfirmation()
        {
            return View();
        }

        // Generates invoice ID
        private string GenerateInvoiceId()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var random = Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
            return $"INV-{date}-{random}";
        }
    }
}
