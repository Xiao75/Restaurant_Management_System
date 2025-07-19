using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models.ViewModels;

namespace Restaurant.Models
{
    public class DineInOrderController : Controller
    {
        private readonly RmsContext _context;

        public DineInOrderController(RmsContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Menu()
        {
            var menu = await _context.MenuItems
                .Where(m => m.Available)
                .ToListAsync();

            var model = new PlaceOrderViewModel
            {
                MenuItems = menu.Select(m => new OrderItemViewModel
                {
                    ItemId = m.ItemId,
                    Name = m.Name,
                    Price = m.Price ?? 0,
                    Quantity = 0
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult PlaceOrder(PlaceOrderViewModel model)
        {
            if (model.MenuItems.All(i => i.Quantity == 0))
            {
                ModelState.AddModelError("", "Please select at least one item.");
                return View("Menu", model);
            }

            var order = new Order
            {
                OrderDate = DateTime.Now,
                Status = "PendingConfirmation", // staff should confirm first
                Source = "DineIn",
                InvoiceId = Guid.NewGuid().ToString().Substring(0, 8)
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var item in model.MenuItems)
            {
                if (item.Quantity > 0)
                {
                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        ItemId = item.ItemId,
                        Quantity = item.Quantity
                    });
                }
            }

            _context.SaveChanges();

            return RedirectToAction("OrderPlaced");
        }

        public IActionResult OrderPlaced()
        {
            return View();
        }
    }
}