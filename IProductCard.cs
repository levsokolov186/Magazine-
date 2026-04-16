namespace ShoesStore
{
    public interface IProductCard
    {
        string Title { get; set; }
        string Description { get; set; }
        bool IsUsed { get; set; }

        float Price { get; set; }
    }
}
