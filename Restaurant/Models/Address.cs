using System;
using System.ComponentModel.DataAnnotations;

namespace Restaurant.Models
{
    public class Address
    {
        public int AddressID { get; set; }

        [Required(ErrorMessage = "Full Address is required.")]
        public string ?FullAddress { get; set; }

        [Required(ErrorMessage = "Label is required.")]
        public string Label { get; set; }

        public int CustomerId { get; set; }

        public bool IsVisible { get; set; } = true;

        public DateTime CreatedAt { get; set; }
    }
}
