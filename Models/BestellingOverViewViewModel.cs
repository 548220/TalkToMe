namespace TalkToMeMario.Models
{
    public class BestellingOverViewViewModel
    {
        required
        public PizzaOverviewViewModel PizzaOverview { get; set; }

        required
        public BestellingViewModel Bestelling { get; set; }
    }
}
