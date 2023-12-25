using System.ComponentModel.DataAnnotations;

namespace GCH211211.Models
{
    public class Product
    {
        // properties Id, Name, Price,Quantity, Description
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public decimal Price { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string Image { get; set; }
        [Required]
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}
