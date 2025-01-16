using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using TalkToMeMario.Models;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;
using System.Transactions;

namespace TalkToMeMario.Controllers
{
    public class PizzaController : Controller
    {
        string _connectionString;
        public PizzaController(string connectionString)
        {
            _connectionString = connectionString;
        }
        // GET: PizzaController
        public ActionResult Index()
        {
            List<PizzaOverviewViewModel> pizzaViewModels = new List<PizzaOverviewViewModel>();
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionString))
                {
                    mySqlConnection.Open();
                    string query = @"SELECT p.product_id, p.naam, pp.prijs
                                    FROM product p
                                    INNER JOIN product_prijs pp ON p.product_id = pp.product_id;";
                    using (MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        using (MySqlDataReader mySqlDataReader = mySqlCommand.ExecuteReader())
                        {
                            while (mySqlDataReader.Read())
                            {
                                pizzaViewModels.Add(new PizzaOverviewViewModel()
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
                //TODO: Exception handling(Logging and notifying user) something went wrong with the db
                // ExLogger.LogException(ex);
                // return View("Error");

            }
            catch (Exception ex)
            {
                // ExLogger.LogException(ex);
                // return View("Error");
                // TODO: Exception handling(Logging and notifying user) something else went wrong
            }
            return View(pizzaViewModels);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string name, double price, int category)
        {
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionString))
                {
                    mySqlConnection.Open();

                    using (MySqlTransaction mySqlTransaction = mySqlConnection.BeginTransaction())
                    {
                        try
                        {
                            string insertProductQuery = $"INSERT INTO `product` (categorie_id, naam) VALUES ({category}, '{name}')";
                            using (MySqlCommand mySqlCommand = new MySqlCommand(insertProductQuery, mySqlConnection, mySqlTransaction))
                            {
                                mySqlCommand.ExecuteNonQuery();
                            }

                            string getLastInsertIdQuery = "SELECT LAST_INSERT_ID();";
                            long productId;
                            using (MySqlCommand getLastInsertIdCommand = new MySqlCommand(getLastInsertIdQuery, mySqlConnection))
                            {
                                productId = Convert.ToInt64(getLastInsertIdCommand.ExecuteScalar());
                            }

                            string insertPriceQuery = $"INSERT INTO `product_prijs` (product_id, prijs, datum_start) VALUES ({productId}, {price}, '2023-11-12');";
                            using (MySqlCommand insertPriceCommand = new MySqlCommand(insertPriceQuery, mySqlConnection, mySqlTransaction))
                            {
                                insertPriceCommand.ExecuteNonQuery();
                            }

                            mySqlTransaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Something went wrong");
                        }
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return View();
            }
            
        }

        // TODO: prijs nog bij
        public ActionResult Edit(int id)
        {
            try
            {
                PizzaOverviewViewModel pizza;
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionString))
                {
                    mySqlConnection.Open();

                    string query = $"SELECT p.product_id, p.naam, pp.prijs, p.categorie_id FROM product p INNER JOIN product_prijs pp ON p.product_id = pp.product_id WHERE p.product_id = {id}";
                    using (MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        using (var reader = mySqlCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                pizza = new PizzaOverviewViewModel
                                {
                                    Id = reader.GetInt32("product_id"),
                                    Name = reader.GetString("naam"),
                                    Price = reader.GetDecimal("prijs"),
                                    CategoryId = reader.GetInt32("categorie_id"),
                                };                              
                            }
                            else
                            {
                                return View();
                            }
                        }
                    }
                }
                return View(pizza);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        // POST: PizzaController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(PizzaOverviewViewModel model)
        {
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionString))
                {
                    mySqlConnection.Open();

                    string productQuery = $"UPDATE `product` SET Naam = '{model.Name}', categorie_id = '{model.CategoryId}' WHERE product_id = {model.Id}";
                    using (MySqlCommand productCommand = new MySqlCommand(productQuery, mySqlConnection))
                    {
                        productCommand.ExecuteNonQuery();
                    }

                    //todo: fix price 
                    decimal priceFix = model.Price / 100;
                    string priceQuery = $"UPDATE `product_prijs` SET prijs = @prijs WHERE product_id = @product_id";
                    using (MySqlCommand priceCommand = new MySqlCommand(priceQuery, mySqlConnection))
                    {
                        priceCommand.Parameters.AddWithValue("@prijs", priceFix);
                        priceCommand.Parameters.AddWithValue("@product_id", model.Id);
                        priceCommand.ExecuteNonQuery();
                    }
                }
                return RedirectToAction("Index");
            }
            catch
            {
                return View(model);
            }
        }

        // GET: PizzaController/Delete/5
        public ActionResult Delete(int id)
        {
            try
            {
                using (MySqlConnection mysqlconnection = new MySqlConnection(_connectionString))
                {
                    mysqlconnection.Open();

                    using (MySqlTransaction mySqlTransaction = mysqlconnection.BeginTransaction())
                    {
                        try
                        {
                            string deleteProductPrijsQuery = $"DELETE FROM `product_prijs` WHERE product_id = {id}";
                            using (MySqlCommand deleteProductPrijsCommand = new MySqlCommand(deleteProductPrijsQuery, mysqlconnection, mySqlTransaction))
                            {
                                deleteProductPrijsCommand.ExecuteNonQuery();
                            }

                            string deleteProductQuery = $"DELETE FROM `product` WHERE product_id = {id}";
                            using (MySqlCommand deleteProductCommand = new MySqlCommand(deleteProductQuery, mysqlconnection, mySqlTransaction))
                            {
                                deleteProductCommand.ExecuteNonQuery();
                            }

                            mySqlTransaction.Commit();
                        }
                        catch (Exception ex)
                        {
                            mySqlTransaction.Rollback();
                            throw;
                        }
                    }
                }
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // Voeg foutafhandeling toe indien nodig
                return RedirectToAction("Index");
            }
        }

        // POST: PizzaController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionString))
                {
                    mySqlConnection.Open();
                    using (MySqlCommand mySqlCommand = new MySqlCommand($"DELETE FROM pizza WHERE id = {id};", mySqlConnection))
                    {
                        mySqlCommand.ExecuteNonQuery();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex) 
            {
                return View();
            }
        }
    }
}
