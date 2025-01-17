namespace TalkToMeMario.Models
{
    public class BestellingDetailsViewModel
    {
        required
        public int BestellingId { get; set; }

        public string KlantNaam { get; set; }

        public int Telefoonnummer { get; set; }

        public DateTime Datum { get; set; }

        required
        public List<PizzaOverviewViewModel> BesteldeProducten { get; set; }

        required
        public DateTime Date { get; set; }

        required
        public double Subtotaal { get; set; }
    }
}
