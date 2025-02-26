using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RADALogisticsWEB.Models
{
    public class Usuarios
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Employee_Number { get; set; }
        public string Login_session { get; set; }
        public string Password { get; set; }
        public string Type_user { get; set; }
        public string Email { get; set; }
        public DateTime DateAdded { get; set; }
        public bool Active { get; set; }
    }
}