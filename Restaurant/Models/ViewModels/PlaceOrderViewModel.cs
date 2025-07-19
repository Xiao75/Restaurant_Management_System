// File: Models/ViewModels/PlaceOrderViewModel.cs
using System.Collections.Generic;

namespace Restaurant.Models.ViewModels
{
    public class PlaceOrderViewModel
    {
        public List<OrderItemViewModel> MenuItems { get; set; } = new();
    }
}
