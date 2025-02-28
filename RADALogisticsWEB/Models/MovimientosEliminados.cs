using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RADALogisticsWEB.Models
{
    public class MovimientosEliminados
    {
        public string Folio { get; set; }
        public string Reason { get; set; }
        public string Company { get; set; }
        public DateTime Datetime { get; set; }
    }
}