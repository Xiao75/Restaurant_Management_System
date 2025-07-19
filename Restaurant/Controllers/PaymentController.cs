using Microsoft.AspNetCore.Mvc;
using Restaurant.Data;
using Restaurant.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Restaurant.Controllers
{
    public class PaymentController : Controller
    {
        private readonly RmsContext _context;

        public PaymentController(RmsContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Payment(int orderId)
        {
            var order = await _context.Orders
                .Include (o => o.OrderItems)
                    .ThenInclude(oi => oi.Item)
                    .FirstOrDefaultAsync (o => o.OrderId == orderId);

            if (order == null)
                return NotFound();

            ViewBag.OrderId = orderId;

            var totalAmount = order.OrderItems.Sum(oi => (oi.Item.Price ?? 0) * oi.Quantity);
            ViewBag.TotalAmount = totalAmount;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CompletePayment(int orderId, string paymentMethod)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Item)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
                return NotFound();

            var payment = new Payment
            {
                OrderId = orderId,
                Amount = order.TotalAmount,
                PaidAt = DateTime.Now,
                PaymentMethod = paymentMethod
            };

            _context.Payments.Add(payment);
            order.Status = "Paid";

            await _context.SaveChangesAsync();

            return RedirectToAction("OrderConfirmation", "Order");
        }
    }
}