using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogiTrack.Models
{
    public class Order
    {
        [Key]
        public int OrderId { get; set; }

        [Required]
        [MaxLength(200)]
        public string CustomerName { get; set; } = string.Empty;

        [Required]
        public DateTime DatePlaced { get; set; } = DateTime.UtcNow;

        // The "List of items" should be modeled with a join entity for proper many-to-many:
        public List<OrderItem> Items { get; set; } = new();
    }

    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }

        [ForeignKey(nameof(Order))]
        public int OrderId { get; set; }
        public Order? Order { get; set; }

        [ForeignKey(nameof(InventoryItem))]
        public int InventoryItemId { get; set; }
        public InventoryItem? InventoryItem { get; set; }

        // Optional: quantity per order line
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }
}
