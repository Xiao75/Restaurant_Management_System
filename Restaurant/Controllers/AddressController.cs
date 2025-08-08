using Microsoft.AspNetCore.Mvc;
using Restaurant.Data;
using Restaurant.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace Restaurant.Controllers
{
    public class AddressController : Controller
    {
        private readonly RmsContext _context;

        public AddressController(RmsContext context)
        {
            _context = context;
        }

        // 🔹 List all visible addresses for the logged-in user
        public async Task<IActionResult> Index()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            var addresses = await _context.Addresses
                .Where(a => a.CustomerId == customerId && a.IsVisible)
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
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            if (ModelState.IsValid)
            {
                address.CustomerId = customerId.Value;
                address.CreatedAt = DateTime.Now;
                address.IsVisible = true;
                _context.Add(address);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(address);
        }

        // 🔹 Edit existing address (GET)
        public async Task<IActionResult> Edit(int id)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressID == id && a.CustomerId == customerId);

            if (address == null)
                return NotFound();

            return View(address);
        }

        // 🔹 Edit existing address (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Address updatedAddress)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressID == updatedAddress.AddressID && a.CustomerId == customerId);

            if (address == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                address.FullAddress = updatedAddress.FullAddress;
                address.Label = updatedAddress.Label;
                _context.Update(address);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(updatedAddress);
        }

        // 🔹 Delete address (GET) - confirmation page
        public async Task<IActionResult> Delete(int id)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressID == id && a.CustomerId == customerId && a.IsVisible);

            if (address == null)
                return NotFound();

            return View(address);  // Confirmation page
        }

        // 🔹 Delete address (POST) - perform soft delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (!customerId.HasValue)
                return RedirectToAction("Login", "Account");

            var address = await _context.Addresses
                .FirstOrDefaultAsync(a => a.AddressID == id && a.CustomerId == customerId && a.IsVisible);

            if (address == null)
                return NotFound();

            // Soft delete
            address.IsVisible = false;
            _context.Update(address);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Address deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
