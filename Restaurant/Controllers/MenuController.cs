using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using Restaurant.Models.ViewModels;
using Microsoft.AspNetCore.Http;

namespace Restaurant.Controllers
{
    public class MenuController : Controller
    {
        private readonly RmsContext _context;

        public MenuController(RmsContext context)
        {
            _context = context;
        }

        // Show all available menu items
        public async Task<IActionResult> Index(string category)
        {
            var categories = await _context.MenuItems
                .Select(m => m.Category)
                .Distinct()
                .ToListAsync();

            ViewBag.Categories = categories;
            ViewBag.SelectedCategory = category;

            var filteredItems = string.IsNullOrEmpty(category)
                ? await _context.MenuItems.Where(m => m.Available).ToListAsync()
                : await _context.MenuItems
                    .Where(m => m.Category == category && m.Available)
                    .ToListAsync();

            return View(filteredItems);
        }


        // GET: Menu/PlaceOrder
        public async Task<IActionResult> PlaceOrder()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            var items = await _context.MenuItems
                .Where(m => m.Available)
                .ToListAsync();

            var viewModel = new PlaceOrderViewModel
            {
                MenuItems = items.Select(i => new OrderItemViewModel
                {
                    ItemId = i.ItemId,
                    Name = i.Name,
                    Description = i.Description,
                    Price = i.Price ?? 0,
                    Quantity = 0
                }).ToList()
            };

            return View(viewModel);
        }

        // POST: Menu/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder(PlaceOrderViewModel model)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            if (model.MenuItems == null || model.MenuItems.All(i => i.Quantity <= 0))
            {
                ModelState.AddModelError("", "Please select at least one item to order.");
                return View(model);
            }

            var order = new Order
            {
                CustomerId = customerId.Value,
                OrderDate = DateTime.Now,
                Status = "Pending",
                InvoiceId = GenerateInvoiceId() // ✅ Generate invoice
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var item in model.MenuItems)
            {
                if (item.Quantity > 0)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.OrderId,
                        ItemId = item.ItemId,
                        Quantity = item.Quantity
                    };
                    _context.OrderItems.Add(orderItem);
                }
            }

            _context.SaveChanges();

            TempData["InvoiceId"] = order.InvoiceId; // ✅ Pass to view

            return RedirectToAction("OrderConfirmation");
        }

        public IActionResult OrderConfirmation()
        {
            return View();
        }

        // ✅ Helper method for unique invoice ID
        private string GenerateInvoiceId()
        {
            return "INV-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
        }
    }
}
