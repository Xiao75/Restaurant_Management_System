using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Restaurant.Filters;


namespace Restaurant.Controllers
    
{
    [AdminOnly]
    public class MenuItemsController : Controller
    {
        private readonly RmsContext _Context;

        public MenuItemsController(RmsContext context)
        {
            _Context = context;
        }

        // GET: MenuItems
        public async Task<IActionResult> Index()
        {
            return View(await _Context.MenuItems.ToListAsync());
        }

        // GET: MenuItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _Context.MenuItems
                .FirstOrDefaultAsync(m => m.ItemId == id);
            if (menuItem == null) return NotFound();

            return View(menuItem);
        }

        // GET: MenuItems/Create
        public IActionResult Create()
        {
            ViewBag.Categories = GetCategories();
            return View();
        }

        // POST: MenuItems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MenuItem menuItem, IFormFile ImageFile)
        {
            ViewBag.Categories = GetCategories();

            if (ModelState.IsValid)
            {
                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    menuItem.ImagePath = fileName;
                }

                _Context.Add(menuItem);
                await _Context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(menuItem);
        }

        // GET: MenuItems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _Context.MenuItems.FindAsync(id);
            if (menuItem == null) return NotFound();

            ViewBag.Categories = GetCategories();
            return View(menuItem);
        }

        // POST: MenuItems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MenuItem menuItem, IFormFile ImageFile)
        {
            if (id != menuItem.ItemId)
                return NotFound();

            ViewBag.Categories = GetCategories();

            if (ModelState.IsValid)
            
            {
                try
                {
                    var existingItem = await _Context.MenuItems.FindAsync(id);
                    if (existingItem == null)
                        return NotFound();

                    // Update fields
                    existingItem.Name = menuItem.Name;
                    existingItem.Description = menuItem.Description;
                    existingItem.Price = menuItem.Price;
                    existingItem.Category = menuItem.Category;
                    existingItem.Available = menuItem.Available;

                    // Handle image
                    if (ImageFile != null && ImageFile.Length > 0)
                    {
                        var fileName = Path.GetFileName(ImageFile.FileName);
                        var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                        using (var stream = new FileStream(savePath, FileMode.Create))
                        {
                            await ImageFile.CopyToAsync(stream);
                        }
                        existingItem.ImagePath = fileName;
                    }

                    await _Context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_Context.MenuItems.Any(e => e.ItemId == menuItem.ItemId))
                        return NotFound();
                    else
                        throw;
                }
            }

            return View(menuItem);
        }


        // GET: MenuItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var menuItem = await _Context.MenuItems
                .FirstOrDefaultAsync(m => m.ItemId == id);

            if (menuItem == null) return NotFound();

            return View(menuItem);
        }

        // POST: MenuItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var menuItem = await _Context.MenuItems.FindAsync(id);
            if (menuItem != null)
            {
                // Optionally delete associated image
                if (!string.IsNullOrEmpty(menuItem.ImagePath))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", menuItem.ImagePath);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _Context.MenuItems.Remove(menuItem);
                await _Context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool MenuItemExists(int id)
        {
            return _Context.MenuItems.Any(e => e.ItemId == id);
        }

        private List<string> GetCategories()
        {
            return new List<string> { "Burgers", "Drinks", "Desserts", "Sides", "Combos", "Pizzas", "Korean", "Dinosaur" };
        }
    }
}
