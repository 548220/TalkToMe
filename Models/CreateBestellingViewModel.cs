namespace TalkToMeMario.Models
{
    public class CreateBestellingViewModel
    {
        required
        public BestellingViewModel BestellingViewModel { get; set; }

        required
        public List<PizzaOverviewViewModel> BesteldePizzas { get; set; }

        required
        public List<PizzaOverviewViewModel> BeschikbarePizzas { get; set; }
        
    }
}
