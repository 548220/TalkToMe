using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace TalkToMeMario.Models
{
    public class BestellingOverViewViewModel
    {
        required
        public int Id { get; set; }

        required 
        public string KlantNaam { get; set; }

        required
        public string Tijd { get; set; }

        required
        public string Status { get; set; }

        required
        public double SubTotaal { get; set; }

    }
}
