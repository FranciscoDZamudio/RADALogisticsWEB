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

        //connection SQL server (database)
        SqlConnection DBSPP = new SqlConnection("Data Source=RADAEmpire.mssql.somee.com ;Initial Catalog=RADAEmpire ;User ID=RooRada; password=rada1311");
        SqlCommand con = new SqlCommand();
        SqlDataReader dr;

        // GET: RADA

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
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            ViewBag.User = Session["Username"];
            ViewBag.Type = Session["Type"];

            DBSPP.Open();
            con.Connection = DBSPP;
            con.CommandText = "  Select top (100) " +
                " a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date, a.shift as Area,a.GruaRequest as Grua " +
                " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio where a.Folio = '" + Folio.ToString() + "' ORDER by a.Folio desc";
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
                });
            }
            DBSPP.Close();

            ViewBag.Records = GetRecords; // Obtener nuevamente los datos
            ViewBag.Count = GetRecords.Count.ToString();
            return PartialView("table", ViewBag.Records);
        }

        public ActionResult Comments(string id, string Status)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
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
                ViewBag.id = id;
                return View();
            }
        }

        public ActionResult GetComments(string Status, string id, string Comment)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
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
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
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

        public ActionResult EntryContainer(string query, string date)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
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
                        DBSPP.Open();
                        con.Connection = DBSPP;
                        con.CommandText = "Select top (100) " +
                            " a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                            " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date, a.shift as Area, a.GruaRequest as Grua " +
                            " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio where a.Date = '" + date.ToString() + "' ORDER by a.Folio desc";
                        dr = con.ExecuteReader();
                        while (dr.Read())
                        {
                            GetRecordsQuery.Add(new Historial()
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

                        DBSPP.Open();
                        con.Connection = DBSPP;
                        con.CommandText = "Select top (100) " +
                            " a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                            " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date, a.shift as Area,a.GruaRequest as Grua" +
                            " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio where a.Date = '" + date.ToString() + "' ORDER by a.Folio desc";
                        dr = con.ExecuteReader();
                        while (dr.Read())
                        {
                            string areaActual = dr["Area"].ToString();

                            if (filtroAreas.Contains(areaActual))
                            {
                                GetRecordsQuery.Add(new Historial()
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
        public JsonResult ConfirmProcess(string ID, string choffer, string requestGrua, string Comment)
        {
            if (Comment == "PENDING")
            {
                // Si el comentario es "PENDING", redirigimos a "ViewConfirm"
                string redirectUrl = Url.Action("ViewConfirm", "RADA", new { ID = ID, choffer = choffer, requestGrua = requestGrua });

                return Json(new { success = true, redirectUrl = redirectUrl });
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

                // create generate randoms int value
                string estatusActual = null, message = null, shift = null;
                SqlCommand statuscontainer = new SqlCommand("Select * from RADAEmpire_BRequestContainers where Active = '1' and Folio = '" + ID.ToString() + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drstatuscontainer = statuscontainer.ExecuteReader();
                if (drstatuscontainer.HasRows)
                {
                    while (drstatuscontainer.Read())
                    {
                        estatusActual = drstatuscontainer["Status"].ToString();
                        message = drstatuscontainer["message"].ToString();
                        shift = drstatuscontainer["shift"].ToString();
                    }
                }
                DBSPP.Close();

                //area de envios
                if (estatusActual == "CAR" && shift == "ENVIOS")
                {
                    if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
                    {
                        //------------------------------------------------------------------------------
                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                        {
                            DBSPP.Open();
                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                            coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                            int rowsAffected = coms.ExecuteNonQuery();
                            DBSPP.Close();
                        }
                        //------------------------------------------------------------------------------
                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                            " message = @message WHERE Folio = @ID";
                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                        {
                            DBSPP.Open();
                            coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                            int rowsAffected = coms.ExecuteNonQuery();
                            DBSPP.Close();
                        }
                        //------------------------------------------------------------------------------

                        //query message ------------------------------------------------------------------------------------------------------------------

                    }
                    else
                    {
                        if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
                        {
                            //------------------------------------------------------------------------------
                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ESTACIONAMIENTO CAJA");
                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                " message = @message WHERE Folio = @ID";
                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ESTACIONAMIENTO CAJA");
                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                            //query message ------------------------------------------------------------------------------------------------------------------

                        }
                        else
                        {
                            if (message == "CHOFER EN PROCESO DE ESTACIONAMIENTO CAJA")
                            {
                                //------------------------------------------------------------------------------
                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                    coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                    " message = @message WHERE Folio = @ID";
                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //query message ------------------------------------------------------------------------------------------------------------------
                            }
                            else
                            {
                                if (message == "CHOFER SOLTANDO CAJA")
                                {
                                    //------------------------------------------------------------------------------
                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                        coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                        " message = @message WHERE Folio = @ID";
                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
                                        " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
                                    using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }

                                    //------------------------------------------------------------------------------
                                    string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
                                        " Status = @Status WHERE Username = @Username";
                                    using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                                        coms.Parameters.AddWithValue("@Username", choffer.ToString());
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
                                else
                                {
                                    if (message == "CHOFER TERMINA MOVIMIENTO")
                                    {
                                        return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
                                        //return RedirectToAction("EntryContainer", "RADA");
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (estatusActual == "VAC" && shift == "ENVIOS")
                    {
                        if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
                        {
                            //------------------------------------------------------------------------------
                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                " message = @message WHERE Folio = @ID";
                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                        }
                        else
                        {
                            if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
                            {
                                //------------------------------------------------------------------------------
                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN RUTA A RAMPA DESTINO");
                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                    " message = @message WHERE Folio = @ID";
                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@message", "CHOFER EN RUTA A RAMPA DESTINO");
                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                            }
                            else
                            {
                                if (message == "CHOFER EN RUTA A RAMPA DESTINO")
                                {
                                    //------------------------------------------------------------------------------
                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                        coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                        " message = @message WHERE Folio = @ID";
                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                }
                                else
                                {
                                    if (message == "CHOFER SOLTANDO CAJA")
                                    {
                                        //------------------------------------------------------------------------------
                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                            coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                            " message = @message WHERE Folio = @ID";
                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                        string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
                                            " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
                                        using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }

                                        //------------------------------------------------------------------------------
                                        string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
                                            " Status = @Status WHERE Username = @Username";
                                        using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                                            coms.Parameters.AddWithValue("@Username", choffer.ToString());
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
                                    else
                                    {
                                        if (message == "CHOFER TERMINA MOVIMIENTO")
                                        {
                                            return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //area de PT
                if (estatusActual == "CAR" && shift == "PT")
                {
                    if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
                    {
                        //------------------------------------------------------------------------------
                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                        {
                            DBSPP.Open();
                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                            coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                            int rowsAffected = coms.ExecuteNonQuery();
                            DBSPP.Close();
                        }

                        //------------------------------------------------------------------------------
                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                            " message = @message WHERE Folio = @ID";
                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                        {
                            DBSPP.Open();
                            coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                            int rowsAffected = coms.ExecuteNonQuery();
                            DBSPP.Close();
                        }
                        //------------------------------------------------------------------------------
                    }
                    else
                    {
                        if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
                        {
                            //------------------------------------------------------------------------------
                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ESTACIONAMIENTO CAJA");
                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }

                            //------------------------------------------------------------------------------
                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                " message = @message WHERE Folio = @ID";
                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ESTACIONAMIENTO CAJA");
                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                        }
                        else
                        {
                            if (message == "CHOFER EN PROCESO DE ESTACIONAMIENTO CAJA")
                            {
                                //------------------------------------------------------------------------------
                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                    coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                    " message = @message WHERE Folio = @ID";
                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                            }
                            else
                            {
                                if (message == "CHOFER SOLTANDO CAJA")
                                {
                                    //------------------------------------------------------------------------------
                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                        coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                        " message = @message WHERE Folio = @ID";
                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
                                        " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
                                    using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }

                                    //------------------------------------------------------------------------------
                                    string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
                                        " Status = @Status WHERE Username = @Username";
                                    using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                                        coms.Parameters.AddWithValue("@Username", choffer.ToString());
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
                                else
                                {
                                    if (message == "CHOFER TERMINA MOVIMIENTO")
                                    {
                                        return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
                                    }
                                }
                            }
                        }
                    }

                }
                else
                {
                    if (estatusActual == "VAC" && shift == "PT")
                    {
                        if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
                        {
                            //------------------------------------------------------------------------------
                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                " message = @message WHERE Folio = @ID";
                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                        }
                        else
                        {
                            if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
                            {
                                //------------------------------------------------------------------------------
                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN RUTA A RAMPA DESTINO");
                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                    " message = @message WHERE Folio = @ID";
                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@message", "CHOFER EN RUTA A RAMPA DESTINO");
                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                            }
                            else
                            {
                                if (message == "CHOFER EN RUTA A RAMPA DESTINO")
                                {
                                    //------------------------------------------------------------------------------
                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                        coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                        " message = @message WHERE Folio = @ID";
                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                }
                                else
                                {
                                    if (message == "CHOFER SOLTANDO CAJA")
                                    {
                                        //------------------------------------------------------------------------------
                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                            coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                            " message = @message WHERE Folio = @ID";
                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                        string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
                                            " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
                                        using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }

                                        //------------------------------------------------------------------------------
                                        string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
                                            " Status = @Status WHERE Username = @Username";
                                        using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                                            coms.Parameters.AddWithValue("@Username", choffer.ToString());
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
                                    else
                                    {
                                        if (message == "CHOFER TERMINA MOVIMIENTO")
                                        {
                                            return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //area de EMPAQUE
                if (shift == "EMPAQUE" && estatusActual == "CAR" && requestGrua == "NO")
                {
                    if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
                    {
                        //------------------------------------------------------------------------------
                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                        {
                            DBSPP.Open();
                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                            coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                            int rowsAffected = coms.ExecuteNonQuery();
                            DBSPP.Close();
                        }
                        //------------------------------------------------------------------------------
                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                            " message = @message WHERE Folio = @ID";
                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                        {
                            DBSPP.Open();
                            coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                            int rowsAffected = coms.ExecuteNonQuery();
                            DBSPP.Close();
                        }
                        //------------------------------------------------------------------------------
                    }
                    else
                    {
                        if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
                        {
                            //------------------------------------------------------------------------------
                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                coms.Parameters.AddWithValue("@ID", "CHOFER EN RUTA A RAMPA DESTINO");
                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                " message = @message WHERE Folio = @ID";
                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@message", "CHOFER EN RUTA A RAMPA DESTINO");
                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                        }
                        else
                        {
                            if (message == "CHOFER EN RUTA A RAMPA DESTINO")
                            {
                                //------------------------------------------------------------------------------
                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                    coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                    " message = @message WHERE Folio = @ID";
                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                            }
                            else
                            {
                                if (message == "CHOFER SOLTANDO CAJA")
                                {
                                    //------------------------------------------------------------------------------
                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                        coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                        " message = @message WHERE Folio = @ID";
                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
                                        " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
                                    using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }

                                    //------------------------------------------------------------------------------
                                    string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
                                        " Status = @Status WHERE Username = @Username";
                                    using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                                        coms.Parameters.AddWithValue("@Username", choffer.ToString());
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
                                else
                                {
                                    if (message == "CHOFER TERMINA MOVIMIENTO")
                                    {
                                        return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (shift == "EMPAQUE" && estatusActual == "VAC" && requestGrua == "NO")
                    {
                        if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
                        {
                            //------------------------------------------------------------------------------
                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                " message = @message WHERE Folio = @ID";
                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                        }
                        else
                        {
                            if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
                            {
                                //------------------------------------------------------------------------------
                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SELLO");
                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                    " message = @message WHERE Folio = @ID";
                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SELLO");
                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                            }
                            else
                            {
                                if (message == "CHOFER EN ESPERA DE SELLO")
                                {
                                    //------------------------------------------------------------------------------
                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                        coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE AREA DE VACIO");
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                        " message = @message WHERE Folio = @ID";
                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE AREA DE VACIO");
                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                }
                                else
                                {
                                    if (message == "CHOFER EN ESPERA DE AREA DE VACIO")
                                    {
                                        //------------------------------------------------------------------------------
                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                            coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                            " message = @message WHERE Folio = @ID";
                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                        string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
                                            " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
                                        using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }

                                        //------------------------------------------------------------------------------
                                        string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
                                            " Status = @Status WHERE Username = @Username";
                                        using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                                            coms.Parameters.AddWithValue("@Username", choffer.ToString());
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
                                    else
                                    {
                                        if (message == "CHOFER TERMINA MOVIMIENTO")
                                        {
                                            return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (shift == "EMPAQUE" && estatusActual == "CAR" && requestGrua == "SI")
                        {
                            if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
                            {
                                //------------------------------------------------------------------------------
                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                    coms.Parameters.AddWithValue("@ID", "CHOFER ENGANCHANDO CHASIS");
                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                    " message = @message WHERE Folio = @ID";
                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@message", "CHOFER ENGANCHANDO CHASIS");
                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                            }
                            else
                            {
                                if (message == "CHOFER ENGANCHANDO CHASIS")
                                {
                                    //------------------------------------------------------------------------------
                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                        coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE ACCESO A GRUA");
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                        " message = @message WHERE Folio = @ID";
                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE ACCESO A GRUA");
                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                }
                                else
                                {
                                    if (message == "CHOFER EN ESPERA DE ACCESO A GRUA")
                                    {
                                        //------------------------------------------------------------------------------
                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                            coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE MANIOBRA EN GRUA");
                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                            " message = @message WHERE Folio = @ID";
                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE MANIOBRA EN GRUA");
                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                    }
                                    else
                                    {
                                        if (message == "CHOFER EN ESPERA DE MANIOBRA EN GRUA")
                                        {
                                            //------------------------------------------------------------------------------
                                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                            {
                                                DBSPP.Open();
                                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SALIDA DE GRUA");
                                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                int rowsAffected = coms.ExecuteNonQuery();
                                                DBSPP.Close();
                                            }
                                            //------------------------------------------------------------------------------
                                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                " message = @message WHERE Folio = @ID";
                                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                            {
                                                DBSPP.Open();
                                                coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SALIDA DE GRUA");
                                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                int rowsAffected = coms.ExecuteNonQuery();
                                                DBSPP.Close();
                                            }
                                            //------------------------------------------------------------------------------
                                        }
                                        else
                                        {
                                            if (message == "CHOFER EN ESPERA DE SALIDA DE GRUA")
                                            {
                                                //------------------------------------------------------------------------------
                                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                {
                                                    DBSPP.Open();
                                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN RUTA A RAMPA DESTINO");
                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                    DBSPP.Close();
                                                }
                                                //------------------------------------------------------------------------------
                                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                    " message = @message WHERE Folio = @ID";
                                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                {
                                                    DBSPP.Open();
                                                    coms.Parameters.AddWithValue("@message", "CHOFER EN RUTA A RAMPA DESTINO");
                                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                    DBSPP.Close();
                                                }
                                                //------------------------------------------------------------------------------
                                            }
                                            else
                                            {
                                                if (message == "CHOFER EN RUTA A RAMPA DESTINO")
                                                {
                                                    //------------------------------------------------------------------------------
                                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                    {
                                                        DBSPP.Open();
                                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                        coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
                                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                        int rowsAffected = coms.ExecuteNonQuery();
                                                        DBSPP.Close();
                                                    }
                                                    //------------------------------------------------------------------------------
                                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                        " message = @message WHERE Folio = @ID";
                                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                    {
                                                        DBSPP.Open();
                                                        coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
                                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                        int rowsAffected = coms.ExecuteNonQuery();
                                                        DBSPP.Close();
                                                    }
                                                    //------------------------------------------------------------------------------
                                                }
                                                else
                                                {
                                                    if (message == "CHOFER SOLTANDO CAJA")
                                                    {
                                                        if (message == "CHOFER SOLTANDO CAJA")
                                                        {
                                                            //------------------------------------------------------------------------------
                                                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                            {
                                                                DBSPP.Open();
                                                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                                coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
                                                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                                int rowsAffected = coms.ExecuteNonQuery();
                                                                DBSPP.Close();
                                                            }
                                                            //------------------------------------------------------------------------------
                                                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                                " message = @message WHERE Folio = @ID";
                                                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                            {
                                                                DBSPP.Open();
                                                                coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
                                                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                                int rowsAffected = coms.ExecuteNonQuery();
                                                                DBSPP.Close();
                                                            }
                                                            //------------------------------------------------------------------------------
                                                            string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
                                                                " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
                                                            using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
                                                            {
                                                                DBSPP.Open();
                                                                coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
                                                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                                int rowsAffected = coms.ExecuteNonQuery();
                                                                DBSPP.Close();
                                                            }

                                                            //------------------------------------------------------------------------------
                                                            string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
                                                                " Status = @Status WHERE Username = @Username";
                                                            using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
                                                            {
                                                                DBSPP.Open();
                                                                coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                                                                coms.Parameters.AddWithValue("@Username", choffer.ToString());
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
                                                        else
                                                        {
                                                            if (message == "CHOFER TERMINA MOVIMIENTO")
                                                            {
                                                                return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (shift == "EMPAQUE" && estatusActual == "VAC" && requestGrua == "SI")
                            {
                                if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
                                {
                                    //------------------------------------------------------------------------------
                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                        coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                        " message = @message WHERE Folio = @ID";
                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                }
                                else
                                {
                                    if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
                                    {
                                        //------------------------------------------------------------------------------
                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                            coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SELLO");
                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                            " message = @message WHERE Folio = @ID";
                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SELLO");
                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                    }
                                    else
                                    {
                                        if (message == "CHOFER EN ESPERA DE SELLO")
                                        {
                                            //------------------------------------------------------------------------------
                                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                            {
                                                DBSPP.Open();
                                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE ACCESO DE GRÚA");
                                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                int rowsAffected = coms.ExecuteNonQuery();
                                                DBSPP.Close();
                                            }
                                            //------------------------------------------------------------------------------
                                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                " message = @message WHERE Folio = @ID";
                                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                            {
                                                DBSPP.Open();
                                                coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE ACCESO DE GRÚA");
                                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                int rowsAffected = coms.ExecuteNonQuery();
                                                DBSPP.Close();
                                            }
                                            //------------------------------------------------------------------------------
                                        }
                                        else
                                        {
                                            if (message == "CHOFER EN ESPERA DE ACCESO DE GRÚA")
                                            {
                                                //------------------------------------------------------------------------------
                                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                {
                                                    DBSPP.Open();
                                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE MANIOBRA DE GRUA");
                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                    DBSPP.Close();
                                                }
                                                //------------------------------------------------------------------------------
                                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                    " message = @message WHERE Folio = @ID";
                                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                {
                                                    DBSPP.Open();
                                                    coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE MANIOBRA DE GRUA");
                                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                    DBSPP.Close();
                                                }
                                                //------------------------------------------------------------------------------
                                            }
                                            else
                                            {
                                                if (message == "CHOFER EN ESPERA DE MANIOBRA DE GRUA")
                                                {
                                                    //------------------------------------------------------------------------------
                                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                    {
                                                        DBSPP.Open();
                                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                        coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE CARGADO DE GRUA");
                                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                        int rowsAffected = coms.ExecuteNonQuery();
                                                        DBSPP.Close();
                                                    }
                                                    //------------------------------------------------------------------------------
                                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                        " message = @message WHERE Folio = @ID";
                                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                    {
                                                        DBSPP.Open();
                                                        coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE CARGADO DE GRUA");
                                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                        int rowsAffected = coms.ExecuteNonQuery();
                                                        DBSPP.Close();
                                                    }
                                                    //------------------------------------------------------------------------------
                                                }
                                                else
                                                {
                                                    if (message == "CHOFER EN ESPERA DE CARGADO DE GRUA")
                                                    {
                                                        //------------------------------------------------------------------------------
                                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                        {
                                                            DBSPP.Open();
                                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                            coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SALIDA DE GRUA");
                                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                            int rowsAffected = coms.ExecuteNonQuery();
                                                            DBSPP.Close();
                                                        }
                                                        //------------------------------------------------------------------------------
                                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                            " message = @message WHERE Folio = @ID";
                                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                        {
                                                            DBSPP.Open();
                                                            coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SALIDA DE GRUA");
                                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                            int rowsAffected = coms.ExecuteNonQuery();
                                                            DBSPP.Close();
                                                        }
                                                        //------------------------------------------------------------------------------
                                                    }
                                                    else
                                                    {
                                                        if (message == "CHOFER EN ESPERA DE SALIDA DE GRUA")
                                                        {
                                                            //------------------------------------------------------------------------------
                                                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                            {
                                                                DBSPP.Open();
                                                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                                coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CHASIS");
                                                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                                int rowsAffected = coms.ExecuteNonQuery();
                                                                DBSPP.Close();
                                                            }
                                                            //------------------------------------------------------------------------------
                                                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                                " message = @message WHERE Folio = @ID";
                                                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                            {
                                                                DBSPP.Open();
                                                                coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CHASIS");
                                                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                                int rowsAffected = coms.ExecuteNonQuery();
                                                                DBSPP.Close();
                                                            }
                                                            //------------------------------------------------------------------------------
                                                        }
                                                        else
                                                        {
                                                            if (message == "CHOFER SOLTANDO CHASIS")
                                                            {
                                                                //------------------------------------------------------------------------------
                                                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                                {
                                                                    DBSPP.Open();
                                                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                                    coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
                                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                                    DBSPP.Close();
                                                                }
                                                                //------------------------------------------------------------------------------
                                                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                                    " message = @message WHERE Folio = @ID";
                                                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                                {
                                                                    DBSPP.Open();
                                                                    coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
                                                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                                    DBSPP.Close();
                                                                }
                                                                //------------------------------------------------------------------------------
                                                                string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
                                                                    " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
                                                                using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
                                                                {
                                                                    DBSPP.Open();
                                                                    coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
                                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                                    DBSPP.Close();
                                                                }

                                                                //------------------------------------------------------------------------------
                                                                string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
                                                                    " Status = @Status WHERE Username = @Username";
                                                                using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
                                                                {
                                                                    DBSPP.Open();
                                                                    coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                                                                    coms.Parameters.AddWithValue("@Username", choffer.ToString());
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
                                                            else
                                                            {
                                                                if (message == "CHOFER TERMINA MOVIMIENTO")
                                                                {
                                                                    return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //area de GENERALES
                if (shift == "GENERALES" && estatusActual == "CAR" && requestGrua == "NO")
                {
                    if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
                    {
                        //------------------------------------------------------------------------------
                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                        {
                            DBSPP.Open();
                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                            coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                            int rowsAffected = coms.ExecuteNonQuery();
                            DBSPP.Close();
                        }
                        //------------------------------------------------------------------------------
                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                            " message = @message WHERE Folio = @ID";
                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                        {
                            DBSPP.Open();
                            coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                            int rowsAffected = coms.ExecuteNonQuery();
                            DBSPP.Close();
                        }
                        //------------------------------------------------------------------------------
                    }
                    else
                    {
                        if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
                        {
                            //------------------------------------------------------------------------------
                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                coms.Parameters.AddWithValue("@ID", "CHOFER EN RUTA A RAMPA DESTINO");
                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                " message = @message WHERE Folio = @ID";
                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@message", "CHOFER EN RUTA A RAMPA DESTINO");
                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                        }
                        else
                        {
                            if (message == "CHOFER EN RUTA A RAMPA DESTINO")
                            {
                                //------------------------------------------------------------------------------
                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                    coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                    " message = @message WHERE Folio = @ID";
                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                            }
                            else
                            {
                                if (message == "CHOFER SOLTANDO CAJA")
                                {
                                    //------------------------------------------------------------------------------
                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                        coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                        " message = @message WHERE Folio = @ID";
                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
                                        " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
                                    using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }

                                    //------------------------------------------------------------------------------
                                    string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
                                        " Status = @Status WHERE Username = @Username";
                                    using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                                        coms.Parameters.AddWithValue("@Username", choffer.ToString());
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
                                else
                                {
                                    if (message == "CHOFER TERMINA MOVIMIENTO")
                                    {
                                        return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (shift == "GENERALES" && estatusActual == "VAC" && requestGrua == "NO")
                    {
                        if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
                        {
                            //------------------------------------------------------------------------------
                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                " message = @message WHERE Folio = @ID";
                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                        }
                        else
                        {
                            if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
                            {
                                //------------------------------------------------------------------------------
                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SELLO");
                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                    " message = @message WHERE Folio = @ID";
                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SELLO");
                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                            }
                            else
                            {
                                if (message == "CHOFER EN ESPERA DE SELLO")
                                {
                                    //------------------------------------------------------------------------------
                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                        coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE AREA DE VACIO");
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                        " message = @message WHERE Folio = @ID";
                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE AREA DE VACIO");
                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                }
                                else
                                {
                                    if (message == "CHOFER EN ESPERA DE AREA DE VACIO")
                                    {
                                        //------------------------------------------------------------------------------
                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                            coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                            " message = @message WHERE Folio = @ID";
                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                        string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
                                            " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
                                        using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }

                                        //------------------------------------------------------------------------------
                                        string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
                                            " Status = @Status WHERE Username = @Username";
                                        using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                                            coms.Parameters.AddWithValue("@Username", choffer.ToString());
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
                                    else
                                    {
                                        if (message == "CHOFER TERMINA MOVIMIENTO")
                                        {
                                            return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (shift == "GENERALES" && estatusActual == "CAR" && requestGrua == "SI")
                        {
                            if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
                            {
                                //------------------------------------------------------------------------------
                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                    coms.Parameters.AddWithValue("@ID", "CHOFER ENGANCHANDO CHASIS");
                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                    " message = @message WHERE Folio = @ID";
                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@message", "CHOFER ENGANCHANDO CHASIS");
                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                            }
                            else
                            {
                                if (message == "CHOFER ENGANCHANDO CHASIS")
                                {
                                    //------------------------------------------------------------------------------
                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                        coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE ACCESO A GRUA");
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                        " message = @message WHERE Folio = @ID";
                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE ACCESO A GRUA");
                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                }
                                else
                                {
                                    if (message == "CHOFER EN ESPERA DE ACCESO A GRUA")
                                    {
                                        //------------------------------------------------------------------------------
                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                            coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE MANIOBRA EN GRUA");
                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                            " message = @message WHERE Folio = @ID";
                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE MANIOBRA EN GRUA");
                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                    }
                                    else
                                    {
                                        if (message == "CHOFER EN ESPERA DE MANIOBRA EN GRUA")
                                        {
                                            //------------------------------------------------------------------------------
                                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                            {
                                                DBSPP.Open();
                                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SALIDA DE GRUA");
                                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                int rowsAffected = coms.ExecuteNonQuery();
                                                DBSPP.Close();
                                            }
                                            //------------------------------------------------------------------------------
                                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                " message = @message WHERE Folio = @ID";
                                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                            {
                                                DBSPP.Open();
                                                coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SALIDA DE GRUA");
                                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                int rowsAffected = coms.ExecuteNonQuery();
                                                DBSPP.Close();
                                            }
                                            //------------------------------------------------------------------------------
                                        }
                                        else
                                        {
                                            if (message == "CHOFER EN ESPERA DE SALIDA DE GRUA")
                                            {
                                                //------------------------------------------------------------------------------
                                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                {
                                                    DBSPP.Open();
                                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN RUTA A RAMPA DESTINO");
                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                    DBSPP.Close();
                                                }
                                                //------------------------------------------------------------------------------
                                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                    " message = @message WHERE Folio = @ID";
                                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                {
                                                    DBSPP.Open();
                                                    coms.Parameters.AddWithValue("@message", "CHOFER EN RUTA A RAMPA DESTINO");
                                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                    DBSPP.Close();
                                                }
                                                //------------------------------------------------------------------------------
                                            }
                                            else
                                            {
                                                if (message == "CHOFER EN RUTA A RAMPA DESTINO")
                                                {
                                                    //------------------------------------------------------------------------------
                                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                    {
                                                        DBSPP.Open();
                                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                        coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
                                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                        int rowsAffected = coms.ExecuteNonQuery();
                                                        DBSPP.Close();
                                                    }
                                                    //------------------------------------------------------------------------------
                                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                        " message = @message WHERE Folio = @ID";
                                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                    {
                                                        DBSPP.Open();
                                                        coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
                                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                        int rowsAffected = coms.ExecuteNonQuery();
                                                        DBSPP.Close();
                                                    }
                                                    //------------------------------------------------------------------------------
                                                }
                                                else
                                                {
                                                    if (message == "CHOFER SOLTANDO CAJA")
                                                    {
                                                        //------------------------------------------------------------------------------
                                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                        {
                                                            DBSPP.Open();
                                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                            coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
                                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                            int rowsAffected = coms.ExecuteNonQuery();
                                                            DBSPP.Close();
                                                        }
                                                        //------------------------------------------------------------------------------
                                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                            " message = @message WHERE Folio = @ID";
                                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                        {
                                                            DBSPP.Open();
                                                            coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
                                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                            int rowsAffected = coms.ExecuteNonQuery();
                                                            DBSPP.Close();
                                                        }
                                                        //------------------------------------------------------------------------------
                                                        string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
                                                            " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
                                                        using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
                                                        {
                                                            DBSPP.Open();
                                                            coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
                                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                            int rowsAffected = coms.ExecuteNonQuery();
                                                            DBSPP.Close();
                                                        }

                                                        //------------------------------------------------------------------------------
                                                        string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
                                                            " Status = @Status WHERE Username = @Username";
                                                        using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
                                                        {
                                                            DBSPP.Open();
                                                            coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                                                            coms.Parameters.AddWithValue("@Username", choffer.ToString());
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
                                                    else
                                                    {
                                                        if (message == "CHOFER TERMINA MOVIMIENTO")
                                                        {
                                                            return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (shift == "GENERALES" && estatusActual == "VAC" && requestGrua == "SI")
                            {
                                if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
                                {
                                    //------------------------------------------------------------------------------
                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                        coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                        " message = @message WHERE Folio = @ID";
                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                }
                                else
                                {
                                    if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
                                    {
                                        //------------------------------------------------------------------------------
                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                            coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SELLO");
                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                            " message = @message WHERE Folio = @ID";
                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SELLO");
                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                    }
                                    else
                                    {
                                        if (message == "CHOFER EN ESPERA DE SELLO")
                                        {
                                            //------------------------------------------------------------------------------
                                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                            {
                                                DBSPP.Open();
                                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE ACCESO DE GRÚA");
                                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                int rowsAffected = coms.ExecuteNonQuery();
                                                DBSPP.Close();
                                            }
                                            //------------------------------------------------------------------------------
                                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                " message = @message WHERE Folio = @ID";
                                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                            {
                                                DBSPP.Open();
                                                coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE ACCESO DE GRÚA");
                                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                int rowsAffected = coms.ExecuteNonQuery();
                                                DBSPP.Close();
                                            }
                                            //------------------------------------------------------------------------------
                                        }
                                        else
                                        {
                                            if (message == "CHOFER EN ESPERA DE ACCESO DE GRÚA")
                                            {
                                                //------------------------------------------------------------------------------
                                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                {
                                                    DBSPP.Open();
                                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE MANIOBRA DE GRUA");
                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                    DBSPP.Close();
                                                }
                                                //------------------------------------------------------------------------------
                                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                    " message = @message WHERE Folio = @ID";
                                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                {
                                                    DBSPP.Open();
                                                    coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE MANIOBRA DE GRUA");
                                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                    DBSPP.Close();
                                                }
                                                //------------------------------------------------------------------------------
                                            }
                                            else
                                            {
                                                if (message == "CHOFER EN ESPERA DE MANIOBRA DE GRUA")
                                                {
                                                    //------------------------------------------------------------------------------
                                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                    {
                                                        DBSPP.Open();
                                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                        coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE CARGADO DE GRUA");
                                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                        int rowsAffected = coms.ExecuteNonQuery();
                                                        DBSPP.Close();
                                                    }
                                                    //------------------------------------------------------------------------------
                                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                        " message = @message WHERE Folio = @ID";
                                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                    {
                                                        DBSPP.Open();
                                                        coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE CARGADO DE GRUA");
                                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                        int rowsAffected = coms.ExecuteNonQuery();
                                                        DBSPP.Close();
                                                    }
                                                    //------------------------------------------------------------------------------
                                                }
                                                else
                                                {
                                                    if (message == "CHOFER EN ESPERA DE CARGADO DE GRUA")
                                                    {
                                                        //------------------------------------------------------------------------------
                                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                        {
                                                            DBSPP.Open();
                                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                            coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SALIDA DE GRUA");
                                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                            int rowsAffected = coms.ExecuteNonQuery();
                                                            DBSPP.Close();
                                                        }
                                                        //------------------------------------------------------------------------------
                                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                            " message = @message WHERE Folio = @ID";
                                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                        {
                                                            DBSPP.Open();
                                                            coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SALIDA DE GRUA");
                                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                            int rowsAffected = coms.ExecuteNonQuery();
                                                            DBSPP.Close();
                                                        }
                                                        //------------------------------------------------------------------------------
                                                    }
                                                    else
                                                    {
                                                        if (message == "CHOFER EN ESPERA DE SALIDA DE GRUA")
                                                        {
                                                            //------------------------------------------------------------------------------
                                                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                            {
                                                                DBSPP.Open();
                                                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                                coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CHASIS");
                                                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                                int rowsAffected = coms.ExecuteNonQuery();
                                                                DBSPP.Close();
                                                            }
                                                            //------------------------------------------------------------------------------
                                                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                                " message = @message WHERE Folio = @ID";
                                                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                            {
                                                                DBSPP.Open();
                                                                coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CHASIS");
                                                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                                int rowsAffected = coms.ExecuteNonQuery();
                                                                DBSPP.Close();
                                                            }
                                                            //------------------------------------------------------------------------------
                                                        }
                                                        else
                                                        {
                                                            if (message == "CHOFER SOLTANDO CHASIS")
                                                            {
                                                                //------------------------------------------------------------------------------
                                                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                                {
                                                                    DBSPP.Open();
                                                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                                    coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
                                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                                    DBSPP.Close();
                                                                }
                                                                //------------------------------------------------------------------------------
                                                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                                    " message = @message WHERE Folio = @ID";
                                                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                                {
                                                                    DBSPP.Open();
                                                                    coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
                                                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                                    DBSPP.Close();
                                                                }
                                                                //------------------------------------------------------------------------------
                                                                string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
                                                                    " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
                                                                using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
                                                                {
                                                                    DBSPP.Open();
                                                                    coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
                                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                                    DBSPP.Close();
                                                                }

                                                                //------------------------------------------------------------------------------
                                                                string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
                                                                    " Status = @Status WHERE Username = @Username";
                                                                using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
                                                                {
                                                                    DBSPP.Open();
                                                                    coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                                                                    coms.Parameters.AddWithValue("@Username", choffer.ToString());
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
                                                            else
                                                            {
                                                                if (message == "CHOFER TERMINA MOVIMIENTO")
                                                                {
                                                                    return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
                                                                    //return RedirectToAction("EntryContainer", "RADA");
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //area de BODEGA 2
                if (shift == "BODEGA 2" && estatusActual == "CAR" && requestGrua == "NO")
                {
                    if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
                    {
                        //------------------------------------------------------------------------------
                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                        {
                            DBSPP.Open();
                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                            coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                            int rowsAffected = coms.ExecuteNonQuery();
                            DBSPP.Close();
                        }
                        //------------------------------------------------------------------------------
                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                            " message = @message WHERE Folio = @ID";
                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                        {
                            DBSPP.Open();
                            coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                            int rowsAffected = coms.ExecuteNonQuery();
                            DBSPP.Close();
                        }
                        //------------------------------------------------------------------------------
                    }
                    else
                    {
                        if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
                        {
                            //------------------------------------------------------------------------------
                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                coms.Parameters.AddWithValue("@ID", "CHOFER EN RUTA A RAMPA DESTINO");
                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                " message = @message WHERE Folio = @ID";
                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@message", "CHOFER EN RUTA A RAMPA DESTINO");
                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                        }
                        else
                        {
                            if (message == "CHOFER EN RUTA A RAMPA DESTINO")
                            {
                                //------------------------------------------------------------------------------
                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                    coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                    " message = @message WHERE Folio = @ID";
                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                            }
                            else
                            {
                                if (message == "CHOFER SOLTANDO CAJA")
                                {
                                    //------------------------------------------------------------------------------
                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                        coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                        " message = @message WHERE Folio = @ID";
                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
                                        " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
                                    using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }

                                    //------------------------------------------------------------------------------
                                    string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
                                        " Status = @Status WHERE Username = @Username";
                                    using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                                        coms.Parameters.AddWithValue("@Username", choffer.ToString());
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
                                else
                                {
                                    if (message == "CHOFER TERMINA MOVIMIENTO")
                                    {
                                        return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (shift == "BODEGA 2" && estatusActual == "VAC" && requestGrua == "NO")
                    {
                        if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
                        {
                            //------------------------------------------------------------------------------
                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                " message = @message WHERE Folio = @ID";
                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                            {
                                DBSPP.Open();
                                coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                int rowsAffected = coms.ExecuteNonQuery();
                                DBSPP.Close();
                            }
                            //------------------------------------------------------------------------------
                        }
                        else
                        {
                            if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
                            {
                                //------------------------------------------------------------------------------
                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SELLO");
                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                    " message = @message WHERE Folio = @ID";
                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SELLO");
                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                            }
                            else
                            {
                                if (message == "CHOFER EN ESPERA DE SELLO")
                                {
                                    //------------------------------------------------------------------------------
                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                        coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE AREA DE VACIO");
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                        " message = @message WHERE Folio = @ID";
                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE AREA DE VACIO");
                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                }
                                else
                                {
                                    if (message == "CHOFER EN ESPERA DE AREA DE VACIO")
                                    {
                                        //------------------------------------------------------------------------------
                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                            coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                            " message = @message WHERE Folio = @ID";
                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                        string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
                                            " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
                                        using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }

                                        //------------------------------------------------------------------------------
                                        string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
                                            " Status = @Status WHERE Username = @Username";
                                        using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                                            coms.Parameters.AddWithValue("@Username", choffer.ToString());
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
                                    else
                                    {
                                        if (message == "CHOFER TERMINA MOVIMIENTO")
                                        {
                                            return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (shift == "BODEGA 2" && estatusActual == "CAR" && requestGrua == "SI")
                        {
                            if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
                            {
                                //------------------------------------------------------------------------------
                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN CAMINO A PLANTA");
                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                    " message = @message WHERE Folio = @ID";
                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                {
                                    DBSPP.Open();
                                    coms.Parameters.AddWithValue("@message", "CHOFER EN CAMINO A PLANTA");
                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                    int rowsAffected = coms.ExecuteNonQuery();
                                    DBSPP.Close();
                                }
                                //------------------------------------------------------------------------------
                            }
                            else
                            {
                                if (message == "CHOFER EN CAMINO A PLANTA")
                                {
                                    //------------------------------------------------------------------------------
                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                        coms.Parameters.AddWithValue("@ID", "CHOFER INGRESANDO A RECIBOS");
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                        " message = @message WHERE Folio = @ID";
                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@message", "CHOFER INGRESANDO A RECIBOS");
                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                }
                                else
                                {
                                    if (message == "CHOFER INGRESANDO A RECIBOS")
                                    {
                                        //------------------------------------------------------------------------------
                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                            coms.Parameters.AddWithValue("@ID", "CHOFER ENGANCHANDO CHASIS");
                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                            " message = @message WHERE Folio = @ID";
                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@message", "CHOFER ENGANCHANDO CHASIS");
                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                    }
                                    else
                                    {
                                        if (message == "CHOFER ENGANCHANDO CHASIS")
                                        {
                                            //------------------------------------------------------------------------------
                                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                            {
                                                DBSPP.Open();
                                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE ACCESO A GRUA");
                                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                int rowsAffected = coms.ExecuteNonQuery();
                                                DBSPP.Close();
                                            }
                                            //------------------------------------------------------------------------------
                                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                " message = @message WHERE Folio = @ID";
                                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                            {
                                                DBSPP.Open();
                                                coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE ACCESO A GRUA");
                                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                int rowsAffected = coms.ExecuteNonQuery();
                                                DBSPP.Close();
                                            }
                                            //------------------------------------------------------------------------------
                                        }
                                        else
                                        {
                                            if (message == "CHOFER EN ESPERA DE ACCESO A GRUA")
                                            {
                                                //------------------------------------------------------------------------------
                                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                {
                                                    DBSPP.Open();
                                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE MANIOBRA EN GRUA");
                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                    DBSPP.Close();
                                                }
                                                //------------------------------------------------------------------------------
                                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                    " message = @message WHERE Folio = @ID";
                                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                {
                                                    DBSPP.Open();
                                                    coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE MANIOBRA EN GRUA");
                                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                    DBSPP.Close();
                                                }
                                                //------------------------------------------------------------------------------
                                            }
                                            else
                                            {
                                                if (message == "CHOFER EN ESPERA DE MANIOBRA EN GRUA")
                                                {
                                                    //------------------------------------------------------------------------------
                                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                    {
                                                        DBSPP.Open();
                                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                        coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SALIDA DE GRUA");
                                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                        int rowsAffected = coms.ExecuteNonQuery();
                                                        DBSPP.Close();
                                                    }
                                                    //------------------------------------------------------------------------------
                                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                        " message = @message WHERE Folio = @ID";
                                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                    {
                                                        DBSPP.Open();
                                                        coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SALIDA DE GRUA");
                                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                        int rowsAffected = coms.ExecuteNonQuery();
                                                        DBSPP.Close();
                                                    }
                                                    //------------------------------------------------------------------------------
                                                }
                                                else
                                                {
                                                    if (message == "CHOFER EN ESPERA DE SALIDA DE GRUA")
                                                    {
                                                        //------------------------------------------------------------------------------
                                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                        {
                                                            DBSPP.Open();
                                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                            coms.Parameters.AddWithValue("@ID", "CHOFER EN RUTA A RAMPA DESTINO");
                                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                            int rowsAffected = coms.ExecuteNonQuery();
                                                            DBSPP.Close();
                                                        }
                                                        //------------------------------------------------------------------------------
                                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                            " message = @message WHERE Folio = @ID";
                                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                        {
                                                            DBSPP.Open();
                                                            coms.Parameters.AddWithValue("@message", "CHOFER EN RUTA A RAMPA DESTINO");
                                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                            int rowsAffected = coms.ExecuteNonQuery();
                                                            DBSPP.Close();
                                                        }
                                                        //------------------------------------------------------------------------------
                                                    }
                                                    else
                                                    {
                                                        if (message == "CHOFER EN RUTA A RAMPA DESTINO")
                                                        {
                                                            //------------------------------------------------------------------------------
                                                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                            {
                                                                DBSPP.Open();
                                                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                                coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
                                                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                                int rowsAffected = coms.ExecuteNonQuery();
                                                                DBSPP.Close();
                                                            }
                                                            //------------------------------------------------------------------------------
                                                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                                " message = @message WHERE Folio = @ID";
                                                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                            {
                                                                DBSPP.Open();
                                                                coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
                                                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                                int rowsAffected = coms.ExecuteNonQuery();
                                                                DBSPP.Close();
                                                            }
                                                            //------------------------------------------------------------------------------
                                                        }
                                                        else
                                                        {
                                                            if (message == "CHOFER SOLTANDO CAJA")
                                                            {
                                                                //------------------------------------------------------------------------------
                                                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                                {
                                                                    DBSPP.Open();
                                                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                                    coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
                                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                                    DBSPP.Close();
                                                                }
                                                                //------------------------------------------------------------------------------
                                                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                                    " message = @message WHERE Folio = @ID";
                                                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                                {
                                                                    DBSPP.Open();
                                                                    coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
                                                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                                    DBSPP.Close();
                                                                }
                                                                //------------------------------------------------------------------------------
                                                                string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
                                                                    " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
                                                                using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
                                                                {
                                                                    DBSPP.Open();
                                                                    coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
                                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                                    DBSPP.Close();
                                                                }

                                                                //------------------------------------------------------------------------------
                                                                string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
                                                                    " Status = @Status WHERE Username = @Username";
                                                                using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
                                                                {
                                                                    DBSPP.Open();
                                                                    coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                                                                    coms.Parameters.AddWithValue("@Username", choffer.ToString());
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
                                                            else
                                                            {
                                                                if (message == "CHOFER TERMINA MOVIMIENTO")
                                                                {
                                                                    return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (shift == "BODEGA 2" && estatusActual == "VAC" && requestGrua == "SI")
                            {
                                if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
                                {
                                    //------------------------------------------------------------------------------
                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                        coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                        " message = @message WHERE Folio = @ID";
                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                    {
                                        DBSPP.Open();
                                        coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                        int rowsAffected = coms.ExecuteNonQuery();
                                        DBSPP.Close();
                                    }
                                    //------------------------------------------------------------------------------
                                }
                                else
                                {
                                    if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
                                    {
                                        //------------------------------------------------------------------------------
                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                            coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SELLO");
                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                            " message = @message WHERE Folio = @ID";
                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                        {
                                            DBSPP.Open();
                                            coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SELLO");
                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                                            int rowsAffected = coms.ExecuteNonQuery();
                                            DBSPP.Close();
                                        }
                                        //------------------------------------------------------------------------------
                                    }
                                    else
                                    {
                                        if (message == "CHOFER EN ESPERA DE SELLO")
                                        {
                                            //------------------------------------------------------------------------------
                                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                            {
                                                DBSPP.Open();
                                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE ACCESO DE GRÚA");
                                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                int rowsAffected = coms.ExecuteNonQuery();
                                                DBSPP.Close();
                                            }
                                            //------------------------------------------------------------------------------
                                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                " message = @message WHERE Folio = @ID";
                                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                            {
                                                DBSPP.Open();
                                                coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE ACCESO DE GRÚA");
                                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                int rowsAffected = coms.ExecuteNonQuery();
                                                DBSPP.Close();
                                            }
                                            //------------------------------------------------------------------------------
                                        }
                                        else
                                        {
                                            if (message == "CHOFER EN ESPERA DE ACCESO DE GRÚA")
                                            {
                                                //------------------------------------------------------------------------------
                                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                {
                                                    DBSPP.Open();
                                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE MANIOBRA DE GRUA");
                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                    DBSPP.Close();
                                                }
                                                //------------------------------------------------------------------------------
                                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                    " message = @message WHERE Folio = @ID";
                                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                {
                                                    DBSPP.Open();
                                                    coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE MANIOBRA DE GRUA");
                                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                    DBSPP.Close();
                                                }
                                                //------------------------------------------------------------------------------
                                            }
                                            else
                                            {
                                                if (message == "CHOFER EN ESPERA DE MANIOBRA DE GRUA")
                                                {
                                                    //------------------------------------------------------------------------------
                                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                    {
                                                        DBSPP.Open();
                                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                        coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE CARGADO DE GRUA");
                                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                        int rowsAffected = coms.ExecuteNonQuery();
                                                        DBSPP.Close();
                                                    }
                                                    //------------------------------------------------------------------------------
                                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                        " message = @message WHERE Folio = @ID";
                                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                    {
                                                        DBSPP.Open();
                                                        coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE CARGADO DE GRUA");
                                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                        int rowsAffected = coms.ExecuteNonQuery();
                                                        DBSPP.Close();
                                                    }
                                                    //------------------------------------------------------------------------------
                                                }
                                                else
                                                {
                                                    if (message == "CHOFER EN ESPERA DE CARGADO DE GRUA")
                                                    {
                                                        //------------------------------------------------------------------------------
                                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                        {
                                                            DBSPP.Open();
                                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                            coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SALIDA DE GRUA");
                                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                            int rowsAffected = coms.ExecuteNonQuery();
                                                            DBSPP.Close();
                                                        }
                                                        //------------------------------------------------------------------------------
                                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                            " message = @message WHERE Folio = @ID";
                                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                        {
                                                            DBSPP.Open();
                                                            coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SALIDA DE GRUA");
                                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                            int rowsAffected = coms.ExecuteNonQuery();
                                                            DBSPP.Close();
                                                        }
                                                        //------------------------------------------------------------------------------
                                                    }
                                                    else
                                                    {
                                                        if (message == "CHOFER EN ESPERA DE SALIDA DE GRUA")
                                                        {
                                                            //------------------------------------------------------------------------------
                                                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                            {
                                                                DBSPP.Open();
                                                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                                coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CHASIS");
                                                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                                int rowsAffected = coms.ExecuteNonQuery();
                                                                DBSPP.Close();
                                                            }
                                                            //------------------------------------------------------------------------------
                                                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                                " message = @message WHERE Folio = @ID";
                                                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                            {
                                                                DBSPP.Open();
                                                                coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CHASIS");
                                                                coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                                int rowsAffected = coms.ExecuteNonQuery();
                                                                DBSPP.Close();
                                                            }
                                                            //------------------------------------------------------------------------------
                                                        }
                                                        else
                                                        {
                                                            if (message == "CHOFER SOLTANDO CHASIS")
                                                            {
                                                                //------------------------------------------------------------------------------
                                                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                                                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                                                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                                                                {
                                                                    DBSPP.Open();
                                                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                                                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                                                                    coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
                                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                                    DBSPP.Close();
                                                                }
                                                                //------------------------------------------------------------------------------
                                                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                                                                    " message = @message WHERE Folio = @ID";
                                                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                                                                {
                                                                    DBSPP.Open();
                                                                    coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
                                                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                                    DBSPP.Close();
                                                                }
                                                                //------------------------------------------------------------------------------
                                                                string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
                                                                    " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
                                                                using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
                                                                {
                                                                    DBSPP.Open();
                                                                    coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
                                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                                                                    int rowsAffected = coms.ExecuteNonQuery();
                                                                    DBSPP.Close();
                                                                }

                                                                //------------------------------------------------------------------------------
                                                                string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
                                                                    " Status = @Status WHERE Username = @Username";
                                                                using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
                                                                {
                                                                    DBSPP.Open();
                                                                    coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                                                                    coms.Parameters.AddWithValue("@Username", choffer.ToString());
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
                                                            else
                                                            {
                                                                if (message == "CHOFER TERMINA MOVIMIENTO")
                                                                {
                                                                    return Json(new { success = false, redirectUrl = Url.Action("EntryContainer", "RADA") });
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return Json(new { success = true });
                //return RedirectToAction("EntryContainer", "RADA");
            }
        }

        public ActionResult ViewConfirm(string ID, string choffer, string requestGrua)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
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
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
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

                //------------------------------------------------------------------------------
                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
                {
                    DBSPP.Open();
                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
                    coms.Parameters.AddWithValue("@ID", "CHOFER ASIGNADO AL MOVIMIENTO");
                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
                    int rowsAffected = coms.ExecuteNonQuery();
                    DBSPP.Close();
                }
                //------------------------------------------------------------------------------
                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
                    " message = @message WHERE Folio = @ID";
                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
                {
                    DBSPP.Open();
                    coms.Parameters.AddWithValue("@message", "CHOFER ASIGNADO AL MOVIMIENTO");
                    coms.Parameters.AddWithValue("@ID", ID.ToString());
                    int rowsAffected = coms.ExecuteNonQuery();
                    DBSPP.Close();
                }
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

        //public ActionResult ViewComplete(string ID, string choffer, string RequestGrua)
        //{
        //    if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
        //    {
        //        Session["Username"] = Request.Cookies["UserCookie"].Value;
        //    }

        //    ViewBag.User = Session["Username"];

        //    if (Session.Count <= 0)
        //    {
        //        return RedirectToAction("LogIn", "Login");
        //    }
        //    else
        //    {
        //        Obtener la fecha y hora actual en Alemania(zona horaria UTC+1 o UTC+2 dependiendo del horario de verano)
        //        DateTime germanTime = DateTime.UtcNow.AddHours(0);  // Alemania es UTC+1

        //        Convertir la hora alemana a la hora en una zona horaria específica de EE. UU. (por ejemplo, Nueva York, UTC - 5)
        //        TimeZoneInfo usEasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        //        DateTime usTime = TimeZoneInfo.ConvertTime(germanTime, usEasternTimeZone);

        //        Formatear la fecha para que sea adecuada para la base de datos
        //        string formattedDate = usTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

        //        create generate randoms int value
        //        string estatusActual = null, message = null, shift = null;
        //        SqlCommand statuscontainer = new SqlCommand("Select * from RADAEmpire_BRequestContainers where Active = '1' and Folio = '" + ID.ToString() + "'", DBSPP);
        //        DBSPP.Open();
        //        SqlDataReader drstatuscontainer = statuscontainer.ExecuteReader();
        //        if (drstatuscontainer.HasRows)
        //        {
        //            while (drstatuscontainer.Read())
        //            {
        //                estatusActual = drstatuscontainer["Status"].ToString();
        //                message = drstatuscontainer["message"].ToString();
        //                shift = drstatuscontainer["shift"].ToString();
        //            }
        //        }
        //        DBSPP.Close();

        //        area de envios
        //        if (estatusActual == "CAR" && shift == "ENVIOS")
        //        {
        //            if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
        //            {
        //                ------------------------------------------------------------------------------
        //                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                {
        //                    DBSPP.Open();
        //                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                    coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                    int rowsAffected = coms.ExecuteNonQuery();
        //                    DBSPP.Close();
        //                }
        //                ------------------------------------------------------------------------------
        //                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                    " message = @message WHERE Folio = @ID";
        //                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                {
        //                    DBSPP.Open();
        //                    coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                    coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                    int rowsAffected = coms.ExecuteNonQuery();
        //                    DBSPP.Close();
        //                }
        //                ------------------------------------------------------------------------------
        //            }
        //            else
        //            {
        //                if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
        //                {
        //                    ------------------------------------------------------------------------------
        //                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                    {
        //                        DBSPP.Open();
        //                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                        coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ESTACIONAMIENTO CAJA");
        //                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                        int rowsAffected = coms.ExecuteNonQuery();
        //                        DBSPP.Close();
        //                    }
        //                    ------------------------------------------------------------------------------
        //                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                        " message = @message WHERE Folio = @ID";
        //                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                    {
        //                        DBSPP.Open();
        //                        coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ESTACIONAMIENTO CAJA");
        //                        coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                        int rowsAffected = coms.ExecuteNonQuery();
        //                        DBSPP.Close();
        //                    }
        //                    ------------------------------------------------------------------------------
        //                }
        //                else
        //                {
        //                    if (message == "CHOFER EN PROCESO DE ESTACIONAMIENTO CAJA")
        //                    {
        //                        ------------------------------------------------------------------------------
        //                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                            coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
        //                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                            " message = @message WHERE Folio = @ID";
        //                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
        //                            coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                    }
        //                    else
        //                    {
        //                        if (message == "CHOFER SOLTANDO CAJA")
        //                        {
        //                            ------------------------------------------------------------------------------
        //                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
        //                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                " message = @message WHERE Folio = @ID";
        //                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
        //                                coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                            string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
        //                                " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
        //                            using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
        //                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }

        //                            ------------------------------------------------------------------------------
        //                            string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
        //                                " Status = @Status WHERE Username = @Username";
        //                            using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
        //                                coms.Parameters.AddWithValue("@Username", choffer.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }

        //                            ------------------------------------------------------------------------------
        //                            string inactive = "UPDATE RADAEmpires_DZChofferMovement SET " +
        //                                " Active = @Active WHERE Foio = @Foio";
        //                            using (SqlCommand coms = new SqlCommand(inactive, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@Active", false);
        //                                coms.Parameters.AddWithValue("@Foio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (message == "CHOFER TERMINA MOVIMIENTO")
        //                            {
        //                                return RedirectToAction("EntryContainer", "RADA");
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (estatusActual == "VAC" && shift == "ENVIOS")
        //            {
        //                if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
        //                {
        //                    ------------------------------------------------------------------------------
        //                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                    {
        //                        DBSPP.Open();
        //                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                        coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                        int rowsAffected = coms.ExecuteNonQuery();
        //                        DBSPP.Close();
        //                    }
        //                    ------------------------------------------------------------------------------
        //                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                        " message = @message WHERE Folio = @ID";
        //                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                    {
        //                        DBSPP.Open();
        //                        coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                        coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                        int rowsAffected = coms.ExecuteNonQuery();
        //                        DBSPP.Close();
        //                    }
        //                    ------------------------------------------------------------------------------
        //                }
        //                else
        //                {
        //                    if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
        //                    {
        //                        ------------------------------------------------------------------------------
        //                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                            coms.Parameters.AddWithValue("@ID", "CHOFER EN RUTA A RAMPA DESTINO");
        //                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                            " message = @message WHERE Folio = @ID";
        //                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@message", "CHOFER EN RUTA A RAMPA DESTINO");
        //                            coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                    }
        //                    else
        //                    {
        //                        if (message == "CHOFER EN RUTA A RAMPA DESTINO")
        //                        {
        //                            ------------------------------------------------------------------------------
        //                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
        //                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                " message = @message WHERE Folio = @ID";
        //                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
        //                                coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                        }
        //                        else
        //                        {
        //                            if (message == "CHOFER SOLTANDO CAJA")
        //                            {
        //                                ------------------------------------------------------------------------------
        //                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                    coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
        //                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                                ------------------------------------------------------------------------------
        //                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                    " message = @message WHERE Folio = @ID";
        //                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
        //                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                                ------------------------------------------------------------------------------
        //                                string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
        //                                    " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
        //                                using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
        //                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }

        //                                ------------------------------------------------------------------------------
        //                                string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
        //                                    " Status = @Status WHERE Username = @Username";
        //                                using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
        //                                    coms.Parameters.AddWithValue("@Username", choffer.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }

        //                                ------------------------------------------------------------------------------
        //                                string inactive = "UPDATE RADAEmpires_DZChofferMovement SET " +
        //                                    " Active = @Active WHERE Foio = @Foio";
        //                                using (SqlCommand coms = new SqlCommand(inactive, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@Active", false);
        //                                    coms.Parameters.AddWithValue("@Foio", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (message == "CHOFER TERMINA MOVIMIENTO")
        //                                {
        //                                    return RedirectToAction("EntryContainer", "RADA");
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        area de PT
        //        if (estatusActual == "CAR" && shift == "PT")
        //        {
        //            if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
        //            {
        //                ------------------------------------------------------------------------------
        //                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                {
        //                    DBSPP.Open();
        //                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                    coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                    int rowsAffected = coms.ExecuteNonQuery();
        //                    DBSPP.Close();
        //                }
        //                ------------------------------------------------------------------------------
        //                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                    " message = @message WHERE Folio = @ID";
        //                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                {
        //                    DBSPP.Open();
        //                    coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                    coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                    int rowsAffected = coms.ExecuteNonQuery();
        //                    DBSPP.Close();
        //                }
        //                ------------------------------------------------------------------------------
        //            }
        //            else
        //            {
        //                if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
        //                {
        //                    ------------------------------------------------------------------------------
        //                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                    {
        //                        DBSPP.Open();
        //                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                        coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ESTACIONAMIENTO CAJA");
        //                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                        int rowsAffected = coms.ExecuteNonQuery();
        //                        DBSPP.Close();
        //                    }
        //                    ------------------------------------------------------------------------------
        //                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                        " message = @message WHERE Folio = @ID";
        //                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                    {
        //                        DBSPP.Open();
        //                        coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ESTACIONAMIENTO CAJA");
        //                        coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                        int rowsAffected = coms.ExecuteNonQuery();
        //                        DBSPP.Close();
        //                    }
        //                    ------------------------------------------------------------------------------
        //                }
        //                else
        //                {
        //                    if (message == "CHOFER EN PROCESO DE ESTACIONAMIENTO CAJA")
        //                    {
        //                        ------------------------------------------------------------------------------
        //                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                            coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
        //                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                            " message = @message WHERE Folio = @ID";
        //                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
        //                            coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                    }
        //                    else
        //                    {
        //                        if (message == "CHOFER SOLTANDO CAJA")
        //                        {
        //                            ------------------------------------------------------------------------------
        //                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
        //                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                " message = @message WHERE Folio = @ID";
        //                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
        //                                coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                            string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
        //                                " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
        //                            using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
        //                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }

        //                            ------------------------------------------------------------------------------
        //                            string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
        //                                " Status = @Status WHERE Username = @Username";
        //                            using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
        //                                coms.Parameters.AddWithValue("@Username", choffer.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }

        //                            ------------------------------------------------------------------------------
        //                            string inactive = "UPDATE RADAEmpires_DZChofferMovement SET " +
        //                                " Active = @Active WHERE Foio = @Foio";
        //                            using (SqlCommand coms = new SqlCommand(inactive, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@Active", false);
        //                                coms.Parameters.AddWithValue("@Foio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (message == "CHOFER TERMINA MOVIMIENTO")
        //                            {
        //                                return RedirectToAction("EntryContainer", "RADA");
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //        }
        //        else
        //        {
        //            if (estatusActual == "VAC" && shift == "PT")
        //            {
        //                if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
        //                {
        //                    ------------------------------------------------------------------------------
        //                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                    {
        //                        DBSPP.Open();
        //                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                        coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                        int rowsAffected = coms.ExecuteNonQuery();
        //                        DBSPP.Close();
        //                    }
        //                    ------------------------------------------------------------------------------
        //                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                        " message = @message WHERE Folio = @ID";
        //                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                    {
        //                        DBSPP.Open();
        //                        coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                        coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                        int rowsAffected = coms.ExecuteNonQuery();
        //                        DBSPP.Close();
        //                    }
        //                    ------------------------------------------------------------------------------
        //                }
        //                else
        //                {
        //                    if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
        //                    {
        //                        ------------------------------------------------------------------------------
        //                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                            coms.Parameters.AddWithValue("@ID", "CHOFER EN RUTA A RAMPA DESTINO");
        //                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                            " message = @message WHERE Folio = @ID";
        //                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@message", "CHOFER EN RUTA A RAMPA DESTINO");
        //                            coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                    }
        //                    else
        //                    {
        //                        if (message == "CHOFER EN RUTA A RAMPA DESTINO")
        //                        {
        //                            ------------------------------------------------------------------------------
        //                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
        //                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                " message = @message WHERE Folio = @ID";
        //                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
        //                                coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                        }
        //                        else
        //                        {
        //                            if (message == "CHOFER SOLTANDO CAJA")
        //                            {
        //                                ------------------------------------------------------------------------------
        //                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                    coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
        //                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                                ------------------------------------------------------------------------------
        //                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                    " message = @message WHERE Folio = @ID";
        //                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
        //                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                                ------------------------------------------------------------------------------
        //                                string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
        //                                    " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
        //                                using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
        //                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }

        //                                ------------------------------------------------------------------------------
        //                                string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
        //                                    " Status = @Status WHERE Username = @Username";
        //                                using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
        //                                    coms.Parameters.AddWithValue("@Username", choffer.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }

        //                                ------------------------------------------------------------------------------
        //                                string inactive = "UPDATE RADAEmpires_DZChofferMovement SET " +
        //                                    " Active = @Active WHERE Foio = @Foio";
        //                                using (SqlCommand coms = new SqlCommand(inactive, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@Active", false);
        //                                    coms.Parameters.AddWithValue("@Foio", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (message == "CHOFER TERMINA MOVIMIENTO")
        //                                {
        //                                    return RedirectToAction("EntryContainer", "RADA");
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        area de EMPAQUE
        //        if (shift == "EMPAQUE" && estatusActual == "CAR" && RequestGrua == "NO")
        //        {
        //            if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
        //            {
        //                ------------------------------------------------------------------------------
        //                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                {
        //                    DBSPP.Open();
        //                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                    coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                    int rowsAffected = coms.ExecuteNonQuery();
        //                    DBSPP.Close();
        //                }
        //                ------------------------------------------------------------------------------
        //                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                    " message = @message WHERE Folio = @ID";
        //                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                {
        //                    DBSPP.Open();
        //                    coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                    coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                    int rowsAffected = coms.ExecuteNonQuery();
        //                    DBSPP.Close();
        //                }
        //                ------------------------------------------------------------------------------
        //            }
        //            else
        //            {
        //                if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
        //                {
        //                    ------------------------------------------------------------------------------
        //                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                    {
        //                        DBSPP.Open();
        //                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                        coms.Parameters.AddWithValue("@ID", "CHOFER EN RUTA A RAMPA DESTINO");
        //                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                        int rowsAffected = coms.ExecuteNonQuery();
        //                        DBSPP.Close();
        //                    }
        //                    ------------------------------------------------------------------------------
        //                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                        " message = @message WHERE Folio = @ID";
        //                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                    {
        //                        DBSPP.Open();
        //                        coms.Parameters.AddWithValue("@message", "CHOFER EN RUTA A RAMPA DESTINO");
        //                        coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                        int rowsAffected = coms.ExecuteNonQuery();
        //                        DBSPP.Close();
        //                    }
        //                    ------------------------------------------------------------------------------
        //                }
        //                else
        //                {
        //                    if (message == "CHOFER EN RUTA A RAMPA DESTINO")
        //                    {
        //                        ------------------------------------------------------------------------------
        //                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                            coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
        //                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                            " message = @message WHERE Folio = @ID";
        //                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
        //                            coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                    }
        //                    else
        //                    {
        //                        if (message == "CHOFER SOLTANDO CAJA")
        //                        {
        //                            ------------------------------------------------------------------------------
        //                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
        //                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                " message = @message WHERE Folio = @ID";
        //                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
        //                                coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                            string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
        //                                " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
        //                            using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
        //                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }

        //                            ------------------------------------------------------------------------------
        //                            string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
        //                                " Status = @Status WHERE Username = @Username";
        //                            using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
        //                                coms.Parameters.AddWithValue("@Username", choffer.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }

        //                            ------------------------------------------------------------------------------
        //                            string inactive = "UPDATE RADAEmpires_DZChofferMovement SET " +
        //                                " Active = @Active WHERE Foio = @Foio";
        //                            using (SqlCommand coms = new SqlCommand(inactive, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@Active", false);
        //                                coms.Parameters.AddWithValue("@Foio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (message == "CHOFER TERMINA MOVIMIENTO")
        //                            {
        //                                return RedirectToAction("EntryContainer", "RADA");
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (shift == "EMPAQUE" && estatusActual == "VAC" && RequestGrua == "NO")
        //            {
        //                if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
        //                {
        //                    ------------------------------------------------------------------------------
        //                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                    {
        //                        DBSPP.Open();
        //                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                        coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                        int rowsAffected = coms.ExecuteNonQuery();
        //                        DBSPP.Close();
        //                    }
        //                    ------------------------------------------------------------------------------
        //                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                        " message = @message WHERE Folio = @ID";
        //                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                    {
        //                        DBSPP.Open();
        //                        coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                        coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                        int rowsAffected = coms.ExecuteNonQuery();
        //                        DBSPP.Close();
        //                    }
        //                    ------------------------------------------------------------------------------
        //                }
        //                else
        //                {
        //                    if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
        //                    {
        //                        ------------------------------------------------------------------------------
        //                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                            coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SELLO");
        //                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                            " message = @message WHERE Folio = @ID";
        //                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SELLO");
        //                            coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                    }
        //                    else
        //                    {
        //                        if (message == "CHOFER EN ESPERA DE SELLO")
        //                        {
        //                            ------------------------------------------------------------------------------
        //                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE AREA DE VACIO");
        //                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                " message = @message WHERE Folio = @ID";
        //                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE AREA DE VACIO");
        //                                coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                        }
        //                        else
        //                        {
        //                            if (message == "CHOFER EN ESPERA DE AREA DE VACIO")
        //                            {
        //                                ------------------------------------------------------------------------------
        //                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                    coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
        //                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                                ------------------------------------------------------------------------------
        //                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                    " message = @message WHERE Folio = @ID";
        //                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
        //                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                                ------------------------------------------------------------------------------
        //                                string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
        //                                    " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
        //                                using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
        //                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }

        //                                ------------------------------------------------------------------------------
        //                                string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
        //                                    " Status = @Status WHERE Username = @Username";
        //                                using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
        //                                    coms.Parameters.AddWithValue("@Username", choffer.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }

        //                                ------------------------------------------------------------------------------
        //                                string inactive = "UPDATE RADAEmpires_DZChofferMovement SET " +
        //                                    " Active = @Active WHERE Foio = @Foio";
        //                                using (SqlCommand coms = new SqlCommand(inactive, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@Active", false);
        //                                    coms.Parameters.AddWithValue("@Foio", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (message == "CHOFER TERMINA MOVIMIENTO")
        //                                {
        //                                    return RedirectToAction("EntryContainer", "RADA");
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (shift == "EMPAQUE" && estatusActual == "CAR" && RequestGrua == "SI")
        //                {
        //                    if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
        //                    {
        //                        ------------------------------------------------------------------------------
        //                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                            coms.Parameters.AddWithValue("@ID", "CHOFER ENGANCHANDO CHASIS");
        //                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                            " message = @message WHERE Folio = @ID";
        //                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@message", "CHOFER ENGANCHANDO CHASIS");
        //                            coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                    }
        //                    else
        //                    {
        //                        if (message == "CHOFER ENGANCHANDO CHASIS")
        //                        {
        //                            ------------------------------------------------------------------------------
        //                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE ACCESO A GRUA");
        //                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                " message = @message WHERE Folio = @ID";
        //                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE ACCESO A GRUA");
        //                                coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                        }
        //                        else
        //                        {
        //                            if (message == "CHOFER EN ESPERA DE ACCESO A GRUA")
        //                            {
        //                                ------------------------------------------------------------------------------
        //                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE MANIOBRA EN GRUA");
        //                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                                ------------------------------------------------------------------------------
        //                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                    " message = @message WHERE Folio = @ID";
        //                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE MANIOBRA EN GRUA");
        //                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                                ------------------------------------------------------------------------------
        //                            }
        //                            else
        //                            {
        //                                if (message == "CHOFER EN ESPERA DE MANIOBRA EN GRUA")
        //                                {
        //                                    ------------------------------------------------------------------------------
        //                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                    {
        //                                        DBSPP.Open();
        //                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                        coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SALIDA DE GRUA");
        //                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                        int rowsAffected = coms.ExecuteNonQuery();
        //                                        DBSPP.Close();
        //                                    }
        //                                    ------------------------------------------------------------------------------
        //                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                        " message = @message WHERE Folio = @ID";
        //                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                    {
        //                                        DBSPP.Open();
        //                                        coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SALIDA DE GRUA");
        //                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                        int rowsAffected = coms.ExecuteNonQuery();
        //                                        DBSPP.Close();
        //                                    }
        //                                    ------------------------------------------------------------------------------
        //                                }
        //                                else
        //                                {
        //                                    if (message == "CHOFER EN ESPERA DE SALIDA DE GRUA")
        //                                    {
        //                                        ------------------------------------------------------------------------------
        //                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                        {
        //                                            DBSPP.Open();
        //                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                            coms.Parameters.AddWithValue("@ID", "CHOFER EN RUTA A RAMPA DESTINO");
        //                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                            DBSPP.Close();
        //                                        }
        //                                        ------------------------------------------------------------------------------
        //                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                            " message = @message WHERE Folio = @ID";
        //                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                        {
        //                                            DBSPP.Open();
        //                                            coms.Parameters.AddWithValue("@message", "CHOFER EN RUTA A RAMPA DESTINO");
        //                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                            DBSPP.Close();
        //                                        }
        //                                        ------------------------------------------------------------------------------
        //                                    }
        //                                    else
        //                                    {
        //                                        if (message == "CHOFER EN RUTA A RAMPA DESTINO")
        //                                        {
        //                                            ------------------------------------------------------------------------------
        //                                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                            {
        //                                                DBSPP.Open();
        //                                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                                coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
        //                                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                                int rowsAffected = coms.ExecuteNonQuery();
        //                                                DBSPP.Close();
        //                                            }
        //                                            ------------------------------------------------------------------------------
        //                                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                                " message = @message WHERE Folio = @ID";
        //                                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                            {
        //                                                DBSPP.Open();
        //                                                coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
        //                                                coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                                int rowsAffected = coms.ExecuteNonQuery();
        //                                                DBSPP.Close();
        //                                            }
        //                                            ------------------------------------------------------------------------------
        //                                        }
        //                                        else
        //                                        {
        //                                            if (message == "CHOFER SOLTANDO CAJA")
        //                                            {
        //                                                if (message == "CHOFER SOLTANDO CAJA")
        //                                                {
        //                                                    ------------------------------------------------------------------------------
        //                                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                                    {
        //                                                        DBSPP.Open();
        //                                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                                        coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
        //                                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                                        int rowsAffected = coms.ExecuteNonQuery();
        //                                                        DBSPP.Close();
        //                                                    }
        //                                                    ------------------------------------------------------------------------------
        //                                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                                        " message = @message WHERE Folio = @ID";
        //                                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                                    {
        //                                                        DBSPP.Open();
        //                                                        coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
        //                                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                                        int rowsAffected = coms.ExecuteNonQuery();
        //                                                        DBSPP.Close();
        //                                                    }
        //                                                    ------------------------------------------------------------------------------
        //                                                    string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
        //                                                        " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
        //                                                    using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
        //                                                    {
        //                                                        DBSPP.Open();
        //                                                        coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
        //                                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                                        int rowsAffected = coms.ExecuteNonQuery();
        //                                                        DBSPP.Close();
        //                                                    }

        //                                                    ------------------------------------------------------------------------------
        //                                                    string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
        //                                                        " Status = @Status WHERE Username = @Username";
        //                                                    using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
        //                                                    {
        //                                                        DBSPP.Open();
        //                                                        coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
        //                                                        coms.Parameters.AddWithValue("@Username", choffer.ToString());
        //                                                        int rowsAffected = coms.ExecuteNonQuery();
        //                                                        DBSPP.Close();
        //                                                    }

        //                                                    ------------------------------------------------------------------------------
        //                                                    string inactive = "UPDATE RADAEmpires_DZChofferMovement SET " +
        //                                                        " Active = @Active WHERE Foio = @Foio";
        //                                                    using (SqlCommand coms = new SqlCommand(inactive, DBSPP))
        //                                                    {
        //                                                        DBSPP.Open();
        //                                                        coms.Parameters.AddWithValue("@Active", false);
        //                                                        coms.Parameters.AddWithValue("@Foio", ID.ToString());
        //                                                        int rowsAffected = coms.ExecuteNonQuery();
        //                                                        DBSPP.Close();
        //                                                    }
        //                                                }
        //                                                else
        //                                                {
        //                                                    if (message == "CHOFER TERMINA MOVIMIENTO")
        //                                                    {
        //                                                        return RedirectToAction("EntryContainer", "RADA");
        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (shift == "EMPAQUE" && estatusActual == "VAC" && RequestGrua == "SI")
        //                    {
        //                        if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
        //                        {
        //                            ------------------------------------------------------------------------------
        //                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                " message = @message WHERE Folio = @ID";
        //                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                                coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                        }
        //                        else
        //                        {
        //                            if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
        //                            {
        //                                ------------------------------------------------------------------------------
        //                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SELLO");
        //                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                                ------------------------------------------------------------------------------
        //                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                    " message = @message WHERE Folio = @ID";
        //                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SELLO");
        //                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                                ------------------------------------------------------------------------------
        //                            }
        //                            else
        //                            {
        //                                if (message == "CHOFER EN ESPERA DE SELLO")
        //                                {
        //                                    ------------------------------------------------------------------------------
        //                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                    {
        //                                        DBSPP.Open();
        //                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                        coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE ACCESO DE GRÚA");
        //                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                        int rowsAffected = coms.ExecuteNonQuery();
        //                                        DBSPP.Close();
        //                                    }
        //                                    ------------------------------------------------------------------------------
        //                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                        " message = @message WHERE Folio = @ID";
        //                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                    {
        //                                        DBSPP.Open();
        //                                        coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE ACCESO DE GRÚA");
        //                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                        int rowsAffected = coms.ExecuteNonQuery();
        //                                        DBSPP.Close();
        //                                    }
        //                                    ------------------------------------------------------------------------------
        //                                }
        //                                else
        //                                {
        //                                    if (message == "CHOFER EN ESPERA DE ACCESO DE GRÚA")
        //                                    {
        //                                        ------------------------------------------------------------------------------
        //                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                        {
        //                                            DBSPP.Open();
        //                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                            coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE MANIOBRA DE GRUA");
        //                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                            DBSPP.Close();
        //                                        }
        //                                        ------------------------------------------------------------------------------
        //                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                            " message = @message WHERE Folio = @ID";
        //                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                        {
        //                                            DBSPP.Open();
        //                                            coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE MANIOBRA DE GRUA");
        //                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                            DBSPP.Close();
        //                                        }
        //                                        ------------------------------------------------------------------------------
        //                                    }
        //                                    else
        //                                    {
        //                                        if (message == "CHOFER EN ESPERA DE MANIOBRA DE GRUA")
        //                                        {
        //                                            ------------------------------------------------------------------------------
        //                                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                            {
        //                                                DBSPP.Open();
        //                                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                                coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE CARGADO DE GRUA");
        //                                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                                int rowsAffected = coms.ExecuteNonQuery();
        //                                                DBSPP.Close();
        //                                            }
        //                                            ------------------------------------------------------------------------------
        //                                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                                " message = @message WHERE Folio = @ID";
        //                                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                            {
        //                                                DBSPP.Open();
        //                                                coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE CARGADO DE GRUA");
        //                                                coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                                int rowsAffected = coms.ExecuteNonQuery();
        //                                                DBSPP.Close();
        //                                            }
        //                                            ------------------------------------------------------------------------------
        //                                        }
        //                                        else
        //                                        {
        //                                            if (message == "CHOFER EN ESPERA DE CARGADO DE GRUA")
        //                                            {
        //                                                ------------------------------------------------------------------------------
        //                                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                                {
        //                                                    DBSPP.Open();
        //                                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SALIDA DE GRUA");
        //                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                                    DBSPP.Close();
        //                                                }
        //                                                ------------------------------------------------------------------------------
        //                                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                                    " message = @message WHERE Folio = @ID";
        //                                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                                {
        //                                                    DBSPP.Open();
        //                                                    coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SALIDA DE GRUA");
        //                                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                                    DBSPP.Close();
        //                                                }
        //                                                ------------------------------------------------------------------------------
        //                                            }
        //                                            else
        //                                            {
        //                                                if (message == "CHOFER EN ESPERA DE SALIDA DE GRUA")
        //                                                {
        //                                                    ------------------------------------------------------------------------------
        //                                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                                    {
        //                                                        DBSPP.Open();
        //                                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                                        coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CHASIS");
        //                                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                                        int rowsAffected = coms.ExecuteNonQuery();
        //                                                        DBSPP.Close();
        //                                                    }
        //                                                    ------------------------------------------------------------------------------
        //                                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                                        " message = @message WHERE Folio = @ID";
        //                                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                                    {
        //                                                        DBSPP.Open();
        //                                                        coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CHASIS");
        //                                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                                        int rowsAffected = coms.ExecuteNonQuery();
        //                                                        DBSPP.Close();
        //                                                    }
        //                                                    ------------------------------------------------------------------------------
        //                                                }
        //                                                else
        //                                                {
        //                                                    if (message == "CHOFER SOLTANDO CHASIS")
        //                                                    {
        //                                                        ------------------------------------------------------------------------------
        //                                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                                        {
        //                                                            DBSPP.Open();
        //                                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                                            coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
        //                                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                                            DBSPP.Close();
        //                                                        }
        //                                                        ------------------------------------------------------------------------------
        //                                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                                            " message = @message WHERE Folio = @ID";
        //                                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                                        {
        //                                                            DBSPP.Open();
        //                                                            coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
        //                                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                                            DBSPP.Close();
        //                                                        }
        //                                                        ------------------------------------------------------------------------------
        //                                                        string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
        //                                                            " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
        //                                                        using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
        //                                                        {
        //                                                            DBSPP.Open();
        //                                                            coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
        //                                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                                            DBSPP.Close();
        //                                                        }

        //                                                        ------------------------------------------------------------------------------
        //                                                        string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
        //                                                            " Status = @Status WHERE Username = @Username";
        //                                                        using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
        //                                                        {
        //                                                            DBSPP.Open();
        //                                                            coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
        //                                                            coms.Parameters.AddWithValue("@Username", choffer.ToString());
        //                                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                                            DBSPP.Close();
        //                                                        }

        //                                                        ------------------------------------------------------------------------------
        //                                                        string inactive = "UPDATE RADAEmpires_DZChofferMovement SET " +
        //                                                            " Active = @Active WHERE Foio = @Foio";
        //                                                        using (SqlCommand coms = new SqlCommand(inactive, DBSPP))
        //                                                        {
        //                                                            DBSPP.Open();
        //                                                            coms.Parameters.AddWithValue("@Active", false);
        //                                                            coms.Parameters.AddWithValue("@Foio", ID.ToString());
        //                                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                                            DBSPP.Close();
        //                                                        }
        //                                                    }
        //                                                    else
        //                                                    {
        //                                                        if (message == "CHOFER TERMINA MOVIMIENTO")
        //                                                        {
        //                                                            return RedirectToAction("EntryContainer", "RADA");
        //                                                        }
        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        area de GENERALES
        //        if (shift == "GENERALES" && estatusActual == "CAR" && RequestGrua == "NO")
        //        {
        //            if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
        //            {
        //                ------------------------------------------------------------------------------
        //                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                {
        //                    DBSPP.Open();
        //                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                    coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                    int rowsAffected = coms.ExecuteNonQuery();
        //                    DBSPP.Close();
        //                }
        //                ------------------------------------------------------------------------------
        //                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                    " message = @message WHERE Folio = @ID";
        //                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                {
        //                    DBSPP.Open();
        //                    coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                    coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                    int rowsAffected = coms.ExecuteNonQuery();
        //                    DBSPP.Close();
        //                }
        //                ------------------------------------------------------------------------------
        //            }
        //            else
        //            {
        //                if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
        //                {
        //                    ------------------------------------------------------------------------------
        //                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                    {
        //                        DBSPP.Open();
        //                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                        coms.Parameters.AddWithValue("@ID", "CHOFER EN RUTA A RAMPA DESTINO");
        //                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                        int rowsAffected = coms.ExecuteNonQuery();
        //                        DBSPP.Close();
        //                    }
        //                    ------------------------------------------------------------------------------
        //                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                        " message = @message WHERE Folio = @ID";
        //                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                    {
        //                        DBSPP.Open();
        //                        coms.Parameters.AddWithValue("@message", "CHOFER EN RUTA A RAMPA DESTINO");
        //                        coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                        int rowsAffected = coms.ExecuteNonQuery();
        //                        DBSPP.Close();
        //                    }
        //                    ------------------------------------------------------------------------------
        //                }
        //                else
        //                {
        //                    if (message == "CHOFER EN RUTA A RAMPA DESTINO")
        //                    {
        //                        ------------------------------------------------------------------------------
        //                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                            coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
        //                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                            " message = @message WHERE Folio = @ID";
        //                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
        //                            coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                    }
        //                    else
        //                    {
        //                        if (message == "CHOFER SOLTANDO CAJA")
        //                        {
        //                            ------------------------------------------------------------------------------
        //                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
        //                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                " message = @message WHERE Folio = @ID";
        //                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
        //                                coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                            string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
        //                                " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
        //                            using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
        //                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }

        //                            ------------------------------------------------------------------------------
        //                            string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
        //                                " Status = @Status WHERE Username = @Username";
        //                            using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
        //                                coms.Parameters.AddWithValue("@Username", choffer.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }

        //                            ------------------------------------------------------------------------------
        //                            string inactive = "UPDATE RADAEmpires_DZChofferMovement SET " +
        //                                " Active = @Active WHERE Foio = @Foio";
        //                            using (SqlCommand coms = new SqlCommand(inactive, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@Active", false);
        //                                coms.Parameters.AddWithValue("@Foio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                        }
        //                        else
        //                        {
        //                            if (message == "CHOFER TERMINA MOVIMIENTO")
        //                            {
        //                                return RedirectToAction("EntryContainer", "RADA");
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        else
        //        {
        //            if (shift == "GENERALES" && estatusActual == "VAC" && RequestGrua == "NO")
        //            {
        //                if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
        //                {
        //                    ------------------------------------------------------------------------------
        //                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                    {
        //                        DBSPP.Open();
        //                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                        coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                        int rowsAffected = coms.ExecuteNonQuery();
        //                        DBSPP.Close();
        //                    }
        //                    ------------------------------------------------------------------------------
        //                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                        " message = @message WHERE Folio = @ID";
        //                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                    {
        //                        DBSPP.Open();
        //                        coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                        coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                        int rowsAffected = coms.ExecuteNonQuery();
        //                        DBSPP.Close();
        //                    }
        //                    ------------------------------------------------------------------------------
        //                }
        //                else
        //                {
        //                    if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
        //                    {
        //                        ------------------------------------------------------------------------------
        //                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                            coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SELLO");
        //                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                            " message = @message WHERE Folio = @ID";
        //                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SELLO");
        //                            coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                    }
        //                    else
        //                    {
        //                        if (message == "CHOFER EN ESPERA DE SELLO")
        //                        {
        //                            ------------------------------------------------------------------------------
        //                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE AREA DE VACIO");
        //                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                " message = @message WHERE Folio = @ID";
        //                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE AREA DE VACIO");
        //                                coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                        }
        //                        else
        //                        {
        //                            if (message == "CHOFER EN ESPERA DE AREA DE VACIO")
        //                            {
        //                                ------------------------------------------------------------------------------
        //                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                    coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
        //                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                                ------------------------------------------------------------------------------
        //                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                    " message = @message WHERE Folio = @ID";
        //                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
        //                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                                ------------------------------------------------------------------------------
        //                                string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
        //                                    " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
        //                                using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
        //                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }

        //                                ------------------------------------------------------------------------------
        //                                string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
        //                                    " Status = @Status WHERE Username = @Username";
        //                                using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
        //                                    coms.Parameters.AddWithValue("@Username", choffer.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }

        //                                ------------------------------------------------------------------------------
        //                                string inactive = "UPDATE RADAEmpires_DZChofferMovement SET " +
        //                                    " Active = @Active WHERE Foio = @Foio";
        //                                using (SqlCommand coms = new SqlCommand(inactive, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@Active", false);
        //                                    coms.Parameters.AddWithValue("@Foio", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                            }
        //                            else
        //                            {
        //                                if (message == "CHOFER TERMINA MOVIMIENTO")
        //                                {
        //                                    return RedirectToAction("EntryContainer", "RADA");
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (shift == "GENERALES" && estatusActual == "CAR" && RequestGrua == "SI")
        //                {
        //                    if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
        //                    {
        //                        ------------------------------------------------------------------------------
        //                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                            coms.Parameters.AddWithValue("@ID", "CHOFER ENGANCHANDO CHASIS");
        //                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                            " message = @message WHERE Folio = @ID";
        //                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                        {
        //                            DBSPP.Open();
        //                            coms.Parameters.AddWithValue("@message", "CHOFER ENGANCHANDO CHASIS");
        //                            coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                            int rowsAffected = coms.ExecuteNonQuery();
        //                            DBSPP.Close();
        //                        }
        //                        ------------------------------------------------------------------------------
        //                    }
        //                    else
        //                    {
        //                        if (message == "CHOFER ENGANCHANDO CHASIS")
        //                        {
        //                            ------------------------------------------------------------------------------
        //                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE ACCESO A GRUA");
        //                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                " message = @message WHERE Folio = @ID";
        //                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE ACCESO A GRUA");
        //                                coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                        }
        //                        else
        //                        {
        //                            if (message == "CHOFER EN ESPERA DE ACCESO A GRUA")
        //                            {
        //                                ------------------------------------------------------------------------------
        //                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE MANIOBRA EN GRUA");
        //                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                                ------------------------------------------------------------------------------
        //                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                    " message = @message WHERE Folio = @ID";
        //                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE MANIOBRA EN GRUA");
        //                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                                ------------------------------------------------------------------------------
        //                            }
        //                            else
        //                            {
        //                                if (message == "CHOFER EN ESPERA DE MANIOBRA EN GRUA")
        //                                {
        //                                    ------------------------------------------------------------------------------
        //                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                    {
        //                                        DBSPP.Open();
        //                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                        coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SALIDA DE GRUA");
        //                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                        int rowsAffected = coms.ExecuteNonQuery();
        //                                        DBSPP.Close();
        //                                    }
        //                                    ------------------------------------------------------------------------------
        //                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                        " message = @message WHERE Folio = @ID";
        //                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                    {
        //                                        DBSPP.Open();
        //                                        coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SALIDA DE GRUA");
        //                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                        int rowsAffected = coms.ExecuteNonQuery();
        //                                        DBSPP.Close();
        //                                    }
        //                                    ------------------------------------------------------------------------------
        //                                }
        //                                else
        //                                {
        //                                    if (message == "CHOFER EN ESPERA DE SALIDA DE GRUA")
        //                                    {
        //                                        ------------------------------------------------------------------------------
        //                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                        {
        //                                            DBSPP.Open();
        //                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                            coms.Parameters.AddWithValue("@ID", "CHOFER EN RUTA A RAMPA DESTINO");
        //                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                            DBSPP.Close();
        //                                        }
        //                                        ------------------------------------------------------------------------------
        //                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                            " message = @message WHERE Folio = @ID";
        //                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                        {
        //                                            DBSPP.Open();
        //                                            coms.Parameters.AddWithValue("@message", "CHOFER EN RUTA A RAMPA DESTINO");
        //                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                            DBSPP.Close();
        //                                        }
        //                                        ------------------------------------------------------------------------------
        //                                    }
        //                                    else
        //                                    {
        //                                        if (message == "CHOFER EN RUTA A RAMPA DESTINO")
        //                                        {
        //                                            ------------------------------------------------------------------------------
        //                                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                            {
        //                                                DBSPP.Open();
        //                                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                                coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CAJA");
        //                                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                                int rowsAffected = coms.ExecuteNonQuery();
        //                                                DBSPP.Close();
        //                                            }
        //                                            ------------------------------------------------------------------------------
        //                                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                                " message = @message WHERE Folio = @ID";
        //                                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                            {
        //                                                DBSPP.Open();
        //                                                coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CAJA");
        //                                                coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                                int rowsAffected = coms.ExecuteNonQuery();
        //                                                DBSPP.Close();
        //                                            }
        //                                            ------------------------------------------------------------------------------
        //                                        }
        //                                        else
        //                                        {
        //                                            if (message == "CHOFER SOLTANDO CAJA")
        //                                            {
        //                                                ------------------------------------------------------------------------------
        //                                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                                {
        //                                                    DBSPP.Open();
        //                                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                                    coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
        //                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                                    DBSPP.Close();
        //                                                }
        //                                                ------------------------------------------------------------------------------
        //                                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                                    " message = @message WHERE Folio = @ID";
        //                                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                                {
        //                                                    DBSPP.Open();
        //                                                    coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
        //                                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                                    DBSPP.Close();
        //                                                }
        //                                                ------------------------------------------------------------------------------
        //                                                string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
        //                                                    " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
        //                                                using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
        //                                                {
        //                                                    DBSPP.Open();
        //                                                    coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
        //                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                                    DBSPP.Close();
        //                                                }

        //                                                ------------------------------------------------------------------------------
        //                                                string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
        //                                                    " Status = @Status WHERE Username = @Username";
        //                                                using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
        //                                                {
        //                                                    DBSPP.Open();
        //                                                    coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
        //                                                    coms.Parameters.AddWithValue("@Username", choffer.ToString());
        //                                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                                    DBSPP.Close();
        //                                                }

        //                                                ------------------------------------------------------------------------------
        //                                                string inactive = "UPDATE RADAEmpires_DZChofferMovement SET " +
        //                                                    " Active = @Active WHERE Foio = @Foio";
        //                                                using (SqlCommand coms = new SqlCommand(inactive, DBSPP))
        //                                                {
        //                                                    DBSPP.Open();
        //                                                    coms.Parameters.AddWithValue("@Active", false);
        //                                                    coms.Parameters.AddWithValue("@Foio", ID.ToString());
        //                                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                                    DBSPP.Close();
        //                                                }
        //                                            }
        //                                            else
        //                                            {
        //                                                if (message == "CHOFER TERMINA MOVIMIENTO")
        //                                                {
        //                                                    return RedirectToAction("EntryContainer", "RADA");
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (shift == "GENERALES" && estatusActual == "VAC" && RequestGrua == "SI")
        //                    {
        //                        if (message == "CHOFER ASIGNADO AL MOVIMIENTO")
        //                        {
        //                            ------------------------------------------------------------------------------
        //                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                coms.Parameters.AddWithValue("@ID", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                " message = @message WHERE Folio = @ID";
        //                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                            {
        //                                DBSPP.Open();
        //                                coms.Parameters.AddWithValue("@message", "CHOFER EN PROCESO DE ENGANCHE DE CAJA");
        //                                coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                int rowsAffected = coms.ExecuteNonQuery();
        //                                DBSPP.Close();
        //                            }
        //                            ------------------------------------------------------------------------------
        //                        }
        //                        else
        //                        {
        //                            if (message == "CHOFER EN PROCESO DE ENGANCHE DE CAJA")
        //                            {
        //                                ------------------------------------------------------------------------------
        //                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SELLO");
        //                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                                ------------------------------------------------------------------------------
        //                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                    " message = @message WHERE Folio = @ID";
        //                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                {
        //                                    DBSPP.Open();
        //                                    coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SELLO");
        //                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                    DBSPP.Close();
        //                                }
        //                                ------------------------------------------------------------------------------
        //                            }
        //                            else
        //                            {
        //                                if (message == "CHOFER EN ESPERA DE SELLO")
        //                                {
        //                                    ------------------------------------------------------------------------------
        //                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                    {
        //                                        DBSPP.Open();
        //                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                        coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE ACCESO DE GRÚA");
        //                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                        int rowsAffected = coms.ExecuteNonQuery();
        //                                        DBSPP.Close();
        //                                    }
        //                                    ------------------------------------------------------------------------------
        //                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                        " message = @message WHERE Folio = @ID";
        //                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                    {
        //                                        DBSPP.Open();
        //                                        coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE ACCESO DE GRÚA");
        //                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                        int rowsAffected = coms.ExecuteNonQuery();
        //                                        DBSPP.Close();
        //                                    }
        //                                    ------------------------------------------------------------------------------
        //                                }
        //                                else
        //                                {
        //                                    if (message == "CHOFER EN ESPERA DE ACCESO DE GRÚA")
        //                                    {
        //                                        ------------------------------------------------------------------------------
        //                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                        {
        //                                            DBSPP.Open();
        //                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                            coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE MANIOBRA DE GRUA");
        //                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                            DBSPP.Close();
        //                                        }
        //                                        ------------------------------------------------------------------------------
        //                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                            " message = @message WHERE Folio = @ID";
        //                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                        {
        //                                            DBSPP.Open();
        //                                            coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE MANIOBRA DE GRUA");
        //                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                            DBSPP.Close();
        //                                        }
        //                                        ------------------------------------------------------------------------------
        //                                    }
        //                                    else
        //                                    {
        //                                        if (message == "CHOFER EN ESPERA DE MANIOBRA DE GRUA")
        //                                        {
        //                                            ------------------------------------------------------------------------------
        //                                            string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                                " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                            using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                            {
        //                                                DBSPP.Open();
        //                                                coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                                coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                                coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE CARGADO DE GRUA");
        //                                                coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                                int rowsAffected = coms.ExecuteNonQuery();
        //                                                DBSPP.Close();
        //                                            }
        //                                            ------------------------------------------------------------------------------
        //                                            string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                                " message = @message WHERE Folio = @ID";
        //                                            using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                            {
        //                                                DBSPP.Open();
        //                                                coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE CARGADO DE GRUA");
        //                                                coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                                int rowsAffected = coms.ExecuteNonQuery();
        //                                                DBSPP.Close();
        //                                            }
        //                                            ------------------------------------------------------------------------------
        //                                        }
        //                                        else
        //                                        {
        //                                            if (message == "CHOFER EN ESPERA DE CARGADO DE GRUA")
        //                                            {
        //                                                ------------------------------------------------------------------------------
        //                                                string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                                    " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                                using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                                {
        //                                                    DBSPP.Open();
        //                                                    coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                                    coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                                    coms.Parameters.AddWithValue("@ID", "CHOFER EN ESPERA DE SALIDA DE GRUA");
        //                                                    coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                                    DBSPP.Close();
        //                                                }
        //                                                ------------------------------------------------------------------------------
        //                                                string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                                    " message = @message WHERE Folio = @ID";
        //                                                using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                                {
        //                                                    DBSPP.Open();
        //                                                    coms.Parameters.AddWithValue("@message", "CHOFER EN ESPERA DE SALIDA DE GRUA");
        //                                                    coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                                    int rowsAffected = coms.ExecuteNonQuery();
        //                                                    DBSPP.Close();
        //                                                }
        //                                                ------------------------------------------------------------------------------
        //                                            }
        //                                            else
        //                                            {
        //                                                if (message == "CHOFER EN ESPERA DE SALIDA DE GRUA")
        //                                                {
        //                                                    ------------------------------------------------------------------------------
        //                                                    string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                                        " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                                    using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                                    {
        //                                                        DBSPP.Open();
        //                                                        coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                                        coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                                        coms.Parameters.AddWithValue("@ID", "CHOFER SOLTANDO CHASIS");
        //                                                        coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                                        int rowsAffected = coms.ExecuteNonQuery();
        //                                                        DBSPP.Close();
        //                                                    }
        //                                                    ------------------------------------------------------------------------------
        //                                                    string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                                        " message = @message WHERE Folio = @ID";
        //                                                    using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                                    {
        //                                                        DBSPP.Open();
        //                                                        coms.Parameters.AddWithValue("@message", "CHOFER SOLTANDO CHASIS");
        //                                                        coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                                        int rowsAffected = coms.ExecuteNonQuery();
        //                                                        DBSPP.Close();
        //                                                    }
        //                                                    ------------------------------------------------------------------------------
        //                                                }
        //                                                else
        //                                                {
        //                                                    if (message == "CHOFER SOLTANDO CHASIS")
        //                                                    {
        //                                                        ------------------------------------------------------------------------------
        //                                                        string queryDetails = "UPDATE RADAEmpires_DZDetailsHisense SET " +
        //                                                            " End_date = @End_date, Status = @Status WHERE Process_Movement = @ID and Folio = @Folio";
        //                                                        using (SqlCommand coms = new SqlCommand(queryDetails, DBSPP))
        //                                                        {
        //                                                            DBSPP.Open();
        //                                                            coms.Parameters.AddWithValue("@End_date", usTime.ToString("HH:mm:ss"));
        //                                                            coms.Parameters.AddWithValue("@Status", "COMPLETADO");
        //                                                            coms.Parameters.AddWithValue("@ID", "CHOFER TERMINA MOVIMIENTO");
        //                                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                                            DBSPP.Close();
        //                                                        }
        //                                                        ------------------------------------------------------------------------------
        //                                                        string Updatemessages = "UPDATE RADAEmpire_BRequestContainers SET " +
        //                                                            " message = @message WHERE Folio = @ID";
        //                                                        using (SqlCommand coms = new SqlCommand(Updatemessages, DBSPP))
        //                                                        {
        //                                                            DBSPP.Open();
        //                                                            coms.Parameters.AddWithValue("@message", "CHOFER TERMINA MOVIMIENTO");
        //                                                            coms.Parameters.AddWithValue("@ID", ID.ToString());
        //                                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                                            DBSPP.Close();
        //                                                        }
        //                                                        ------------------------------------------------------------------------------
        //                                                        string Timefinished = "UPDATE RADAEmpire_CEntryContrainers SET " +
        //                                                            " Time_Finished = @Time_Finished WHERE Folio_Request = @Folio";
        //                                                        using (SqlCommand coms = new SqlCommand(Timefinished, DBSPP))
        //                                                        {
        //                                                            DBSPP.Open();
        //                                                            coms.Parameters.AddWithValue("@Time_Finished", usTime.ToString("HH:mm:ss"));
        //                                                            coms.Parameters.AddWithValue("@Folio", ID.ToString());
        //                                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                                            DBSPP.Close();
        //                                                        }

        //                                                        ------------------------------------------------------------------------------
        //                                                        string CHOFFERS = "UPDATE RADAEmpire_AChoffer SET " +
        //                                                            " Status = @Status WHERE Username = @Username";
        //                                                        using (SqlCommand coms = new SqlCommand(CHOFFERS, DBSPP))
        //                                                        {
        //                                                            DBSPP.Open();
        //                                                            coms.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
        //                                                            coms.Parameters.AddWithValue("@Username", choffer.ToString());
        //                                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                                            DBSPP.Close();
        //                                                        }

        //                                                        ------------------------------------------------------------------------------
        //                                                        string inactive = "UPDATE RADAEmpires_DZChofferMovement SET " +
        //                                                            " Active = @Active WHERE Foio = @Foio";
        //                                                        using (SqlCommand coms = new SqlCommand(inactive, DBSPP))
        //                                                        {
        //                                                            DBSPP.Open();
        //                                                            coms.Parameters.AddWithValue("@Active", false);
        //                                                            coms.Parameters.AddWithValue("@Foio", ID.ToString());
        //                                                            int rowsAffected = coms.ExecuteNonQuery();
        //                                                            DBSPP.Close();
        //                                                        }
        //                                                    }
        //                                                    else
        //                                                    {
        //                                                        if (message == "CHOFER TERMINA MOVIMIENTO")
        //                                                        {
        //                                                            return RedirectToAction("EntryContainer", "RADA");
        //                                                        }
        //                                                    }
        //                                                }
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        return RedirectToAction("EntryContainer", "RADA");
        //    }
        //}

        public ActionResult Inventory()
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
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

        //public ActionResult RemoveInventary(string ID)
        //{
        //    if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
        //    {
        //        Session["Username"] = Request.Cookies["UserCookie"].Value;
        //    }


        //    ViewBag.User = Session["Username"];

        //    if (Session.Count <= 0)
        //    {
        //        return RedirectToAction("LogIn", "Login");
        //    }
        //    else
        //    {
        //        string updateQuery = "UPDATE RADAEmpire_CInventoryControl SET Active = @Active WHERE ID = @ID";
        //        using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
        //        {
        //            DBSPP.Open();
        //            command.Parameters.AddWithValue("@Active", false);
        //            command.Parameters.AddWithValue("@ID", ID);
        //            int rowsAffected = command.ExecuteNonQuery();
        //            DBSPP.Close();
        //        }

        //        return RedirectToAction("Inventory", "RADA");
        //    }
        //}

        [HttpPost]
        public JsonResult RemoveInventary(string ID)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

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
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
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
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
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
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
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
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
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
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
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
                string formattedDate = usTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                string datenow = usTime.ToString();
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "  Select top (100) " +
                    " a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                    " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date, a.shift as Area,a.GruaRequest as Grua " +
                    " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio where a.Date = '" + datenow.ToString() + "' ORDER by a.Folio desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    string areaActual = dr["Area"].ToString();

                    if (filtroAreas.Contains(areaActual))
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
                string formattedDate = usTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                string datenow = usTime.ToString();
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "  Select top (100) " +
                    " a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                    " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date, a.shift as Area , a.GruaRequest as Grua " +
                    " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio where a.Date = '" + datenow.ToString() + "' ORDER by a.Folio desc";
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