// File: Models/ViewModels/OrderItemViewModel.cs
namespace Restaurant.Models.ViewModels
{
    public class OrderItemViewModel
    {
        public int ItemId { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }
    }
}
