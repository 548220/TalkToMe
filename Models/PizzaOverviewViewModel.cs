using System.ComponentModel.DataAnnotations;

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

        [Range(0.01, 100000.00, ErrorMessage = "De prijs moet tussen 0.01 en 100000.00 liggen.")]
        required
        
        public decimal Price
        { get; set; }
    }
}
