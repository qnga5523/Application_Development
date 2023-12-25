namespace GCH211211.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string? UserId { get; set; }

        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Image { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public decimal TotalPrice { get; set; }
        public ICollection<Cart>? cartItems { get; set; }
    }
}
