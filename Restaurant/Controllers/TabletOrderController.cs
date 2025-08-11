using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Extensions;
using Restaurant.Models;
using Restaurant.Models.ViewModels;
using Restaurant.Filters;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant.Controllers
{
    [TabletPinOnly]
    public class TabletOrderController : Controller
    {
        private readonly RmsContext _context;
        public TabletOrderController(RmsContext context) => _context = context;

        /* -------------  MENU DISPLAY ------------- */
        public IActionResult Index(string category)
        {
            var menuItemsQuery = _context.MenuItems.Where(m => m.Available);
            if (!string.IsNullOrEmpty(category))
                menuItemsQuery = menuItemsQuery.Where(m => m.Category == category);

            var menuItems = menuItemsQuery.OrderBy(m => m.Category).ToList();
            ViewBag.Categories = _context.MenuItems
                                        .Where(m => m.Available && !string.IsNullOrEmpty(m.Category))
                                        .Select(m => m.Category).Distinct().ToList();
            ViewBag.SelectedCategory = category;
            ViewBag.TableNumber = HttpContext.Session.GetInt32("TableNumber") ?? 0;
            return View(menuItems);
        }

        /* -------------  ADD TO CART  ------------- */
        [HttpPost]
        public IActionResult AddToCart(int itemId, int quantity = 1)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("TabletCart") ?? new();
            var existing = cart.FirstOrDefault(i => i.ItemId == itemId);
            if (existing != null) existing.Quantity += quantity;
            else cart.Add(new CartItem { ItemId = itemId, Quantity = quantity });

            HttpContext.Session.SetObjectAsJson("TabletCart", cart);
            return RedirectToAction("Index");
        }

        /* -------------  CART DISPLAY ------------- */
        public async Task<IActionResult> ViewCart() =>
            View(await BuildTabletCartViewModelsAsync());

        /* -------------  AJAX READY ACTIONS ------------- */
        // +1
        [HttpPost]
        public IActionResult IncreaseQuantity(int itemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("TabletCart") ?? new();
            var item = cart.FirstOrDefault(i => i.ItemId == itemId);
            if (item == null) return NotFound();
            item.Quantity++;
            HttpContext.Session.SetObjectAsJson("TabletCart", cart);
            return NoContent();
        }

        // -1
        [HttpPost]
        public IActionResult DecreaseQuantity(int itemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("TabletCart") ?? new();
            var item = cart.FirstOrDefault(i => i.ItemId == itemId);
            if (item == null) return NotFound();
            item.Quantity--;
            if (item.Quantity <= 0) cart.Remove(item);
            HttpContext.Session.SetObjectAsJson("TabletCart", cart);
            return NoContent();
        }

        // remove completely
        [HttpPost]
        public IActionResult RemoveFromCart(int itemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("TabletCart") ?? new();
            cart.RemoveAll(i => i.ItemId == itemId);
            HttpContext.Session.SetObjectAsJson("TabletCart", cart);
            return NoContent();
        }

        // returns the partial used by the sidebar
        public async Task<IActionResult> GetTabletCart()
        {
            var vm = await BuildTabletCartViewModelsAsync();
            return PartialView("~/Views/TabletOrder/TabletPartialCart.cshtml", vm);
        }

        /* -------------  PLACE ORDER ------------- */
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder(int tableNumber)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("TabletCart");
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Cart is empty.";
                return RedirectToAction("ViewCart");
            }

            decimal totalAmount = 0;
            foreach (var ci in cart)
            {
                var menuItem = _context.MenuItems.FirstOrDefault(m => m.ItemId == ci.ItemId);
                if (menuItem?.Price != null)
                    totalAmount += menuItem.Price.Value * ci.Quantity;
            }

            var order = new Order
            {
                TableNumber = tableNumber,
                OrderDate = DateTime.Now,
                Status = "Pending",
                Source = "Offline",
                TotalAmount = totalAmount,
                InvoiceId = GenerateInvoiceId()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var ci in cart)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderId = order.OrderId,
                    ItemId = ci.ItemId,
                    Quantity = ci.Quantity
                });
            }
            _context.SaveChanges();

            HttpContext.Session.Remove("TabletCart");
            return RedirectToAction("OrderConfirmation");
        }

        public IActionResult OrderConfirmation() => View();

        /* -------------  PRIVATE HELPERS ------------- */
        private async Task<List<CartItemViewModel>> BuildTabletCartViewModelsAsync()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("TabletCart") ?? new();
            var ids = cart.Select(c => c.ItemId).ToList();
            var menuItems = await _context.MenuItems
                                          .Where(m => ids.Contains(m.ItemId))
                                          .ToListAsync();

            return (from ci in cart
                    join mi in menuItems on ci.ItemId equals mi.ItemId
                    select new CartItemViewModel
                    {
                        ItemId = mi.ItemId,
                        Name = mi.Name,
                        Price = mi.Price ?? 0,
                        Quantity = ci.Quantity
                    }).ToList();
        }

        private string GenerateInvoiceId()
        {
            var date = DateTime.Now.ToString("yyyyMMdd");
            var rnd = Guid.NewGuid().ToString()[..4].ToUpper();
            return $"INV-{date}-{rnd}";
        }

        /* -------------  PIN ENTRY ------------- */
        [HttpGet]
        public IActionResult EnterPin() => View();

        [HttpPost]
        public IActionResult EnterPin(string pin)
        {
            if (pin == "1234")
            {
                HttpContext.Session.SetString("TabletPinVerified", "true");
                return RedirectToAction("Index");
            }
            ViewBag.Error = "Invalid PIN";
            return View();
        }
    }
}