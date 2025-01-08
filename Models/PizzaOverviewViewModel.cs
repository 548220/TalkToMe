namespace TalkToMeMario.Models
{
    public class PizzaOverviewViewModel
    {
        required
        public int Id
        { get; set; }
        
        required
        public string Name
        { get; set; }

        public int CategoryId { get; set; }

        required
        public decimal Price
        { get; set; }
    }
}
