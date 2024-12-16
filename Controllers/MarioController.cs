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

            bestellingViewModels.Add(new BestellingOverViewViewModel() { Id = 1, KlantNaam = "Bertha Alkemade", Tijd = "17.00", Status = "Bezig", SubTotaal = 25.50 });
            bestellingViewModels.Add(new BestellingOverViewViewModel() { Id = 2, KlantNaam = "Floris Puts", Tijd = "17.30", Status = "Klaar", SubTotaal = 85.00 });
            bestellingViewModels.Add(new BestellingOverViewViewModel() { Id = 3, KlantNaam = "Vince Schoutrop", Tijd = "17.45", Status = "Bezig", SubTotaal = 32.00 });
            bestellingViewModels.Add(new BestellingOverViewViewModel() { Id = 4, KlantNaam = "Alexander Vroemen", Tijd = "18.20", Status = "Bezig", SubTotaal = 23.00 });
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
                                    Tijd =  mySqlDataReader.GetString(2),
                                    Status = mySqlDataReader.GetString(3),
                                    SubTotaal = mySqlDataReader.GetDouble(4)
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
                                    Tijd = mySqlDataReader.GetString(2),
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

            pizzaViewModels.Add(new PizzaOverviewViewModel() { Id = 1, Name = "Pizza Margarita", Price = 10.00m });
            pizzaViewModels.Add(new PizzaOverviewViewModel() { Id = 2, Name = "Pizza Tonno", Price = 12.50m });
            pizzaViewModels.Add(new PizzaOverviewViewModel() { Id = 3, Name = "Pizza Buffala", Price = 14.00m });
            pizzaViewModels.Add(new PizzaOverviewViewModel() { Id = 4, Name = "Pizza Parmaham", Price = 16.00m });
            pizzaViewModels.Add(new PizzaOverviewViewModel() { Id = 5, Name = "Pizza Ansjovis", Price = 13.00m });

            BestellingViewModel bestellingViewModel = new BestellingViewModel() { Id = 1, KlantNaam = "Bert", Status = "Bezig", SubTotaal = 12.50, Pizzas = pizzaViewModels };

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
    }
}
