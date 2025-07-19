namespace Restaurant.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int ItemId { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public decimal Total => Price * Quantity;
    }
}
