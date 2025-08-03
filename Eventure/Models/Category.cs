namespace Eventure.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DefaultImageUrl { get; set; }
        public ICollection<Event>? Events { get; set; }
    }
}
