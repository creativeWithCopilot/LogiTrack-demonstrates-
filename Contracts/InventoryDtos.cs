namespace LogiTrack.Contracts
{
    public class InventoryCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    public class InventoryReadDto
    {
        public int ItemId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Location { get; set; } = string.Empty;
    }
}
