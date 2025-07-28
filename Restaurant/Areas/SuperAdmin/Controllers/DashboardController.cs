using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Data;
using Restaurant.Filters;
using Restaurant.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Restaurant.Areas.SuperAdmin.Controllers
{
    [Area("SuperAdmin")]
    [SuperAdminOnly]
    public class DashboardController : Controller
    {
        private readonly RmsContext _context;

        public DashboardController(RmsContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string range = "daily")
        {
            DateTime today = DateTime.Today;
            DateTime startDate;
            DateTime endDate = today.AddDays(1); // exclusive end

            // ✅ Set startDate and endDate based on selected range
            switch (range.ToLower())
            {
                case "daily":
                    startDate = today;
                    endDate = today.AddDays(1); // only today
                    break;
                case "weekly":
                    startDate = today.AddDays(-6); // today + last 6 = 7 days total
                    endDate = today.AddDays(1);
                    break;
                case "monthly":
                    startDate = today.AddDays(-29); // 30 days
                    endDate = today.AddDays(1);
                    break;
                case "yearly":
                    startDate = today.AddDays(-364); // 365 days
                    endDate = today.AddDays(1);
                    break;
                case "all":
                    startDate = new DateTime(1753, 1, 1);
                    endDate = DateTime.Today.AddDays(1);
                    break;
                default:
                    startDate = today;
                    endDate = today.AddDays(1);
                    break;
            }

            // ✅ Get OrderItems within the date range
            var orderItems = _context.OrderItems
                .Include(oi => oi.Item)
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.OrderDate >= startDate && oi.Order.OrderDate < endDate);

            // ✅ Calculate TotalRevenue by summing OrderItem subtotals
            var totalRevenue = await orderItems.SumAsync(oi => (decimal?)(oi.Quantity * oi.Item.Price)) ?? 0m;

            // ✅ Group by Item and calculate sales summary
            var itemSales = await orderItems
                .GroupBy(oi => oi.ItemId)
                .Select(g => new
                {
                    ItemName = g.First().Item.Name,
                    QuantitySold = g.Sum(x => x.Quantity),
                    SubTotal = g.Sum(x => x.Quantity * x.Item.Price)
                })
                .ToListAsync();

            var totalItemsSold = itemSales.Sum(i => i.QuantitySold);

            var best = itemSales.OrderByDescending(i => i.QuantitySold).FirstOrDefault();
            var worst = itemSales.OrderBy(i => i.QuantitySold).FirstOrDefault();

            var model = new SuperAdminDashboardViewModel
            {
                TotalRevenue = totalRevenue,
                TotalItemsSold = totalItemsSold,
                BestSellingItem = best?.ItemName ?? "N/A",
                BestSellingQuantity = best?.QuantitySold ?? 0,
                WorstSellingItem = worst?.ItemName ?? "N/A",
                WorstSellingQuantity = worst?.QuantitySold ?? 0,
                ItemSalesSummary = itemSales.Select(i => new SalesSummaryItem
                {
                    ItemName = i.ItemName,
                    QuantitySold = i.QuantitySold,
                    SubTotal = i.SubTotal ?? 0m
                }).ToList()
            };

            return View(model);
        }
    }
}
