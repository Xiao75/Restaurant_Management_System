using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Filters;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    [AdminOnly]
    public class IncomingOrderController : Controller
    {
        private readonly RmsContext _context;

        public IncomingOrderController(RmsContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include (o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .Where(o => o.Source == "Offline" && o.Status != "Paid")
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Print(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return NotFound();

            return View(order);
        }


        [HttpPost]
        public async Task<IActionResult> MarkasPaid(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);

            if (order != null && order.Status != "Paid" && order.Source == "Offline")
            {
                order.Status = "Paid";

                var payment = new Payment
                {
                    OrderId = order.OrderId,
                    Amount = order.TotalAmount, 
                    PaidAt = DateTime.Now,
                    Status = "Success"
                };

                _context.Payments.Add(payment); 
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }

    }
}
