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

        List<Historial> GetRecordsDespachador = new List<Historial>();

        List<Historial> GetRecordsDespachadorExcle2 = new List<Historial>();

        List<Historial> GetRecordsExcel = new List<Historial>();

        List<Inventario> GetInventary = new List<Inventario>();

        List<Inventario> GetInventaryquery = new List<Inventario>();

        List<Historial> GetRecordsQeury = new List<Historial>();

        List<Historial> GetRecordsNow = new List<Historial>();

        List<Usuarios> GetChoffer = new List<Usuarios>();

        List<string> filtroAreas = new List<string>();

        List<UsuarioRada> UsuarioRadasss = new List<UsuarioRada>();

        //connection SQL server (database)
        SqlConnection DBSPP = new SqlConnection("Data Source=RADAEmpire.mssql.somee.com ;Initial Catalog=RADAEmpire ;User ID=RooRada; password=rada1311");
        SqlCommand con = new SqlCommand();
        SqlDataReader dr;
        // GET: History

        public ActionResult ChangesProcess(string ID, string ChofferOld, string Choffers)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] == null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            if (Session["Type"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Type"] = Request.Cookies["UserCookie"].Value;
            }

            ViewBag.User = Session["Username"];
            ViewBag.Type = Session["Type"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                //create generate randoms int value
                string username = null, fastcard = null;
                SqlCommand asiggne = new SqlCommand("Select * from RADAEmpire_AChoffer where Active = '1' and Username = '" + Choffers + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drasiggne = asiggne.ExecuteReader();
                if (drasiggne.HasRows)
                {
                    while (drasiggne.Read())
                    {
                        username = drasiggne["Username"].ToString();
                        fastcard = drasiggne["Fastcard"].ToString();
                    }
                }
                DBSPP.Close();

                string updateQuery = "UPDATE RADAEmpire_CEntryContrainers SET Choffer = @Choffer, FastCard = @FastCard  WHERE Folio_Request = @ID";
                using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                {
                    DBSPP.Open();
                    command.Parameters.AddWithValue("@Choffer", username.ToString());
                    command.Parameters.AddWithValue("@FastCard", fastcard.ToString());
                    command.Parameters.AddWithValue("@ID", ID);
                    int rowsAffected = command.ExecuteNonQuery();
                    DBSPP.Close();
                }

                string QUERY2 = "UPDATE RADAEmpires_DZChofferMovement SET Choffer = @Choffer,Message = @Message WHERE Foio = @ID";
                using (SqlCommand drqeuery = new SqlCommand(QUERY2, DBSPP))
                {
                    DBSPP.Open();
                    drqeuery.Parameters.AddWithValue("@Choffer", username.ToString());
                    drqeuery.Parameters.AddWithValue("@Message", "EL CHOFER " +  username.ToString() + " ESTA EN MOVIMIENTO");
                    drqeuery.Parameters.AddWithValue("@ID", ID);
                    int rowsAffected = drqeuery.ExecuteNonQuery();
                    DBSPP.Close();
                }

                string StatusChoffer = "UPDATE RADAEmpire_AChoffer SET Status = @Status WHERE Username = @ID";
                using (SqlCommand drqeuery = new SqlCommand(StatusChoffer, DBSPP))
                {
                    DBSPP.Open();
                    drqeuery.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                    drqeuery.Parameters.AddWithValue("@ID", ChofferOld.ToString());
                    int rowsAffected = drqeuery.ExecuteNonQuery();
                    DBSPP.Close();
                }

                string StatusChoffer2 = "UPDATE RADAEmpire_AChoffer SET Status = @Status WHERE Username = @ID";
                using (SqlCommand drqeuery = new SqlCommand(StatusChoffer2, DBSPP))
                {
                    DBSPP.Open();
                    drqeuery.Parameters.AddWithValue("@Status", "CHOFFER EN MOVIMIENTO");
                    drqeuery.Parameters.AddWithValue("@ID", username.ToString());
                    int rowsAffected = drqeuery.ExecuteNonQuery();
                    DBSPP.Close();
                }

                return RedirectToAction("EntryContainer", "RADA");
            }
        }

        public ActionResult ChangesCHR(string ID)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] == null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            if (Session["Type"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Type"] = Request.Cookies["UserCookie"].Value;
            }

            ViewBag.User = Session["Username"];
            ViewBag.Type = Session["Type"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                string UsuarioRada = Session["Username"]?.ToString();
                //create generate randoms int value
                string rol = null;
                SqlCommand asiggne = new SqlCommand("Select Type_user from RADAEmpire_AUsers where Active = '1' and Username = '" + UsuarioRada.ToString() + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drasiggne = asiggne.ExecuteReader();
                if (drasiggne.HasRows)
                {
                    while (drasiggne.Read())
                    {
                        rol = drasiggne["Type_user"].ToString();
                    }
                }
                DBSPP.Close();

                if (rol == "ADMINISTRATOR")
                {
                    GetUsers();
                    ViewBag.RadaUsers = GetChoffer;
                }
                else
                {
                    List<string> Areas = new List<string>();

                    // Paso 1: Obtener áreas del usuario
                    string queryAreas = "SELECT Areas FROM RADAEmpire_ARoles WHERE Active = '1' AND Username = @username";
                    using (SqlCommand RadaAreas = new SqlCommand(queryAreas, DBSPP))
                    {
                        RadaAreas.Parameters.AddWithValue("@username", UsuarioRada);
                        DBSPP.Open();
                        using (SqlDataReader drRadaAreas = RadaAreas.ExecuteReader())
                        {
                            while (drRadaAreas.Read())
                            {
                                Areas.Add(drRadaAreas["Areas"].ToString());
                            }
                        }
                        DBSPP.Close();
                    }

                    // Paso 2: Obtener usuarios por áreas
                    List<UsuarioRada> usuarios = new List<UsuarioRada>();

                    if (Areas.Any())
                    {
                        string queryUsers = "SELECT DISTINCT   r.Username,  r.Areas,  a.Status,  a.Fastcard, a.Shift, a.Date, a.Datetime " +
                            " FROM RADAEmpire_ARolesChoffer r LEFT JOIN RADAEmpire_AChoffer a  ON r.Username = a.Username AND a.Active = '1' " +
                            " WHERE r.Active = '1' AND r.Areas IN ({0}) AND (a.Status = 'SIN MOVIMIENTO' OR a.Status IS NULL)" +
                            " ORDER BY r.Username ASC";

                        List<string> parametros = new List<string>();
                        for (int i = 0; i < Areas.Count; i++)
                        {
                            parametros.Add("@area" + i);
                        }

                        string inClause = string.Join(", ", parametros);
                        queryUsers = string.Format(queryUsers, inClause);

                        using (SqlCommand cmd = new SqlCommand(queryUsers, DBSPP))
                        {
                            for (int i = 0; i < Areas.Count; i++)
                            {
                                cmd.Parameters.AddWithValue(parametros[i], Areas[i]);
                            }

                            DBSPP.Open();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    usuarios.Add(new UsuarioRada { Username = reader["Username"].ToString() });
                                }
                            }
                            DBSPP.Close();
                        }
                    }

                    ViewBag.RadaUsers = usuarios;
                }

                string Choffer = null, fastcard = null;
                //create generate randoms int value
                SqlCommand conse = new SqlCommand("Select * from RADAEmpire_CEntryContrainers where Active = '1' and Folio_Request = '" + ID.ToString() + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drconse = conse.ExecuteReader();
                if (drconse.HasRows)
                {
                    while (drconse.Read())
                    {
                        Choffer = drconse["Choffer"].ToString();
                        fastcard = drconse["FastCard"].ToString();
                    }
                }
                DBSPP.Close();

                ViewBag.Choffer = Choffer.ToString();
                ViewBag.ID = ID;
                return View();
            }
        }

        [HttpPost]
        public ActionResult Dashboard(string TimeStart)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] == null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            if (Session["Type"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Type"] = Request.Cookies["UserCookie"].Value;
            }


            ViewBag.User = Session["Username"];
            ViewBag.Type = Session["Type"];

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
                    con.CommandText = "  Select " +
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
            if (Session["Username"] == null && Request.Cookies["UserCookie"] == null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            if (Session["Type"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Type"] = Request.Cookies["UserCookie"].Value;
            }


            ViewBag.User = Session["Username"];
            ViewBag.Type = Session["Type"];

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
            if (Session["Username"] == null && Request.Cookies["UserCookie"] == null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            if (Session["Type"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Type"] = Request.Cookies["UserCookie"].Value;
            }


            ViewBag.User = Session["Username"];
            ViewBag.Type = Session["Type"];

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
            if (Session["Username"] == null && Request.Cookies["UserCookie"] == null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            if (Session["Type"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Type"] = Request.Cookies["UserCookie"].Value;
            }


            ViewBag.User = Session["Username"];
            ViewBag.Type = Session["Type"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
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
                    con.CommandText = "Select * from RADAEmpire_CInventoryControl where Active = '1' " + sqlTimeStart + sqlTimeend + " order by ID desc";
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
                con.CommandText = "Select top (1000) * from RADAEmpire_CInventoryControl where Active = '1' order by ID desc";
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
            if (Session["Username"] == null && Request.Cookies["UserCookie"] == null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            if (Session["Type"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Type"] = Request.Cookies["UserCookie"].Value;
            }

            ViewBag.User = Session["Username"];
            ViewBag.Type = Session["Type"];

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

                GetRecordHisense(filtroAreas);
                ViewBag.Records = GetRecords;
                ViewBag.Count = GetRecords.Count.ToString();
                return View();
            }
        }

        [HttpPost]
        public ActionResult HRecord(string Timeend, string TimeStart)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] == null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            if (Session["Type"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Type"] = Request.Cookies["UserCookie"].Value;
            }


            ViewBag.User = Session["Username"];
            ViewBag.Type = Session["Type"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
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

                    GetRecordHisense(filtroAreas);
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
                    con.CommandText = "  Select " +
                        " a.Urgencia AS Urgencia, b.FastCard as FastCard, a.Folio as Folio, a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                        " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date, a.shift as Area " +
                        " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio " + sqlTimeStart + sqlTimeend + " ORDER by a.Folio desc";
                    dr = con.ExecuteReader();
                    while (dr.Read())
                    {
                        string areaActual = dr["Area"].ToString();

                        if (filtroAreas.Contains(areaActual))
                        {
                            GetRecordsQeury.Add(new Historial()
                            {
                                Urgencia = (dr["Urgencia"].ToString()),
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
                                Area = (dr["Area"].ToString()),
                                Date = Convert.ToDateTime(dr["Date"]).ToString("MM/dd/yyyy"),
                            });
                        }
                    }
                    DBSPP.Close();

                    ViewBag.Records = GetRecordsQeury;
                    ViewBag.Count = GetRecordsQeury.Count.ToString();
                    return View();
                }
            }
        }

        //public ActionResult Record()
        //{
        //    if (Session["Username"] == null && Request.Cookies["UserCookie"] == null)
        //    {
        //        Session["Username"] = Request.Cookies["UserCookie"].Value;
        //    }

        //    if (Session["Type"] == null && Request.Cookies["UserCookie"] != null)
        //    {
        //        Session["Type"] = Request.Cookies["UserCookie"].Value;
        //    }
        //    ViewBag.User = Session["Username"];
        //    ViewBag.Type = Session["Type"];

        //    if (Session.Count <= 0)
        //    {
        //        return RedirectToAction("LogIn", "Login");
        //    }
        //    else
        //    {
        //        string name = Session["Username"].ToString();
        //        DBSPP.Open();
        //        con.Connection = DBSPP;
        //        con.CommandText = "Select * from RADAEmpire_ARoles where Active = '1' order by ID desc";
        //        dr = con.ExecuteReader();
        //        while (dr.Read())
        //        {
        //            if (dr["Username"].ToString() == name)
        //            {
        //                filtroAreas.Add(dr["Areas"].ToString());
        //            }
        //        }
        //        DBSPP.Close();

        //        //GetRecord(filtroAreas);
        //        //ViewBag.Records = GetRecords;
        //        //ViewBag.Count = GetRecords.Count.ToString();
        //        //return View();
        //    }
        //}

        //[HttpPost]
        //public ActionResult Record(string Timeend, string TimeStart)
        //{
        //    if (Session["Username"] == null && Request.Cookies["UserCookie"] == null)
        //    {
        //        Session["Username"] = Request.Cookies["UserCookie"].Value;
        //    }

        //    if (Session["Type"] == null && Request.Cookies["UserCookie"] != null)
        //    {
        //        Session["Type"] = Request.Cookies["UserCookie"].Value;
        //    }


        //    ViewBag.User = Session["Username"];
        //    ViewBag.Type = Session["Type"];

        //    if (Session.Count <= 0)
        //    {
        //        return RedirectToAction("LogIn", "Login");
        //    }
        //    else
        //    {

        //        int count = 0;
        //        string sqlTimeStart = null;
        //        string sqlTimeend = null;

        //        if (Timeend != "" && TimeStart != "")
        //        {
        //            if (TimeStart == "")
        //            {
        //                sqlTimeStart = "";
        //            }
        //            else
        //            {
        //                sqlTimeStart = " Where a.Date BETWEEN '" + TimeStart + "'";
        //            }

        //            if (Timeend == "")
        //            {
        //                sqlTimeend = "";
        //            }
        //            else
        //            {
        //                sqlTimeend = " and '" + Timeend + "' and a.Active = '1'";
        //            }
        //        }
        //        else
        //        {
        //            count = count + 2;
        //        }

        //        if (count >= 2)
        //        {
        //            string name = Session["Username"].ToString();
        //            DBSPP.Open();
        //            con.Connection = DBSPP;
        //            con.CommandText = "Select * from RADAEmpire_ARoles where Active = '1' order by ID desc";
        //            dr = con.ExecuteReader();
        //            while (dr.Read())
        //            {
        //                if (dr["Username"].ToString() == name)
        //                {
        //                    filtroAreas.Add(dr["Areas"].ToString());
        //                }
        //            }
        //            DBSPP.Close();

        //            GetRecordss(filtroAreas);
        //            ViewBag.Records = GetRecords;
        //            ViewBag.Count = GetRecords.Count.ToString();
        //            return View();
        //        }
        //        else
        //        {
        //            string name = Session["Username"].ToString();
        //            DBSPP.Open();
        //            con.Connection = DBSPP;
        //            con.CommandText = "Select * from RADAEmpire_ARoles where Active = '1' order by ID desc";
        //            dr = con.ExecuteReader();
        //            while (dr.Read())
        //            {
        //                if (dr["Username"].ToString() == name)
        //                {
        //                    filtroAreas.Add(dr["Areas"].ToString());
        //                }
        //            }
        //            DBSPP.Close();

        //            DBSPP.Open();
        //            con.Connection = DBSPP;
        //            con.CommandText = "  Select " +
        //                " a.Folio as Folio, a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
        //                " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date,a.shift as Area " +
        //                " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio " + sqlTimeStart + sqlTimeend + " ORDER by a.Folio desc";
        //            dr = con.ExecuteReader();
        //            while (dr.Read())
        //            {
        //                string areaActual = dr["Area"].ToString();

        //                if (filtroAreas.Contains(areaActual))
        //                {
        //                    GetRecordsQeury.Add(new Historial()
        //                    {
        //                        Folio = (dr["Folio"].ToString()),
        //                        Container = (dr["Container"].ToString()),
        //                        Origen = (dr["Origen"].ToString()),
        //                        Destination = (dr["Destination"].ToString()),
        //                        Status = (dr["Status"].ToString()),
        //                        HSolicitud = (dr["HSolicitud"].ToString()),
        //                        HConfirm = (dr["HConfirm"].ToString()),
        //                        HFinish = (dr["HFinish"].ToString()),
        //                        WhoRequest = (dr["WhoRequest"].ToString()),
        //                        Choffer = (dr["Choffer"].ToString()),
        //                        Comment = (dr["Comment"].ToString()),
        //                        Date = Convert.ToDateTime(dr["Date"]).ToString("MM/dd/yyyy"),
        //                        Area = (dr["Area"].ToString()),
        //                    });
        //                }
        //            }
        //            DBSPP.Close();

        //            ViewBag.Records = GetRecordsQeury;
        //            ViewBag.Count = GetRecordsQeury.Count.ToString();
        //            return View();
        //        }
        //    }
        //}

        public ActionResult Records()
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] == null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            if (Session["Type"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Type"] = Request.Cookies["UserCookie"].Value;
            }


            ViewBag.User = Session["Username"];
            ViewBag.Type = Session["Type"];

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

                    GetRecordexcel();
                    ViewBag.RecordExcel = GetRecordsExcel;

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

                    GetRecordss(filtroAreas);
                    ViewBag.Records = GetRecordsDespachador;
                    ViewBag.Count = GetRecordsDespachador.Count.ToString();

                    GetRecordExcel2(filtroAreas);
                    ViewBag.RecordExcel = GetRecordsDespachadorExcle2;
                    return View();
                }
            }
        }

        [HttpPost]
        public ActionResult Records(string Timeend, string TimeStart)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] == null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            if (Session["Type"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Type"] = Request.Cookies["UserCookie"].Value;
            }


            ViewBag.User = Session["Username"];
            ViewBag.Type = Session["Type"];

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
                        sqlTimeend = " and '" + Timeend + "' and a.Active = '1'";
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
                        //GetRecord();
                        //ViewBag.Records = GetRecords;
                        //ViewBag.Count = GetRecords.Count.ToString();

                        //GetRecordexcel();
                        //ViewBag.RecordExcel = GetRecordsExcel;
                        //return View();

                        GetRecord();
                        ViewBag.Records = GetRecords;
                        ViewBag.Count = GetRecords.Count.ToString();

                        GetRecordexcel();
                        ViewBag.RecordExcel = GetRecordsExcel;
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

                        GetRecordss(filtroAreas);
                        ViewBag.Records = GetRecordsDespachador;
                        ViewBag.Count = GetRecordsDespachador.Count.ToString();

                        GetRecordExcel2(filtroAreas);
                        ViewBag.RecordExcel = GetRecordsDespachadorExcle2;
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
                        DBSPP.Open();
                        con.Connection = DBSPP;
                        con.CommandText = @"
WITH EtapasNumeradas AS (
    SELECT
        dz.Folio,
        ROW_NUMBER() OVER (PARTITION BY dz.Folio ORDER BY TRY_CAST(dz.Date_Process AS datetime)) AS EtapaNum,
        CONCAT(
            '[', dz.Type_StatusContainer, '] ',
            'Status: ', dz.Status, ', ',
            'Comentario: ', dz.Comment, ', ',
            'Grúa: ', dz.GruaMov, ', ',
            'Proceso: ', dz.Process_Movement, ', ',
            'Fecha: ', FORMAT(TRY_CAST(dz.Date_Process AS date), 'yyyy-MM-dd'), ', ',
            'Hora: ', CONVERT(varchar, dz.End_date, 108)
        ) AS DetalleEtapa
    FROM RADAEmpires_DZDetailsHisense dz
    WHERE dz.Activo = 1
)

SELECT
    a.Urgencia,
    b.FastCard,
    a.Folio,
    a.Container,
    a.Origins_Location AS Origen,
    a.Destination_Location AS Destination,
    a.Status,
    a.Datetime AS HSolicitud,
    b.Time_Confirm AS HConfirm,
    b.Time_Finished AS HFinish,
    a.Who_Send AS WhoRequest,
    b.Choffer,
    a.Message AS Comment,
    a.Date,
    a.Shift AS Area,

    ISNULL(MAX(CASE WHEN e.EtapaNum = 1 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_1,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 2 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_2,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 3 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_3,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 4 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_4,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 5 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_5,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 6 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_6,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 7 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_7,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 8 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_8,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 9 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_9,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 10 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_10,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 11 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_11,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 12 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_12,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 13 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_13,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 14 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_14,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 15 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_15,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 16 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_16,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 17 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_17,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 18 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_18,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 19 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_19,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 20 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_20

FROM RADAEmpire_BRequestContainers a
INNER JOIN RADAEmpire_CEntryContrainers b ON b.Folio_Request = a.Folio
LEFT JOIN EtapasNumeradas e ON e.Folio = a.Folio
WHERE a.Active = '1'
  AND a.Date BETWEEN @FechaInicio AND @FechaFin
GROUP BY
    a.Urgencia,
    b.FastCard,
    a.Folio,
    a.Container,
    a.Origins_Location,
    a.Destination_Location,
    a.Status,
    a.Datetime,
    b.Time_Confirm,
    b.Time_Finished,
    a.Who_Send,
    b.Choffer,
    a.Message,
    a.Date,
    a.Shift
ORDER BY a.Folio DESC;
";

                        // Agrega los parámetros de fecha
                        con.Parameters.Clear();
                        con.Parameters.AddWithValue("@FechaInicio", TimeStart);
                        con.Parameters.AddWithValue("@FechaFin", Timeend);

                        dr = con.ExecuteReader();
                        while (dr.Read())
                        {
                            GetRecordsExcel.Add(new Historial()
                            {
                                Urgencia = dr["Urgencia"].ToString(),
                                Folio = dr["Folio"].ToString(),
                                Container = dr["Container"].ToString(),
                                Origen = dr["Origen"].ToString(),
                                Destination = dr["Destination"].ToString(),
                                Status = dr["Status"].ToString(),
                                HSolicitud = dr["HSolicitud"].ToString(),
                                HConfirm = dr["HConfirm"].ToString(),
                                HFinish = dr["HFinish"].ToString(),
                                WhoRequest = dr["WhoRequest"].ToString(),
                                Choffer = dr["Choffer"].ToString(),
                                fastcard = dr["FastCard"].ToString(),
                                Comment = dr["Comment"].ToString(),
                                Date = Convert.ToDateTime(dr["Date"]).ToString("MM/dd/yyyy"),
                                Area = dr["Area"].ToString(),

                                Etapa_1 = dr["Etapa_1"].ToString(),
                                Etapa_2 = dr["Etapa_2"].ToString(),
                                Etapa_3 = dr["Etapa_3"].ToString(),
                                Etapa_4 = dr["Etapa_4"].ToString(),
                                Etapa_5 = dr["Etapa_5"].ToString(),
                                Etapa_6 = dr["Etapa_6"].ToString(),
                                Etapa_7 = dr["Etapa_7"].ToString(),
                                Etapa_8 = dr["Etapa_8"].ToString(),
                                Etapa_9 = dr["Etapa_9"].ToString(),
                                Etapa_10 = dr["Etapa_10"].ToString(),
                                Etapa_11 = dr["Etapa_11"].ToString(),
                                Etapa_12 = dr["Etapa_12"].ToString(),
                                Etapa_13 = dr["Etapa_13"].ToString(),
                                Etapa_14 = dr["Etapa_14"].ToString(),
                                Etapa_15 = dr["Etapa_15"].ToString(),
                                Etapa_16 = dr["Etapa_16"].ToString(),
                                Etapa_17 = dr["Etapa_17"].ToString(),
                                Etapa_18 = dr["Etapa_18"].ToString(),
                                Etapa_19 = dr["Etapa_19"].ToString(),
                                Etapa_20 = dr["Etapa_20"].ToString()
                            });
                        }
                        DBSPP.Close();


                        DBSPP.Open();
                        con.Connection = DBSPP;
                        con.CommandText = "  Select " +
                            " a.Urgencia AS Urgencia, a.Folio as Folio, a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                            " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date,a.shift as Area " +
                            " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio " + sqlTimeStart + sqlTimeend + " ORDER by a.Folio desc";
                        dr = con.ExecuteReader();
                        while (dr.Read())
                        {
                            GetRecordsQeury.Add(new Historial()
                            {
                                Urgencia = (dr["Urgencia"].ToString()),
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
                        DBSPP.Close();

                        ViewBag.Records = GetRecordsQeury;
                        ViewBag.Count = GetRecordsQeury.Count.ToString();

                        ViewBag.RecordExcel = GetRecordsExcel;
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

                        if (GetRecordsDespachador.Count > 0)
                        {
                            GetRecordsDespachador.Clear();
                        }
                        else
                        {
                            DBSPP.Open();
                            con.Connection = DBSPP;
                            con.CommandText = "Select top (500) " +
                               " a.Urgencia AS Urgencia, b.FastCard as FastCard, a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                               " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date,a.shift as Area  " +
                               " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio and a.Active = '1'  " + sqlTimeStart + sqlTimeend + "  ORDER by a.Folio desc"; 
                            dr = con.ExecuteReader();
                            while (dr.Read())
                            {
                                string areaActual = dr["Area"].ToString();

                                if (filtroAreas.Contains(areaActual))
                                {
                                    GetRecordsDespachador.Add(new Historial()
                                    {
                                        Urgencia = (dr["Urgencia"].ToString()),
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

                        ViewBag.Records = GetRecordsDespachador;
                        ViewBag.Count = GetRecordsDespachador.Count.ToString();

                        if (GetRecordsDespachadorExcle2.Count > 0)
                        {
                            GetRecordsDespachadorExcle2.Clear();
                        }
                        else
                        {
                            DBSPP.Open();
                            con.Connection = DBSPP;
                            con.CommandText = @"WITH EtapasNumeradas AS (
    SELECT
        dz.Folio,
        ROW_NUMBER() OVER (PARTITION BY dz.Folio ORDER BY TRY_CAST(dz.Date_Process AS datetime)) AS EtapaNum,
        CONCAT(
            '[', dz.Type_StatusContainer, '] ',
            'Status: ', dz.Status, ', ',
            'Comentario: ', dz.Comment, ', ',
            'Grúa: ', dz.GruaMov, ', ',
            'Proceso: ', dz.Process_Movement, ', ',
            'Fecha: ', FORMAT(TRY_CAST(dz.Date_Process AS date), 'yyyy-MM-dd'), ', ',
            'Hora: ', CONVERT(varchar, dz.End_date, 108)
        ) AS DetalleEtapa
    FROM RADAEmpires_DZDetailsHisense dz
    WHERE dz.Activo = 1
)

SELECT top (500)
    a.Urgencia,
    b.FastCard,
    a.Folio,
    a.Container,
    a.Origins_Location AS Origen,
    a.Destination_Location AS Destination,
    a.Status,
    a.Datetime AS HSolicitud,
    b.Time_Confirm AS HConfirm,
    b.Time_Finished AS HFinish,
    a.Who_Send AS WhoRequest,
    b.Choffer,
    a.Message AS Comment,
    a.Date,
    a.Shift AS Area,

    ISNULL(MAX(CASE WHEN e.EtapaNum = 1 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_1,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 2 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_2,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 3 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_3,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 4 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_4,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 5 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_5,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 6 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_6,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 7 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_7,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 8 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_8,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 9 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_9,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 10 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_10,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 11 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_11,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 12 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_12,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 13 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_13,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 14 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_14,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 15 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_15,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 16 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_16,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 17 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_17,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 18 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_18,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 19 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_19,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 20 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_20

FROM RADAEmpire_BRequestContainers a
INNER JOIN RADAEmpire_CEntryContrainers b ON b.Folio_Request = a.Folio
LEFT JOIN EtapasNumeradas e ON e.Folio = a.Folio
WHERE a.Active = '1'
  AND a.Date BETWEEN @FechaInicio AND @FechaFin
GROUP BY
    a.Urgencia,
    b.FastCard,
    a.Folio,
    a.Container,
    a.Origins_Location,
    a.Destination_Location,
    a.Status,
    a.Datetime,
    b.Time_Confirm,
    b.Time_Finished,
    a.Who_Send,
    b.Choffer,
    a.Message,
    a.Date,
    a.Shift
ORDER BY a.Folio DESC;";
                            // Agrega los parámetros de fecha
                            con.Parameters.Clear();
                            con.Parameters.AddWithValue("@FechaInicio", TimeStart);
                            con.Parameters.AddWithValue("@FechaFin", Timeend);

                            dr = con.ExecuteReader();
                            while (dr.Read())
                            {
                                string areaActual = dr["Area"].ToString();

                                if (filtroAreas.Contains(areaActual))
                                {
                                    GetRecordsDespachadorExcle2.Add(new Historial()
                                    {
                                        Urgencia = dr["Urgencia"].ToString(),
                                        Folio = dr["Folio"].ToString(),
                                        Container = dr["Container"].ToString(),
                                        Origen = dr["Origen"].ToString(),
                                        Destination = dr["Destination"].ToString(),
                                        Status = dr["Status"].ToString(),
                                        HSolicitud = dr["HSolicitud"].ToString(),
                                        HConfirm = dr["HConfirm"].ToString(),
                                        HFinish = dr["HFinish"].ToString(),
                                        WhoRequest = dr["WhoRequest"].ToString(),
                                        Choffer = dr["Choffer"].ToString(),
                                        fastcard = dr["FastCard"].ToString(),
                                        Comment = dr["Comment"].ToString(),
                                        Date = Convert.ToDateTime(dr["Date"]).ToString("MM/dd/yyyy"),
                                        Area = dr["Area"].ToString(),

                                        Etapa_1 = dr["Etapa_1"].ToString(),
                                        Etapa_2 = dr["Etapa_2"].ToString(),
                                        Etapa_3 = dr["Etapa_3"].ToString(),
                                        Etapa_4 = dr["Etapa_4"].ToString(),
                                        Etapa_5 = dr["Etapa_5"].ToString(),
                                        Etapa_6 = dr["Etapa_6"].ToString(),
                                        Etapa_7 = dr["Etapa_7"].ToString(),
                                        Etapa_8 = dr["Etapa_8"].ToString(),
                                        Etapa_9 = dr["Etapa_9"].ToString(),
                                        Etapa_10 = dr["Etapa_10"].ToString(),
                                        Etapa_11 = dr["Etapa_11"].ToString(),
                                        Etapa_12 = dr["Etapa_12"].ToString(),
                                        Etapa_13 = dr["Etapa_13"].ToString(),
                                        Etapa_14 = dr["Etapa_14"].ToString(),
                                        Etapa_15 = dr["Etapa_15"].ToString(),
                                        Etapa_16 = dr["Etapa_16"].ToString(),
                                        Etapa_17 = dr["Etapa_17"].ToString(),
                                        Etapa_18 = dr["Etapa_18"].ToString(),
                                        Etapa_19 = dr["Etapa_19"].ToString(),
                                        Etapa_20 = dr["Etapa_20"].ToString()
                                    });
                                }
                            }
                            DBSPP.Close();
                        }

                        ViewBag.RecordExcel = GetRecordsDespachadorExcle2;
                        return View();
                    }
                }
            }
        }

        public ActionResult CancelContainerR(string ID)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] == null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            if (Session["Type"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Type"] = Request.Cookies["UserCookie"].Value;
            }


            ViewBag.User = Session["Username"];
            ViewBag.Type = Session["Type"];

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
            if (Session["Username"] == null && Request.Cookies["UserCookie"] == null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            if (Session["Type"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Type"] = Request.Cookies["UserCookie"].Value;
            }


            ViewBag.User = Session["Username"];
            ViewBag.Type = Session["Type"];

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
            if (Session["Username"] == null && Request.Cookies["UserCookie"] == null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            if (Session["Type"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Type"] = Request.Cookies["UserCookie"].Value;
            }


            ViewBag.User = Session["Username"];
            ViewBag.Type = Session["Type"];

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
                    else
                    {
                        Chofferss = "CHOFER LIBRE";
                    }
                    DBSPP.Close();

                    if (Chofferss != "CHOFER LIBRE")
                    {
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
                    }

                    return RedirectToAction("EntryContainer", "RADA");
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

                    return RedirectToAction("EntryContainer", "RADA");
                }
            }
        }

        private void GetRecordss(List<string> filtroAreas)
        {
            if (GetRecordsDespachador.Count > 0)
            {
                GetRecordsDespachador.Clear();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select top (500) " +
                    " a.Urgencia AS Urgencia, b.FastCard as FastCard, a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                    " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date,a.shift as Area  " +
                    " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio and a.Active = '1'  ORDER by a.Folio desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    string areaActual = dr["Area"].ToString();

                    if (filtroAreas.Contains(areaActual))
                    {
                        GetRecordsDespachador.Add(new Historial()
                        {
                            Urgencia = (dr["Urgencia"].ToString()),
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

        private void GetRecordExcel2(List<string> filtroAreas)
        {
            if (GetRecordsDespachadorExcle2.Count > 0)
            {
                GetRecordsDespachadorExcle2.Clear();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = @"WITH EtapasNumeradas AS (
    SELECT
        dz.Folio,
        ROW_NUMBER() OVER (PARTITION BY dz.Folio ORDER BY TRY_CAST(dz.Date_Process AS datetime)) AS EtapaNum,
        CONCAT(
            '[', dz.Type_StatusContainer, '] ',
            'Status: ', dz.Status, ', ',
            'Comentario: ', dz.Comment, ', ',
            'Grúa: ', dz.GruaMov, ', ',
            'Proceso: ', dz.Process_Movement, ', ',
            'Fecha: ', FORMAT(TRY_CAST(dz.Date_Process AS date), 'yyyy-MM-dd'), ', ',
            'Hora: ', CONVERT(varchar, dz.End_date, 108)
        ) AS DetalleEtapa
    FROM RADAEmpires_DZDetailsHisense dz
    WHERE dz.Activo = 1
)

SELECT top (500)
    a.Urgencia,
    b.FastCard,
    a.Folio,
    a.Container,
    a.Origins_Location AS Origen,
    a.Destination_Location AS Destination,
    a.Status,
    a.Datetime AS HSolicitud,
    b.Time_Confirm AS HConfirm,
    b.Time_Finished AS HFinish,
    a.Who_Send AS WhoRequest,
    b.Choffer,
    a.Message AS Comment,
    a.Date,
    a.Shift AS Area,

    ISNULL(MAX(CASE WHEN e.EtapaNum = 1 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_1,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 2 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_2,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 3 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_3,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 4 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_4,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 5 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_5,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 6 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_6,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 7 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_7,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 8 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_8,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 9 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_9,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 10 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_10,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 11 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_11,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 12 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_12,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 13 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_13,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 14 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_14,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 15 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_15,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 16 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_16,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 17 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_17,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 18 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_18,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 19 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_19,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 20 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_20

FROM RADAEmpire_BRequestContainers a
INNER JOIN RADAEmpire_CEntryContrainers b ON b.Folio_Request = a.Folio
LEFT JOIN EtapasNumeradas e ON e.Folio = a.Folio
WHERE a.Active = '1'
GROUP BY
    a.Urgencia,
    b.FastCard,
    a.Folio,
    a.Container,
    a.Origins_Location,
    a.Destination_Location,
    a.Status,
    a.Datetime,
    b.Time_Confirm,
    b.Time_Finished,
    a.Who_Send,
    b.Choffer,
    a.Message,
    a.Date,
    a.Shift
ORDER BY a.Folio DESC;";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    string areaActual = dr["Area"].ToString();

                    if (filtroAreas.Contains(areaActual))
                    {
                        GetRecordsDespachadorExcle2.Add(new Historial()
                        {
                            Urgencia = dr["Urgencia"].ToString(),
                            Folio = dr["Folio"].ToString(),
                            Container = dr["Container"].ToString(),
                            Origen = dr["Origen"].ToString(),
                            Destination = dr["Destination"].ToString(),
                            Status = dr["Status"].ToString(),
                            HSolicitud = dr["HSolicitud"].ToString(),
                            HConfirm = dr["HConfirm"].ToString(),
                            HFinish = dr["HFinish"].ToString(),
                            WhoRequest = dr["WhoRequest"].ToString(),
                            Choffer = dr["Choffer"].ToString(),
                            fastcard = dr["FastCard"].ToString(),
                            Comment = dr["Comment"].ToString(),
                            Date = Convert.ToDateTime(dr["Date"]).ToString("MM/dd/yyyy"),
                            Area = dr["Area"].ToString(),

                            Etapa_1 = dr["Etapa_1"].ToString(),
                            Etapa_2 = dr["Etapa_2"].ToString(),
                            Etapa_3 = dr["Etapa_3"].ToString(),
                            Etapa_4 = dr["Etapa_4"].ToString(),
                            Etapa_5 = dr["Etapa_5"].ToString(),
                            Etapa_6 = dr["Etapa_6"].ToString(),
                            Etapa_7 = dr["Etapa_7"].ToString(),
                            Etapa_8 = dr["Etapa_8"].ToString(),
                            Etapa_9 = dr["Etapa_9"].ToString(),
                            Etapa_10 = dr["Etapa_10"].ToString(),
                            Etapa_11 = dr["Etapa_11"].ToString(),
                            Etapa_12 = dr["Etapa_12"].ToString(),
                            Etapa_13 = dr["Etapa_13"].ToString(),
                            Etapa_14 = dr["Etapa_14"].ToString(),
                            Etapa_15 = dr["Etapa_15"].ToString(),
                            Etapa_16 = dr["Etapa_16"].ToString(),
                            Etapa_17 = dr["Etapa_17"].ToString(),
                            Etapa_18 = dr["Etapa_18"].ToString(),
                            Etapa_19 = dr["Etapa_19"].ToString(),
                            Etapa_20 = dr["Etapa_20"].ToString()
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
                con.CommandText = "  Select top (500) " +
                    " a.Urgencia AS Urgencia,b.FastCard as FastCard, a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                    " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date,a.shift as Area  " +
                    " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio and a.Active = '1' ORDER by a.Folio desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetRecords.Add(new Historial()
                    {
                        Urgencia = (dr["Urgencia"].ToString()),
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

        private void GetRecordHisense(List<string> filtroAreas)
        {
            if (GetRecords.Count > 0)
            {
                GetRecords.Clear();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "  Select top (500) " +
                    " a.Urgencia AS Urgencia,b.FastCard as FastCard, a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                    " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date,a.shift as Area  " +
                    " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio and a.Active = '1' ORDER by a.Folio desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    string areaActual = dr["Area"].ToString();

                    if (filtroAreas.Contains(areaActual))
                    {
                        GetRecords.Add(new Historial()
                        {
                            Urgencia = (dr["Urgencia"].ToString()),
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

        private void GetRecordexcel()
        {
            if (GetRecordsExcel.Count > 0)
            {
                GetRecordsExcel.Clear();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = @"WITH EtapasNumeradas AS (
    SELECT
        dz.Folio,
        ROW_NUMBER() OVER (PARTITION BY dz.Folio ORDER BY TRY_CAST(dz.Date_Process AS datetime)) AS EtapaNum,
        CONCAT(
            '[', dz.Type_StatusContainer, '] ',
            'Status: ', dz.Status, ', ',
            'Comentario: ', dz.Comment, ', ',
            'Grúa: ', dz.GruaMov, ', ',
            'Proceso: ', dz.Process_Movement, ', ',
            'Fecha: ', FORMAT(TRY_CAST(dz.Date_Process AS date), 'yyyy-MM-dd'), ', ',
            'Hora: ', CONVERT(varchar, dz.End_date, 108)
        ) AS DetalleEtapa
    FROM RADAEmpires_DZDetailsHisense dz
    WHERE dz.Activo = 1
)

SELECT top (500)
    a.Urgencia,
    b.FastCard,
    a.Folio,
    a.Container,
    a.Origins_Location AS Origen,
    a.Destination_Location AS Destination,
    a.Status,
    a.Datetime AS HSolicitud,
    b.Time_Confirm AS HConfirm,
    b.Time_Finished AS HFinish,
    a.Who_Send AS WhoRequest,
    b.Choffer,
    a.Message AS Comment,
    a.Date,
    a.Shift AS Area,

    ISNULL(MAX(CASE WHEN e.EtapaNum = 1 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_1,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 2 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_2,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 3 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_3,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 4 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_4,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 5 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_5,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 6 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_6,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 7 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_7,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 8 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_8,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 9 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_9,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 10 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_10,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 11 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_11,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 12 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_12,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 13 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_13,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 14 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_14,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 15 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_15,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 16 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_16,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 17 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_17,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 18 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_18,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 19 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_19,
    ISNULL(MAX(CASE WHEN e.EtapaNum = 20 THEN e.DetalleEtapa END), 'No hay etapa') AS Etapa_20

FROM RADAEmpire_BRequestContainers a
INNER JOIN RADAEmpire_CEntryContrainers b ON b.Folio_Request = a.Folio
LEFT JOIN EtapasNumeradas e ON e.Folio = a.Folio
WHERE a.Active = '1'
GROUP BY
    a.Urgencia,
    b.FastCard,
    a.Folio,
    a.Container,
    a.Origins_Location,
    a.Destination_Location,
    a.Status,
    a.Datetime,
    b.Time_Confirm,
    b.Time_Finished,
    a.Who_Send,
    b.Choffer,
    a.Message,
    a.Date,
    a.Shift
ORDER BY a.Folio DESC;";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetRecordsExcel.Add(new Historial()
                    {
                        Urgencia = dr["Urgencia"].ToString(),
                        Folio = dr["Folio"].ToString(),
                        Container = dr["Container"].ToString(),
                        Origen = dr["Origen"].ToString(),
                        Destination = dr["Destination"].ToString(),
                        Status = dr["Status"].ToString(),
                        HSolicitud = dr["HSolicitud"].ToString(),
                        HConfirm = dr["HConfirm"].ToString(),
                        HFinish = dr["HFinish"].ToString(),
                        WhoRequest = dr["WhoRequest"].ToString(),
                        Choffer = dr["Choffer"].ToString(),
                        fastcard = dr["FastCard"].ToString(),
                        Comment = dr["Comment"].ToString(),
                        Date = Convert.ToDateTime(dr["Date"]).ToString("MM/dd/yyyy"),
                        Area = dr["Area"].ToString(),

                        Etapa_1 = dr["Etapa_1"].ToString(),
                        Etapa_2 = dr["Etapa_2"].ToString(),
                        Etapa_3 = dr["Etapa_3"].ToString(),
                        Etapa_4 = dr["Etapa_4"].ToString(),
                        Etapa_5 = dr["Etapa_5"].ToString(),
                        Etapa_6 = dr["Etapa_6"].ToString(),
                        Etapa_7 = dr["Etapa_7"].ToString(),
                        Etapa_8 = dr["Etapa_8"].ToString(),
                        Etapa_9 = dr["Etapa_9"].ToString(),
                        Etapa_10 = dr["Etapa_10"].ToString(),
                        Etapa_11 = dr["Etapa_11"].ToString(),
                        Etapa_12 = dr["Etapa_12"].ToString(),
                        Etapa_13 = dr["Etapa_13"].ToString(),
                        Etapa_14 = dr["Etapa_14"].ToString(),
                        Etapa_15 = dr["Etapa_15"].ToString(),
                        Etapa_16 = dr["Etapa_16"].ToString(),
                        Etapa_17 = dr["Etapa_17"].ToString(),
                        Etapa_18 = dr["Etapa_18"].ToString(),
                        Etapa_19 = dr["Etapa_19"].ToString(),
                        Etapa_20 = dr["Etapa_20"].ToString()
                    });
                }
                DBSPP.Close();
            }
        }

        private void GetUsers()
        {
            if (GetChoffer.Count > 0)
            {
                GetChoffer.Clear();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select * from RADAEmpire_AChoffer where Active = '1' AND Status = 'SIN MOVIMIENTO' order by Username asc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetChoffer.Add(new Usuarios()
                    {
                        Username = (dr["Username"].ToString()),
                    });
                }
                DBSPP.Close();
            }
        }
    }
}