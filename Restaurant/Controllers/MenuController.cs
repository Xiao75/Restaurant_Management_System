using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Extensions;
using Restaurant.Models;
using Restaurant.Models.ViewModels;
using System.Net;

namespace Restaurant.Controllers
{
    public class MenuController : Controller
    {
        private readonly RmsContext _context;

        public MenuController(RmsContext context)
        {
            _context = context;
        }

        // Show all available menu items
     public async Task<IActionResult> Index(string category)
{
    var categories = await _context.MenuItems
        .Select(m => m.Category)
        .Distinct()
        .ToListAsync();

    ViewBag.Categories = categories;
    ViewBag.SelectedCategory = category;

    var filteredItems = string.IsNullOrEmpty(category)
        ? await _context.MenuItems.Where(m => m.Available).ToListAsync()
        : await _context.MenuItems
            .Where(m => m.Category == category && m.Available)
            .ToListAsync();

    // Get customerId from session
    int? customerId = HttpContext.Session.GetInt32("CustomerId");
    List<Address> addresses = new List<Address>();

    if (customerId != null)
    {
        // Fetch addresses for this customer
        addresses = await _context.Addresses
                                  .Where(a => a.CustomerId == customerId)
                                  .ToListAsync();
    }

    // Pass addresses to the view
    ViewBag.Addresses = addresses;

    return View(filteredItems);
}




        // GET: Menu/PlaceOrder
        public async Task<IActionResult> PlaceOrder()
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            var items = await _context.MenuItems
                .Where(m => m.Available)
                .ToListAsync();

            // 🔧 ADD THIS: Load addresses for the view
            var addresses = await _context.Addresses
                .Where(a => a.CustomerId == customerId.Value)
                .ToListAsync();
            ViewBag.Addresses = addresses;

            var viewModel = new PlaceOrderViewModel
            {
                MenuItems = items.Select(i => new OrderItemViewModel
                {
                    ItemId = i.ItemId,
                    Name = i.Name,
                    Description = i.Description,
                    Price = i.Price ?? 0,
                    Quantity = 0
                }).ToList()
            };

            return View(viewModel); // ✅ Now ViewBag.Addresses is available
        }


        public IActionResult GetCart()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            //  Reload prices from DB
            foreach (var cartItem in cart)
            {
                var dbItem = _context.MenuItems.FirstOrDefault(m => m.ItemId == cartItem.ItemId);
                if (dbItem != null)
                {
                    cartItem.Price = dbItem.Price ?? 0;
                    cartItem.Name = dbItem.Name; // Optional: refresh name
                }
            }

            // Save back updated cart with correct prices
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            return PartialView("~/Views/Shared/PartialCart.cshtml", cart);
        }


        [HttpPost]
        public IActionResult AddToCart([FromBody] CartAddViewModel model)
        {
            var item = _context.MenuItems.FirstOrDefault(m => m.ItemId == model.ItemId);
            if (item == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            var existing = cart.FirstOrDefault(i => i.ItemId == model.ItemId);
            if (existing != null)
                existing.Quantity += model.Quantity;
            else
                cart.Add(new CartItemViewModel
                {
                    ItemId = item.ItemId,
                    Name = item.Name,
                    Price = item.Price ?? 0,
                    Quantity = model.Quantity
                });

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return Ok();
        }

        [HttpPost]
        public IActionResult IncreaseQuantity(int itemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            var dbItem = _context.MenuItems.FirstOrDefault(m => m.ItemId == itemId);
            if (dbItem == null) return NotFound();

            var cartItem = cart.FirstOrDefault(i => i.ItemId == itemId);
            if (cartItem != null)
            {
                cartItem.Quantity++;
                cartItem.Price = dbItem.Price ?? 0;  //  Always refresh price here
                cartItem.Name = dbItem.Name;         //  Optional: refresh name in case it changed
            }
            else
            {
                cart.Add(new CartItemViewModel
                {
                    ItemId = dbItem.ItemId,
                    Name = dbItem.Name,
                    Price = dbItem.Price ?? 0,
                    Quantity = 1
                });
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index", "Menu");
        }



        [HttpPost]
        public IActionResult DecreaseQuantity(int itemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            var dbItem = _context.MenuItems.FirstOrDefault(m => m.ItemId == itemId);
            if (dbItem == null) return NotFound();

            var cartItem = cart.FirstOrDefault(i => i.ItemId == itemId);
            if (cartItem != null)
            {
                cartItem.Quantity--;
                cartItem.Price = dbItem.Price ?? 0;  // Refresh price again
                cartItem.Name = dbItem.Name;

                if (cartItem.Quantity <= 0)
                    cart.Remove(cartItem);
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index", "Menu");
        }


        [HttpPost]
        public IActionResult RemoveItem(int itemId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>("Cart") ?? new List<CartItemViewModel>();

            var dbItem = _context.MenuItems.FirstOrDefault(m => m.ItemId == itemId);
            if (dbItem == null) return NotFound();

            var cartItem = cart.FirstOrDefault(i => i.ItemId == itemId);
            if (cartItem != null)
            {
                // Optional: Refresh name/price just like other actions
                cartItem.Name = dbItem.Name;
                cartItem.Price = dbItem.Price ?? 0;

                cart.Remove(cartItem);  // Actually remove the item
            }

            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return Ok(); // Allows JS to handle the reload
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PlaceOrder( PlaceOrderViewModel model)
        {
            int? customerId = HttpContext.Session.GetInt32("CustomerId");
            if (customerId == null)
                return RedirectToAction("Login", "Account");

            // Validate addressId
            if (model.AddressId == 0)
            {
                ModelState.AddModelError("", "Please select a delivery address.");
                var addresses = _context.Addresses
                    .Where(a => a.CustomerId == customerId.Value).ToList();
                ViewBag.Addresses = addresses;
                return View(model);
            }

            // Validate address existence
            var address = _context.Addresses
                .FirstOrDefault(a => a.AddressID == model.AddressId && a.CustomerId == customerId.Value);
            if (address == null)
            {
                ModelState.AddModelError("", "Invalid delivery address.");
                var addresses = _context.Addresses
                    .Where(a => a.CustomerId == customerId.Value).ToList();
                ViewBag.Addresses = addresses;
                return View(model);
            }

            // Validate selected menu items
            if (model.MenuItems == null || model.MenuItems.All(i => i.Quantity <= 0))
            {
                ModelState.AddModelError("", "Please select at least one item to order.");
                var addresses = _context.Addresses
                    .Where(a => a.CustomerId == customerId.Value).ToList();
                ViewBag.Addresses = addresses;
                return View(model);
            }

            var order = new Order
            {
                CustomerId = customerId.Value,
                OrderDate = DateTime.Now,
                Status = "Pending",
                InvoiceId = GenerateInvoiceId(),
                AddressID = model.AddressId
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            foreach (var item in model.MenuItems)
            {
                if (item.Quantity > 0)
                {
                    _context.OrderItems.Add(new OrderItem
                    {
                        OrderId = order.OrderId,
                        ItemId = item.ItemId,
                        Quantity = item.Quantity
                    });
                }
            }

            _context.SaveChanges();

            TempData["InvoiceId"] = order.InvoiceId;
            return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
        }


        public IActionResult OrderConfirmation()
        {
            return View();
        }

        // Helper method for unique invoice ID
        private string GenerateInvoiceId()
        {
            return "INV-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + "-" + Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
        }
    }
}