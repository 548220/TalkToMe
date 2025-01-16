using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
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
                    string query = @"SELECT 
                                     b.bestel_id, 
                                     k.naam AS klant_naam, 
                                     b.datum AS tijd, 
                                     MAX(s.statusOmschrijving) AS status
                                     FROM bestelling b
                                     JOIN klant k ON b.klant_id = k.klant_id
                                     JOIN bestel_regel br ON b.bestel_id = br.bestel_id
                                     JOIN pizza_status ps ON br.bestelregel_id = ps.bestelregel_id
                                     JOIN status s ON ps.status_id = s.status_id
                                     GROUP BY b.bestel_id, k.naam, b.datum
                                     ORDER BY b.datum ASC;";
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

        //[HttpGet]
        //public IActionResult GetBestellingDetails(int id)
        //{
        //    try
        //    {
        //        using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
        //        {
        //            mySqlConnection.Open();
        //            string query = $@"SELECT 
        //                             p.product_id, 
        //                             p.naam AS pizza_naam, 
        //                             pp.prijs AS pizza_prijs, 
        //                             br.aantal 
        //                             FROM bestel_regel br 
        //                             JOIN product p ON br.product_id = p.product_id 
        //                             JOIN product_prijs pp ON p.product_id = pp.product_id 
        //                             WHERE br.bestel_id = {id}";
        //            using (MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection))
        //            {
        //                mySqlCommand.Parameters.AddWithValue("@bestlId", id);
        //                using (MySqlDataReader reader = mySqlCommand.ExecuteReader())
        //                {
        //                    List<PizzaOverviewViewModel> pizzas = new List<PizzaOverviewViewModel>();
        //                    decimal subtotaal = 0;

        //                    while (reader.Read())
        //                    {
        //                        decimal prijs = reader.GetDecimal(2);
        //                        int aantal = reader.GetInt32(3);
        //                        subtotaal += prijs * aantal;

        //                        pizzas.Add(new PizzaOverviewViewModel
        //                        {
        //                            Id = reader.GetInt32(0),
        //                            Name = reader.GetString(1),
        //                            Price = prijs
        //                        });
        //                    }

        //                    return Json(new
        //                    {
        //                        pizzas,
        //                        subtotaal
        //                    });
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //        return StatusCode(500, "Fout bij het ophalen van bestellingsdetails.");
        //    }
        //}

        public ActionResult CreateBestellingTafel(int? bestellingId)
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
            return View();
        }

        private BestellingViewModel GetBestellingDataFromDataBase(int klantId)
        {
            
            List<PizzaOverviewViewModel> pizzas = new List<PizzaOverviewViewModel>();
            BestellingViewModel bestellingViewModel = null;

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
                {
                    mySqlConnection.Open();
                    string getBestellingQuery = "SELECT b.bestel_id, k.naam AS klant_naam, b.datum AS tijd, COALESCE(s.statusOmschrijving, 'Nog geen status') AS status FROM bestelling b JOIN klant k ON b.klant_id = k.klant_id LEFT JOIN bestel_regel br ON b.bestel_id = br.bestel_id LEFT JOIN pizza_status ps ON br.bestelregel_id = ps.bestelregel_id LEFT JOIN status s ON ps.status_id = s.status_id WHERE b.klant_id = @klantId ORDER BY b.datum DESC LIMIT 1;";
                    using (MySqlCommand mySqlCommand = new MySqlCommand(getBestellingQuery, mySqlConnection))
                    {
                        mySqlCommand.Parameters.AddWithValue("@klantId", klantId);
                        using (MySqlDataReader reader = mySqlCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                bestellingViewModel = new BestellingViewModel
                                {
                                    Id = reader.GetInt32(0),
                                    KlantNaam = reader.GetString(1),
                                    Tijd = reader.GetDateTime(2),
                                    SubTotaal = 0,
                                    Pizzas = pizzas 
                                };
                            }
                        }
                    }

                    if (bestellingViewModel == null)
                    {
                        Console.WriteLine($"Geen bestelling gevonden voor klantId: {klantId}");
                        return null;
                    }

                    string getPizzasQuery = "SELECT p.product_id, p.naam, pp.prijs " +
                                            "FROM product p " +
                                            "INNER JOIN product_prijs pp ON p.product_id = pp.product_id " +
                                            "JOIN bestel_regel br ON p.product_id = br.product_id " +
                                            "WHERE br.bestel_id = @bestelId";
                    using (MySqlCommand getPizzasCommand = new MySqlCommand(getPizzasQuery, mySqlConnection))
                    {
                        getPizzasCommand.Parameters.AddWithValue("bestelId", bestellingViewModel?.Id ?? 0);
                        using (MySqlDataReader pizzaReader = getPizzasCommand.ExecuteReader())
                        {
                            while (pizzaReader.Read())
                            {
                                pizzas.Add(new PizzaOverviewViewModel
                                {
                                    Id = pizzaReader.GetInt32(0),
                                    Name = pizzaReader.GetString(1),
                                    Price = pizzaReader.GetDecimal(2)
                                });
                            }
                        }
                    }

                    bestellingViewModel.Pizzas = pizzas;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fout bij ophalen van bestelling: {ex.Message}");
            }

            return bestellingViewModel;
        }


        public ActionResult VoegToe(int bestellingId, int pizzaId)
        {
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
                {
                    mySqlConnection.Open();

                    using (MySqlTransaction transaction = mySqlConnection.BeginTransaction())
                    {
                        try
                        {
                            string voegToeQuery = "INSERT INTO bestel_regel (bestel_id, product_id, aantal) VALUES (@bestellingId, @pizzaId, 1)";
                            int bestelRegelId;
                            using (MySqlCommand mySqlCommand = new MySqlCommand(voegToeQuery, mySqlConnection))
                            {
                                mySqlCommand.Parameters.AddWithValue("@bestellingId", bestellingId);
                                mySqlCommand.Parameters.AddWithValue("@pizzaId", pizzaId);
                                mySqlCommand.ExecuteNonQuery();

                                bestelRegelId = (int)mySqlCommand.LastInsertedId;
                            }

                            string voegStatusToeQuery = "INSERT INTO pizza_status (bestelregel_id, status_id) VALUES (@bestelRegelId, 2)";
                            using (MySqlCommand statusCommand = new MySqlCommand(voegStatusToeQuery, mySqlConnection))
                            {
                                statusCommand.Parameters.AddWithValue("bestelRegelId", bestelRegelId);
                                statusCommand.ExecuteNonQuery();
                            }

                            transaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            throw new Exception("Fout bij het toevoegen van een pizza en status", ex);
                        }
                    }
                    

                    BestellingViewModel bestellingViewModel = GetBestellingDataFromDataBase(bestellingId);

                    List<PizzaOverviewViewModel> beschikbarePizzas = new List<PizzaOverviewViewModel>();
                    string getAllProductsQuery = "SELECT p.product_id, p.naam, pp.prijs FROM product p INNER JOIN product_prijs pp ON p.product_id = pp.product_id";
                    using (MySqlCommand getAllProductsCommand = new MySqlCommand(getAllProductsQuery, mySqlConnection))
                    {
                        using (MySqlDataReader reader = getAllProductsCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                beschikbarePizzas.Add(new PizzaOverviewViewModel
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Price = reader.GetDecimal(2)
                                });
                            }
                        }
                    }

                    CreateBestellingViewModel createBestellingViewModel = new CreateBestellingViewModel
                    {
                        BestellingViewModel = bestellingViewModel,
                        BeschikbarePizzas = beschikbarePizzas,
                        BesteldePizzas = bestellingViewModel?.Pizzas ?? new List<PizzaOverviewViewModel>()
                    };

                    return View("createBestellingKlant", createBestellingViewModel);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding pizza to order: {ex.Message} {ex.StackTrace}");
                return View("Index");
            }
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

        public ActionResult CreateBestellingKlant()
        {
            List<PizzaOverviewViewModel> beschikbarePizzas = new List<PizzaOverviewViewModel>();
            List<PizzaOverviewViewModel> besteldePizzas = new List<PizzaOverviewViewModel>();
            BestellingViewModel bestellingViewModel = null;

            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
                {
                    mySqlConnection.Open();

                    string insertKlantQuery = @"INSERT INTO klant (naam, telefoonnummer) VALUES ('', '')";
                    using (MySqlCommand insertKlantCommand = new MySqlCommand(insertKlantQuery, mySqlConnection))
                    {
                        insertKlantCommand.ExecuteNonQuery();
                    }

                    string getKlantIdQuery = "SELECT LAST_INSERT_ID()";
                    int klantId = 0;
                    using (MySqlCommand getKlantIdCommand = new MySqlCommand(getKlantIdQuery, mySqlConnection))
                    {
                        klantId = Convert.ToInt32(getKlantIdCommand.ExecuteScalar());
                    }

                    string insertBestellingQuery = "INSERT INTO bestelling (klant_id, datum) VALUES (@klantId, NOW())";
                    using (MySqlCommand insertBestellingCommand = new MySqlCommand(insertBestellingQuery, mySqlConnection))
                    {
                        insertBestellingCommand.Parameters.AddWithValue("@klantId", klantId);
                        insertBestellingCommand.ExecuteNonQuery();
                    }

                    bestellingViewModel = GetBestellingDataFromDataBase(klantId);

                    string getAllProductsQuery = "SELECT p.product_id, p.naam, pp.prijs FROM product p INNER JOIN product_prijs pp ON p.product_id = pp.product_id";
                    using (MySqlCommand getAllProductsCommand = new MySqlCommand(getAllProductsQuery, mySqlConnection))
                    {
                        using (MySqlDataReader reader = getAllProductsCommand.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                beschikbarePizzas.Add(new PizzaOverviewViewModel
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Price = reader.GetDecimal(2),
                                });
                            }
                        }
                    }

                    CreateBestellingViewModel createBestellingViewModel = new CreateBestellingViewModel
                    {
                        BestellingViewModel = bestellingViewModel,
                        BeschikbarePizzas = beschikbarePizzas,
                        BesteldePizzas = besteldePizzas
                    };

                    return View(createBestellingViewModel);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View("Index");
            }
        }


        public ActionResult MaakBestelling(int bestellingId, string klantNaam, string telefoonnummer, DateTime datum, TimeSpan tijd)
        {
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
                {
                    mySqlConnection.Open();

                    DateTime datumTijd = datum.Date + tijd;

                    string updateKlantQuery = "UPDATE klant SET naam = @klantNaam, telefoonnummer = @telefoonnummer WHERE klant_id = (SELECT klant_id FROM bestelling WHERE bestel_id = @bestellingId)";
                    using (MySqlCommand updateKlantCommand = new MySqlCommand(updateKlantQuery, mySqlConnection))
                    {
                        updateKlantCommand.Parameters.AddWithValue("@klantNaam", klantNaam);
                        updateKlantCommand.Parameters.AddWithValue("@telefoonnummer", telefoonnummer);
                        updateKlantCommand.Parameters.AddWithValue("@bestellingId", bestellingId);
                        updateKlantCommand.ExecuteNonQuery();
                    }

                    string updateBestellingQuery = "UPDATE bestelling SET datum = @datum WHERE bestel_id = @bestellingid";
                    using (MySqlCommand updateBestellingCommand = new MySqlCommand(updateBestellingQuery, mySqlConnection))
                    {
                        updateBestellingCommand.Parameters.AddWithValue("@datum", datumTijd);
                        updateBestellingCommand.Parameters.AddWithValue("bestellingid", bestellingId);
                        updateBestellingCommand.ExecuteNonQuery();
                    }
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fout bij bijwerken klant");
                return View("Error");
            }
        }

        public JsonResult GetBestellingDetails(int bestellingId)
        {
            try
            {
                BestellingDetailsViewModel bestellingdetails = null;
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
                {
                    mySqlConnection.Open();
                    string query = @"SELECT p.product_id, p.naam, pp.prijs, b.bestel_id, b.datum FROM bestelling b JOIN bestel_regel br ON b.bestel_id = br.bestel_id JOIN product p ON br.product_id = p.product_id JOIN product_prijs pp ON p.product_id = pp.product_id WHERE b.bestel_id = @bestellingId;";

                    using (MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        mySqlCommand.Parameters.AddWithValue("@bestellingId", bestellingId);

                        using (MySqlDataReader reader = mySqlCommand.ExecuteReader())
                        {
                            List<PizzaOverviewViewModel> producten = new List<PizzaOverviewViewModel>();
                            DateTime? bestellingDatum = null;

                            while (reader.Read())
                            {
                                producten.Add(new PizzaOverviewViewModel
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Price = reader.GetDecimal(2)
                                });

                                if (bestellingDatum == null)
                                {
                                    bestellingDatum = reader.GetDateTime(4);
                                }
                            }

                            if (producten.Count > 0 && bestellingDatum.HasValue)
                            {
                                bestellingdetails = new BestellingDetailsViewModel
                                {
                                    BestellingId = bestellingId,
                                    Producten = producten,
                                    Date = bestellingDatum.Value,
                                    Subtotaal = 32
                                };
                            }
                        }
                    }
                }
                return Json(new
                {
                    succes = true,
                    data = new
                    {
                        products = bestellingdetails.Producten.Select(p => new
                        {
                            p.Id,
                            p.Name,
                            p.Price
                        }),
                        subTotal = bestellingdetails.Subtotaal,
                        date = bestellingdetails.Date
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { succes = false, message = ex.Message });
            }
        }
    }
}
