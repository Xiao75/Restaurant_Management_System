using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Filters;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<IActionResult> Dashboard(DateTime? fromDate, DateTime? toDate)
        {
            // Default to today if no dates provided
            DateTime from = fromDate ?? DateTime.Today;
            DateTime to = toDate?.AddDays(1) ?? DateTime.Today.AddDays(1); // Include whole 'to' day

            var orders = await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                .Where(o => o.OrderDate >= from && o.OrderDate < to)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

    }
}
