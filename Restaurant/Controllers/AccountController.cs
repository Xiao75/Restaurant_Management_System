using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Restaurant.Models;
using Restaurant.Data;
using Restaurant.Helpers; // Include the helper namespace
using System.Linq;

namespace Restaurant.Controllers
{
    public class AccountController : Controller
    {
        private readonly RmsContext _context;

        public AccountController(RmsContext context)
        {
            _context = context;
        }

        public IActionResult MyAccount()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
            {
                return RedirectToAction("Login");
            }

            var customer = _context.Customers.FirstOrDefault(c => c.CustomerId == customerId);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(Customer model)
        {
            if (_context.Customers.Any(c => c.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
                return View(model);
            }

            if (ModelState.IsValid)
            {
                model.Password = PasswordHelper.HashPassword(model.Password);
                _context.Customers.Add(model);
                _context.SaveChanges();

                TempData["Success"] = "Registration successful. Please log in.";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                string hashedPassword = PasswordHelper.HashPassword(model.Password);

                var customer = _context.Customers
                    .FirstOrDefault(c => c.Email == model.Email && c.Password == hashedPassword);

                if (customer != null)
                {
                    HttpContext.Session.SetInt32("CustomerId", customer.CustomerId);
                    HttpContext.Session.SetString("CustomerName", customer.Name ?? "Guest");

                    HttpContext.Session.SetInt32("IsAdmin", customer.IsAdmin ? 1 : 0);
                    HttpContext.Session.SetInt32("IsSuperAdmin", customer.IsSuperAdmin ? 1 : 0);
                    HttpContext.Session.SetInt32("IsStaff", customer.IsStaff ? 1 : 0);

                    return RedirectToAction("Index", "Menu");
                }

                ModelState.AddModelError("", "Invalid email or password.");
            }

            return View(model);
        }

        // Temporary hash utility – REMOVE after use
        [HttpGet]
        public IActionResult GenerateAdminHash()
        {
            string plainPassword = "admin123"; // your desired admin password
            string hashed = Helpers.PasswordHelper.HashPassword(plainPassword);
            return Content("Hashed password: " + hashed);
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
