using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RADALogisticsWEB.Models
{
    public class Inventario
    {
        public int ID { get; set; }
        public string Username { get; set; }
        public string Container { get; set; }
        public string LocationCode { get; set; }
        public string Status { get; set; }
        public DateTime Date { get; set; }
        public DateTime Datetimes { get; set; }
        public bool Active { get; set; }
    }
}