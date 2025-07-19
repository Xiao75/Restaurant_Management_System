using Microsoft.AspNetCore.Mvc;
using Restaurant.Data;
using Microsoft.EntityFrameworkCore;

namespace Restaurant.Controllers
{
    public class StaffController : Controller
    {
        private readonly RmsContext _context;

        public StaffController(RmsContext context)
        {
            _context = context;
        }

        // ✅ View all pending online orders
        public async Task<IActionResult> Orders()
        {
            var pendingOrders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .Where(o => o.Status == "Pending" && o.Source == "Online")
                .OrderBy(o => o.OrderDate)
                .ToListAsync();

            return View(pendingOrders);
        }

        // ✅ View all offline (tablet) orders
        public async Task<IActionResult> OfflineOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .Where(o => o.Source == "Offline")
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // ✅ Confirm an order
        [HttpPost]
        public async Task<IActionResult> ConfirmOrder(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = "Confirmed";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("OfflineOrders");
        }

        // ✅ Mark offline order as paid (from OfflineOrders view)
        [HttpPost]
        public async Task<IActionResult> MarkAsPaid(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order != null && order.Source == "Offline")
            {
                order.Status = "Paid";
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("OfflineOrders");
        }
    }
}
