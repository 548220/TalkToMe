using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using TalkToMeMario.Models;
using Microsoft.VisualBasic;

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

            pizzaViewModels.Add(new PizzaOverviewViewModel() { Id = 1, Name = "Pizza Margarita", Price = 10.00m });
            pizzaViewModels.Add(new PizzaOverviewViewModel() { Id = 2, Name = "Pizza Tonno", Price = 12.50m });
            try
            {
                using (MySqlConnection mySqlConnection = new MySqlConnection(_connectionString))
                {
                    mySqlConnection.Open();
                    using (MySqlCommand mySqlCommand = new MySqlCommand("Select id,name,price FROM Pizza;", mySqlConnection))
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
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
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
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
