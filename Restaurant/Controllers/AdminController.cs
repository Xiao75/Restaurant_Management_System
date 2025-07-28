using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant.Controllers
{
    [AdminOnly]  // Ensures only Admin and SuperAdmin can access
    public class AdminController : Controller
    {
        private readonly RmsContext _context;

        public AdminController(RmsContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            DateTime today = DateTime.Today;
            DateTime tomorrow = today.AddDays(1);

            //Filter today's orders
            var todaysOrders = await _context.Orders
                .Include(o => o.Customer)
                .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var totalOrders = todaysOrders.Count;
            var totalRevenue = todaysOrders.Sum(o => o.TotalAmount);
            var totalMenuItems = await _context.MenuItems.CountAsync();

            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalMenuItems = totalMenuItems;

            return View(todaysOrders);
        }
    }
}
