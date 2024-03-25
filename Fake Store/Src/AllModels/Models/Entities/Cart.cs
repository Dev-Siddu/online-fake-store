
namespace Models.Entities
{
    public class Cart
    {
        public int UserID { get; set; }
        public List<int>? ProdID { get; set; }   = new List<int>(); 
    }
}
