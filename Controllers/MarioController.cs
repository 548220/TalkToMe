using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
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
            List<BestellingViewModel> bestellingViewModels = new List<BestellingViewModel>();

            bestellingViewModels.Add(new BestellingViewModel() { Id = 1, KlantNaam = "Bertha Alkemade", Status = "Bezig", SubTotaal = 25.50 });
            bestellingViewModels.Add(new BestellingViewModel() { Id = 2, KlantNaam = "Floris Puts", Status = "Klaar", SubTotaal = 85.00 });
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
                                bestellingViewModels.Add(new BestellingViewModel()
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

        public IActionResult details(int id)
        {
            BestellingViewModel bestelling = null;

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
                                bestelling = new BestellingViewModel()
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

    }
}
