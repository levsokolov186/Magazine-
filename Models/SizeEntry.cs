namespace ShoesStore.Models
{
    public class SizeEntry
    {
        public int Id { get; set; }
        public decimal Size { get; set; }
        public bool InStock { get; set; } = true;
    }
}
