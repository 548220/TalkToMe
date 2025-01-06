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
                                               b.bestel_id AS Id,
                                               K.naam AS KlantNaam,
                                               bs.status AS Status,
                                               p.bedrag AS Subtotaal,
                                               p.created_at AS tijd
                                               FROM bestelling b
                                               JOIN klant k ON b.klant_id = k.klant_id
                                               JOIN betaling p ON b.bestel_id = p.bestel_id
                                               JOIN betaal_status bs ON p.betaalstatus_id = bs.betaalstatus_id;";
                    using (MySqlCommand mySqlCommand = new MySqlCommand(bestellingQuery, mySqlConnection))
                    {
                        using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                        {
                            while (mySqlDataReader.Read())
                            {
                                bestellingViewModels.Add(new BestellingViewModel()
                                {
                                    Id=mySqlDataReader.GetInt32(0),
                                    KlantNaam = mySqlDataReader.GetString(1),
                                    Status = mySqlDataReader.GetString(2),
                                    SubTotaal = mySqlDataReader.GetDouble(3),
                                    Tijd = mySqlDataReader.GetString(4)
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
            bestellingViewModels.Add(new BestellingViewModel(){Id = 1, KlantNaam = "Bert", Tijd = "17.00", Pizzas = pizzas, Status= "klaar", SubTotaal = 12.50 });
            bestellingViewModels.Add(new BestellingViewModel(){Id = 2, KlantNaam = "Jan", Tijd = "17.30", Pizzas = pizzas, Status = "bezig", SubTotaal = 35.80 });

            return View(bestellingViewModels);
        }

    }
}   
