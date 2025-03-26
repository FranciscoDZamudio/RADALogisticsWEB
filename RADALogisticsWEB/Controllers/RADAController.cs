using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RADALogisticsWEB.Models;

namespace RADALogisticsWEB.Controllers
{
    public class RADAController : Controller
    {
        List<Usuarios> GetChoffer = new List<Usuarios>();
        List<Historial> GetRecords = new List<Historial>();
        List<Historial> GetRecordsQuery = new List<Historial>();
        List<Inventario> GetInventary = new List<Inventario>();

        List<string> filtroAreas = new List<string>();

        //connection SQL server (database)
        SqlConnection DBSPP = new SqlConnection("Data Source=RADAEmpire.mssql.somee.com ;Initial Catalog=RADAEmpire ;User ID=RooRada; password=rada1311");
        SqlCommand con = new SqlCommand();
        SqlDataReader dr;
        // GET: RADA
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Query(string fecha)
        {
            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                if (fecha == "")
                {
                    string data = fecha.ToString();
                    return RedirectToAction("EntryContainer", "RADA", new { query = '0'});
                }
                else
                {
                    string data = fecha.ToString();
                    return RedirectToAction("EntryContainer", "RADA", new { query = '1', date = data });
                }
            }
        }

        public ActionResult EntryContainer(string query, string date)
        {
            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                string name = Session["Username"].ToString();

                if (query == "1")
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
                        con.CommandText = "Select top (1000) " +
                            " a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                            " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date, a.shift as Area  " +
                            " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio where a.Date = '" + date.ToString() + "' ORDER by a.Folio desc";
                        dr = con.ExecuteReader();
                        while (dr.Read())
                        {
                            GetRecordsQuery.Add(new Historial()
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
                        DBSPP.Close();
                        ViewBag.Records = GetRecordsQuery;
                        ViewBag.Count = GetRecordsQuery.Count.ToString();
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

                        DBSPP.Open();
                        con.Connection = DBSPP;
                        con.CommandText = "Select top (1000) " +
                            " a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                            " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date, a.shift as Area  " +
                            " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio where a.Date = '" + date.ToString() + "' ORDER by a.Folio desc";
                        dr = con.ExecuteReader();
                        while (dr.Read())
                        {
                            string areaActual = dr["Area"].ToString();

                            if (filtroAreas.Contains(areaActual))
                            {
                                GetRecordsQuery.Add(new Historial()
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
                        ViewBag.Records = GetRecordsQuery;
                        ViewBag.Count = GetRecordsQuery.Count.ToString();
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
                        GetEntryRequest();
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

                        GetEntryRequest(filtroAreas);
                        ViewBag.Records = GetRecords;
                        ViewBag.Count = GetRecords.Count.ToString();
                        return View();
                    }
                }
            }
        }

        public PartialViewResult ActualizarTabla(string name)
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

            GetEntryRequest(filtroAreas);
            ViewBag.Records = GetRecords; // Obtener nuevamente los datos
            ViewBag.Count = GetRecords.Count.ToString();
            return PartialView("table", ViewBag.Records);
        }

        public ActionResult ViewConfirm(string ID)
        {
            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            } 
            else
            {
                string messages = null, WhoSend = null, Container = null, Destination = null, Origins = null, Status = null, DateTime = null, Date = null;
                //create generate randoms int value
                SqlCommand conse = new SqlCommand("Select * from RADAEmpire_BRequestContainers where Active = '1' and Folio = '" + ID + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drconse = conse.ExecuteReader();
                if (drconse.HasRows)
                {
                    while (drconse.Read())
                    {
                        WhoSend = drconse["Who_Send"].ToString();
                        messages = drconse["message"].ToString();
                        Container = drconse["Container"].ToString();
                        Destination = drconse["Destination_Location"].ToString();
                        Origins = drconse["Origins_Location"].ToString();
                        Status = drconse["Status"].ToString();
                        DateTime = drconse["Datetime"].ToString();
                        Date = Convert.ToDateTime(drconse["Date"]).ToString("MM/dd/yyyy");
                    }
                }
                else
                {
                    messages = null;
                }
                DBSPP.Close();

                if (messages == "Canceled by Hisense")
                {
                    return RedirectToAction("EntryContainer", "RADA");
                }
                else
                {
                    if (messages == "Canceled by Rada")
                    {
                        return RedirectToAction("EntryContainer", "RADA");
                    }
                    else
                    {
                        if (messages == "PENDING")
                        {
                            ViewBag.WhoSend = WhoSend.ToString();
                            ViewBag.Container = Container.ToString();
                            ViewBag.Destination = Destination.ToString();
                            ViewBag.Origins = Origins.ToString();
                            ViewBag.Status = Status.ToString();
                            ViewBag.Datetime = DateTime.ToString();
                            ViewBag.Date = Date.ToString();
                            ViewBag.Username = Session["Username"];
                            ViewBag.id = ID.ToString();
                            
                            GetUsers();
                            ViewBag.RadaUsers = GetChoffer;
                            return View();
                        }
                        else
                        {
                            return RedirectToAction("ViewComplete", "RADA", new { ID = ID });
                        }
                    }
                }
            }
        }

        public ActionResult UpdateConfirmContainer(string ID, string Choffer, string Username)
        {
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

                string fast = null;
                // create generate randoms int value
                SqlCommand conse = new SqlCommand("Select Fastcard from RADAEmpire_AChoffer where Active = '1' and Username = '" + Choffer.ToString() + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drconse = conse.ExecuteReader();
                if (drconse.HasRows)
                {
                    while (drconse.Read())
                    {
                        fast = drconse["Fastcard"].ToString();
                    }
                }
                DBSPP.Close();

                //query message ------------------------------------------------------------------------------------------------------------------
                string updateQuery = "UPDATE RADAEmpire_BRequestContainers SET message = @message WHERE Folio = @ID";
                using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                {
                    DBSPP.Open();
                    command.Parameters.AddWithValue("@message", "Movement in process");
                    command.Parameters.AddWithValue("@ID", ID);
                    int rowsAffected = command.ExecuteNonQuery();
                    DBSPP.Close();
                }

                //query message ------------------------------------------------------------------------------------------------------------------
                string query = "UPDATE RADAEmpire_CEntryContrainers SET Username = @Username, Time_Confirm = @Time_Confirm, Choffer = @Choffer, FastCard = @FastCard WHERE Folio_Request = @ID";
                using (SqlCommand coms = new SqlCommand(query, DBSPP))
                {
                    DBSPP.Open();
                    coms.Parameters.AddWithValue("@Username", Username.ToString());
                    coms.Parameters.AddWithValue("@Time_Confirm", usTime.ToString("HH:mm:ss"));
                    coms.Parameters.AddWithValue("@Choffer", Choffer.ToString());
                    coms.Parameters.AddWithValue("@ID", ID);
                    coms.Parameters.AddWithValue("@FastCard", fast.ToString());
                    int rowsAffected = coms.ExecuteNonQuery();
                    DBSPP.Close();
                }

                return RedirectToAction("EntryContainer", "RADA");
            }
        }

        public ActionResult ViewComplete(string ID)
        {
            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                string messages = null, WhoSend = null, Container = null, Destination = null, Origins = null, Status = null, DateTime = null, Date = null;
                //create generate randoms int value
                SqlCommand conse = new SqlCommand("Select * from RADAEmpire_BRequestContainers where Active = '1' and Folio = '" + ID + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drconse = conse.ExecuteReader();
                if (drconse.HasRows)
                {
                    while (drconse.Read())
                    {
                        WhoSend = drconse["Who_Send"].ToString();
                        messages = drconse["message"].ToString();
                        Container = drconse["Container"].ToString();
                        Destination = drconse["Destination_Location"].ToString();
                        Origins = drconse["Origins_Location"].ToString();
                        Status = drconse["Status"].ToString();
                        DateTime = drconse["Datetime"].ToString();
                        Date = drconse["Date"].ToString();
                    }
                }
                else
                {
                    messages = null;
                }
                DBSPP.Close();

                if (messages == "Canceled by Hisense")
                {
                    return RedirectToAction("EntryContainer", "RADA");
                }
                else
                {
                    if (messages == "Canceled by Rada")
                    {
                        return RedirectToAction("EntryContainer", "RADA");
                    }
                    else
                    {
                        if (messages == "PENDING")
                        {
                            ViewBag.WhoSend = WhoSend.ToString();
                            ViewBag.Container = Container;
                            ViewBag.Destination = Destination;
                            ViewBag.Origins = Origins;
                            ViewBag.Status = Status;
                            ViewBag.Datetime = DateTime;
                            ViewBag.Date = Date;
                            return RedirectToAction("ViewConfirm", "RADA", new { ID = ID });
                        }
                        else
                        {
                            string container = null, origins = null, destination = null, status = null, solicitud = null, confirmacion = null, entrega = null, request = null, choffer = null, comment = null, date = null;
                            //create generate randoms int value
                            SqlCommand conse2 = new SqlCommand("Select " +
                                " a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                                " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date  " +
                                " from " +
                                " RADAEmpire_BRequestContainers as a " +
                                " inner join " +
                                " RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio where a.Folio = '" + ID + "'", DBSPP);
                            DBSPP.Open();
                            SqlDataReader drconse2 = conse2.ExecuteReader();
                            if (drconse2.HasRows)
                            {
                                while (drconse2.Read())
                                {
                                    container = drconse2["Container"].ToString();//234 + 1216 + 
                                    origins = drconse2["Origen"].ToString();
                                    destination = drconse2["Destination"].ToString();
                                    status = drconse2["Status"].ToString();
                                    solicitud = drconse2["HSolicitud"].ToString();
                                    confirmacion = drconse2["HConfirm"].ToString();
                                    entrega = drconse2["HFinish"].ToString();
                                    request = drconse2["WhoRequest"].ToString();
                                    choffer = drconse2["Choffer"].ToString();
                                    comment = drconse2["Comment"].ToString();
                                    date = drconse2["Date"].ToString();
                                }
                            }
                            DBSPP.Close();

                            ViewBag.Container = container;
                            ViewBag.Status = status;
                            ViewBag.Solicitud = solicitud;
                            ViewBag.Confirmacion = confirmacion;
                            ViewBag.entrega = entrega;
                            ViewBag.choffer = choffer;
                            ViewBag.origins = origins;
                            ViewBag.destination = destination;
                            ViewBag.comment = comment;
                            ViewBag.date = date;
                            ViewBag.Request = request;
                            ViewBag.ID = ID.ToString();
                            return View();
                        }
                    }
                }
            }
        }

        public ActionResult Inventory()
        {
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

        public ActionResult RemoveInventary(string ID)
        {
            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                string updateQuery = "UPDATE RADAEmpire_CInventoryControl SET Active = @Active WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                {
                    DBSPP.Open();
                    command.Parameters.AddWithValue("@Active", false);
                    command.Parameters.AddWithValue("@ID", ID);
                    int rowsAffected = command.ExecuteNonQuery();
                    DBSPP.Close();
                }

                return RedirectToAction("Inventory", "RADA");
            }
        }

        public ActionResult ReturnContainer(string ID)
        {
            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                

                return View();
            }
        }

        public ActionResult ConfirmContainer(string id)
        {
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

                //query message ------------------------------------------------------------------------------------------------------------------
                string updateQuery = "UPDATE RADAEmpire_CEntryContrainers SET Time_Finished = @Time_Finished WHERE Folio_Request = @ID";
                using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                {
                    DBSPP.Open();
                    command.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
                    command.Parameters.AddWithValue("@ID", id);
                    int rowsAffected = command.ExecuteNonQuery();
                    DBSPP.Close();
                }

                //query message ------------------------------------------------------------------------------------------------------------------
                string updateQuery2 = "UPDATE RADAEmpire_BRequestContainers SET message = @message WHERE Folio = @ID";
                using (SqlCommand command = new SqlCommand(updateQuery2, DBSPP))
                {
                    DBSPP.Open();
                    command.Parameters.AddWithValue("@message", "Movement Completed");
                    command.Parameters.AddWithValue("@ID", id);
                    int rowsAffected = command.ExecuteNonQuery();
                    DBSPP.Close();
                }

                return RedirectToAction("EntryContainer", "RADA");
            }
        }

        public ActionResult ProcessData(string User, string Container, string Location)
        {
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

                //Guardar informacion a la base de datos del proyecto
                DBSPP.Open();
                SqlCommand PalletControl = new SqlCommand("insert into RADAEmpire_CInventoryControl" +
                    "(Username, Container, LocationCode, Status, Date, Datetime, Active) values " +
                    "(@Username, @Container, @LocationCode, @Status, @Date, @Datetime, @Active) ", DBSPP);
                //--------------------------------------------------------------------------------------------------------------------------------
                PalletControl.Parameters.AddWithValue("@Username", User.ToString());
                PalletControl.Parameters.AddWithValue("@Container", Container.ToString());
                PalletControl.Parameters.AddWithValue("@LocationCode", Location.ToString());
                PalletControl.Parameters.AddWithValue("@Status", "Container is move to " + Location);
                PalletControl.Parameters.AddWithValue("@Date", usTime.ToString());
                PalletControl.Parameters.AddWithValue("@Datetime", usTime.ToString());
                PalletControl.Parameters.AddWithValue("@Active", true);
               
                PalletControl.ExecuteNonQuery();
                DBSPP.Close();
                //--------------------------------------------------------------------------------------------------------------------------------

                return RedirectToAction("Inventory", "RADA");
            }
        }

        public ActionResult UpdateInventory(string ID)
        {
            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                string container = null, date = null, location = null;
                // create generate randoms int value
                SqlCommand conse = new SqlCommand("Select * from RADAEmpire_CInventoryControl where Active = '1' and ID = '" + ID.ToString() + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drconse = conse.ExecuteReader();
                if (drconse.HasRows)
                {
                    while (drconse.Read())
                    {
                        container = drconse["Container"].ToString();
                        date = drconse["Datetime"].ToString();
                        location = drconse["LocationCode"].ToString();
                    }
                }
                DBSPP.Close();

                ViewBag.ID = ID;
                ViewBag.Container = container.ToString();
                ViewBag.Datetime = date.ToString();
                ViewBag.LocationCode = location.ToString(); ;
                return View();
            }
        }

        public ActionResult Update(string id, string Username, string Container, string LocationCode, string Date)
        {
            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                string updateQuery = "UPDATE RADAEmpire_CInventoryControl SET Status = @Status, " +
                " Username = @Username, Container = @Container, " +
                " LocationCode = @LocationCode, Date = @Date, Datetime = @Datetime WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                {
                    DBSPP.Open();
                    command.Parameters.AddWithValue("@Status", "Container is move to " + LocationCode);
                    command.Parameters.AddWithValue("@Username", Username);
                    command.Parameters.AddWithValue("@Container", Container);
                    command.Parameters.AddWithValue("@LocationCode", LocationCode);
                    command.Parameters.AddWithValue("@Date", Convert.ToDateTime(Date).ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@Datetime", Date);
                    command.Parameters.AddWithValue("@ID", id);
                    int rowsAffected = command.ExecuteNonQuery();
                    DBSPP.Close();
                }

                return RedirectToAction("Inventory", "RADA");
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

        private void GetEntryRequest(List<string> filtroAreas)
        {

            if (GetRecords.Count > 0)
            {
                GetRecords.Clear();
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

                string datenow = usTime.ToString();
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "  Select top (1000) " +
                    " a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                    " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date, a.shift as Area  " +
                    " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio where a.Date = '" + datenow.ToString() + "' ORDER by a.Folio desc";
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
                            Choffer = (dr["Choffer"].ToString()),
                            Comment = (dr["Comment"].ToString()),
                            Date = Convert.ToDateTime(dr["Date"]).ToString("MM/dd/yyyy"),
                            Area = (dr["Area"].ToString()),
                        });
                    }
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
                con.CommandText = "Select * from RADAEmpire_AChoffer where Active = '1' order by ID desc";
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

        private void GetEntryRequest()
        {

            if (GetRecords.Count > 0)
            {
                GetRecords.Clear();
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

                string datenow = usTime.ToString();
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "  Select top (1000) " +
                    " a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                    " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date, a.shift as Area  " +
                    " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio where a.Date = '" + datenow.ToString() + "' ORDER by a.Folio desc";
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
                        Choffer = (dr["Choffer"].ToString()),
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