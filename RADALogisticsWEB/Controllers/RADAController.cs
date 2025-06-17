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

        List<Movement> Movements = new List<Movement>();

        List<UsuarioRada> UsuarioRadasss = new List<UsuarioRada>();

        List<string> filtroAreas = new List<string>();
        List<Areas> getAreas = new List<Areas>();

        //connection SQL server (database)
        SqlConnection DBSPP = new SqlConnection("Data Source=RADAEmpire.mssql.somee.com ;Initial Catalog=RADAEmpire ;User ID=RooRada; password=rada1311");
        SqlCommand con = new SqlCommand();
        SqlDataReader dr;
        // GET: RADA


        [HttpGet]
        public JsonResult GetGruaRequest(string area)
        {
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

        public ActionResult Changes(string ID, string oldArea, string Area, string Type, string ActivoRampa,
             string Container,string ActivoHidden) 
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
                using (SqlConnection conn = new SqlConnection("Data Source=RADAEmpire.mssql.somee.com ;Initial Catalog=RADAEmpire ;User ID=RooRada; password=rada1311"))
                {
                    conn.Open();
                    //Delete process
                    string deleteQuery = @"DELETE FROM [RADAEmpire].[dbo].[RADAEmpires_DZDetailsHisense]
                           WHERE Folio = @Folio";

                    using (SqlCommand cmd = new SqlCommand(deleteQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@Folio", "MOV1658");
                        int rowsAffected = cmd.ExecuteNonQuery();
                    }
                    conn.Close();

                    List<string> lista = new List<string>();

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
                    conn.Close();
                    ViewBag.ID = ID;

                    // Obtener hora en US (Rosarito - Pacific Time)
                    TimeZoneInfo usTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                    DateTime usTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, usTimeZone);
                    string dateOnly = usTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                    string timeOnly = usTime.ToString("HH:mm:ss");

                    //query message ------------------------------------------------------------------------------------------------------------------
                    string updateQuery = "UPDATE RADAEmpire_BRequestContainers SET shift = @shift WHERE Folio = @ID";
                    using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                    {
                        DBSPP.Open();
                        command.Parameters.AddWithValue("@shift", Area.ToUpper());
                        command.Parameters.AddWithValue("@ID", ID.ToString());
                        int rowsAffected = command.ExecuteNonQuery();
                        DBSPP.Close();
                    }

                    //    // Insertar en RADAEmpire_BRequestContainers
                    //    using (SqlCommand insertReq = new SqlCommand(@"INSERT INTO RADAEmpire_BRequestContainers 
                    //(Placa,Urgencia, Folio, Who_Send, Container, Destination_Location, Origins_Location, Status, message, shift, Date, Datetime, Active, GruaRequest, RaRRequest)
                    //VALUES (@Placa,@Urgencia, @Folio, @Who_Send, @Container, @Destination_Location, @Origins_Location, @Status, @message, @shift, @Date, @Datetime, @Active, @GruaRequest, @RaRRequest)", conn))
                    //    {
                    //        insertReq.Parameters.AddWithValue("@Urgencia", NivelUrgencia.ToString());
                    //        insertReq.Parameters.AddWithValue("@Placa", Placa?.ToString() ?? "NO SE REGISTRO");
                    //        insertReq.Parameters.AddWithValue("@Folio", Folio);
                    //        insertReq.Parameters.AddWithValue("@Who_Send", User);
                    //        insertReq.Parameters.AddWithValue("@Container", Container.ToUpper());
                    //        insertReq.Parameters.AddWithValue("@Destination_Location", Destination.ToUpper());
                    //        insertReq.Parameters.AddWithValue("@Origins_Location", Origins.ToUpper());
                    //        insertReq.Parameters.AddWithValue("@Status", Type);
                    //        insertReq.Parameters.AddWithValue("@message", "PENDING");
                    //        insertReq.Parameters.AddWithValue("@shift", Area);
                    //        insertReq.Parameters.AddWithValue("@Date", dateOnly);
                    //        insertReq.Parameters.AddWithValue("@Datetime", timeOnly);
                    //        insertReq.Parameters.AddWithValue("@Active", true);
                    //        insertReq.Parameters.AddWithValue("@GruaRequest", ActivoHidden);
                    //        insertReq.Parameters.AddWithValue("@RaRRequest", ActivoRampa);

                    //        insertReq.ExecuteNonQuery();
                    //    }

                    //// Insertar en RADAEmpire_BRequestContainers
                    //using (SqlCommand insertReq = new SqlCommand(@"INSERT INTO RADAERmpire_AMessagesUrgent 
                    // (Folio,Status,Description,Active,Dateadded,DateTimeadded)
                    //   VALUES 
                    // (@Folio,@Status,@Description,@Active,@Dateadded,@DateTimeadded)", conn))
                    //{
                    //    insertReq.Parameters.AddWithValue("@Folio", Folio.ToString());
                    //    insertReq.Parameters.AddWithValue("@Status", NivelUrgencia.ToString());
                    //    insertReq.Parameters.AddWithValue("@Description", MotivoUrgencia.ToUpper());
                    //    insertReq.Parameters.AddWithValue("@Active", true);
                    //    insertReq.Parameters.AddWithValue("@Dateadded", usTime.ToString());
                    //    insertReq.Parameters.AddWithValue("@DateTimeadded", usTime.ToString());

                    //    insertReq.ExecuteNonQuery();
                    //}

                    //    // Insertar en RADAEmpire_CEntryContrainers
                    //    using (SqlCommand insertEntry = new SqlCommand(@"INSERT INTO RADAEmpire_CEntryContrainers
                    //(Folio_Request, Username, Time_Confirm, Choffer, FastCard, Time_Finished, Date, AreaWork, Active, Cancel)
                    //VALUES (@Folio_Request, @Username, @Time_Confirm, @Choffer, @FastCard, @Time_Finished, @Date, @AreaWork, @Active, @Cancel)", conn))
                    //    {
                    //        insertEntry.Parameters.AddWithValue("@Folio_Request", Folio);
                    //        insertEntry.Parameters.AddWithValue("@Username", "PENDNING CONFIRM");
                    //        insertEntry.Parameters.AddWithValue("@Time_Confirm", "00:00:00");
                    //        insertEntry.Parameters.AddWithValue("@Choffer", "PENDNING CONFIRM");
                    //        insertEntry.Parameters.AddWithValue("@FastCard", "PENDNING CONFIRM");
                    //        insertEntry.Parameters.AddWithValue("@Time_Finished", "00:00:00");
                    //        insertEntry.Parameters.AddWithValue("@Date", dateOnly);
                    //        insertEntry.Parameters.AddWithValue("@AreaWork", "RADALogistics");
                    //        insertEntry.Parameters.AddWithValue("@Active", true);
                    //        insertEntry.Parameters.AddWithValue("@Cancel", false);

                    //        insertEntry.ExecuteNonQuery();
                    //    }

                    // Insertar pasos en RADAEmpires_DZDetailsHisense

                    conn.Open();
                    foreach (var paso in lista)
                    {
                        using (SqlCommand insertPaso = new SqlCommand(@"INSERT INTO RADAEmpires_DZDetailsHisense 
                    (Folio, Type_StatusContainer, GruaMov, Process_Movement, End_date, Status, Comment, Date_Process, Activo)
                    VALUES (@Folio, @Type_StatusContainer, @GruaMov, @Process_Movement, @End_date, @Status, @Comment, GETDATE(), @Activo)", conn))
                        {
                            insertPaso.Parameters.AddWithValue("@Folio", ID);
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

                return RedirectToAction("EntryContainer", "RADA");
            }
        }

        public ActionResult ChangeArea(string ID)
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
                string Status = null, shif = null;
                SqlCommand asiggne = new SqlCommand("Select * from RADAEmpire_BRequestContainers " +
                    " where Active = '1' and Folio = '" + ID + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drasiggne = asiggne.ExecuteReader();
                if (drasiggne.HasRows)
                {
                    while (drasiggne.Read())
                    {
                        shif = drasiggne["shift"].ToString();
                    }
                }
                DBSPP.Close();

                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select top (100) * from RADAEmpire_AAreas where Active = '1' order by ID desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    getAreas.Add(new Areas()
                    {
                        id = (dr["ID"].ToString()),
                        Who_create = (dr["Who_create"].ToString()),
                        Name = (dr["Name"].ToString()),
                        Datetime = (dr["Datetime"].ToString()),
                    });
                }
                DBSPP.Close();

                ViewBag.Records = getAreas;
                ViewBag.Shiftws = shif.ToUpper();
                ViewBag.ID = ID.ToString();
                return View();
            }
        }

        public ActionResult ChangeStatus(string ID)
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
                ViewBag.ID = ID.ToString();
                return View();
            }
        }

        public ActionResult MoreOptions(string ID)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] == null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            if (Session["Type"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Type"] = Request.Cookies["UserCookie"].Value;
            }

            //create generate randoms int value
            string Status = null;
            SqlCommand asiggne = new SqlCommand("Select * from RADAEmpire_BRequestContainers where Active = '1' and Folio = '" + ID.ToString() + "'", DBSPP);
            DBSPP.Open();
            SqlDataReader drasiggne = asiggne.ExecuteReader();
            if (drasiggne.HasRows)
            {
                while (drasiggne.Read())
                {
                    Status = drasiggne["message"].ToString();
                }
            }
            DBSPP.Close();

            //create generate randoms int value
            string MENSAJE = null, ESTADO = null;
            SqlCommand querys = new SqlCommand("Select * from RADAERmpire_AMessagesUrgent where Active = '1' and Folio = '" + ID.ToString() + "'", DBSPP);
            DBSPP.Open();
            SqlDataReader drquerys = querys.ExecuteReader();
            if (drquerys.HasRows)
            {
                while (drquerys.Read())
                {
                    ESTADO = drquerys["Status"].ToString();
                    MENSAJE = drquerys["Description"].ToString();
                }
            }
            else
            {
                ESTADO = "Normal";
                MENSAJE = "MOVIMIENTO NORMAL";
            }
            DBSPP.Close();

            ViewBag.User = Session["Username"];
            ViewBag.Type = Session["Type"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                ViewBag.ESTADO = ESTADO.ToString();
                ViewBag.MENSAJE = MENSAJE.ToString();
                ViewBag.Comment = Status.ToString();
                ViewBag.ID = ID.ToString();
                return View();
            }
        }

        public PartialViewResult mov2(string Folio)
        {
            string name = Session["Username"].ToString();

            // Convertir hora si es necesario
            DateTime germanTime = DateTime.UtcNow.AddHours(0);
            TimeZoneInfo usEasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            DateTime usTime = TimeZoneInfo.ConvertTime(germanTime, usEasternTimeZone);

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
                if (UsuarioRadasss.Count > 0)
                {
                    UsuarioRadasss.Clear();
                }
                else
                {
                    DBSPP.Open();
                    con.Connection = DBSPP;
                    con.CommandText = @"SELECT DISTINCT 
    A.[Foio] AS Folio, 
    A.[Choffer] AS Chofer, 
    B.[message]
FROM [RADAEmpire].[dbo].[RADAEmpires_DZChofferMovement] A 
LEFT JOIN [RADAEmpire].[dbo].[RADAEmpire_BRequestContainers] B 
    ON A.[Foio] = B.[Folio] 
WHERE A.[Active] = '1' 
ORDER BY A.[Foio] DESC;
";
                    dr = con.ExecuteReader();
                    while (dr.Read())
                    {
                        UsuarioRadasss.Add(new UsuarioRada()
                        {
                            Username = dr["Chofer"].ToString(),
                            message = dr["message"].ToString(),
                            Mov = dr["Folio"].ToString()
                        });
                    }
                    DBSPP.Close();
                }

                ViewBag.RadaUsers = UsuarioRadasss;
            }
            else
            {
                // Paso 1: Obtener áreas del usuario
                List<string> Areas = new List<string>();
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

                List<UsuarioRada> usuarios = new List<UsuarioRada>();

                if (Areas.Any())
                {
                    string queryUsers = @"
    WITH UltimosMovimientos AS (
        SELECT 
            A.Foio AS Folio,
            A.Choffer AS Chofer,
            ROW_NUMBER() OVER (PARTITION BY A.Choffer ORDER BY A.Datetime DESC) AS rn
        FROM RADAEmpires_DZChofferMovement A
        INNER JOIN RADAEmpire_ARolesChoffer R ON A.Choffer = R.Username
        WHERE A.Active = 1 AND R.Areas IN ({0})
    )
    SELECT 
        UM.Folio,
        UM.Chofer,
        B.message
    FROM UltimosMovimientos UM
    LEFT JOIN (
        SELECT Folio, message
        FROM (
            SELECT Folio, message,
                   ROW_NUMBER() OVER (PARTITION BY Folio ORDER BY FechaRegistro DESC) AS rn
            FROM RADAEmpire_BRequestContainers
        ) t
        WHERE rn = 1
    ) B ON UM.Folio = B.Folio
    WHERE UM.rn = 1
    ORDER BY UM.Folio DESC;
    ";

                    List<string> parametros = new List<string>();
                    for (int i = 0; i < Areas.Count; i++)
                    {
                        parametros.Add($"@area{i}");
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
                                usuarios.Add(new UsuarioRada
                                {
                                    Username = reader["Chofer"].ToString(),
                                    message = reader["message"]?.ToString(),
                                    Mov = reader["Folio"]?.ToString()
                                });
                            }
                        }
                        DBSPP.Close();
                    }
                }

                ViewBag.RadaUsers = usuarios;

            }

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
                return PartialView("table", ViewBag.Records);  // Luego, pasa la lista a la vista
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
                return PartialView("table", ViewBag.Records);  // Luego, pasa la lista a la vista
            }
        }

        public PartialViewResult mov(string Folio)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null && Session["Type"] == null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
                Session["Type"] = Request.Cookies["UserCookie"].Value;
            }

            ViewBag.User = Session["Username"];
            ViewBag.Type = Session["Type"];

            DBSPP.Open();
            con.Connection = DBSPP;
            con.CommandText = "  Select top (100) " +
                " a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date, a.shift as Area,a.GruaRequest as Grua,  a.RaRRequest as RaR " +
                " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio where a.Folio = '" + Folio.ToString() + "' and a.Active = '1' ORDER by a.Folio desc";
            dr = con.ExecuteReader();
            while (dr.Read())
            {
                GetRecords.Add(new Historial()
                {
                    RequestGrua = (dr["Grua"].ToString()),
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
                    RaR = (dr["RaR"].ToString()),
                });
            }
            DBSPP.Close();

            ViewBag.Records = GetRecords; // Obtener nuevamente los datos
            ViewBag.Count = GetRecords.Count.ToString();
            return PartialView("table", ViewBag.Records);
        }

        public ActionResult Comments(string ID, string Status)
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
                ViewBag.Status = Status;
                ViewBag.id = ID;
                return View();
            }
        }

        public ActionResult GetComments(string Status, string id, string Comment)
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
                if (Status == "PENDING")
                {
                    return RedirectToAction("EntryContainer", "RADA");
                }
                else
                {
                    //query message ------------------------------------------------------------------------------------------------------------------
                    string updateQuery = "UPDATE RADAEmpires_DZDetailsHisense SET Comment = @Comment WHERE Folio = @ID and Process_Movement = @Process_Movement";
                    using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                    {
                        DBSPP.Open();
                        command.Parameters.AddWithValue("@Comment", Comment.ToUpper());
                        command.Parameters.AddWithValue("@ID", id.ToString());
                        command.Parameters.AddWithValue("@Process_Movement", Status);
                        int rowsAffected = command.ExecuteNonQuery();
                        DBSPP.Close();
                    }

                    return RedirectToAction("EntryContainer", "RADA");
                }
            }
        }

        public ActionResult Query(string fecha)
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

        public ActionResult Replace(string ID)
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

            string Contenedor = null;
            SqlCommand asiggne = new SqlCommand("Select Container from RADAEmpire_BRequestContainers where Active = '1' and Folio = '" + ID.ToString() + "'", DBSPP);
            DBSPP.Open();
            SqlDataReader drasiggne = asiggne.ExecuteReader();
            if (drasiggne.HasRows)
            {
                while (drasiggne.Read())
                {
                    Contenedor = drasiggne["Container"].ToString();
                }
            }
            DBSPP.Close();

            ViewBag.ID = ID.ToString();
            ViewBag.Contenedor = Contenedor.ToString();

            return View();
        }

        [HttpPost]
        public ActionResult ReplaceProcess(string OldID, string OldContainer, string NewContainer, bool guardarAnterior)
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

            if (guardarAnterior == true)
            {
                //Generar nuevo reporte de contenedor anterior
                string folio = null, whosend = null, destination = null, 
                    origins = null, status = null, shift = null,
                    date = null, datetimeadded = null, grua = null, rAR = null;

                SqlCommand asiggne = new SqlCommand("Select * from RADAEmpire_BRequestContainers where Active = '1' and Folio = '" + OldID.ToString() + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drasiggne = asiggne.ExecuteReader();
                if (drasiggne.HasRows)
                {
                    while (drasiggne.Read())
                    {
                        folio = drasiggne["Folio"].ToString();
                        whosend = drasiggne["Who_Send"].ToString();
                        //container = drasiggne["Container"].ToString();
                        destination = drasiggne["Destination_Location"].ToString();
                        origins = drasiggne["Origins_Location"].ToString();
                        status = drasiggne["Status"].ToString();
                        //message = drasiggne["Container"].ToString();
                        shift = drasiggne["shift"].ToString();
                        date = drasiggne["Date"].ToString();
                        datetimeadded = drasiggne["Datetime"].ToString();
                        //Active = drasiggne["Container"].ToString();
                        grua = drasiggne["GruaRequest"].ToString();
                        rAR = drasiggne["RaRRequest"].ToString();

                    }
                }
                DBSPP.Close();

                // Crear la lista
                List<string> lista = new List<string>();

                // Consulta SQL
                string query = @"SELECT Process_Movement 
                 FROM RADAEmpires_DZDetailsHisense 
                 WHERE Activo = '1' AND Folio = '" + OldID.ToString() + "' ORDER BY ID ASC";

                SqlCommand Moviments = new SqlCommand(query, DBSPP);

                DBSPP.Open();
                SqlDataReader drMoviments = Moviments.ExecuteReader();

                // Leer y agregar a la lista
                while (drMoviments.Read())
                {
                    string movimiento = drMoviments["Process_Movement"].ToString();
                    lista.Add(movimiento);
                }

                drMoviments.Close(); // Cierra primero el lector
                DBSPP.Close();

                //create generate randoms int value
                string randomval = null;
                SqlCommand conse = new SqlCommand("Select top (1) ID from RADAEmpire_BRequestContainers order by ID desc", DBSPP);
                DBSPP.Open();
                SqlDataReader drconse = conse.ExecuteReader();
                if (drconse.HasRows)
                {
                    while (drconse.Read())
                    {
                        randomval = drconse[0].ToString();//234 + 1216 + 
                    }
                }
                else
                {
                    randomval = "0";
                }
                DBSPP.Close();

                int sum = int.Parse(randomval) + 1;

                //Random folio = new Random();
                //int val = folio.Next(1, 1000000000);
                string Folio = null;
                Folio = "MOV" + sum.ToString();

                // Obtener la fecha y hora actual en Alemania (zona horaria UTC+1 o UTC+2 dependiendo del horario de verano)
                DateTime germanTime = DateTime.UtcNow.AddHours(0);  // Alemania es UTC+1

                // Convertir la hora alemana a la hora en una zona horaria específica de EE. UU. (por ejemplo, Nueva York, UTC-5)
                TimeZoneInfo usEasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                DateTime usTime = TimeZoneInfo.ConvertTime(germanTime, usEasternTimeZone);

                // Formatear la fecha para que sea adecuada para la base de datos
                string formattedDate = usTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);


                //Guardar informacion a la base de datos del proyecto
                DBSPP.Open();
                SqlCommand PalletControl = new SqlCommand("insert into RADAEmpire_BRequestContainers" +
                    "(Folio, Who_Send, Container, Destination_Location, Origins_Location, Status, message, shift, Date, Datetime, Active,GruaRequest,RaRRequest) values " +
                    "(@Folio, @Who_Send, @Container, @Destination_Location, @Origins_Location, @Status, @message, @shift, @Date, @Datetime, @Active,@GruaRequest,@RaRRequest) ", DBSPP);
                //--------------------------------------------------------------------------------------------------------------------------------
                PalletControl.Parameters.AddWithValue("@Folio", Folio.ToString());
                PalletControl.Parameters.AddWithValue("@Who_Send", whosend.ToString());
                PalletControl.Parameters.AddWithValue("@Container", OldContainer.ToUpper());
                PalletControl.Parameters.AddWithValue("@Destination_Location", destination.ToUpper());
                PalletControl.Parameters.AddWithValue("@Origins_Location", origins.ToUpper());
                PalletControl.Parameters.AddWithValue("@Status", status.ToString());
                PalletControl.Parameters.AddWithValue("@message", "PENDING");
                PalletControl.Parameters.AddWithValue("@shift", shift.ToString());
                PalletControl.Parameters.AddWithValue("@Date", date.ToString());
                PalletControl.Parameters.AddWithValue("@Datetime", datetimeadded.ToString());
                PalletControl.Parameters.AddWithValue("@Active", true);
                PalletControl.Parameters.AddWithValue("@GruaRequest", grua.ToString());
                PalletControl.Parameters.AddWithValue("@RaRRequest", rAR.ToString());
                PalletControl.ExecuteNonQuery();
                DBSPP.Close();

                //Guardar informacion a la base de datos del proyecto
                DBSPP.Open();
                SqlCommand RADAdocument = new SqlCommand("insert into RADAEmpire_CEntryContrainers" +
                    "(Folio_Request, Username, Time_Confirm, Choffer, FastCard, Time_Finished, Date,AreaWork, Active,Cancel) values " +
                    "(@Folio_Request, @Username, @Time_Confirm, @Choffer, @FastCard, @Time_Finished, @Date,@AreaWork, @Active, @Cancel) ", DBSPP);
                //--------------------------------------------------------------------------------------------------------------------------------
                RADAdocument.Parameters.AddWithValue("@Folio_Request", Folio.ToString());
                RADAdocument.Parameters.AddWithValue("@Username", "PENDNING CONFIRM");
                RADAdocument.Parameters.AddWithValue("@Time_Confirm", "00:00:00");
                RADAdocument.Parameters.AddWithValue("@Choffer", "PENDNING CONFIRM");
                RADAdocument.Parameters.AddWithValue("@FastCard", "PENDNING CONFIRM");
                RADAdocument.Parameters.AddWithValue("@Time_Finished", "00:00:00");
                RADAdocument.Parameters.AddWithValue("@Date", usTime.ToString());
                RADAdocument.Parameters.AddWithValue("@AreaWork", "RADALogistics");
                RADAdocument.Parameters.AddWithValue("@Active", true);
                RADAdocument.Parameters.AddWithValue("@Cancel", false);
                RADAdocument.ExecuteNonQuery();
                DBSPP.Close();

                // Guardar información de cada paso en la base de datos
                DBSPP.Open();
                foreach (string paso in lista)
                {
                    SqlCommand ProcessDB = new SqlCommand("INSERT INTO RADAEmpires_DZDetailsHisense " +
                        "(Folio, Type_StatusContainer, GruaMov, Process_Movement, End_date, Status, Comment, Date_Process, Activo) " +
                        "VALUES (@Folio, @Type_StatusContainer, @GruaMov, @Process_Movement, @End_date, @Status, @Comment, GETDATE(), @Activo)", DBSPP);

                    ProcessDB.Parameters.AddWithValue("@Folio", Folio.ToString());
                    ProcessDB.Parameters.AddWithValue("@Type_StatusContainer", status.ToString());
                    ProcessDB.Parameters.AddWithValue("@GruaMov", grua.ToString());
                    ProcessDB.Parameters.AddWithValue("@Process_Movement", paso); // Aquí se guarda cada paso de la lista
                    ProcessDB.Parameters.AddWithValue("@End_date", "00:00:00");
                    ProcessDB.Parameters.AddWithValue("@Status", "PENDIENTE");
                    ProcessDB.Parameters.AddWithValue("@Comment", "SIN COMENTARIOS");
                    ProcessDB.Parameters.AddWithValue("@Activo", true);

                    ProcessDB.ExecuteNonQuery();
                }
                DBSPP.Close();

                //Actualizar numero de contenedor
                string updateQuery = "UPDATE RADAEmpire_BRequestContainers " +
                    " SET Container = @Container WHERE Folio = @ID";
                using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                {
                    DBSPP.Open();
                    command.Parameters.AddWithValue("@Container", NewContainer.ToUpper());
                    command.Parameters.AddWithValue("@ID", OldID.ToString());
                    int rowsAffected = command.ExecuteNonQuery();
                    DBSPP.Close();
                }

            }
            else
            {
                //Generar nuevo reporte de contenedor anterior
                string folio = null, whosend = null, destination = null,
                    origins = null, status = null, shift = null,
                    date = null, datetimeadded = null, grua = null, rAR = null;

                SqlCommand asiggne = new SqlCommand("Select * from RADAEmpire_BRequestContainers where Active = '1' and Folio = '" + OldID.ToString() + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drasiggne = asiggne.ExecuteReader();
                if (drasiggne.HasRows)
                {
                    while (drasiggne.Read())
                    {
                        folio = drasiggne["Folio"].ToString();
                        whosend = drasiggne["Who_Send"].ToString();
                        //container = drasiggne["Container"].ToString();
                        destination = drasiggne["Destination_Location"].ToString();
                        origins = drasiggne["Origins_Location"].ToString();
                        status = drasiggne["Status"].ToString();
                        //message = drasiggne["Container"].ToString();
                        shift = drasiggne["shift"].ToString();
                        date = drasiggne["Date"].ToString();
                        datetimeadded = drasiggne["Datetime"].ToString();
                        //Active = drasiggne["Container"].ToString();
                        grua = drasiggne["GruaRequest"].ToString();
                        rAR = drasiggne["RaRRequest"].ToString();
                    }
                }
                DBSPP.Close();

                // Crear la lista
                List<string> lista = new List<string>();

                // Consulta SQL
                string query = @"SELECT Process_Movement 
                 FROM RADAEmpires_DZDetailsHisense 
                 WHERE Activo = '1' AND Folio = '" + OldID.ToString() + "' ORDER BY ID ASC";

                SqlCommand Moviments = new SqlCommand(query, DBSPP);

                DBSPP.Open();
                SqlDataReader drMoviments = Moviments.ExecuteReader();

                // Leer y agregar a la lista
                while (drMoviments.Read())
                {
                    string movimiento = drMoviments["Process_Movement"].ToString();
                    lista.Add(movimiento);
                }

                drMoviments.Close(); // Cierra primero el lector
                DBSPP.Close();

                //create generate randoms int value
                string randomval = null;
                SqlCommand conse = new SqlCommand("Select top (1) ID from RADAEmpire_BRequestContainers order by ID desc", DBSPP);
                DBSPP.Open();
                SqlDataReader drconse = conse.ExecuteReader();
                if (drconse.HasRows)
                {
                    while (drconse.Read())
                    {
                        randomval = drconse[0].ToString();//234 + 1216 + 
                    }
                }
                else
                {
                    randomval = "0";
                }
                DBSPP.Close();

                int sum = int.Parse(randomval) + 1;

                //Random folio = new Random();
                //int val = folio.Next(1, 1000000000);
                string Folio = null;
                Folio = "MOV" + sum.ToString();

                // Obtener la fecha y hora actual en Alemania (zona horaria UTC+1 o UTC+2 dependiendo del horario de verano)
                DateTime germanTime = DateTime.UtcNow.AddHours(0);  // Alemania es UTC+1

                // Convertir la hora alemana a la hora en una zona horaria específica de EE. UU. (por ejemplo, Nueva York, UTC-5)
                TimeZoneInfo usEasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                DateTime usTime = TimeZoneInfo.ConvertTime(germanTime, usEasternTimeZone);

                // Formatear la fecha para que sea adecuada para la base de datos
                string formattedDate = usTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);


                //Guardar informacion a la base de datos del proyecto
                DBSPP.Open();
                SqlCommand PalletControl = new SqlCommand("insert into RADAEmpire_BRequestContainers" +
                    "(Folio, Who_Send, Container, Destination_Location, Origins_Location, Status, message, shift, Date, Datetime, Active,GruaRequest, RaRRequest) values " +
                    "(@Folio, @Who_Send, @Container, @Destination_Location, @Origins_Location, @Status, @message, @shift, @Date, @Datetime, @Active,@GruaRequest, @RaRRequest) ", DBSPP);
                //--------------------------------------------------------------------------------------------------------------------------------
                PalletControl.Parameters.AddWithValue("@Folio", Folio.ToString());
                PalletControl.Parameters.AddWithValue("@Who_Send", whosend.ToString());
                PalletControl.Parameters.AddWithValue("@Container", OldContainer.ToUpper());
                PalletControl.Parameters.AddWithValue("@Destination_Location", destination.ToUpper());
                PalletControl.Parameters.AddWithValue("@Origins_Location", origins.ToUpper());
                PalletControl.Parameters.AddWithValue("@Status", status.ToString());
                PalletControl.Parameters.AddWithValue("@message", "Canceled by Rada");
                PalletControl.Parameters.AddWithValue("@shift", shift.ToString());
                PalletControl.Parameters.AddWithValue("@Date", date.ToString());
                PalletControl.Parameters.AddWithValue("@Datetime", datetimeadded.ToString());
                PalletControl.Parameters.AddWithValue("@Active", true);
                PalletControl.Parameters.AddWithValue("@GruaRequest", grua.ToString());
                PalletControl.Parameters.AddWithValue("@RaRRequest", rAR.ToString());
                PalletControl.ExecuteNonQuery();
                DBSPP.Close();

                DBSPP.Open();
                SqlCommand GuardaRegistro = new SqlCommand("insert into RADAEmpires_DRemoves" +
                    "(Folio, Reason, Datetime, Active,Company) values " +
                    "(@Folio, @Reason, @Datetime, @Active,@Company) ", DBSPP);
                //--------------------------------------------------------------------------------------------------------------------------------
                GuardaRegistro.Parameters.AddWithValue("@Folio", Folio.ToString());
                GuardaRegistro.Parameters.AddWithValue("@Reason", "Movimiento Remplazado por un contenedor nuevo");
                GuardaRegistro.Parameters.AddWithValue("@Datetime", usTime.ToString());
                GuardaRegistro.Parameters.AddWithValue("@Active", true);
                GuardaRegistro.Parameters.AddWithValue("@Company", "RADA");
                GuardaRegistro.ExecuteNonQuery();
                DBSPP.Close();

                //query message
                string updateQuer3y = "UPDATE RADAEmpire_CEntryContrainers SET Cancel = @Cancel WHERE Folio_Request = @ID";
                using (SqlCommand comdmand = new SqlCommand(updateQuer3y, DBSPP))
                {
                    DBSPP.Open();
                    comdmand.Parameters.AddWithValue("@ID", Folio);
                    comdmand.Parameters.AddWithValue("@Cancel", true);
                    int rowsAffected = comdmand.ExecuteNonQuery();
                    DBSPP.Close();
                }

                //Guardar informacion a la base de datos del proyecto
                DBSPP.Open();
                SqlCommand RADAdocument = new SqlCommand("insert into RADAEmpire_CEntryContrainers" +
                    "(Folio_Request, Username, Time_Confirm, Choffer, FastCard, Time_Finished, Date,AreaWork, Active,Cancel) values " +
                    "(@Folio_Request, @Username, @Time_Confirm, @Choffer, @FastCard, @Time_Finished, @Date,@AreaWork, @Active, @Cancel) ", DBSPP);
                //--------------------------------------------------------------------------------------------------------------------------------
                RADAdocument.Parameters.AddWithValue("@Folio_Request", Folio.ToString());
                RADAdocument.Parameters.AddWithValue("@Username", "PENDNING CONFIRM");
                RADAdocument.Parameters.AddWithValue("@Time_Confirm", "00:00:00");
                RADAdocument.Parameters.AddWithValue("@Choffer", "PENDNING CONFIRM");
                RADAdocument.Parameters.AddWithValue("@FastCard", "PENDNING CONFIRM");
                RADAdocument.Parameters.AddWithValue("@Time_Finished", "00:00:00");
                RADAdocument.Parameters.AddWithValue("@Date", usTime.ToString());
                RADAdocument.Parameters.AddWithValue("@AreaWork", "RADALogistics");
                RADAdocument.Parameters.AddWithValue("@Active", true);
                RADAdocument.Parameters.AddWithValue("@Cancel", false);
                RADAdocument.ExecuteNonQuery();
                DBSPP.Close();

                // Guardar información de cada paso en la base de datos
                DBSPP.Open();
                foreach (string paso in lista)
                {
                    SqlCommand ProcessDB = new SqlCommand("INSERT INTO RADAEmpires_DZDetailsHisense " +
                        "(Folio, Type_StatusContainer, GruaMov, Process_Movement, End_date, Status, Comment, Date_Process, Activo) " +
                        "VALUES (@Folio, @Type_StatusContainer, @GruaMov, @Process_Movement, @End_date, @Status, @Comment, GETDATE(), @Activo)", DBSPP);

                    ProcessDB.Parameters.AddWithValue("@Folio", Folio.ToString());
                    ProcessDB.Parameters.AddWithValue("@Type_StatusContainer", status.ToString());
                    ProcessDB.Parameters.AddWithValue("@GruaMov", grua.ToString());
                    ProcessDB.Parameters.AddWithValue("@Process_Movement", paso); // Aquí se guarda cada paso de la lista
                    ProcessDB.Parameters.AddWithValue("@End_date", "00:00:00");
                    ProcessDB.Parameters.AddWithValue("@Status", "PENDIENTE");
                    ProcessDB.Parameters.AddWithValue("@Comment", "SIN COMENTARIOS");
                    ProcessDB.Parameters.AddWithValue("@Activo", true);

                    ProcessDB.ExecuteNonQuery();
                }
                DBSPP.Close();

                //Actualizar numero de contenedor
                string updateQuery = "UPDATE RADAEmpire_BRequestContainers " +
                    " SET Container = @Container WHERE Folio = @ID";
                using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                {
                    DBSPP.Open();
                    command.Parameters.AddWithValue("@Container", NewContainer.ToUpper());
                    command.Parameters.AddWithValue("@ID", OldID.ToString());
                    int rowsAffected = command.ExecuteNonQuery();
                    DBSPP.Close();
                }

            }

            return RedirectToAction("EntryContainer", "RADA");
        }

        public ActionResult EntryContainer(string query, string date)
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

                if (query == "1")
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
                        if (UsuarioRadasss.Count > 0)
                        {
                            UsuarioRadasss.Clear();
                        }
                        else
                        {
                            DBSPP.Open();
                            con.Connection = DBSPP;
                            con.CommandText = "SELECT A.[Foio] AS Folio, A.[Choffer] AS Chofer,B.[message] " +
                                " FROM [RADAEmpire].[dbo].[RADAEmpires_DZChofferMovement] " +
                                " A LEFT JOIN [RADAEmpire].[dbo].[RADAEmpire_BRequestContainers] " +
                                " B ON A.[Foio] = B.[Folio]" +
                                " WHERE A.[Active] = '1' ORDER BY  A.[Foio] DESC;";
                            dr = con.ExecuteReader();
                            while (dr.Read())
                            {
                                UsuarioRadasss.Add(new UsuarioRada()
                                {
                                    Username = dr["Chofer"].ToString(),
                                    message = dr["message"].ToString(),
                                    Mov = dr["Folio"].ToString()
                                });
                            }
                            DBSPP.Close();
                        }

                        ViewBag.RadaUsers = UsuarioRadasss;
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
                            string queryUsers = @"
                                SELECT 
                                  A.[Foio] AS Folio,
                                  A.[Choffer] AS Chofer,
                                  B.[message]
                                FROM 
                                [RADAEmpire].[dbo].[RADAEmpires_DZChofferMovement] A
                                LEFT JOIN 
                                [RADAEmpire].[dbo].[RADAEmpire_BRequestContainers] B 
                                ON A.[Foio] = B.[Folio]
                                INNER JOIN 
                                [RADAEmpire].[dbo].[RADAEmpire_ARolesChoffer] R 
                                ON A.[Choffer] = R.[Username]
                                WHERE 
                                A.[Active] = '1'
                                AND R.[Areas] IN ({0})  -- Reemplazar dinámicamente por parámetros
                                ORDER BY  
                                 A.[Foio] DESC;"
                            ;

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
                                        usuarios.Add(new UsuarioRada
                                        {
                                            Username = reader["Chofer"].ToString(),
                                            message = reader["message"].ToString(),
                                            Mov = reader["Folio"].ToString()
                                        });
                                    }
                                }
                                DBSPP.Close();
                            }
                        }

                        ViewBag.RadaUsers = usuarios;
                    }

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
        SELECT TOP (500)
    a.Urgencia AS Urgencia, 
    a.Folio AS Folio, 
    a.Container AS Container, 
    a.Origins_Location AS Origen, 
    a.Destination_Location AS Destination, 
    a.Status AS Status, 
    a.Datetime AS HSolicitud, 
    b.Time_Confirm AS HConfirm, 
    b.Time_Finished AS HFinish, 
    a.Who_Send AS WhoRequest, 
    b.Choffer AS Choffer, 
    a.message AS Comment, 
    a.Date AS Date, 
    a.shift AS Area, 
    a.GruaRequest AS Grua, 
    a.RaRRequest AS RaR
FROM RADAEmpire_BRequestContainers AS a
INNER JOIN RADAEmpire_CEntryContrainers AS b ON b.Folio_Request = a.Folio
WHERE a.Date = @Date and a.Active = '1'
ORDER BY 
    CASE 
        WHEN a.message = 'Canceled by Rada' THEN 1 ELSE 0
    END,
    CASE 
        WHEN UPPER(a.Urgencia) = 'CRITICO' THEN 0
        WHEN UPPER(a.Urgencia) = 'URGENTE' THEN 1
        WHEN UPPER(a.Urgencia) = 'NORMAL' THEN 2
        ELSE 3
    END,
    CASE 
        WHEN a.message = 'PENDING' THEN 1
        WHEN a.message = 'CHOFER TERMINA MOVIMIENTO' THEN 3
        ELSE 2
    END,
    a.Folio DESC;
;
    ";

                        // Parámetro para evitar SQL Injection
                        con.Parameters.Clear();
                        con.Parameters.AddWithValue("@Date", date);

                        dr = con.ExecuteReader();
                        while (dr.Read())
                        {
                            GetRecordsQuery.Add(new Historial()
                            {
                                Urgencia = (dr["Urgencia"].ToString()),
                                RequestGrua = (dr["Grua"].ToString()),
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
                                RaR = (dr["RaR"].ToString()),
                            });
                        }
                        DBSPP.Close();
                        ViewBag.Records = GetRecordsQuery;
                        ViewBag.Count = GetRecordsQuery.Count.ToString();
                        return View();
                    }
                    else
                    {
                        string UsuarioRada2 = Session["Username"]?.ToString();
                        //create generate randoms int value
                        string rols = null;
                        SqlCommand asiggnes = new SqlCommand("Select Type_user from RADAEmpire_AUsers where Active = '1' and Username = '" + UsuarioRada2.ToString() + "'", DBSPP);
                        DBSPP.Open();
                        SqlDataReader drasiggnes = asiggnes.ExecuteReader();
                        if (drasiggnes.HasRows)
                        {
                            while (drasiggnes.Read())
                            {
                                rols = drasiggnes["Type_user"].ToString();
                            }
                        }
                        DBSPP.Close();

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

                        

                        while (dr.Read())
                        {
                            string areaActual = dr["Area"].ToString();

                            if (filtroAreas.Contains(areaActual))
                            {
                                GetRecordsQuery.Add(new Historial()
                                {
                                    Urgencia = (dr["Urgencia"].ToString()),
                                    RequestGrua = (dr["Grua"].ToString()),
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
                                    RaR = (dr["RaR"].ToString()),
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
                        if (UsuarioRadasss.Count > 0)
                        {
                            UsuarioRadasss.Clear();
                        }
                        else
                        {
                            DBSPP.Open();
                            con.Connection = DBSPP;
                            con.CommandText = "SELECT A.[Foio] AS Folio, A.[Choffer] AS Chofer,B.[message] FROM [RADAEmpire].[dbo].[RADAEmpires_DZChofferMovement] A LEFT JOIN [RADAEmpire].[dbo].[RADAEmpire_BRequestContainers] B ON A.[Foio] = B.[Folio] WHERE A.[Active] = '1' ORDER BY  A.[Foio] DESC;";
                            dr = con.ExecuteReader();
                            while (dr.Read())
                            {
                                UsuarioRadasss.Add(new UsuarioRada()
                                {
                                    Username = dr["Chofer"].ToString(),
                                    message = dr["message"].ToString(),
                                    Mov = dr["Folio"].ToString()
                                });
                            }
                            DBSPP.Close();
                        }

                        ViewBag.RadaUsers = UsuarioRadasss;
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
                            //string queryUsers = "SELECT DISTINCT r.Username AS Choffer,a.Status,m.Foio AS Folio,m.Message " +
                            //    " FROM RADAEmpire_ARolesChoffer r  " +
                            //    "" +
                            //    " LEFT JOIN RADAEmpire_AChoffer a ON r.Username = a.Username AND a.Active = '1' " +
                            //    " LEFT JOIN ( SELECT Choffer, Foio, Message, ROW_NUMBER() OVER (PARTITION BY Choffer ORDER BY Datetime DESC)  " +
                            //    " AS rn FROM RADAEmpires_DZChofferMovement WHERE Active = 1 ) m ON r.Username = m.Choffer AND m.rn = 1 WHERE r.Active = '1' " +
                            //    " AND r.Areas IN ({0}) AND (a.Status = 'CHOFFER EN MOVIMIENTO' OR a.Status IS NULL) ORDER BY r.Username ASC";

                            string queryUsers = @"
                                SELECT 
                                  A.[Foio] AS Folio,
                                  A.[Choffer] AS Chofer,
                                  B.[message]
                                FROM 
                                [RADAEmpire].[dbo].[RADAEmpires_DZChofferMovement] A
                                LEFT JOIN 
                                [RADAEmpire].[dbo].[RADAEmpire_BRequestContainers] B 
                                ON A.[Foio] = B.[Folio]
                                INNER JOIN 
                                [RADAEmpire].[dbo].[RADAEmpire_ARolesChoffer] R 
                                ON A.[Choffer] = R.[Username]
                                WHERE 
                                A.[Active] = '1'
                                AND R.[Areas] IN ({0})  -- Reemplazar dinámicamente por parámetros
                                ORDER BY  
                                 A.[Foio] DESC;"
                            ;

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
                                        usuarios.Add(new UsuarioRada
                                        {
                                            Username = reader["Chofer"].ToString(),
                                            message = reader["message"].ToString(),
                                            Mov = reader["Folio"].ToString()
                                        });
                                    }
                                }
                                DBSPP.Close();
                            }
                        }

                        ViewBag.RadaUsers = usuarios;
                    }

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
            //create generate randoms int value
          string rol = null;
            SqlCommand asiggne = new SqlCommand("Select Type_user from RADAEmpire_AUsers where Active = '1' and Username = '" + name.ToString() + "'", DBSPP);
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
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select * from RADAEmpire_AAreas where Active = '1' order by ID desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    filtroAreas.Add(dr["Name"].ToString());
                }
                DBSPP.Close();

                GetEntryRequest(filtroAreas);
                ViewBag.Records = GetRecords; // Obtener nuevamente los datos
                ViewBag.Count = GetRecords.Count.ToString();
                return PartialView("table", ViewBag.Records);
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
                ViewBag.Records = GetRecords; // Obtener nuevamente los datos
                ViewBag.Count = GetRecords.Count.ToString();
                return PartialView("table", ViewBag.Records);
            }
        }

        [HttpPost]
        public JsonResult ConfirmProcess(string ID, string choffer, string requestGrua, string Comment, string RaR)
        {
            if (Comment == "PENDING")
            {
                string redirectUrl = Url.Action("ViewConfirm", "RADA", new { ID = ID, choffer = choffer, requestGrua = requestGrua });
                return Json(new { success = true, redirectUrl });
            }

            if (Comment == "CHOFER TERMINA MOVIMIENTO")
            {
                return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
            }


            if (Comment == "Canceled by Rada")
            {
                return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
            }

            // Hora local de Alemania (ya viene en UTC del servidor, solo ajustar si es necesario)
            DateTime germanTime = DateTime.UtcNow;
            TimeZoneInfo pacificZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
            DateTime usTime = TimeZoneInfo.ConvertTimeFromUtc(germanTime, pacificZone);

            string estatusActual = null, message = null, shift = null;
            using (SqlCommand cmd = new SqlCommand("SELECT * FROM RADAEmpire_BRequestContainers WHERE Active = '1' AND Folio = @Folio", DBSPP))
            {
                cmd.Parameters.AddWithValue("@Folio", ID);
                DBSPP.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        estatusActual = reader["Status"].ToString();
                        message = reader["message"].ToString();
                        shift = reader["shift"].ToString();
                    }
                }
                DBSPP.Close();
            }

            int count = 0;
            using (SqlCommand cmd = new SqlCommand(@"
        SELECT COUNT(*) AS Total
        FROM RADAEmpires_DZDetailsHisense
        WHERE Activo = '1' AND Folio = @Folio AND Status = 'PENDIENTE'", DBSPP))
            {
                cmd.Parameters.AddWithValue("@Folio", ID);
                DBSPP.Open();
                count = (int)cmd.ExecuteScalar();
                DBSPP.Close();
            }

            if (count == 1)
            {
                // CHOFER TERMINA PROCESO
                EjecutarUpdate("RADAEmpires_DZDetailsHisense", "Process_Movement", "CHOFER TERMINA MOVIMIENTO", ID, usTime);
                ActualizarMensajes(ID, "CHOFER TERMINA MOVIMIENTO");
                ActualizarCampo("RADAEmpire_CEntryContrainers", "Time_Finished", usTime.ToString("HH:mm:ss"), "Folio_Request", ID);
                ActualizarCampo("RADAEmpire_AChoffer", "Status", "SIN MOVIMIENTO", "Username", choffer);
                ActualizarCampo("RADAEmpires_DZChofferMovement", "Active", "0", "Foio", ID); // ¡Verifica si 'Foio' está mal escrito!
            }
            else
            {
                string processMovement = null;
                using (SqlCommand cmd = new SqlCommand(@"
            SELECT TOP 1 Process_Movement
            FROM (
                SELECT ROW_NUMBER() OVER (ORDER BY ID DESC) AS RowNumberInverso, Process_Movement
                FROM RADAEmpires_DZDetailsHisense
                WHERE Activo = 1 AND Folio = @Folio AND Status = 'PENDIENTE'
            ) AS Subquery
            WHERE RowNumberInverso = @RowNumber", DBSPP))
                {
                    cmd.Parameters.AddWithValue("@Folio", ID);
                    cmd.Parameters.AddWithValue("@RowNumber", count);

                    DBSPP.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            processMovement = reader["Process_Movement"].ToString();
                        }
                    }
                    DBSPP.Close();
                }

                if (!string.IsNullOrEmpty(processMovement))
                {
                    EjecutarUpdate("RADAEmpires_DZDetailsHisense", "Process_Movement", processMovement.ToUpper(), ID, usTime);
                    ActualizarMensajes(ID, processMovement.ToUpper());
                }
            }

            return Json(new { success = true });
        }

        private void EjecutarUpdate(string table, string columnFilter, string valueFilter, string folio, DateTime endTime)
        {
            string query = $@"
        UPDATE {table}
        SET End_date = @EndDate, Status = @Status
        WHERE {columnFilter} = @ValueFilter AND Folio = @Folio";

            using (SqlCommand cmd = new SqlCommand(query, DBSPP))
            {
                cmd.Parameters.AddWithValue("@EndDate", endTime.ToString("HH:mm:ss"));
                cmd.Parameters.AddWithValue("@Status", "COMPLETADO");
                cmd.Parameters.AddWithValue("@ValueFilter", valueFilter);
                cmd.Parameters.AddWithValue("@Folio", folio);
                DBSPP.Open();
                cmd.ExecuteNonQuery();
                DBSPP.Close();
            }
        }

        private void ActualizarMensajes(string folio, string mensaje)
        {
            string query = "UPDATE RADAEmpire_BRequestContainers SET message = @Message WHERE Folio = @Folio";

            using (SqlCommand cmd = new SqlCommand(query, DBSPP))
            {
                cmd.Parameters.AddWithValue("@Message", mensaje);
                cmd.Parameters.AddWithValue("@Folio", folio);
                DBSPP.Open();
                cmd.ExecuteNonQuery();
                DBSPP.Close();
            }
        }

        private void ActualizarCampo(string tabla, string campo, string valor, string columnaFiltro, string valorFiltro)
        {
            string query = $"UPDATE {tabla} SET {campo} = @Valor WHERE {columnaFiltro} = @Filtro";
            using (SqlCommand cmd = new SqlCommand(query, DBSPP))
            {
                cmd.Parameters.AddWithValue("@Valor", valor);
                cmd.Parameters.AddWithValue("@Filtro", valorFiltro);
                DBSPP.Open();
                cmd.ExecuteNonQuery();
                DBSPP.Close();
            }
        }

        public ActionResult ViewConfirm(string ID, string choffer, string requestGrua, string RaR)
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

                string area = null, messages = null, WhoSend = null, Container = null, Destination = null, Origins = null, Status = null, DateTimes = null, Date = null;
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
                        DateTimes = drconse["Datetime"].ToString();
                        Date = Convert.ToDateTime(drconse["Date"]).ToString("MM/dd/yyyy");
                        area = drconse["shift"].ToString();
                    }
                }
                else
                {
                    messages = null;
                }
                DBSPP.Close();

                if (messages == "Canceled by Hisense")
                {
                    return Json(new { success = true });
                }
                else
                {
                    if (messages == "Canceled by Rada")
                    {
                        return Json(new { success = true });
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
                            ViewBag.Datetime = DateTimes.ToString();
                            ViewBag.Date = Date.ToString();
                            ViewBag.Username = Session["Username"];
                            ViewBag.id = ID.ToString();
                            ViewBag.area = area.ToString();
                        }

                        // Pasar al ViewBag
                        GetMovement();
                        ViewBag.Records = Movements;
                        return View();
                    }
                }
            }
        }

        public ActionResult UpdateConfirmContainer(string ID, string Choffer, string Username, string area, string Container)
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

                // create generate randoms int value
                string estatusActual = null, message = null;
                SqlCommand statuscontainer = new SqlCommand("Select * from RADAEmpire_BRequestContainers where Active = '1' and Folio = '" + ID.ToString() + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drstatuscontainer = statuscontainer.ExecuteReader();
                if (drstatuscontainer.HasRows)
                {
                    while (drstatuscontainer.Read())
                    {
                        estatusActual = drstatuscontainer["Status"].ToString();
                        message = drstatuscontainer["message"].ToString();
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

                int count = 0;
                using (SqlCommand cmd = new SqlCommand(@"
        SELECT COUNT(*) AS Total
        FROM RADAEmpires_DZDetailsHisense
        WHERE Activo = '1' AND Folio = @Folio AND Status = 'PENDIENTE'", DBSPP))
                {
                    cmd.Parameters.AddWithValue("@Folio", ID);
                    DBSPP.Open();
                    count = (int)cmd.ExecuteScalar();
                    DBSPP.Close();
                }

                string processMovement = null;
                using (SqlCommand cmd = new SqlCommand(@"
            SELECT TOP 1 Process_Movement
            FROM (
                SELECT ROW_NUMBER() OVER (ORDER BY ID DESC) AS RowNumberInverso, Process_Movement
                FROM RADAEmpires_DZDetailsHisense
                WHERE Activo = 1 AND Folio = @Folio AND Status = 'PENDIENTE'
            ) AS Subquery
            WHERE RowNumberInverso = @RowNumber", DBSPP))
                {
                    cmd.Parameters.AddWithValue("@Folio", ID);
                    cmd.Parameters.AddWithValue("@RowNumber", count);

                    DBSPP.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            processMovement = reader["Process_Movement"].ToString();
                        }
                    }
                    DBSPP.Close();
                }

                if (!string.IsNullOrEmpty(processMovement))
                {
                    EjecutarUpdate("RADAEmpires_DZDetailsHisense", "Process_Movement", processMovement.ToUpper(), ID, usTime);
                    ActualizarMensajes(ID, processMovement.ToUpper());
                }

                //------------------------------------------------------------------------------
                //string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                //    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                //using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                //{
                //    DBSPP.Open();
                //    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                //    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                //    coms.Parameters.AddWithValue("@ID", "CHOFER ASIGNADO AL MOVIMIENTO");
                //    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                //    int rowsAffected = coms.ExecuteNonQuery();
                //    DBSPP.Close();
                //}
                ////------------------------------------------------------------------------------
                //string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                //    " message = @message WHERE Folio = @ID";
                //using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                //{
                //    DBSPP.Open();
                //    coms.Parameters.AddWithValue("@message", "CHOFER ASIGNADO AL MOVIMIENTO");
                //    coms.Parameters.AddWithValue("@ID", ID.ToString());
                //    int rowsAffected = coms.ExecuteNonQuery();
                //    DBSPP.Close();
                //}
                //------------------------------------------------------------------------------

                //Guardar informacion a la base de datos del proyecto
                DBSPP.Open();
                SqlCommand PalletControl = new SqlCommand("insert into RADAEmpires_DZChofferMovement" +
                    "(Foio, Choffer, StatusNow, Container, Message, Active, Date,Datetime) values " +
                    "(@Foio, @Choffer, @StatusNow, @Container, @Message, @Active, @Date, @Datetime) ", DBSPP);
                //--------------------------------------------------------------------------------------------------------------------------------
                PalletControl.Parameters.AddWithValue("@Foio", ID.ToString());
                PalletControl.Parameters.AddWithValue("@Choffer", Choffer.ToString());
                PalletControl.Parameters.AddWithValue("@StatusNow", "CHOFER EN MOVIMIENTO");
                PalletControl.Parameters.AddWithValue("@Container", Container.ToString());
                PalletControl.Parameters.AddWithValue("@Message", "EL CHOFER " + Choffer + " ESTA EN MOVIMIENTO");
                PalletControl.Parameters.AddWithValue("@Active", true);
                PalletControl.Parameters.AddWithValue("@Date", usTime.ToString());
                PalletControl.Parameters.AddWithValue("@Datetime", usTime.ToString());
                PalletControl.ExecuteNonQuery();
                DBSPP.Close();

                string statusmv = "UPDATE RADAEmpire_AChoffer SET " +
                   " Status = @Status WHERE Username = @Username";
                using (SqlCommand coms = new SqlCommand(statusmv, DBSPP))
                {
                    DBSPP.Open();
                    coms.Parameters.AddWithValue("@Status", "CHOFFER EN MOVIMIENTO");
                    coms.Parameters.AddWithValue("@Username", Choffer.ToString());
                    int rowsAffected = coms.ExecuteNonQuery();
                    DBSPP.Close();
                }
                //--------------------------------------------------------------------------------------------------------------------------------

                return RedirectToAction("EntryContainer", "RADA");
            }
        }

        public ActionResult Inventory()
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
        public JsonResult RemoveInventary(string ID)
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
                return Json(new { success = false, redirectUrl = Url.Action("LogIn", "Login") });
            }

            string updateQuery = "UPDATE RADAEmpire_CInventoryControl SET Active = @Active WHERE ID = @ID";
            using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
            {
                DBSPP.Open();
                command.Parameters.AddWithValue("@Active", false);
                command.Parameters.AddWithValue("@ID", ID);
                command.ExecuteNonQuery();
                DBSPP.Close();
            }

            return Json(new { success = true });
        }

        public ActionResult ReturnContainer(string ID)
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
                

                return View();
            }
        }

        public ActionResult ConfirmContainer(string id)
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
                string formattedDate = usTime.ToString("yyy-MM-dd", CultureInfo.InvariantCulture);

                string datenow = usTime.ToString();
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = @"
        SELECT TOP (500)
    a.Urgencia AS Urgencia, 
    a.Folio AS Folio, 
    a.Container AS Container, 
    a.Origins_Location AS Origen, 
    a.Destination_Location AS Destination, 
    a.Status AS Status, 
    a.Datetime AS HSolicitud, 
    b.Time_Confirm AS HConfirm, 
    b.Time_Finished AS HFinish, 
    a.Who_Send AS WhoRequest, 
    b.Choffer AS Choffer, 
    a.message AS Comment, 
    a.Date AS Date, 
    a.shift AS Area, 
    a.GruaRequest AS Grua, 
    a.RaRRequest AS RaR
FROM RADAEmpire_BRequestContainers AS a
INNER JOIN RADAEmpire_CEntryContrainers AS b ON b.Folio_Request = a.Folio
WHERE a.Date = @Date and a.Active = '1'
ORDER BY 
    CASE 
        WHEN a.message = 'Canceled by Rada' THEN 1 ELSE 0
    END,
    CASE 
        WHEN UPPER(a.Urgencia) = 'CRITICO' THEN 0
        WHEN UPPER(a.Urgencia) = 'URGENTE' THEN 1
        WHEN UPPER(a.Urgencia) = 'NORMAL' THEN 2
        ELSE 3
    END,
    CASE 
        WHEN a.message = 'PENDING' THEN 1
        WHEN a.message = 'CHOFER TERMINA MOVIMIENTO' THEN 3
        ELSE 2
    END,
    a.Folio DESC;
;
    ";
                // Parámetro para evitar SQL Injection
                con.Parameters.Clear();
                con.Parameters.AddWithValue("@Date", datenow);

                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    string areaActual = dr["Area"].ToString();

                    if (filtroAreas.Contains(areaActual))
                    {
                        GetRecords.Add(new Historial()
                        {
                            Urgencia = dr["Urgencia"].ToString(),
                            RequestGrua = (dr["Grua"].ToString()),
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
                            RaR = (dr["RaR"].ToString()),
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
                string formattedDate = usTime.ToString("yyy-MM-dd", CultureInfo.InvariantCulture);

                string datenow = usTime.ToString();
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = @"
        SELECT TOP (500)
    a.Urgencia AS Urgencia, 
    a.Folio AS Folio, 
    a.Container AS Container, 
    a.Origins_Location AS Origen, 
    a.Destination_Location AS Destination, 
    a.Status AS Status, 
    a.Datetime AS HSolicitud, 
    b.Time_Confirm AS HConfirm, 
    b.Time_Finished AS HFinish, 
    a.Who_Send AS WhoRequest, 
    b.Choffer AS Choffer, 
    a.message AS Comment, 
    a.Date AS Date, 
    a.shift AS Area, 
    a.GruaRequest AS Grua, 
    a.RaRRequest AS RaR
FROM RADAEmpire_BRequestContainers AS a
INNER JOIN RADAEmpire_CEntryContrainers AS b ON b.Folio_Request = a.Folio
WHERE a.Date = @Date and a.Active = '1'
ORDER BY 
    CASE 
        WHEN a.message = 'Canceled by Rada' THEN 1 ELSE 0
    END,
    CASE 
        WHEN UPPER(a.Urgencia) = 'CRITICO' THEN 0
        WHEN UPPER(a.Urgencia) = 'URGENTE' THEN 1
        WHEN UPPER(a.Urgencia) = 'NORMAL' THEN 2
        ELSE 3
    END,
    CASE 
        WHEN a.message = 'PENDING' THEN 1
        WHEN a.message = 'CHOFER TERMINA MOVIMIENTO' THEN 3
        ELSE 2
    END,
    a.Folio DESC;
;
    ";
                // Parámetro para evitar SQL Injection
                con.Parameters.Clear();
                con.Parameters.AddWithValue("@Date", datenow);

                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetRecords.Add(new Historial()
                    {
                        Urgencia = dr["Urgencia"].ToString(),
                        RequestGrua = (dr["Grua"].ToString()),
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
                        RaR = (dr["RaR"].ToString()),

                    });
                }
                DBSPP.Close();
            }

        }

        private void GetMovement()
        {
            if (Movements.Count > 0)
            {
                Movements.Clear();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select * from RADAEmpires_DZChofferMovement where Active = '1' order by Choffer asc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    Movements.Add(new Movement()
                    {
                        Foio = (dr["Foio"].ToString()),
                        Choffer = (dr["Choffer"].ToString()),
                        StatusNow = (dr["StatusNow"].ToString()),
                        Container = (dr["Container"].ToString()),
                        Message = (dr["Message"].ToString()),
                        Active = (dr["Active"].ToString()),
                    });
                }
                DBSPP.Close();
            }
        }
    }
}