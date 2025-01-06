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
        public ActionResult Create(string name, int price)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionString))
                    {
                        mySqlConnection.Open();
                        string query = $"INSERT INTO pizza ({name}, {price}) VALUES ({name}, {price});";
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
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PizzaController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionString))
                {
                    mySqlConnection.Open();
                    string query = $"UPDATE pizza SET name = naam, price = prijs, description = lekker WHERE id= {id}";
                    using (MySqlCommand mySqlCommand = new MySqlCommand(query, mySqlConnection))
                    {
                        mySqlCommand.ExecuteNonQuery();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: PizzaController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
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
