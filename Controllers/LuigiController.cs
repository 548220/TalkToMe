using Microsoft.AspNetCore.Mvc;
using System.Net;
using TalkToMeMario.Models;

namespace TalkToMeMario.Controllers
{
    public class LuigiController : Controller
    {
        public IActionResult Index()
        {
            List<PizzaOverviewViewModel> pizzas = new List<PizzaOverviewViewModel>();
            pizzas.Add(new PizzaOverviewViewModel() { Id = 1, Name = "Margharita", Price = 12.50m });
            pizzas.Add(new PizzaOverviewViewModel() { Id = 2, Name = "Tonno", Price = 16.00m });
            List<BestellingViewModel> bestellingViewModels = new List<BestellingViewModel>();
            bestellingViewModels.Add ( new BestellingViewModel(){Id = 1, KlantNaam = "Bert", Pizzas = pizzas, Status= "klaar", SubTotaal = 12.50 });
            bestellingViewModels.Add(new BestellingViewModel() { Id = 2, KlantNaam = "Jan", Pizzas = pizzas, Status = "bezig", SubTotaal = 35.80 });
            return View(bestellingViewModels);
        }

    }
}
