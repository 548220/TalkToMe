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

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
                {
                    mySqlConnection.Open();
                    string query = "SELECT b.bestel_id, k.naam AS klant_naam, b.datum AS tijd, s.statusOmschrijving AS status FROM bestelling b JOIN klant k ON b.klant_id = k.klant_id JOIN bestel_regel br ON b.bestel_id = br.bestel_id JOIN pizza_status ps ON br.bestelregel_id = ps.bestelregel_id JOIN status s ON ps.status_id = s.status_id;";
                    using (MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                        {
                            while (mySqlDataReader.Read())
                            {
                                bestellingViewModels.Add(new BestellingOverViewViewModel()
                                {
                                    Id = mySqlDataReader.GetInt32(0),
                                    KlantNaam = mySqlDataReader.GetString(1),
                                    Tijd =  mySqlDataReader.GetDateTime(2),
                                    Status = mySqlDataReader.GetString(3)
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
                                    Tijd = mySqlDataReader.GetDateTime(2),
                                    Status = mySqlDataReader.GetString(3),
                                    SubTotaal = mySqlDataReader.GetDouble(4)
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
                try
                { 
                    using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
                    {
                        mySqlConnection.Open();
                        string query = "INSERT INTO Bestellingen (KlantNaam, Status, SubTotaal) VALUES (@KlantNaam, @Status, @SubTotaal);";
                        using (MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection))
                        {
                            mySqlCommand.Parameters.AddWithValue("@Klantnaam", "Nieuwe klant");
                            mySqlCommand.Parameters.AddWithValue("@Status", "Bezig");
                            mySqlCommand.Parameters.AddWithValue("@SubTotaal", "0.00");

                            mySqlCommand.ExecuteNonQuery();

                            mySqlCommand.CommandText = "SELECT LAST_INSERT_ID();";
                            bestellingId = Convert.ToInt32(mySqlCommand.ExecuteScalar());
                        }
                    
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("Network error");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Something went wrong adding a new order");
                }

                //Todo: bepaal het nieuwe bestellingId
            }
            return View(GetBestellingDataFromDataBase());
        }

        private CreateBestellingViewModel GetBestellingDataFromDataBase()
        {
            //Haal bestelling op uit database
            List<PizzaOverviewViewModel> pizzaViewModels = new List<PizzaOverviewViewModel>();

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
                {
                    mySqlConnection.Open();
                    string query = "SELECT p.product_id, p.naam, pp.prijs FROM product p INNER JOIN product_prijs pp ON p.product_id = pp.product_id;";
                    using (MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        using (MySqlDataReader reader = mySqlCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                pizzaViewModels.Add(new PizzaOverviewViewModel()
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Price = reader.GetDecimal(2)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            BestellingViewModel bestellingViewModel = new BestellingViewModel() { Id = 1, KlantNaam = "Bert", Status = "Bezig", Tijd = "17.00", SubTotaal = 12.50, Pizzas = pizzaViewModels };

            CreateBestellingViewModel createBestellingViewModel = new CreateBestellingViewModel() { BestellingViewModel = bestellingViewModel, Pizzas = pizzaViewModels };
            return createBestellingViewModel;
        }

        public ActionResult VoegToe(int bestellingId,int pizzaId)
        {
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

        public IActionResult CheckKlaarStatus()
        {
            bool heeftKlaarBestelling = false;
            using MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring);
            {
                try
                {
                    mySqlConnection.Open();
                    string query = "SELECT COUNT(1) FROM pizza_status WHERE status_id = 1";
                    MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection);
                    {
                        int count = (int)mySqlCommand.ExecuteScalar();
                        heeftKlaarBestelling = count > 0;
                    }
                }
                catch (MySqlException ex)
                {
                    Console.WriteLine("Something went wrong");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Something went wrong");
                }
            }
            return Json(new { heeftklaar = heeftKlaarBestelling });
        }

    }
}
