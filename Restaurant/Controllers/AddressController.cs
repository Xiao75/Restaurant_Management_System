using Microsoft.AspNetCore.Mvc;
using Restaurant.Data;
using Restaurant.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Restaurant.Controllers
{
    public class AddressController : Controller
    {
        private readonly RmsContext _context;

        public AddressController(RmsContext context)
        {
            _context = context;
        }

        // 🔹 List all addresses for the logged-in user
        public async Task<IActionResult> Index()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            var addresses = await _context.Addresses
                .Where(a => a.CustomerId == customerId)
                .ToListAsync();

            return View(addresses);
        }

        // 🔹 Show form to create a new address
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Address address)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                address.CustomerId = customerId.Value;
                address.CreatedAt = DateTime.Now;
                _context.Add(address);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // DEBUG: Print errors
            foreach (var kvp in ModelState)
            {
                foreach (var err in kvp.Value.Errors)
                {
                    Console.WriteLine($"Model error in {kvp.Key}: {err.ErrorMessage}");
                }
            }

            return View(address);
        }


        // 🔹 Edit existing address
        public async Task<IActionResult> Edit(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
                return NotFound();

            return View(address);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Address address)
        {
            if (ModelState.IsValid)
            {
                _context.Update(address);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(address);
        }

        // 🔹 Delete address
        public async Task<IActionResult> Delete(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            if (address == null)
                return NotFound();

            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
