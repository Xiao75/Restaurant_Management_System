using Microsoft.AspNetCore.Mvc;

namespace Restaurant.Models.ViewModels
{
    public class CartAddViewModel
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }
}
