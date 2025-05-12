using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RADALogisticsWEB.Models
{
    public class Historial
    {
        public string RequestGrua { get; set; }
        public string Folio { get; set; }
        public string Container { get; set; }
        public string Origen { get; set; }
        public string Destination { get; set; }
        public string Status { get; set; }
        public string HSolicitud { get; set; }
        public string HConfirm { get; set; }
        public string HFinish { get; set; }
        public string WhoRequest { get; set; }
        public string Choffer { get; set; }
        public string fastcard { get; set; }
        public string Comment { get; set; }
        public string Date { get; set; }
        public string Area { get; set; }
        public string RaR { get; set; }
    }
}