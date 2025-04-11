using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RADALogisticsWEB.Models
{
    public class DetailsTable
    {
        public string id { get; set; }
        public string Folio { get; set; }
        public string Type_Status { get; set; }
        public string GruaMov { get; set; }
        public string ProcessMovement { get; set; }
        public string End_date { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
        public string Date_Process { get; set; }
    }
}