using TalkToMeMario.Models;
using MySql.Data.MySqlClient;

namespace TalkToMeMario.Services
{
    public class PizzaService
    {
        string _connectionString;
        public PizzaService(string connectionString)
        {
            _connectionString = connectionString;
        }
        List<PizzaOverviewViewModel> GetAllPizzas()
        {
            List<PizzaOverviewViewModel> pizzas = new List<PizzaOverviewViewModel>();
            pizzas.Add(new PizzaOverviewViewModel() { Id = 1, Name = "Pizza Margarita", Price = 10.00m });
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
                                pizzas.Add(new PizzaOverviewViewModel()
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
            }
            catch (Exception ex)
            {
                //TODO: Exception handling(Logging and notifying user) something else went wrong
            }
            return pizzas;
        }
    }
}

 

