using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RADALogisticsWEB.Models
{
    public class Movement
    {
        public int ID { get; set; }
        public string Foio { get; set; }
        public string Choffer { get; set; }
        public string StatusNow { get; set; }
        public string Container { get; set; }
        public string Message { get; set; }
        public string Active { get; set; }
        public string Date { get; set; }
    }
}