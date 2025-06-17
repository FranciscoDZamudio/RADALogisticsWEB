using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RADALogisticsWEB.Models
{
    public class Solicitud_Contenedores
    {
        public string Urgencia { get; set; }
        public int ID { get; set; }
        public string Folio { get; set; }
        public string Who_Send { get; set; }
        public string Container { get; set; }
        public string Destination_Location { get; set; }
        public string Origins_Location { get; set; }
        public string Status { get; set; }
        public string message { get; set; }
        public string shift { get; set; }
        public DateTime Date { get; set; }
        public string Datetime { get; set; }
    }
}