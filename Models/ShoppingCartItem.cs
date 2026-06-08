namespace E_Commerce.Models
{
    public class ShoppingCartItem
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }

        public string ShoppingCartId { get; set; } = string.Empty;
    }
}
