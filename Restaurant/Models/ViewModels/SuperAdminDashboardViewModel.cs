using System.Collections.Generic;

namespace Restaurant.Models.ViewModels
{
    public class SalesSummaryItem
    {
        public string ItemName { get; set; }
        public int QuantitySold { get; set; }
        public decimal? SubTotal { get; set; }
    }

    public class SuperAdminDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalItemsSold { get; set; }

        public string BestSellingItem { get; set; }
        public int BestSellingQuantity { get; set; }

        public string WorstSellingItem { get; set; }
        public int WorstSellingQuantity { get; set; }

        public List<SalesSummaryItem> ItemSalesSummary { get; set; }
    }
}
