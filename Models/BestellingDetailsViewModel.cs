namespace TalkToMeMario.Models
{
    public class BestellingDetailsViewModel
    {
        required
        public int BestellingId { get; set; }

        required
        public List<PizzaOverviewViewModel> Producten { get; set; }

        required
        public DateTime Date { get; set; }

        required
        public decimal Subtotaal { get; set; }
    }
}
