namespace LogiTrack.Contracts
{
    public class OrderCreateDto
    {
        public string CustomerName { get; set; } = string.Empty;
        public DateTime? DatePlaced { get; set; } // optional override
        public List<OrderLineDto> Items { get; set; } = new();
    }

    public class OrderLineDto
    {
        public int InventoryItemId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class OrderReadDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime DatePlaced { get; set; }
        public List<OrderItemReadDto> Items { get; set; } = new();
    }

    public class OrderItemReadDto
    {
        public int InventoryItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
