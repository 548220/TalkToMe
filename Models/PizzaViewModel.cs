namespace TalkToMeMario.Models
{
    public class PizzaViewModel
    {
        required
        public int Id
        { get; set; }
        required
        public string Name
        { get; set; }
        required
        public string Description
        { get; set; }
        required
        public string ImageUrl
        { get; set; }
        required
        public Decimal Price
        { get; set; }
    }
}
