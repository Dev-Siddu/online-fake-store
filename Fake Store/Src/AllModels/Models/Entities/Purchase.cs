
namespace Models.Entities
{
    public class Purchase
    {
        public int UserID { get; set; }
        public List<int>? ProdIDs { get; set; } = new List<int>();   
    }
}   
