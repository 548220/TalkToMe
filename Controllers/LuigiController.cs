using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Globalization;
using System.Net;
using System.Security.AccessControl;
using TalkToMeMario.Models;

namespace TalkToMeMario.Controllers
{
    public class LuigiController : Controller
    {
        string _connectionstring;

        public LuigiController(string connectionstring)
        {
            _connectionstring = connectionstring;
        }

        public IActionResult Index()
        {
            List<PizzaOverviewViewModel> pizzas = new List<PizzaOverviewViewModel>();
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
                {
                    mySqlConnection.Open();
                    string pizzaQuery = @"SELECT p.product_id, p.naam, pp.prijs
                                    FROM product p
                                    INNER JOIN product_prijs pp ON p.product_id = pp.product_id;";
                    using (MySqlCommand mySqlCommand = new MySqlCommand(pizzaQuery, mySqlConnection))
                    {
                        using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                        {
                            while (mySqlDataReader.Read())
                            {
                                pizzas.Add(new PizzaOverviewViewModel()
                                {
                                    Id = mySqlDataReader.GetInt32(0),
                                    Name = mySqlDataReader.GetString(1),
                                    Price = mySqlDataReader.GetDecimal(2),
                                });
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            List<BestellingViewModel> bestellingViewModels = new List<BestellingViewModel>();
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
                {
                    mySqlConnection.Open();
                    string bestellingQuery = @"SELECT 
                                               b.bestel_id,
                                               k.naam AS klant_naam,
                                               b.datum AS tijd,
                                               s.statusOmschrijving AS status
                                               FROM 
                                               bestelling b
                                               JOIN 
                                               klant k ON b.klant_id = k.klant_id
                                               JOIN 
                                               bestel_regel br ON b.bestel_id = br.bestel_id
                                               JOIN 
                                               pizza_status ps ON br.bestelregel_id = ps.bestelregel_id
                                               JOIN 
                                               status s ON ps.status_id = s.status_id;";
                    using (MySqlCommand mySqlCommand = new MySqlCommand(bestellingQuery, mySqlConnection))
                    {
                        using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                        {
                            while (mySqlDataReader.Read())
                            {
                                bestellingViewModels.Add(new BestellingViewModel()
                                {
                                    Id= mySqlDataReader.GetInt32(0),
                                    KlantNaam = mySqlDataReader.GetString(1),
                                    Tijd = mySqlDataReader.GetDateTime(2),
                                    Status = mySqlDataReader.GetString(3),
                                    Pizzas = pizzas
                                });
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            DateTime huidigeDateTime = DateTime.Now;
            bestellingViewModels.Add(new BestellingViewModel(){Id = 1, KlantNaam = "Bert", Tijd = huidigeDateTime, Pizzas = pizzas, Status= "klaar", SubTotaal = 12.50 });
            bestellingViewModels.Add(new BestellingViewModel(){Id = 2, KlantNaam = "Jan", Tijd = huidigeDateTime, Pizzas = pizzas, Status = "bezig", SubTotaal = 35.80 });

            return View(bestellingViewModels);
        }

    }
}   
