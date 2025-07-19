using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Filters;

namespace Restaurant.Controllers
{
    [AdminOnly]
    public class AdminController : Controller
    {
        private readonly RmsContext _context;

        public AdminController(RmsContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Dashboard()
        {
            var totalOrders = await _context.Orders.CountAsync();
            var totalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);
            var totalCustomer = await _context.Customers.CountAsync();
            var totalMenuItems = await _context.MenuItems.CountAsync();

            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalCustomer = totalCustomer;
            ViewBag.TotalMenuItems = totalMenuItems;

            var recentOrders = await _context.Orders
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();
            return View(recentOrders);

            
        }
    }
}
