using RADALogisticsWEB.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RADALogisticsWEB.Controllers
{
    public class HistoryController : Controller
    {
        List<Historial> GetRecords = new List<Historial>();
        List<Inventario> GetInventary = new List<Inventario>();
        List<Inventario> GetInventaryquery = new List<Inventario>();
        List<Historial> GetRecordsQeury = new List<Historial>();
        List<Historial> GetRecordsNow = new List<Historial>();

        List<string> filtroAreas = new List<string>();

        //connection SQL server (database)
        SqlConnection DBSPP = new SqlConnection("Data Source=RADAEmpire.mssql.somee.com ;Initial Catalog=RADAEmpire ;User ID=RooRada; password=rada1311");
        SqlCommand con = new SqlCommand();
        SqlDataReader dr;
        // GET: History

        [HttpPost]
        public ActionResult Dashboard(string TimeStart)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                if (TimeStart == "")
                {
                    // Obtener la fecha y hora actual en Alemania (zona horaria UTC+1 o UTC+2 dependiendo del horario de verano)
                    DateTime germanTime = DateTime.UtcNow.AddHours(0);  // Alemania es UTC+1

                    // Convertir la hora alemana a la hora en una zona horaria específica de EE. UU. (por ejemplo, Nueva York, UTC-5)
                    TimeZoneInfo usEasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                    DateTime usTime = TimeZoneInfo.ConvertTime(germanTime, usEasternTimeZone);

                    // Formatear la fecha para que sea adecuada para la base de datos
                    string formattedDate = usTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    string moreMovement = null, totalRecs = null, CanceledRows = null;
                    //create generate randoms int value
                    SqlCommand conse = new SqlCommand("Select Top (1) shift, COUNT(Active) as Total " +
                        " from RADAEmpire_BRequestContainers where Active = '1' and Date = '" + usTime.ToString("yyyy-MM-dd") + "' group by shift order by Total desc", DBSPP);
                    DBSPP.Open();
                    SqlDataReader drconse = conse.ExecuteReader();
                    if (drconse.HasRows)
                    {
                        while (drconse.Read())
                        {
                            moreMovement = drconse["shift"].ToString();
                        }
                    }
                    else
                    {
                        moreMovement = "AREA NO FOUND";
                    }
                    DBSPP.Close();

                    //create generate randoms int value
                    SqlCommand totalRecords = new SqlCommand("Select COUNT(Active) as Total " +
                        " from RADAEmpire_BRequestContainers where Active = '1' and Date = '" + usTime.ToString("yyyy-MM-dd") + "' order by Total desc", DBSPP);
                    DBSPP.Open();
                    SqlDataReader drtotalRecords = totalRecords.ExecuteReader();
                    if (drtotalRecords.HasRows)
                    {
                        while (drtotalRecords.Read())
                        {
                            totalRecs = drtotalRecords["Total"].ToString();
                        }
                    }
                    else
                    {
                        totalRecs = "0";
                    }
                    DBSPP.Close();

                    //create generate randoms int value
                    SqlCommand Deleterows = new SqlCommand("Select COUNT(Active) as Total " +
                        " from RADAEmpire_BRequestContainers where Active = '1' and Date = '" + usTime.ToString("yyyy-MM-dd") + "' and message = 'Canceled by Rada' order by Total desc", DBSPP);
                    DBSPP.Open();
                    SqlDataReader drDeleterows = Deleterows.ExecuteReader();
                    if (drDeleterows.HasRows)
                    {
                        while (drDeleterows.Read())
                        {
                            CanceledRows = drDeleterows["Total"].ToString();
                        }
                    }
                    else
                    {
                        CanceledRows = "0";
                    }
                    DBSPP.Close();

                    ViewBag.Date = DateTime.Now.ToString("yyyy-MM-dd");
                    //impresion de informacion
                    ViewBag.CanceledTotals = CanceledRows;
                    ViewBag.RecordTotals = totalRecs;
                    ViewBag.MoreMovement = moreMovement;

                    GetRecordNows();
                    ViewBag.Records = GetRecordsNow;
                    ViewBag.count = GetRecordsNow.Count.ToString();

                    return View();
                }
                else
                {
                    // Obtener la fecha y hora actual en Alemania (zona horaria UTC+1 o UTC+2 dependiendo del horario de verano)
                    DateTime germanTime = DateTime.UtcNow.AddHours(0);  // Alemania es UTC+1

                    // Convertir la hora alemana a la hora en una zona horaria específica de EE. UU. (por ejemplo, Nueva York, UTC-5)
                    TimeZoneInfo usEasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                    DateTime usTime = TimeZoneInfo.ConvertTime(germanTime, usEasternTimeZone);

                    // Formatear la fecha para que sea adecuada para la base de datos
                    string formattedDate = usTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    string moreMovement = null, totalRecs = null, CanceledRows = null;
                    //create generate randoms int value
                    SqlCommand conse = new SqlCommand("Select Top (1) shift, COUNT(Active) as Total " +
                        " from RADAEmpire_BRequestContainers where Active = '1' and Date = '" + Convert.ToDateTime(TimeStart).ToString("yyyy-MM-dd") + "' group by shift order by Total desc", DBSPP);
                    DBSPP.Open();
                    SqlDataReader drconse = conse.ExecuteReader();
                    if (drconse.HasRows)
                    {
                        while (drconse.Read())
                        {
                            moreMovement = drconse["shift"].ToString();
                        }
                    }
                    else
                    {
                        moreMovement = "AREA NO FOUND";
                    }
                    DBSPP.Close();

                    //create generate randoms int value
                    SqlCommand totalRecords = new SqlCommand("Select COUNT(Active) as Total " +
                        " from RADAEmpire_BRequestContainers where Active = '1' and Date = '" + Convert.ToDateTime(TimeStart).ToString("yyyy-MM-dd") + "' order by Total desc", DBSPP);
                    DBSPP.Open();
                    SqlDataReader drtotalRecords = totalRecords.ExecuteReader();
                    if (drtotalRecords.HasRows)
                    {
                        while (drtotalRecords.Read())
                        {
                            totalRecs = drtotalRecords["Total"].ToString();
                        }
                    }
                    else
                    {
                        totalRecs = "0";
                    }
                    DBSPP.Close();

                    //create generate randoms int value
                    SqlCommand Deleterows = new SqlCommand("Select COUNT(Active) as Total " +
                        " from RADAEmpire_BRequestContainers where Active = '1' and Date = '" + Convert.ToDateTime(TimeStart).ToString("yyyy-MM-dd") + "' and message = 'Canceled by Rada' order by Total desc", DBSPP);
                    DBSPP.Open();
                    SqlDataReader drDeleterows = Deleterows.ExecuteReader();
                    if (drDeleterows.HasRows)
                    {
                        while (drDeleterows.Read())
                        {
                            CanceledRows = drDeleterows["Total"].ToString();
                        }
                    }
                    else
                    {
                        CanceledRows = "0";
                    }
                    DBSPP.Close();

                    ViewBag.Date = Convert.ToDateTime(TimeStart).ToString("yyyy-MM-dd");

                    //impresion de informacion
                    ViewBag.CanceledTotals = CanceledRows;
                    ViewBag.RecordTotals = totalRecs;
                    ViewBag.MoreMovement = moreMovement;

                    DBSPP.Open();
                    con.Connection = DBSPP;
                    con.CommandText = "  Select top (100) " +
                        " b.FastCard as FastCard, a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                        " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date,a.shift as Area  " +
                        " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio where a.Date = '" + Convert.ToDateTime(TimeStart).ToString("yyyy-MM-dd") + "' ORDER by a.Folio desc";
                    dr = con.ExecuteReader();
                    while (dr.Read())
                    {
                        GetRecordsNow.Add(new Historial()
                        {
                            Folio = (dr["Folio"].ToString()),
                            Container = (dr["Container"].ToString()),
                            Origen = (dr["Origen"].ToString()),
                            Destination = (dr["Destination"].ToString()),
                            Status = (dr["Status"].ToString()),
                            HSolicitud = (dr["HSolicitud"].ToString()),
                            HConfirm = (dr["HConfirm"].ToString()),
                            HFinish = (dr["HFinish"].ToString()),
                            WhoRequest = (dr["WhoRequest"].ToString()),
                            Choffer = dr["Choffer"].ToString(),
                            fastcard = dr["FastCard"].ToString(),
                            Comment = (dr["Comment"].ToString()),
                            Date = Convert.ToDateTime(dr["Date"]).ToString("MM/dd/yyyy"),
                            Area = (dr["Area"].ToString()),
                        });
                    }
                    DBSPP.Close();

                    ViewBag.Records = GetRecordsNow;
                    ViewBag.count = GetRecordsNow.Count.ToString();

                    return View();
                }
            }
        }

        public ActionResult Dashboard()
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                // Obtener la fecha y hora actual en Alemania (zona horaria UTC+1 o UTC+2 dependiendo del horario de verano)
                DateTime germanTime = DateTime.UtcNow.AddHours(0);  // Alemania es UTC+1

                // Convertir la hora alemana a la hora en una zona horaria específica de EE. UU. (por ejemplo, Nueva York, UTC-5)
                TimeZoneInfo usEasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                DateTime usTime = TimeZoneInfo.ConvertTime(germanTime, usEasternTimeZone);

                // Formatear la fecha para que sea adecuada para la base de datos
                string formattedDate = usTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                string moreMovement = null, totalRecs = null, CanceledRows = null;
                //create generate randoms int value
                SqlCommand conse = new SqlCommand("Select Top (1) shift, COUNT(Active) as Total " +
                    " from RADAEmpire_BRequestContainers where Active = '1' and Date = '" + usTime.ToString("yyyy-MM-dd") + "' group by shift order by Total desc", DBSPP);
                DBSPP.Open();
                SqlDataReader drconse = conse.ExecuteReader();
                if (drconse.HasRows)
                {
                    while (drconse.Read())
                    {
                        moreMovement = drconse["shift"].ToString();
                    }
                }
                else
                {
                    moreMovement = "AREA NO FOUND";
                }
                DBSPP.Close();

                //create generate randoms int value
                SqlCommand totalRecords = new SqlCommand("Select COUNT(Active) as Total " +
                    " from RADAEmpire_BRequestContainers where Active = '1' and Date = '" + usTime.ToString("yyyy-MM-dd") + "' order by Total desc", DBSPP);
                DBSPP.Open();
                SqlDataReader drtotalRecords = totalRecords.ExecuteReader();
                if (drtotalRecords.HasRows)
                {
                    while (drtotalRecords.Read())
                    {
                        totalRecs = drtotalRecords["Total"].ToString();
                    }
                }
                else
                {
                    totalRecs = "0";
                }
                DBSPP.Close();

                //create generate randoms int value
                SqlCommand Deleterows = new SqlCommand("Select COUNT(Active) as Total " +
                    " from RADAEmpire_BRequestContainers where Active = '1' and Date = '" + usTime.ToString("yyyy-MM-dd") + "' and message = 'Canceled by Rada' order by Total desc", DBSPP);
                DBSPP.Open();
                SqlDataReader drDeleterows = Deleterows.ExecuteReader();
                if (drDeleterows.HasRows)
                {
                    while (drDeleterows.Read())
                    {
                        CanceledRows = drDeleterows["Total"].ToString();
                    }
                }
                else
                {
                    CanceledRows = "0";
                }
                DBSPP.Close();

                ViewBag.Date = DateTime.Now.ToString("yyyy-MM-dd");
                //impresion de informacion
                ViewBag.CanceledTotals = CanceledRows;
                ViewBag.RecordTotals = totalRecs;
                ViewBag.MoreMovement = moreMovement;

                GetRecordNows();
                ViewBag.Records = GetRecordsNow;
                ViewBag.count = GetRecordsNow.Count.ToString();

                return View();
            }
        }

        public List<Chart2Statusfiles> ChartToColumn()
        {
            List<Chart2Statusfiles> ListedLine = new List<Chart2Statusfiles>();

            DBSPP.Open();
            SqlCommand Line = new SqlCommand("Select message, COUNT(Active) as Total,Date from RADAEmpire_BRequestContainers " +
                " where Active = '1' group by message, Date order by Total desc", DBSPP);
            SqlDataReader drLine = Line.ExecuteReader();
            if (drLine.HasRows)
            {
                while (drLine.Read())
                {
                    ListedLine.Add(new Chart2Statusfiles()
                    {
                        message = (drLine["message"].ToString()),
                        Count = int.Parse(drLine["Total"].ToString()),
                        Date = Convert.ToDateTime(drLine["Date"].ToString()),
                    });
                }
            }
            else
            {
                ListedLine.Clear();
            }
            DBSPP.Close();
            return ListedLine;
        }

        public List<Chart2Statusfiles> ChartToColumnAreas()
        {
            List<Chart2Statusfiles> ListedLine = new List<Chart2Statusfiles>();

            DBSPP.Open();
            SqlCommand Line = new SqlCommand("Select shift, COUNT(Active) as Total,Date from RADAEmpire_BRequestContainers " +
                " where Active = '1' group by shift, Date order by Total desc", DBSPP);
            SqlDataReader drLine = Line.ExecuteReader();
            if (drLine.HasRows)
            {
                while (drLine.Read())
                {
                    ListedLine.Add(new Chart2Statusfiles()
                    {
                        messageAreas = (drLine["shift"].ToString()),
                        CountAreas = int.Parse(drLine["Total"].ToString()),
                        DateAreas = Convert.ToDateTime(drLine["Date"].ToString()),
                    });
                }
            }
            else
            {
                ListedLine.Clear();
            }
            DBSPP.Close();
            return ListedLine;
        }

        [HttpGet]
        public JsonResult ReporteLineasAreas(string fechaFiltro)
        {
            HistoryController chartsController = new HistoryController();
            List<Chart2Statusfiles> objetlisted = chartsController.ChartToColumnAreas();
            // Filtrar los datos por la fecha proporcionada (asumiendo que r.Date es DateTime)
            objetlisted = objetlisted
            .Where(r => Convert.ToDateTime(r.DateAreas).Date == DateTime.Parse(fechaFiltro).Date)
            .ToList();


            return Json(objetlisted, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult ReporteLineas(string fechaFiltro)
        {
            HistoryController chartsController = new HistoryController();
            List<Chart2Statusfiles> objetlisted = chartsController.ChartToColumn();
            // Filtrar los datos por la fecha proporcionada (asumiendo que r.Date es DateTime)
            objetlisted = objetlisted
            .Where(r => Convert.ToDateTime(r.Date).Date == DateTime.Parse(fechaFiltro).Date)
            .ToList();


            return Json(objetlisted, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InventoryRecords()
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                GetInventoryControl();
                ViewBag.Records = GetInventary;
                ViewBag.Count = GetInventary.Count.ToString();
                return View();
            }
        }

        [HttpPost]
        public ActionResult InventoryRecords(string Timeend, string TimeStart)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            int count = 0;
            string sqlTimeStart = null;
            string sqlTimeend = null;

            if (Timeend != "" && TimeStart != "")
            {
                if (TimeStart == "")
                {
                    sqlTimeStart = "";
                }
                else
                {
                    sqlTimeStart = " and Date BETWEEN '" + TimeStart + "'";
                }

                if (Timeend == "")
                {
                    sqlTimeend = "";
                }
                else
                {
                    sqlTimeend = " and '" + Timeend + "'";
                }
            }
            else
            {
                count = count + 2;
            }

            if (count >= 2)
            {
                GetInventoryControl();
                ViewBag.Records = GetInventary;
                ViewBag.Count = GetInventary.Count.ToString();
                return View();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select top (100) * from RADAEmpire_CInventoryControl where Active = '1' " + sqlTimeStart + sqlTimeend + " order by ID desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetInventaryquery.Add(new Inventario()
                    {
                        ID = int.Parse(dr["ID"].ToString()),
                        Username = (dr["Username"].ToString()),
                        Container = (dr["Container"].ToString()),
                        LocationCode = (dr["LocationCode"].ToString()),
                        Status = (dr["Status"].ToString()),
                        Datetimes = Convert.ToDateTime(dr["Datetime"]),
                    });
                }
                DBSPP.Close();

                ViewBag.Records = GetInventaryquery;
                ViewBag.Count = GetInventaryquery.Count.ToString();
                return View();
            }
        }

        private void GetInventoryControl()
        {
            if (GetInventary.Count > 0)
            {
                GetInventary.Clear();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select top (100) * from RADAEmpire_CInventoryControl where Active = '1' order by ID desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetInventary.Add(new Inventario()
                    {
                        ID = int.Parse(dr["ID"].ToString()),
                        Username = (dr["Username"].ToString()),
                        Container = (dr["Container"].ToString()),
                        LocationCode = (dr["LocationCode"].ToString()),
                        Status = (dr["Status"].ToString()),
                        Datetimes = Convert.ToDateTime(dr["Datetime"]),
                    });
                }
                DBSPP.Close();
            }
        }

        public ActionResult HRecord()
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                string name = Session["Username"].ToString();
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select * from RADAEmpire_ARoles where Active = '1' order by ID desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    if (dr["Username"].ToString() == name)
                    {
                        filtroAreas.Add(dr["Areas"].ToString());
                    }
                }
                DBSPP.Close();

                GetRecord();
                ViewBag.Records = GetRecords;
                ViewBag.Count = GetRecords.Count.ToString();
                return View();
            }
        }

        [HttpPost]
        public ActionResult HRecord(string Timeend, string TimeStart)
        {
            int count = 0;
            string sqlTimeStart = null;
            string sqlTimeend = null;

            if (Timeend != "" && TimeStart != "")
            {
                if (TimeStart == "")
                {
                    sqlTimeStart = "";
                }
                else
                {
                    sqlTimeStart = " Where a.Date BETWEEN '" + TimeStart + "'";
                }

                if (Timeend == "")
                {
                    sqlTimeend = "";
                }
                else
                {
                    sqlTimeend = " and '" + Timeend + "'";
                }
            }
            else
            {
                count = count + 2;
            }

            if (count >= 2)
            {

                GetRecord();
                ViewBag.Records = GetRecords;
                ViewBag.Count = GetRecords.Count.ToString();
                return View();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "  Select top (100) " +
                    " b.FastCard as FastCard, a.Folio as Folio, a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                    " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date " +
                    " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio " + sqlTimeStart + sqlTimeend + " ORDER by a.Folio desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetRecordsQeury.Add(new Historial()
                    {
                        Folio = (dr["Folio"].ToString()),
                        Container = (dr["Container"].ToString()),
                        Origen = (dr["Origen"].ToString()),
                        Destination = (dr["Destination"].ToString()),
                        Status = (dr["Status"].ToString()),
                        HSolicitud = (dr["HSolicitud"].ToString()),
                        HConfirm = (dr["HConfirm"].ToString()),
                        HFinish = (dr["HFinish"].ToString()),
                        WhoRequest = (dr["WhoRequest"].ToString()),
                        Choffer = (dr["Choffer"].ToString()),
                        fastcard = dr["FastCard"].ToString(),
                        Comment = (dr["Comment"].ToString()),
                        Date = Convert.ToDateTime(dr["Date"]).ToString("MM/dd/yyyy"),
                    });
                }
                DBSPP.Close();

                GetRecord();
                ViewBag.Records = GetRecordsQeury;
                ViewBag.Count = GetRecordsQeury.Count.ToString();
                return View();
            }
        }

        public ActionResult Record()
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                string name = Session["Username"].ToString();
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select * from RADAEmpire_ARoles where Active = '1' order by ID desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    if (dr["Username"].ToString() == name)
                    {
                        filtroAreas.Add(dr["Areas"].ToString());
                    }
                }
                DBSPP.Close();

                GetRecord(filtroAreas);
                ViewBag.Records = GetRecords;
                ViewBag.Count = GetRecords.Count.ToString();
                return View();
            }
        }

        [HttpPost]
        public ActionResult Record(string Timeend, string TimeStart)
        {
            int count = 0;
            string sqlTimeStart = null;
            string sqlTimeend = null;

            if (Timeend != "" && TimeStart != "")
            {
                if (TimeStart == "")
                {
                    sqlTimeStart = "";
                }
                else
                {
                    sqlTimeStart = " Where a.Date BETWEEN '" + TimeStart + "'";
                }

                if (Timeend == "")
                {
                    sqlTimeend = "";
                }
                else
                {
                    sqlTimeend = " and '" + Timeend + "'";
                }
            }
            else
            {
                count = count + 2;
            }

            if (count >= 2)
            {
                string name = Session["Username"].ToString();
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select * from RADAEmpire_ARoles where Active = '1' order by ID desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    if (dr["Username"].ToString() == name)
                    {
                        filtroAreas.Add(dr["Areas"].ToString());
                    }
                }
                DBSPP.Close();

                GetRecord(filtroAreas);
                ViewBag.Records = GetRecords;
                ViewBag.Count = GetRecords.Count.ToString();
                return View();
            }
            else
            {
                string name = Session["Username"].ToString();
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select * from RADAEmpire_ARoles where Active = '1' order by ID desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    if (dr["Username"].ToString() == name)
                    {
                        filtroAreas.Add(dr["Areas"].ToString());
                    }
                }
                DBSPP.Close();

                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "  Select top (100) " +
                    " a.Folio as Folio, a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                    " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date,a.shift as Area " +
                    " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio " + sqlTimeStart + sqlTimeend + " ORDER by a.Folio desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    string areaActual = dr["Area"].ToString();

                    if (filtroAreas.Contains(areaActual))
                    {
                        GetRecordsQeury.Add(new Historial()
                        {
                            Folio = (dr["Folio"].ToString()),
                            Container = (dr["Container"].ToString()),
                            Origen = (dr["Origen"].ToString()),
                            Destination = (dr["Destination"].ToString()),
                            Status = (dr["Status"].ToString()),
                            HSolicitud = (dr["HSolicitud"].ToString()),
                            HConfirm = (dr["HConfirm"].ToString()),
                            HFinish = (dr["HFinish"].ToString()),
                            WhoRequest = (dr["WhoRequest"].ToString()),
                            Choffer = (dr["Choffer"].ToString()),
                            Comment = (dr["Comment"].ToString()),
                            Date = Convert.ToDateTime(dr["Date"]).ToString("MM/dd/yyyy"),
                            Area = (dr["Area"].ToString()),
                        });
                    }
                }
                DBSPP.Close();

                ViewBag.Records = GetRecordsQeury;
                ViewBag.Count = GetRecordsQeury.Count.ToString();
                return View();
            }
        }

        public ActionResult Records()
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                string name = Session["Username"].ToString();

                string validation = null;
                //create generate randoms int value
                SqlCommand conse = new SqlCommand("Select Type_user from RADAEmpire_AUsers where Active = '1' and Username = '" + name + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drconse = conse.ExecuteReader();
                if (drconse.HasRows)
                {
                    while (drconse.Read())
                    {
                        validation = drconse["Type_user"].ToString();
                    }
                }
                DBSPP.Close();

                if (validation == "ADMINISTRATOR")
                {
                    GetRecord();
                    ViewBag.Records = GetRecords;
                    ViewBag.Count = GetRecords.Count.ToString();
                    return View();
                }
                else
                {
                    DBSPP.Open();
                    con.Connection = DBSPP;
                    con.CommandText = "Select * from RADAEmpire_ARoles where Active = '1' order by ID desc";
                    dr = con.ExecuteReader();
                    while (dr.Read())
                    {
                        if (dr["Username"].ToString() == name)
                        {
                            filtroAreas.Add(dr["Areas"].ToString());
                        }
                    }
                    DBSPP.Close();

                    GetRecord(filtroAreas);
                    ViewBag.Records = GetRecords;
                    ViewBag.Count = GetRecords.Count.ToString();
                    return View();
                }
            }
        }

        [HttpPost]
        public ActionResult Records(string Timeend, string TimeStart)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                string name = Session["Username"].ToString();

                int count = 0;
                string sqlTimeStart = null;
                string sqlTimeend = null;

                if (Timeend != "" && TimeStart != "")
                {
                    if (TimeStart == "")
                    {
                        sqlTimeStart = "";
                    }
                    else
                    {
                        sqlTimeStart = " Where a.Date BETWEEN '" + TimeStart + "'";
                    }

                    if (Timeend == "")
                    {
                        sqlTimeend = "";
                    }
                    else
                    {
                        sqlTimeend = " and '" + Timeend + "'";
                    }
                }
                else
                {
                    count = count + 2;
                }

                if (count >= 2)
                {
                    string validation = null;
                    //create generate randoms int value
                    SqlCommand conse = new SqlCommand("Select Type_user from RADAEmpire_AUsers where Active = '1' and Username = '" + name + "'", DBSPP);
                    DBSPP.Open();
                    SqlDataReader drconse = conse.ExecuteReader();
                    if (drconse.HasRows)
                    {
                        while (drconse.Read())
                        {
                            validation = drconse["Type_user"].ToString();
                        }
                    }
                    DBSPP.Close();

                    if (validation == "ADMINISTRATOR")
                    {
                        GetRecord();
                        ViewBag.Records = GetRecords;
                        ViewBag.Count = GetRecords.Count.ToString();
                        return View();
                    }
                    else
                    {
                        DBSPP.Open();
                        con.Connection = DBSPP;
                        con.CommandText = "Select * from RADAEmpire_ARoles where Active = '1' order by ID desc";
                        dr = con.ExecuteReader();
                        while (dr.Read())
                        {
                            if (dr["Username"].ToString() == name)
                            {
                                filtroAreas.Add(dr["Areas"].ToString());
                            }
                        }
                        DBSPP.Close();

                        GetRecord(filtroAreas);
                        ViewBag.Records = GetRecords;
                        ViewBag.Count = GetRecords.Count.ToString();
                        return View();
                    }
                }
                else
                {
                    string validation = null;
                    //create generate randoms int value
                    SqlCommand conse = new SqlCommand("Select Type_user from RADAEmpire_AUsers where Active = '1' and Username = '" + name + "'", DBSPP);
                    DBSPP.Open();
                    SqlDataReader drconse = conse.ExecuteReader();
                    if (drconse.HasRows)
                    {
                        while (drconse.Read())
                        {
                            validation = drconse["Type_user"].ToString();
                        }
                    }
                    DBSPP.Close();

                    if (validation == "ADMINISTRATOR")
                    {
                        GetRecord();
                        ViewBag.Records = GetRecords;
                        ViewBag.Count = GetRecords.Count.ToString();
                        return View();
                    }
                    else
                    {
                        DBSPP.Open();
                        con.Connection = DBSPP;
                        con.CommandText = "  Select top (100) " +
                            " a.Folio as Folio, a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                            " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date " +
                            " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio " + sqlTimeStart + sqlTimeend + " ORDER by a.Folio desc";
                        dr = con.ExecuteReader();
                        while (dr.Read())
                        {
                            GetRecordsQeury.Add(new Historial()
                            {
                                Folio = (dr["Folio"].ToString()),
                                Container = (dr["Container"].ToString()),
                                Origen = (dr["Origen"].ToString()),
                                Destination = (dr["Destination"].ToString()),
                                Status = (dr["Status"].ToString()),
                                HSolicitud = (dr["HSolicitud"].ToString()),
                                HConfirm = (dr["HConfirm"].ToString()),
                                HFinish = (dr["HFinish"].ToString()),
                                WhoRequest = (dr["WhoRequest"].ToString()),
                                Choffer = (dr["Choffer"].ToString()),
                                Comment = (dr["Comment"].ToString()),
                                Date = Convert.ToDateTime(dr["Date"]).ToString("MM/dd/yyyy"),
                            });
                        }
                        DBSPP.Close();

                        ViewBag.Records = GetRecordsQeury;
                        ViewBag.Count = GetRecordsQeury.Count.ToString();
                        return View();
                    }
                }
            }
        }

        public ActionResult CancelContainerR(string ID)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                string validation = null;
                //create generate randoms int value
                SqlCommand conse = new SqlCommand("Select ID from RADAEmpires_DRemoves where Active = '1' and Folio = '" + ID + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drconse = conse.ExecuteReader();
                if (drconse.HasRows)
                {
                    while (drconse.Read())
                    {
                        validation = drconse["ID"].ToString();
                    }
                }
                else
                {
                    validation = null;
                }
                DBSPP.Close();

                if (validation == null)
                {
                    ViewBag.ID = ID;
                    string TypeLog = null;
                    SqlCommand log = new SqlCommand("Select Type_user from RADAEmpire_AUsers where Active = '1' and Username = '" + Session["Username"].ToString() + "'", DBSPP);
                    DBSPP.Open();
                    SqlDataReader drlog = log.ExecuteReader();
                    if (drlog.HasRows)
                    {
                        while (drlog.Read())
                        {
                            TypeLog = drlog[0].ToString();
                        }
                    }
                    DBSPP.Close();

                    if (TypeLog == "HISENSE")
                    {
                        return RedirectToAction("CancelContainerH", "History", new { ID = ID });
                    }
                    else
                    {
                        return View();
                    }
                }
                else
                {
                    return RedirectToAction("Records", "History");
                }
            }
        }

        public ActionResult CancelContainerH(string ID)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                string validation = null;
                //create generate randoms int value
                SqlCommand conse = new SqlCommand("Select ID from RADAEmpires_DRemoves where Active = '1' and Folio = '" + ID + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drconse = conse.ExecuteReader();
                if (drconse.HasRows)
                {
                    while (drconse.Read())
                    {
                        validation = drconse["ID"].ToString();
                    }
                }
                else
                {
                    validation = null;
                }
                DBSPP.Close();

                if (validation == null)
                {
                    ViewBag.ID = ID;
                    string TypeLog = null;
                    SqlCommand log = new SqlCommand("Select Type_user from RADAEmpire_AUsers where Active = '1' and Username = '" + Session["Username"].ToString() + "'", DBSPP);
                    DBSPP.Open();
                    SqlDataReader drlog = log.ExecuteReader();
                    if (drlog.HasRows)
                    {
                        while (drlog.Read())
                        {
                            TypeLog = drlog[0].ToString();
                        }
                    }
                    DBSPP.Close();

                    if (TypeLog == "HISENSE")
                    {
                        return View();
                    }
                    else
                    {
                        return RedirectToAction("CancelContainerR", "History", new { ID = ID });
                    }
                }
                else
                {
                    return RedirectToAction("Records", "History");
                }
            }
        }

        public ActionResult Delete(string ID, string Reason, string Company)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                if (Company == "RADA")
                {
                    // Obtener la fecha y hora actual en Alemania (zona horaria UTC+1 o UTC+2 dependiendo del horario de verano)
                    DateTime germanTime = DateTime.UtcNow.AddHours(0);  // Alemania es UTC+1

                    // Convertir la hora alemana a la hora en una zona horaria específica de EE. UU. (por ejemplo, Nueva York, UTC-5)
                    TimeZoneInfo usEasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                    DateTime usTime = TimeZoneInfo.ConvertTime(germanTime, usEasternTimeZone);

                    // Formatear la fecha para que sea adecuada para la base de datos
                    string formattedDate = usTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    DBSPP.Open();
                    SqlCommand PalletControl = new SqlCommand("insert into RADAEmpires_DRemoves" +
                        "(Folio, Reason, Datetime, Active,Company) values " +
                        "(@Folio, @Reason, @Datetime, @Active,@Company) ", DBSPP);
                    //--------------------------------------------------------------------------------------------------------------------------------
                    PalletControl.Parameters.AddWithValue("@Folio", ID.ToString());
                    PalletControl.Parameters.AddWithValue("@Reason", Reason.ToString());
                    PalletControl.Parameters.AddWithValue("@Datetime", usTime.ToString());
                    PalletControl.Parameters.AddWithValue("@Active", true);
                    PalletControl.Parameters.AddWithValue("@Company", "RADA");
                    PalletControl.ExecuteNonQuery();
                    DBSPP.Close();

                    //query message ------------------------------------------------------------------------------------------------------------------
                    string updateQuery = "UPDATE RADAEmpire_BRequestContainers SET message = @message WHERE Folio = @ID";
                    using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                    {
                        DBSPP.Open();
                        command.Parameters.AddWithValue("@message", "Canceled by Rada");
                        command.Parameters.AddWithValue("@ID", ID);
                        int rowsAffected = command.ExecuteNonQuery();
                        DBSPP.Close();
                    }

                    //query message
                    string updateQuer3y = "UPDATE RADAEmpire_CEntryContrainers SET Cancel = @Cancel WHERE Folio_Request = @ID";
                    using (SqlCommand comdmand = new SqlCommand(updateQuer3y, DBSPP))
                    {
                        DBSPP.Open();
                        comdmand.Parameters.AddWithValue("@ID", ID);
                        comdmand.Parameters.AddWithValue("@Cancel", true);
                        int rowsAffected = comdmand.ExecuteNonQuery();
                        DBSPP.Close();
                    }

                    string Chofferss = null;
                    SqlCommand log = new SqlCommand("Select Choffer from RADAEmpires_DZChofferMovement where Active = '1' and Foio = '" + ID.ToString() + "'", DBSPP);
                    DBSPP.Open();
                    SqlDataReader drlog = log.ExecuteReader();
                    if (drlog.HasRows)
                    {
                        while (drlog.Read())
                        {
                            Chofferss = drlog["Choffer"].ToString();
                        }
                    }
                    DBSPP.Close();

                    //------------------------------------------------------------------------------
                    string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
                        " Status = @Status WHERE Username = @Username";
                    using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
                    {
                        DBSPP.Open();
                        coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                        coms.Parameters.AddWithValue("@Username", Chofferss.ToString());
                        int rowsAffected = coms.ExecuteNonQuery();
                        DBSPP.Close();
                    }

                    //------------------------------------------------------------------------------
                    string inactive = "UPDATE RADAEmpires_DZChofferMovement SET " +
                        " Active = @Active WHERE Foio = @Foio";
                    using (SqlCommand coms = new SqlCommand(inactive, DBSPP))
                    {
                        DBSPP.Open();
                        coms.Parameters.AddWithValue("@Active", false);
                        coms.Parameters.AddWithValue("@Foio", ID.ToString());
                        int rowsAffected = coms.ExecuteNonQuery();
                        DBSPP.Close();
                    }

                    return RedirectToAction("Records", "History");
                }
                else
                {
                    // Obtener la fecha y hora actual en Alemania (zona horaria UTC+1 o UTC+2 dependiendo del horario de verano)
                    DateTime germanTime = DateTime.UtcNow.AddHours(0);  // Alemania es UTC+1

                    // Convertir la hora alemana a la hora en una zona horaria específica de EE. UU. (por ejemplo, Nueva York, UTC-5)
                    TimeZoneInfo usEasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                    DateTime usTime = TimeZoneInfo.ConvertTime(germanTime, usEasternTimeZone);

                    // Formatear la fecha para que sea adecuada para la base de datos
                    string formattedDate = usTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                    DBSPP.Open();
                    SqlCommand PalletControl = new SqlCommand("insert into RADAEmpires_DRemoves" +
                        "(Folio, Reason, Datetime, Active,Company) values " +
                        "(@Folio, @Reason, @Datetime, @Active,@Company) ", DBSPP);
                    //--------------------------------------------------------------------------------------------------------------------------------
                    PalletControl.Parameters.AddWithValue("@Folio", ID.ToString());
                    PalletControl.Parameters.AddWithValue("@Reason", Reason.ToString());
                    PalletControl.Parameters.AddWithValue("@Datetime", usTime.ToString());
                    PalletControl.Parameters.AddWithValue("@Active", true);
                    PalletControl.Parameters.AddWithValue("@Company", "HISENSE");
                    PalletControl.ExecuteNonQuery();
                    DBSPP.Close();

                    //query message ------------------------------------------------------------------------------------------------------------------
                    string updateQuery = "UPDATE RADAEmpire_BRequestContainers SET message = @message WHERE Folio = @ID";
                    using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                    {
                        DBSPP.Open();
                        command.Parameters.AddWithValue("@message", "Canceled by Hisense");
                        command.Parameters.AddWithValue("@ID", ID);
                        int rowsAffected = command.ExecuteNonQuery();
                        DBSPP.Close();
                    }

                    //query message
                    string updateQuer3y = "UPDATE RADAEmpire_CEntryContrainers SET Cancel = @Cancel WHERE Folio_Request = @ID";
                    using (SqlCommand comdmand = new SqlCommand(updateQuer3y, DBSPP))
                    {
                        DBSPP.Open();
                        comdmand.Parameters.AddWithValue("@ID", ID);
                        comdmand.Parameters.AddWithValue("@Cancel", true);
                        int rowsAffected = comdmand.ExecuteNonQuery();
                        DBSPP.Close();
                    }
                    return RedirectToAction("Records", "History");
                }
            }
        }

        private void GetRecord(List<string> filtroAreas)
        {
            if (GetRecords.Count > 0)
            {
                GetRecords.Clear();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "  Select top (100) " +
                    " b.FastCard as FastCard, a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                    " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date,a.shift as Area  " +
                    " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio ORDER by a.Folio desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    string areaActual = dr["Area"].ToString();

                    if (filtroAreas.Contains(areaActual))
                    {
                        GetRecords.Add(new Historial()
                        {
                            Folio = (dr["Folio"].ToString()),
                            Container = (dr["Container"].ToString()),
                            Origen = (dr["Origen"].ToString()),
                            Destination = (dr["Destination"].ToString()),
                            Status = (dr["Status"].ToString()),
                            HSolicitud = (dr["HSolicitud"].ToString()),
                            HConfirm = (dr["HConfirm"].ToString()),
                            HFinish = (dr["HFinish"].ToString()),
                            WhoRequest = (dr["WhoRequest"].ToString()),
                            Choffer = dr["Choffer"].ToString(),
                            fastcard = dr["FastCard"].ToString(),
                            Comment = (dr["Comment"].ToString()),
                            Date = Convert.ToDateTime(dr["Date"]).ToString("MM/dd/yyyy"),
                            Area = (dr["Area"].ToString()),
                        });
                    }
                }
            DBSPP.Close();
            }
        }

        private void GetInventory()
        {
            if (GetInventary.Count > 0)
            {
                GetInventary.Clear();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select top (100) * from RADAEmpire_CInventoryControl where Active = '1' order by ID desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetInventary.Add(new Inventario()
                    {
                        Username = (dr["Username"].ToString()),
                        Container = (dr["Container"].ToString()),
                        LocationCode = (dr["LocationCode"].ToString()),
                        Status = (dr["Status"].ToString()),
                        Date = Convert.ToDateTime(dr["Datetime"]),
                    });
                }
                DBSPP.Close();
            }
        }

        private void GetRecordNows()
        {
            if (GetRecordsNow.Count > 0)
            {
                GetRecordsNow.Clear();
            }
            else
            {
                // Obtener la fecha y hora actual en Alemania (zona horaria UTC+1 o UTC+2 dependiendo del horario de verano)
                DateTime germanTime = DateTime.UtcNow.AddHours(0);  // Alemania es UTC+1

                // Convertir la hora alemana a la hora en una zona horaria específica de EE. UU. (por ejemplo, Nueva York, UTC-5)
                TimeZoneInfo usEasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                DateTime usTime = TimeZoneInfo.ConvertTime(germanTime, usEasternTimeZone);

                // Formatear la fecha para que sea adecuada para la base de datos
                string formattedDate = usTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "  Select top (100) " +
                    " b.FastCard as FastCard, a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                    " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date,a.shift as Area  " +
                    " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio where a.Date = '" + usTime + "' ORDER by a.Folio desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetRecordsNow.Add(new Historial()
                    {
                        Folio = (dr["Folio"].ToString()),
                        Container = (dr["Container"].ToString()),
                        Origen = (dr["Origen"].ToString()),
                        Destination = (dr["Destination"].ToString()),
                        Status = (dr["Status"].ToString()),
                        HSolicitud = (dr["HSolicitud"].ToString()),
                        HConfirm = (dr["HConfirm"].ToString()),
                        HFinish = (dr["HFinish"].ToString()),
                        WhoRequest = (dr["WhoRequest"].ToString()),
                        Choffer = dr["Choffer"].ToString(),
                        fastcard = dr["FastCard"].ToString(),
                        Comment = (dr["Comment"].ToString()),
                        Date = Convert.ToDateTime(dr["Date"]).ToString("MM/dd/yyyy"),
                        Area = (dr["Area"].ToString()),
                    });
                }
                DBSPP.Close();
            }
        }

        private void GetRecord()
        {
            if (GetRecords.Count > 0)
            {
                GetRecords.Clear();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "  Select top (100) " +
                    " b.FastCard as FastCard, a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                    " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date,a.shift as Area  " +
                    " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio ORDER by a.Folio desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetRecords.Add(new Historial()
                    {
                        Folio = (dr["Folio"].ToString()),
                        Container = (dr["Container"].ToString()),
                        Origen = (dr["Origen"].ToString()),
                        Destination = (dr["Destination"].ToString()),
                        Status = (dr["Status"].ToString()),
                        HSolicitud = (dr["HSolicitud"].ToString()),
                        HConfirm = (dr["HConfirm"].ToString()),
                        HFinish = (dr["HFinish"].ToString()),
                        WhoRequest = (dr["WhoRequest"].ToString()),
                        Choffer = dr["Choffer"].ToString(),
                        fastcard = dr["FastCard"].ToString(),
                        Comment = (dr["Comment"].ToString()),
                        Date = Convert.ToDateTime(dr["Date"]).ToString("MM/dd/yyyy"),
                        Area = (dr["Area"].ToString()),
                    });
                }
                DBSPP.Close();
            }
        }
    }
}