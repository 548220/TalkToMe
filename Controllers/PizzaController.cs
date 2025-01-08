using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using TalkToMeMario.Models;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices;

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

        // GET: PizzaController/Details/5
        public ActionResult Details(int id)
        {

            return View();
        }

        // GET: PizzaController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: PizzaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string name, int price, int category)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionString))
                    {
                        mySqlConnection.Open();
                        string query = $"START TRANSACTION; INSERT INTO `product` (product_id, categorie_id, naam) VALUES (13, {category}, '{name}'); INSERT INTO `product_prijs` (product_id, prijs, datum_start) VALUES (13, {price}, '2025-06-01'); COMMIT;";
                        using (MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection))
                        {
                            mySqlCommand.ExecuteNonQuery();
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    
                }
            }
            return View();
        }

        // GET: PizzaController/Edit/5

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
                                return NotFound();
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

                    string query = $"UPDATE `product` SET Naam = '{model.Name}', categorie_id = '{model.CategoryId}' WHERE product_id = {model.Id}";
                    using (MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        mySqlCommand.ExecuteNonQuery();
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
