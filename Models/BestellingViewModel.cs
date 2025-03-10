﻿namespace TalkToMeMario.Models
{
    public class BestellingViewModel
    {
        required
            public int Id { get; set; }

        required
            public string KlantNaam { get; set; }

            public int TelefoonNummer { get; set; }

            public string Status { get; set; }
        
        required
            public DateTime Tijd { get; set; }

        
            public List<PizzaOverviewViewModel> Pizzas { get; set; } = new List<PizzaOverviewViewModel>();

        
            public double SubTotaal { get; set; }
    }
}
