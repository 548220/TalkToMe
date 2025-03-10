﻿using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Mysqlx.Crud;
using Org.BouncyCastle.Asn1.X509;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.Marshalling;
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
                    bestellingViewModel.SubTotaal = Convert.ToDouble(pizzas.Sum(p => p.Price));
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

                            string voegStatusToeQuery = "INSERT INTO pizza_status (bestelregel_id, status_id, changed_at) VALUES (@bestelRegelId, 2, @dateTime)";
                            DateTime dateTime = DateTime.Now;
                            using (MySqlCommand statusCommand = new MySqlCommand(voegStatusToeQuery, mySqlConnection))
                            {
                                statusCommand.Parameters.AddWithValue("@bestelRegelId", bestelRegelId);
                                statusCommand.Parameters.AddWithValue("@dateTime", dateTime);
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

        public ActionResult FinaliserenBestellingKlant(int bestellingId)
        {
            BestellingViewModel bestellingViewModel = null;
            List<PizzaOverviewViewModel> pizzas = new List<PizzaOverviewViewModel>();
            using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
            {
                mySqlConnection.Open();
                string query = "SELECT b.bestel_id, k.naam AS klantnaam, k.telefoonnummer, b.datum FROM bestelling b JOIN klant k ON b.klant_id = k.klant_id WHERE b.bestel_id = @bestelId;";
                using (MySqlCommand getBestellingCommand = new MySqlCommand(query, mySqlConnection))
                {
                    getBestellingCommand.Parameters.AddWithValue("@bestelId", bestellingId);
                    using (MySqlDataReader reader = getBestellingCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            bestellingViewModel = new BestellingViewModel
                            {
                                Id = reader.GetInt32(0),
                                KlantNaam = reader.GetString(1),
                                TelefoonNummer = reader.GetInt32(2),
                                Tijd = reader.GetDateTime(3),
                                SubTotaal = 0,
                                Pizzas = pizzas
                            };
                        }
                    }
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

                BestellingDetailsViewModel detailsViewModel = new BestellingDetailsViewModel
                {
                    BesteldeProducten = pizzas,
                    BestellingId = bestellingViewModel.Id,
                    KlantNaam = bestellingViewModel.KlantNaam,
                    Date = bestellingViewModel.Tijd,
                    Subtotaal = Convert.ToDouble(pizzas.Sum(p => p.Price)),
                    Telefoonnummer = bestellingViewModel.TelefoonNummer,
                };

                return View(detailsViewModel);
            }
        }

        public ActionResult BetaalBestellingContant(int bestellingId)
        {
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
                {
                    mySqlConnection.Open();
                    string query = "INSERT INTO `betaling` (`bestel_id`, `betaalWijze_id`, `betaalStatus_id`, `bedrag`) VALUES (@bestelId, '1', '1', '17');";

                    using (MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        mySqlCommand.Parameters.AddWithValue("@bestelId", bestellingId);

                        mySqlCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return RedirectToAction("Index");
        }

        public ActionResult BetaalBestellingPin(int bestellingId)
        {
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
                {
                    mySqlConnection.Open();
                    string query = "INSERT INTO `betaling` (`bestel_id`, `betaalWijze_id`, `betaalStatus_id`, `bedrag`) VALUES (@bestelId, '2', '1', '17');";

                    using (MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        mySqlCommand.Parameters.AddWithValue("@bestelId", bestellingId);

                        mySqlCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return RedirectToAction("Index");
        }

        public ActionResult AnnuleerBestelling(int bestellingId)
        {
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionstring))
                {
                    mySqlConnection.Open();
                    string query = "UPDATE pizza_status ps JOIN bestel_regel br ON ps.bestelRegel_id = br.bestelRegel_id SET ps.status_id = 3 WHERE br.bestel_id = @bestellingId;";

                    using (MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        mySqlCommand.Parameters.AddWithValue("@bestellingId", bestellingId);

                        mySqlCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return RedirectToAction("Index");
        }
    }
}
