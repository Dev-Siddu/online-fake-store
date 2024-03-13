namespace Models.Entities
{
    public class Product
    {
        public long ID { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ImagePath { get; set; }
        public double Price { get; set; }
        public Rating? Rating { get; set; }
    }
}
