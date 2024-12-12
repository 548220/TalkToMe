namespace TalkToMeMario.Models
{
    public class CreateBestellingViewModel
    {
        required
        public BestellingViewModel BestellingViewModel { get; set; }
        required
        public List<PizzaOverviewViewModel> Pizzas { get; set; }
    }
}
