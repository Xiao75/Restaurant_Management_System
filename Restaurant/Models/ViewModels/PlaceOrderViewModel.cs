using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models.ViewModels
{
    public class PlaceOrderViewModel
    {
        [Required(ErrorMessage = "Please select a delivery address.")]
        public int AddressId { get; set; } 

        public List<OrderItemViewModel> MenuItems { get; set; } = new();
    }
}
