using Microsoft.AspNetCore.Mvc;
using Restaurant.Data;
using Restaurant.Models;

namespace Restaurant.Controllers
{
    public class FakePaymentController : Controller
    {
        private readonly RmsContext _context;

        public FakePaymentController(RmsContext context)
        {
            _context = context;
        }

        // ✅ Shows the fake payment UI (manual selection of success/fail)
        [HttpGet]
        public IActionResult FakePay(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
            if (order == null || order.Source != "Online")
            {
                return NotFound("Order not found or not an online order.");
            }

            ViewBag.OrderId = orderId;
            return View(); // Views/FakePayment/FakePay.cshtml
        }

        // ✅ Process based on the fake result input (from FakePay.cshtml)
        [HttpPost]
        public IActionResult Process(int orderId, int result)
        {
            var order = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);

            if (order == null || order.Source != "Online")
            {
                return NotFound("Order not found or not an online order.");
            }

            if (result == 1)
            {
                // Record successful payment
                var payment = new Payment
                {
                    OrderId = orderId,
                    Amount = order.TotalAmount,
                    PaidAt = DateTime.Now,
                    Status = "Success",
                    PaymentMethod = "Online"
                };

                _context.Payments.Add(payment);
                order.Status = "Paid";

                TempData["Status"] = "Payment Successful!";
            }
            else
            {
                TempData["Status"] = "Payment Failed!";
                order.Status = "Failed";
            }

            _context.SaveChanges();

            return RedirectToAction("Result", new { orderId });
        }

        // ✅ Show result (success/fail)
        public IActionResult Result(int orderId)
        {
            ViewBag.Status = TempData["Status"];
            ViewBag.OrderId = orderId;
            return View();
        }
    }
}
