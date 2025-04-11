using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RADALogisticsWEB.Models
{
    public class Chart2Statusfiles
    {
        public string message { get; set; }
        public int Count { get; set; }
        public DateTime Date { get; set; }

        public string messageAreas { get; set; }
        public int CountAreas { get; set; }
        public DateTime DateAreas { get; set; }
    }
}