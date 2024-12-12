using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Reflection.Metadata.Ecma335;
using TalkToMeMario.Models;

namespace TalkToMeMario.Controllers
{
    public class MarioController : Controller
    {
        string _connectionstring;

        public MarioController(string connectionstring)
        {
            _connectionstring = connectionstring;
        }

        public ActionResult Index()
        {
            List<BestellingOverViewViewModel> bestellingViewModels = new List<BestellingOverViewViewModel>();

            bestellingViewModels.Add(new BestellingOverViewViewModel() { Id = 1, KlantNaam = "Bertha Alkemade", Status = "Bezig", SubTotaal = 25.50 });
            bestellingViewModels.Add(new BestellingOverViewViewModel() { Id = 2, KlantNaam = "Floris Puts", Status = "Klaar", SubTotaal = 85.00 });
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
                {
                    mySqlConnection.Open();
                    using (MySqlCommand mySqlCommand = new MySqlCommand("SELECT id, status, subtotaal FROM bestellingen;", mySqlConnection))
                    {
                        using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                        {
                            while (mySqlDataReader.Read())
                            {
                                bestellingViewModels.Add(new BestellingOverViewViewModel()
                                {
                                    Id = mySqlDataReader.GetInt32(0),
                                    KlantNaam = mySqlDataReader.GetString(1),
                                    Status = mySqlDataReader.GetString(2),
                                    SubTotaal = mySqlDataReader.GetDouble(3)
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
                Console.WriteLine($"Error: {ex.Message}");
            }
            return View(bestellingViewModels);
        }

        public ActionResult details(int id)
        {
            BestellingOverViewViewModel bestelling = null;

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
                {
                    mySqlConnection.Open();
                    using (MySqlCommand mySqlCommand = new MySqlCommand($"SELECT id, klantnaam, status, subtotaal FROM bestellingen WHERE id = {id}", mySqlConnection))
                    {
                        using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                        {
                            if (mySqlDataReader.Read())
                            {
                                bestelling = new BestellingOverViewViewModel()
                                {
                                    Id = mySqlDataReader.GetInt32(0),
                                    KlantNaam = mySqlDataReader.GetString(1),
                                    Status = mySqlDataReader.GetString(2),
                                    SubTotaal = mySqlDataReader.GetDouble(3)
                                };
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
                Console.WriteLine("jammer");
            }

            if (bestelling == null)
            {
                return NotFound();
            }

            return PartialView("_BestellingDetails", bestelling);
        }

        public ActionResult CreateBestelling(int? bestellingId)
        {
            if (bestellingId==null)
            {
                //Todo: bepaal het nieuwe bestellingId
            }
            return View(GetBestellingDataFromDataBase());
        }

        private CreateBestellingViewModel GetBestellingDataFromDataBase()
        {
            //Haal bestelling op uit database
            List<PizzaOverviewViewModel> pizzaViewModels = new List<PizzaOverviewViewModel>();

            pizzaViewModels.Add(new PizzaOverviewViewModel() { Id = 1, Name = "Pizza Margarita", Price = 10.00m });
            pizzaViewModels.Add(new PizzaOverviewViewModel() { Id = 2, Name = "Pizza Tonno", Price = 12.50m });

            BestellingViewModel bestellingViewModel = new BestellingViewModel() { Id = 1, KlantNaam = "Bert", Status = "Bezig", SubTotaal = 12.50, Pizzas = pizzaViewModels };

            CreateBestellingViewModel createBestellingViewModel = new CreateBestellingViewModel() { BestellingViewModel = bestellingViewModel, Pizzas = pizzaViewModels };
            return createBestellingViewModel;
        }

        public ActionResult VoegToe(int bestellingId,int pizzaId)
        {
            //Todo: stap1 voeg pizza toe aan bestelling in database
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
                {
                    mySqlConnection.Open();
                    string query = $"INSERT Pizza{pizzaId} INTO bestelling{bestellingId}";
                    using (MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader();
                    }
                }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Network Error");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Something went wrong adding pizza to order in database");
            }
            return RedirectToAction("CreateBestelling", new { bestellingId });
        }
    }
}
