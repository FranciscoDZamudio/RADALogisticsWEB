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
    public class HISENSEController : Controller
    {
        List<Solicitud_Contenedores> GetListed = new List<Solicitud_Contenedores>();
        List<DetailsTable> GetDetails = new List<DetailsTable>();

        List<Areas> getArea = new List<Areas>();
        List<Areas> getAreaAll = new List<Areas>();
        //connection SQL server (database)
        SqlConnection DBSPP = new SqlConnection("Data Source=RADAEmpire.mssql.somee.com ;Initial Catalog=RADAEmpire ;User ID=RooRada; password=rada1311");
        SqlCommand con = new SqlCommand();
        SqlDataReader dr;

        [HttpGet]
        public JsonResult GetGruaRequest(string area)
        {
            if (Session["Username"] == null && Request.Cookies["UserInfo"] != null)
            {
                var userCookie = Request.Cookies["UserInfo"];

                if (!string.IsNullOrEmpty(userCookie["Username"]))
                {
                    Session["Username"] = userCookie["Username"];
                    Session["Type"] = userCookie["Rol"]; // Si también necesitas el rol
                }
            }

            if (Session["Username"] == null)
            {
                return Json(new { success = false, redirectUrl = Url.Action("LogIn", "Login") });
            }

            string validation = null;
            using (SqlConnection conn = new SqlConnection("Data Source=RADAEmpire.mssql.somee.com ;Initial Catalog=RADAEmpire ;User ID=RooRada; password=rada1311"))
            {
                SqlCommand cmd = new SqlCommand("SELECT GruaRequest FROM RADAEmpire_AAreas WHERE Active = '1' AND Name = @Area", conn);
                cmd.Parameters.AddWithValue("@Area", area);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        validation = reader[0].ToString();
                    }
                }
            }

            return Json(new { gruaRequest = validation }, JsonRequestBehavior.AllowGet);
        }

        // GET: HISENSE
        public ActionResult ChangeStatus(string ID, string Page, string data)
        {
            // Intentar recuperar Username desde la cookie si está nulo
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            // Intentar recuperar Type desde la cookie si está nulo
            if (Session["Type"] == null && Request.Cookies["TypeCookie"] != null)
            {
                Session["Type"] = Request.Cookies["TypeCookie"].Value;
            }

            // Validación final: si sigue sin Username o Type, redirigir al login
            if (Session["Username"] == null || Session["Type"] == null)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                ViewBag.User = Session["Username"];
                ViewBag.Type = Session["Type"];
                ViewBag.data = data;
                ViewBag.Page = Page;
                ViewBag.ID = ID;
                return View();
            }
        }

        public ActionResult NewConcept(string id, string NivelUrgencia, string MotivoUrgencia, string Page, string data)
        {
            // Intentar recuperar Username desde la cookie si está nulo
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            // Intentar recuperar Type desde la cookie si está nulo
            if (Session["Type"] == null && Request.Cookies["TypeCookie"] != null)
            {
                Session["Type"] = Request.Cookies["TypeCookie"].Value;
            }

            // Validación final: si sigue sin Username o Type, redirigir al login
            if (Session["Username"] == null || Session["Type"] == null)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                ViewBag.User = Session["Username"];
                ViewBag.Type = Session["Type"];

                //query message
                string updateQuery = "UPDATE RADAERmpire_AMessagesUrgent SET Description = @Description,Status = @Status  WHERE Folio = @ID";
                using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                {
                    DBSPP.Open();
                    command.Parameters.AddWithValue("@Status", NivelUrgencia.ToString());
                    command.Parameters.AddWithValue("@Description", MotivoUrgencia.ToString());
                    command.Parameters.AddWithValue("@ID", id);
                    int rowsAffected = command.ExecuteNonQuery();
                    DBSPP.Close();
                }

                //query message
                string Updt = "UPDATE RADAEmpire_BRequestContainers SET Urgencia = @Urgencia WHERE Folio = @ID";
                using (SqlCommand drUpdt = new SqlCommand(Updt, DBSPP))
                {
                    DBSPP.Open();
                    drUpdt.Parameters.AddWithValue("@Urgencia", NivelUrgencia.ToString());
                    drUpdt.Parameters.AddWithValue("@ID", id);
                    int rowsAffected = drUpdt.ExecuteNonQuery();
                    DBSPP.Close();
                }

                if (Page == "Hisense" && data == "iNDEX")
                {
                    return RedirectToAction("EntryContainer", "RADA");
                }
                else
                {
                    if (Page == "Hisense" && data == "RADASers")
                    {
                        return RedirectToAction("Records", "History");
                    }
                    else
                    {
                        return RedirectToAction("RequestContainer", "HISENSE");
                    }
                }
            }
        }

        public ActionResult Details(string ID, string Record)
        {
            // Intentar recuperar Username desde la cookie si está nulo
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            // Intentar recuperar Type desde la cookie si está nulo
            if (Session["Type"] == null && Request.Cookies["TypeCookie"] != null)
            {
                Session["Type"] = Request.Cookies["TypeCookie"].Value;
            }

            // Validación final: si sigue sin Username o Type, redirigir al login
            if (Session["Username"] == null || Session["Type"] == null)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                ViewBag.User = Session["Username"];
                ViewBag.Type = Session["Type"];
                string placas = null, Area = null,container = null, origins = null, destination = null, status = null, solicitud = null, confirmacion = null, entrega = null, request = null, choffer = null, comment = null, date = null;
                //create generate randoms int value
                SqlCommand conse = new SqlCommand("Select " +
                    " a.Container as Container, a.Placa as Placa,a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                    " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.shift as Area, a.Date as Date  " +
                    " from " +
                    " RADAEmpire_BRequestContainers as a " +
                    " inner join " +
                    " RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio where a.Folio = '" + ID + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drconse = conse.ExecuteReader();
                if (drconse.HasRows)
                {
                    while (drconse.Read())
                    {
                        placas = drconse["Placa"].ToString();//234 + 1216 + 
                        container = drconse["Container"].ToString();//234 + 1216 + 
                        origins = drconse["Origen"].ToString();
                        destination = drconse["Destination"].ToString();
                        status = drconse["Status"].ToString();
                        solicitud = drconse["HSolicitud"].ToString();
                        confirmacion = drconse["HConfirm"].ToString();
                        entrega = drconse["HFinish"].ToString();
                        request = drconse["WhoRequest"].ToString();
                        choffer = drconse["Choffer"].ToString();
                        Area = drconse["Area"].ToString();
                        comment = drconse["Comment"].ToString();
                        date = Convert.ToDateTime(drconse["Date"]).ToString("d");
                    }
                }
                DBSPP.Close();

                //create generate randoms int value
                string ReasonMT = null, datetime = null;
                SqlCommand reason = new SqlCommand("Select * from RADAEmpires_DRemoves where Active = '1' and Folio = '" + ID.ToString() + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drreason = reason.ExecuteReader();
                if (drreason.HasRows)
                {
                    while (drreason.Read())
                    {
                        ReasonMT = drreason["Reason"].ToString();
                        datetime = drreason["Datetime"].ToString();
                    }
                }
                else
                {
                    ReasonMT = "Report not canceled";
                    datetime = "0000-00-00 00:00:00.000";
                }
                DBSPP.Close();

                //create generate randoms int value
                string Desc = null, sts = null;
                SqlCommand QuerySq = new SqlCommand("Select * from RADAERmpire_AMessagesUrgent where Active = '1' and Folio = '" + ID.ToString() + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drQuerySq = QuerySq.ExecuteReader();
                if (drQuerySq.HasRows)
                {
                    while (drQuerySq.Read())
                    {
                        Desc = drQuerySq["Description"].ToString();
                        sts = drQuerySq["Status"].ToString();
                    }
                }
                else
                {
                    Desc = "REPORTE SIN MOTIVO DE URGENCIA";
                    sts = "Normal";
                }
                DBSPP.Close();

                ViewBag.Placa = placas;
                ViewBag.ID = ID;
                ViewBag.Desc = Desc;  
                ViewBag.sts = sts;
                ViewBag.DateCancel = datetime;
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
                ViewBag.ReasonMT = ReasonMT;
                ViewBag.mot = Record;
                ViewBag.Areas = Area;

                GetDetailss(ID);
                ViewBag.Records = GetDetails;
                return View();
            }
        }

        public ActionResult RequestContainer()
        {
            // Intentar recuperar Username desde la cookie si está nulo
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            // Intentar recuperar Type desde la cookie si está nulo
            if (Session["Type"] == null && Request.Cookies["TypeCookie"] != null)
            {
                Session["Type"] = Request.Cookies["TypeCookie"].Value;
            }

            // Validación final: si sigue sin Username o Type, redirigir al login
            if (Session["Username"] == null || Session["Type"] == null)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                ViewBag.User = Session["Username"];
            ViewBag.Type = Session["Type"];
                string name = Session["Username"].ToString();
                string val = null;
                //create generate randoms int value
                SqlCommand conse = new SqlCommand("Select Type_user from RADAEmpire_AUsers where Active = '1' and Username = '" + Session["Username"].ToString() + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drconse = conse.ExecuteReader();
                if (drconse.HasRows)
                {
                    while (drconse.Read())
                    {
                        val = drconse[0].ToString();
                    }
                }
                DBSPP.Close();

                if (val == "HISENSE")
                {
                    GetContainers();
                    GetAreas(name);
                    ViewBag.Records = GetListed;
                    ViewBag.listed = getArea;
                    ViewBag.Count = GetListed.Count.ToString();
                    return View();
                }
                else
                {
                    GetContainers();
                    GetAreasAll();
                    ViewBag.Records = GetListed;
                    ViewBag.listed = getAreaAll;
                    ViewBag.Count = GetListed.Count.ToString();
                    return View();
                }
            }
        }

        private void GetAreasAll()
        {
            if (getAreaAll.Count > 0)
            {
                getAreaAll.Clear();
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
                con.CommandText = "Select Name from RADAEmpire_AAreas where Active = '1'";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    getAreaAll.Add(new Areas()
                    {
                        Area = (dr["Name"].ToString()),
                    });
                }
                DBSPP.Close();
            }
        }

        private void GetAreas(string name)
        {
            if (getArea.Count > 0)
            {
                getArea.Clear();
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
                con.CommandText = "Select * from RADAEmpire_ARoles where Active = '1'";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    if (dr["Username"].ToString() == name)
                    {
                        getArea.Add(new Areas()
                        {
                            Username = (dr["Username"].ToString()),
                            Area = (dr["Areas"].ToString()),
                        });
                    }
                }
                DBSPP.Close();
            }
        }

        public PartialViewResult ActualizarTabla()
        {
            GetContainers();
            ViewBag.Records = GetListed; // Obtener nuevamente los datos
            return PartialView("table", ViewBag.Records);
        }

        public ActionResult ProcessData(string ActivoRampa, string User, string Type, 
            string Container, string Origins, string Destination, string Area, 
            string ActivoHidden, string NivelUrgencia , string MotivoUrgencia, string Placa)
        {
            try
            {
                // Intentar recuperar Username desde la cookie si está nulo
                if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
                {
                    Session["Username"] = Request.Cookies["UserCookie"].Value;
                }

                // Intentar recuperar Type desde la cookie si está nulo
                if (Session["Type"] == null && Request.Cookies["TypeCookie"] != null)
                {
                    Session["Type"] = Request.Cookies["TypeCookie"].Value;
                }

                // Validación final: si sigue sin Username o Type, redirigir al login
                if (Session["Username"] == null || Session["Type"] == null)
                {
                    return RedirectToAction("LogIn", "Login");
                }
                else
                {
                    ViewBag.User = Session["Username"];
                    ViewBag.Type = Session["Type"];

                    if (Placa == "")
                    {
                        Placa = "NO SE REGISTRO";
                    }

                    if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
                    {
                        Session["Username"] = Request.Cookies["UserCookie"].Value;
                    }


                    if (Session["Type"] == null && Request.Cookies["UserCookie"] != null)
                    {
                        Session["Type"] = Request.Cookies["UserCookie"].Value;
                    }

                    if (Area == null)
                    {
                        return RedirectToAction("RequestContainer", "HISENSE");
                    }

                    List<string> lista = new List<string>();

                    using (SqlConnection conn = new SqlConnection(DBSPP.ConnectionString))
                    {
                        conn.Open();

                        // Obtener lista de pasos
                        using (SqlCommand cmd = new SqlCommand("SELECT * FROM RADAEmpire_AAreasAsign WHERE Active = 1 AND Status = @Status AND AreaAssign = @AreaAssign AND GruaRequest = @GruaRequest AND RaR = @RaR ORDER BY NumberOrder ASC", conn))
                        {
                            cmd.Parameters.AddWithValue("@Status", Type);
                            cmd.Parameters.AddWithValue("@AreaAssign", Area);
                            cmd.Parameters.AddWithValue("@GruaRequest", ActivoHidden);
                            cmd.Parameters.AddWithValue("@RaR", ActivoRampa);

                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (!reader.HasRows)
                                    return RedirectToAction("RequestContainer", "HISENSE");

                                while (reader.Read())
                                {
                                    lista.Add(reader["Message"].ToString()); // Puedes cambiar reader[0] por reader["NombreCampo"]
                                }
                            }
                        }

                        // Obtener último ID
                        int lastId = 0;
                        using (SqlCommand cmd = new SqlCommand("SELECT TOP (1) ID FROM RADAEmpire_BRequestContainers ORDER BY ID DESC", conn))
                        {
                            var result = cmd.ExecuteScalar();
                            if (result != null)
                                lastId = Convert.ToInt32(result);
                        }

                        string Folio = "MOV" + (lastId + 1);

                        // Obtener hora en US (Rosarito - Pacific Time)
                        TimeZoneInfo usTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                        DateTime usTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, usTimeZone);
                        string dateOnly = usTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                        string timeOnly = usTime.ToString("HH:mm:ss");

                        // Insertar en RADAEmpire_BRequestContainers
                        using (SqlCommand insertReq = new SqlCommand(@"INSERT INTO RADAEmpire_BRequestContainers 
                (Placa,Urgencia, Folio, Who_Send, Container, Destination_Location, Origins_Location, Status, message, shift, Date, Datetime, Active, GruaRequest, RaRRequest)
                VALUES (@Placa,@Urgencia, @Folio, @Who_Send, @Container, @Destination_Location, @Origins_Location, @Status, @message, @shift, @Date, @Datetime, @Active, @GruaRequest, @RaRRequest)", conn))
                        {
                            insertReq.Parameters.AddWithValue("@Urgencia", NivelUrgencia.ToString());
                            insertReq.Parameters.AddWithValue("@Placa", Placa?.ToString() ?? "NO SE REGISTRO");
                            insertReq.Parameters.AddWithValue("@Folio", Folio);
                            insertReq.Parameters.AddWithValue("@Who_Send", User);
                            insertReq.Parameters.AddWithValue("@Container", Container.ToUpper());
                            insertReq.Parameters.AddWithValue("@Destination_Location", Destination.ToUpper());
                            insertReq.Parameters.AddWithValue("@Origins_Location", Origins.ToUpper());
                            insertReq.Parameters.AddWithValue("@Status", Type);
                            insertReq.Parameters.AddWithValue("@message", "PENDING");
                            insertReq.Parameters.AddWithValue("@shift", Area);
                            insertReq.Parameters.AddWithValue("@Date", dateOnly);
                            insertReq.Parameters.AddWithValue("@Datetime", timeOnly);
                            insertReq.Parameters.AddWithValue("@Active", true);
                            insertReq.Parameters.AddWithValue("@GruaRequest", ActivoHidden);
                            insertReq.Parameters.AddWithValue("@RaRRequest", ActivoRampa);

                            insertReq.ExecuteNonQuery();
                        }

                        // Insertar en RADAEmpire_BRequestContainers
                        using (SqlCommand insertReq = new SqlCommand(@"INSERT INTO RADAERmpire_AMessagesUrgent 
                     (Folio,Status,Description,Active,Dateadded,DateTimeadded)
                       VALUES 
                     (@Folio,@Status,@Description,@Active,@Dateadded,@DateTimeadded)", conn))
                        {
                            insertReq.Parameters.AddWithValue("@Folio", Folio.ToString());
                            insertReq.Parameters.AddWithValue("@Status", NivelUrgencia.ToString());
                            insertReq.Parameters.AddWithValue("@Description", MotivoUrgencia.ToUpper());
                            insertReq.Parameters.AddWithValue("@Active", true);
                            insertReq.Parameters.AddWithValue("@Dateadded", usTime.ToString());
                            insertReq.Parameters.AddWithValue("@DateTimeadded", usTime.ToString());

                            insertReq.ExecuteNonQuery();
                        }

                        // Insertar en RADAEmpire_CEntryContrainers
                        using (SqlCommand insertEntry = new SqlCommand(@"INSERT INTO RADAEmpire_CEntryContrainers
                (Folio_Request, Username, Time_Confirm, Choffer, FastCard, Time_Finished, Date, AreaWork, Active, Cancel)
                VALUES (@Folio_Request, @Username, @Time_Confirm, @Choffer, @FastCard, @Time_Finished, @Date, @AreaWork, @Active, @Cancel)", conn))
                        {
                            insertEntry.Parameters.AddWithValue("@Folio_Request", Folio);
                            insertEntry.Parameters.AddWithValue("@Username", "PENDNING CONFIRM");
                            insertEntry.Parameters.AddWithValue("@Time_Confirm", "00:00:00");
                            insertEntry.Parameters.AddWithValue("@Choffer", "PENDNING CONFIRM");
                            insertEntry.Parameters.AddWithValue("@FastCard", "PENDNING CONFIRM");
                            insertEntry.Parameters.AddWithValue("@Time_Finished", "00:00:00");
                            insertEntry.Parameters.AddWithValue("@Date", dateOnly);
                            insertEntry.Parameters.AddWithValue("@AreaWork", "RADALogistics");
                            insertEntry.Parameters.AddWithValue("@Active", true);
                            insertEntry.Parameters.AddWithValue("@Cancel", false);

                            insertEntry.ExecuteNonQuery();
                        }

                        // Insertar pasos en RADAEmpires_DZDetailsHisense
                        foreach (var paso in lista)
                        {
                            using (SqlCommand insertPaso = new SqlCommand(@"INSERT INTO RADAEmpires_DZDetailsHisense 
                    (Folio, Type_StatusContainer, GruaMov, Process_Movement, End_date, Status, Comment, Date_Process, Activo)
                    VALUES (@Folio, @Type_StatusContainer, @GruaMov, @Process_Movement, @End_date, @Status, @Comment, GETDATE(), @Activo)", conn))
                            {
                                insertPaso.Parameters.AddWithValue("@Folio", Folio);
                                insertPaso.Parameters.AddWithValue("@Type_StatusContainer", Type);
                                insertPaso.Parameters.AddWithValue("@GruaMov", ActivoHidden);
                                insertPaso.Parameters.AddWithValue("@Process_Movement", paso);
                                insertPaso.Parameters.AddWithValue("@End_date", "00:00:00");
                                insertPaso.Parameters.AddWithValue("@Status", "PENDIENTE");
                                insertPaso.Parameters.AddWithValue("@Comment", "SIN COMENTARIOS");
                                insertPaso.Parameters.AddWithValue("@Activo", true);

                                insertPaso.ExecuteNonQuery();
                            }
                        }

                        conn.Close();
                    }

                    return RedirectToAction("RequestContainer", "HISENSE");
                }  
            }
            catch (Exception ex)
            {
                // Log error aquí si tienes logger
                ViewBag.Error = "Error al procesar la solicitud: " + ex.Message;
                return View("Error");
            }
        }

        private void GetDetailss(string ID)
        {
            if (GetDetails.Count > 0)
            {
                GetDetails.Clear();
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
                con.CommandText = "Select * from RADAEmpires_DZDetailsHisense where Activo = '1' and Folio = '" + ID + "' order by ID ASC";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetDetails.Add(new DetailsTable()
                    {
                        Folio = (dr["Folio"].ToString()),
                        Type_Status = (dr["Type_StatusContainer"].ToString()),
                        GruaMov = (dr["GruaMov"].ToString()),
                        ProcessMovement = (dr["Process_Movement"].ToString()),
                        End_date = (dr["End_date"].ToString()),
                        Status = ((dr["Status"].ToString())),
                        Comment = (dr["Comment"].ToString()),
                        Date_Process = Convert.ToDateTime(dr["Date_Process"]).ToString("d"),
                    });
                }
                DBSPP.Close();
            }
        }

        private void GetContainers()
        {
            if (GetListed.Count > 0)
            {
                GetListed.Clear();
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

                // Zona horaria de Rosarito
                TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

                // Obtener la hora actual en UTC y convertirla a Rosarito
                DateTime rosaritoNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, pacificZone);
                string rosaritoDateOnly = rosaritoNow.ToString("yyyy-MM-dd");

                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = @"SELECT * 
FROM RADAEmpire_BRequestContainers 
WHERE Active = '1' AND Date = @Date
ORDER BY 
    -- Cancelados al final
    CASE 
        WHEN UPPER(message) = 'CANCELED BY RADA' THEN 1 ELSE 0
    END,
    
    -- Prioridad por urgencia (solo si NO está cancelado)
    CASE 
        WHEN UPPER(Urgencia) = 'CRITICO' THEN 0
        WHEN UPPER(Urgencia) = 'URGENTE' THEN 1
        WHEN UPPER(Urgencia) = 'NORMAL' THEN 2
        ELSE 3
    END,

    -- Prioridad por mensaje
    CASE 
        WHEN UPPER(message) = 'PENDING' THEN 0
        WHEN UPPER(message) = 'CHOFER TERMINA MOVIMIENTO' THEN 2
        ELSE 1
    END,

    ID DESC;
";

                con.Parameters.Clear();
                con.Parameters.AddWithValue("@Date", rosaritoDateOnly);

                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    // Convertir fechas a zona horaria de Rosarito
                    DateTime originalDate = Convert.ToDateTime(dr["Date"]);
                    DateTime utcDate = DateTime.SpecifyKind(originalDate, DateTimeKind.Utc);
                    DateTime rosaritoDate = TimeZoneInfo.ConvertTimeFromUtc(utcDate, pacificZone);

                    DateTime originalDateTime = Convert.ToDateTime(dr["Datetime"]);
                    DateTime utcDateTime = DateTime.SpecifyKind(originalDateTime, DateTimeKind.Utc);
                    DateTime rosaritoDateTime = TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, pacificZone);

                    GetListed.Add(new Solicitud_Contenedores()
                    {
                        Urgencia = dr["Urgencia"].ToString(),
                        ID = int.Parse(dr["ID"].ToString()),
                        Folio = dr["Folio"].ToString(),
                        Who_Send = dr["Who_Send"].ToString(),
                        Container = dr["Container"].ToString(),
                        Destination_Location = dr["Destination_Location"].ToString(),
                        Origins_Location = dr["Origins_Location"].ToString(),
                        Status = dr["Status"].ToString(),
                        shift = dr["shift"].ToString(),
                        message = dr["message"].ToString(),
                        Datetime = Convert.ToDateTime(dr["Date"].ToString()).ToString("d"),
                    });
                }
                DBSPP.Close();

            }
        }

        public ActionResult CancelContainer(string ID)
        {
            // Intentar recuperar Username desde la cookie si está nulo
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            // Intentar recuperar Type desde la cookie si está nulo
            if (Session["Type"] == null && Request.Cookies["TypeCookie"] != null)
            {
                Session["Type"] = Request.Cookies["TypeCookie"].Value;
            }

            // Validación final: si sigue sin Username o Type, redirigir al login
            if (Session["Username"] == null || Session["Type"] == null)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                ViewBag.User = Session["Username"];
                ViewBag.Type = Session["Type"];
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
                    return View();
                }
                else
                {
                    return RedirectToAction("RequestContainer", "HISENSE");
                }
            }
        }

        public ActionResult Delete(string Reason, string ID)
        {
            // Intentar recuperar Username desde la cookie si está nulo
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            // Intentar recuperar Type desde la cookie si está nulo
            if (Session["Type"] == null && Request.Cookies["TypeCookie"] != null)
            {
                Session["Type"] = Request.Cookies["TypeCookie"].Value;
            }

            // Validación final: si sigue sin Username o Type, redirigir al login
            if (Session["Username"] == null || Session["Type"] == null)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                ViewBag.User = Session["Username"];
                ViewBag.Type = Session["Type"];
                // Obtener la fecha y hora actual en Alemania (zona horaria UTC+1 o UTC+2 dependiendo del horario de verano)
                DateTime germanTime = DateTime.UtcNow.AddHours(0);  // Alemania es UTC+1

                // Convertir la hora alemana a la hora en una zona horaria específica de EE. UU. (por ejemplo, Nueva York, UTC-5)
                TimeZoneInfo usEasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                DateTime usTime = TimeZoneInfo.ConvertTime(germanTime, usEasternTimeZone);

                // Formatear la fecha para que sea adecuada para la base de datos
                string formattedDate = usTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                //Guardar informacion a la base de datos del proyecto
                DBSPP.Open();
                SqlCommand PalletControl = new SqlCommand("insert into RADAEmpires_DRemoves" +
                    "(Folio, Reason, Datetime, Active,Company) values " +
                    "(@Folio, @Reason, @Datetime, @Active,@Company) ", DBSPP);
                //--------------------------------------------------------------------------------------------------------------------------------
                PalletControl.Parameters.AddWithValue("@Folio", ID.ToString());
                PalletControl.Parameters.AddWithValue("@Reason", Reason.ToString());
                PalletControl.Parameters.AddWithValue("@Datetime", usTime.ToString());
                PalletControl.Parameters.AddWithValue("@Active", true);
                PalletControl.Parameters.AddWithValue("@Company","HISENSE");
                PalletControl.ExecuteNonQuery();
                DBSPP.Close();

                //query message
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
                return RedirectToAction("RequestContainer", "HISENSE");
            }
        }
    }
}