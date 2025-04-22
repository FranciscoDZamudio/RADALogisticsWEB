using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using RADALogisticsWEB.Models;

namespace RADALogisticsWEB.Controllers
{
    public class UsersController : Controller
    {
        List<Usuarios> GetUsers = new List<Usuarios>();
        List<Areas> GetArea = new List<Areas>();
        List<AsignacionDeAreas> Asignacion = new List<AsignacionDeAreas>();
        List<AsignacionDeAreas> Asignacions = new List<AsignacionDeAreas>();
        List<Choferes> GetChofers = new List<Choferes>();
        //connection SQL server (database)
        SqlConnection DBSPP = new SqlConnection("Data Source=RADAEmpire.mssql.somee.com ;Initial Catalog=RADAEmpire ;User ID=RooRada; password=rada1311");
        SqlCommand con = new SqlCommand();
        SqlDataReader dr;

        public string message { get; set; }

        public ActionResult AssignmentDelete(string name, string area, string status)
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
                string id = null;
                if (status == "RADA")
                {
                    SqlCommand log = new SqlCommand("Select * from RADAEmpire_AUsers where Username = '" + name + "' and Active = '1'", DBSPP);
                    DBSPP.Open();
                    SqlDataReader drlog = log.ExecuteReader();
                    if (drlog.HasRows)
                    {
                        while (drlog.Read())
                        {
                            id = drlog["ID"].ToString();
                        }
                    }
                    DBSPP.Close();
                }
                else
                {
                    SqlCommand log = new SqlCommand("Select * from RADAEmpire_AUsers where Username = '" + name + "' and Active = '1'", DBSPP);
                    DBSPP.Open();
                    SqlDataReader drlog = log.ExecuteReader();
                    if (drlog.HasRows)
                    {
                        while (drlog.Read())
                        {
                            id = drlog["ID"].ToString();
                        }
                    }
                    DBSPP.Close();
                }

                string updateQuery = "UPDATE RADAEmpire_ARoles SET Active = @Active WHERE Areas = @Areas and Username = @Username";
                using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                {
                    DBSPP.Open();
                    command.Parameters.AddWithValue("@Active", false);
                    command.Parameters.AddWithValue("@Areas", area);
                    command.Parameters.AddWithValue("@Username", name);
                    int rowsAffected = command.ExecuteNonQuery();
                    DBSPP.Close();
                }
                if (status == "RADA")
                {
                    return RedirectToAction("Assignments", "Users", new { id = id });
                }
                else
                {
                    return RedirectToAction("Assignment", "Users", new { id = id });
                }
            }
        }

        public ActionResult Assignments(string id, string star, string ETP)
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
                if (ETP == "RADA")
                {
                    string name = null;
                    SqlCommand log = new SqlCommand("Select * from RADAEmpire_AUsers where ID = '" + id + "' and Active = '1'", DBSPP);
                    DBSPP.Open();
                    SqlDataReader drlog = log.ExecuteReader();
                    if (drlog.HasRows)
                    {
                        while (drlog.Read())
                        {
                            name = drlog["Username"].ToString();
                            ViewBag.Username = drlog["Username"].ToString();
                        }
                    }

                    DBSPP.Close();
                    GetAreas();
                    ViewBag.Records = GetArea;
                    getAsignacion(name);
                    ViewBag.Count = Asignacion.Count.ToString();
                    ViewBag.Recordstable = Asignacion;

                    ViewBag.id = id;
                    return View();
                }
                else
                {
                    string name = null;
                    SqlCommand log = new SqlCommand("Select * from RADAEmpire_AUsers where ID = '" + id + "' and Active = '1'", DBSPP);
                    DBSPP.Open();
                    SqlDataReader drlog = log.ExecuteReader();
                    if (drlog.HasRows)
                    {
                        while (drlog.Read())
                        {
                            name = drlog["Username"].ToString();
                            ViewBag.Username = drlog["Username"].ToString();
                        }
                    }

                    DBSPP.Close();
                    GetAreas();
                    ViewBag.Records = GetArea;
                    getAsignacion(name);
                    ViewBag.Count = Asignacion.Count.ToString();
                    ViewBag.Recordstable = Asignacion;

                    ViewBag.id = id;
                    return View();
                }
            }
        }

        public ActionResult Assignment(string id, string star)
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
                string name = null;
                SqlCommand log = new SqlCommand("Select * from RADAEmpire_AChoffer where ID = '" + id + "' and Active = '1'", DBSPP);
                DBSPP.Open();
                SqlDataReader drlog = log.ExecuteReader();
                if (drlog.HasRows)
                {
                    while (drlog.Read())
                    {
                        name = drlog["Username"].ToString();
                        ViewBag.Username = drlog["Username"].ToString();
                    }
                }

                DBSPP.Close();
                GetAreas();
                ViewBag.Records = GetArea;
                getAsignacions(name);
                ViewBag.Count = Asignacions.Count.ToString();
                ViewBag.Recordstable = Asignacions;
                ViewBag.id = id;
                return View();
            }
        }

        public ActionResult AsignacionChofees(string id, string Area, string username, string user, string status)
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
                string vl = null;
                SqlCommand log = new SqlCommand("Select * from RADAEmpire_ARolesChoffer where Active = '1' and Areas = '" + Area + "' and Username = '" + username + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drlog = log.ExecuteReader();
                if (drlog.HasRows)
                {
                    while (drlog.Read())
                    {
                        vl = drlog[0].ToString();
                    }
                }
                else
                {
                    vl = null;
                }
                DBSPP.Close();

                if (vl == null)
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
                    SqlCommand PalletControl = new SqlCommand("insert into RADAEmpire_ARolesChoffer" +
                        "(Who_Create, Username, Enterprise, Areas, Date, Datetime, Active) values " +
                        "(@Who_Create, @Username, @Enterprise, @Areas, @Date, @Datetime, @Active) ", DBSPP);
                    //--------------------------------------------------------------------------------------------------------------------------------
                    PalletControl.Parameters.AddWithValue("@Who_Create", user.ToString());
                    PalletControl.Parameters.AddWithValue("@Username", username.ToString());
                    PalletControl.Parameters.AddWithValue("@Enterprise", status.ToString());
                    PalletControl.Parameters.AddWithValue("@Areas", Area.ToString());
                    PalletControl.Parameters.AddWithValue("@Date", usTime.ToString());
                    PalletControl.Parameters.AddWithValue("@Datetime", usTime.ToString());
                    PalletControl.Parameters.AddWithValue("@Active", true);
                    PalletControl.ExecuteNonQuery();
                    DBSPP.Close();
                    //--------------------------------------------------------------------------------------------------------------------------------

                    return RedirectToAction("Assignment", "Users", new { id = id });
                }
                else
                {
                    return RedirectToAction("Assignment", "Users", new { id = id });
                }
            }
        }

        public ActionResult Removess(string name, string area)
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
                string id = null;
                SqlCommand logs = new SqlCommand("Select * from RADAEmpire_AChoffer where Username = '" + name + "' and Active = '1'", DBSPP);
                DBSPP.Open();
                SqlDataReader drlogs = logs.ExecuteReader();
                if (drlogs.HasRows)
                {
                    while (drlogs.Read())
                    {
                        id = drlogs["ID"].ToString();
                    }
                }
                DBSPP.Close();

                string updateQuery = "UPDATE RADAEmpire_ARolesChoffer SET Active = @Active WHERE Areas = @Areas and Username = @Username";
                using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                {
                    DBSPP.Open();
                    command.Parameters.AddWithValue("@Active", false);
                    command.Parameters.AddWithValue("@Areas", area);
                    command.Parameters.AddWithValue("@Username", name);
                    int rowsAffected = command.ExecuteNonQuery();
                    DBSPP.Close();
                }

                return RedirectToAction("Assignment", "Users", new { id = id });
            }
        }

        public ActionResult AsignacionHisense(string id, string Area, string username, string user, string status)
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
                string vl = null, enterpris = null;

                SqlCommand empresa = new SqlCommand("Select * from RADAEmpire_AUsers where Active = '1' and ID = '" + id + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drempresa = empresa.ExecuteReader();
                if (drempresa.HasRows)
                {
                    while (drempresa.Read())
                    {
                        enterpris = drempresa["Type_user"].ToString();
                    }
                }
                DBSPP.Close();

                SqlCommand log = new SqlCommand("Select * from RADAEmpire_ARoles where Active = '1' and Areas = '" + Area + "' and Username = '" + username + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drlog = log.ExecuteReader();
                if (drlog.HasRows)
                {
                    while (drlog.Read())
                    {
                        vl = drlog[0].ToString();
                    }
                }
                else
                {
                    vl = null;
                }
                DBSPP.Close();

                if (vl == null)
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
                    SqlCommand PalletControl = new SqlCommand("insert into RADAEmpire_ARoles" +
                        "(Username, Enterprise, Areas, Date, Datetime, Active, Who_create) values " +
                        "(@Username, @Enterprise, @Areas, @Date, @Datetime, @Active, @Who_create) ", DBSPP);
                    //--------------------------------------------------------------------------------------------------------------------------------
                    PalletControl.Parameters.AddWithValue("@Username", username.ToString());
                    PalletControl.Parameters.AddWithValue("@Enterprise", enterpris.ToString());
                    PalletControl.Parameters.AddWithValue("@Areas", Area.ToString());
                    PalletControl.Parameters.AddWithValue("@Active", true);
                    PalletControl.Parameters.AddWithValue("@Who_create", user.ToString());
                    PalletControl.Parameters.AddWithValue("@Datetime", usTime.ToString());
                    PalletControl.Parameters.AddWithValue("@Date", usTime.ToString());
                    PalletControl.ExecuteNonQuery();
                    DBSPP.Close();
                    //--------------------------------------------------------------------------------------------------------------------------------
                    if (status == "HISENSE")
                    {
                        return RedirectToAction("Assignments", "Users", new { id = id });
                    }
                    else
                    {
                        return RedirectToAction("Assignments", "Users", new { id = id });
                    }
                }
                else
                {
                    if (status == "HISENSE")
                    {
                        return RedirectToAction("Assignments", "Users", new { id = id });
                    }
                    else
                    {
                        return RedirectToAction("Assignments", "Users", new { id = id });
                    }
                }
            }
        }

        public ActionResult UpdateProcess(string ID)
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
                string Username = null, EmployeeNumber = null, loginSession = null, passwords = null, typeUsers = null, email = null;
                SqlCommand log = new SqlCommand("Select * from RADAEmpire_AUsers where Active = '1' and ID = '" + ID + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drlog = log.ExecuteReader();
                if (drlog.HasRows)
                {
                    while (drlog.Read())
                    {
                        Username = drlog["Username"].ToString();
                        EmployeeNumber = drlog["Employee_Number"].ToString();
                        loginSession = drlog["Login_session"].ToString();
                        passwords = drlog["Password"].ToString();
                        typeUsers = drlog["Type_user"].ToString();
                        email = drlog["Email"].ToString();

                    }
                }
                else
                {
                    Username = null;
                    EmployeeNumber = null;
                    loginSession = null;
                    passwords = null;
                    typeUsers = null;
                    email = null;
                }
                DBSPP.Close();

                ViewBag.User = Username;
                ViewBag.EmployeeNumber = EmployeeNumber;
                ViewBag.LoginSession = loginSession;
                ViewBag.Passwords = passwords;
                ViewBag.TypeUsers = typeUsers;
                ViewBag.Email = email;
                ViewBag.ID = ID;
                return View();
            }
        }

        public ActionResult Update(string Username, string Credentials, string Email, string Log, string Pass, string TypeUsers, string id)
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
                string updateQuery = "UPDATE RADAEmpire_AUsers SET Username = @Username, " +
                " Employee_Number = @Employee_Number, Login_session = @Login_session, " +
                " Password = @Password, Email = @Email, Type_user = @Type_user WHERE ID = @ID";

                using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                {
                    DBSPP.Open();
                    command.Parameters.AddWithValue("@Username", Username.ToUpper());
                    command.Parameters.AddWithValue("@Employee_Number", Credentials.ToUpper());
                    command.Parameters.AddWithValue("@Login_session", Log.ToUpper());
                    command.Parameters.AddWithValue("@Password", Pass.ToString());
                    command.Parameters.AddWithValue("@Email", Email.ToString());
                    command.Parameters.AddWithValue("@Type_user", TypeUsers.ToUpper());
                    command.Parameters.AddWithValue("@ID", id);
                    int rowsAffected = command.ExecuteNonQuery();
                    DBSPP.Close();
                }

                return RedirectToAction("NewUser", "Users");
            }
        }

        public ActionResult NewUser()
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
                GetUsuarios();
                ViewBag.Records = GetUsers;
                ViewBag.Count = GetUsers.Count.ToString();
                ViewBag.message = message;
                ViewBag.Create = Session["Username"].ToString();
                return View();
            }
        }

        public ActionResult Home()
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
                return View();
            }
        }

        public ActionResult NewChoffer()
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
                Choffer();
                ViewBag.Count = GetChofers.Count.ToString();
                ViewBag.RecordChoffers = GetChofers;
                GetAreas();
                ViewBag.Records = GetArea;
                return View();
            }
        }

        public ActionResult ProcessChoffer(string Username, string FastCard, string Area, string Shift, string User)
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
                SqlCommand log = new SqlCommand("Select Username " +
                    " from RADAEmpire_AChoffer where Active = '1' and Username = '" + Username + "' and Fastcard = '" + FastCard + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drlog = log.ExecuteReader();
                if (drlog.HasRows)
                {
                    while (drlog.Read())
                    {
                        validation = drlog[0].ToString();
                    }
                }
                else
                {
                    validation = null;
                }
                DBSPP.Close();

                if (validation == null)
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
                    SqlCommand PalletControl = new SqlCommand("insert into RADAEmpire_AChoffer" +
                        "(Who_create, Username, Fastcard, Area, Active,Shift,Date,Datetime, Status) values " +
                        "(@Who_create, @Username, @Fastcard, @Area, @Active,@Shift,@Date,@Datetime, @Status) ", DBSPP);
                    //--------------------------------------------------------------------------------------------
                    PalletControl.Parameters.AddWithValue("@Who_create", User.ToUpper());
                    PalletControl.Parameters.AddWithValue("@Username", Username.ToUpper());
                    PalletControl.Parameters.AddWithValue("@Fastcard",FastCard.ToUpper());
                    PalletControl.Parameters.AddWithValue("@Area", "Assignments");
                    PalletControl.Parameters.AddWithValue("@Shift", Shift.ToString());
                    PalletControl.Parameters.AddWithValue("@Active", true);
                    PalletControl.Parameters.AddWithValue("@Date", usTime.ToString());
                    PalletControl.Parameters.AddWithValue("@Datetime", usTime.ToString());
                    PalletControl.Parameters.AddWithValue("@Status", "SIN MOVIMIENTO");
                    PalletControl.ExecuteNonQuery();
                    DBSPP.Close();
                    //--------------------------------------------------------------------------------------------

                    return RedirectToAction("NewChoffer", "Users");
                }
                else
                {
                    return RedirectToAction("NewChoffer", "Users");
                }
            }
        }

        [HttpPost]
        public JsonResult RemoverChoffer(string id)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            if (Session.Count <= 0)
            {
                return Json(new { success = false, redirectUrl = Url.Action("LogIn", "Login") });
            }

            string updateQuery = "UPDATE RADAEmpire_AChoffer SET Active = @Active WHERE ID = @ID";
            using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
            {
                DBSPP.Open();
                command.Parameters.AddWithValue("@Active", false);
                command.Parameters.AddWithValue("@ID", id);
                int rowsAffected = command.ExecuteNonQuery();
                DBSPP.Close();
            }

            return Json(new { success = true });
        }

        //public ActionResult RemoverChoffer(string id)
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
        //        string updateQuery = "UPDATE RADAEmpire_AChoffer SET Active = @Active WHERE ID = @ID";
        //        using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
        //        {
        //            DBSPP.Open();
        //            command.Parameters.AddWithValue("@Active", false);
        //            command.Parameters.AddWithValue("@ID", id);
        //            int rowsAffected = command.ExecuteNonQuery();
        //            DBSPP.Close();
        //        }

        //        return RedirectToAction("NewChoffer", "Users");
        //    }
        //}

        public ActionResult UpdateDriver(string id)
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
                ViewBag.id = id;
                string Username = null, Fastcard = null, Area = null, shift = null, datetime = null;
                SqlCommand log = new SqlCommand("Select * from RADAEmpire_AChoffer where Active = '1' and ID = '" + id + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drlog = log.ExecuteReader();
                if (drlog.HasRows)
                {
                    while (drlog.Read())
                    {
                        Username = drlog["Username"].ToString();
                        Fastcard = drlog["Fastcard"].ToString();
                        Area = drlog["Area"].ToString();
                        shift = drlog["Shift"].ToString();
                        datetime = drlog["Datetime"].ToString();
                    }
                }
                DBSPP.Close();

                ViewBag.User = Username;
                ViewBag.Fastcard = Fastcard;
                ViewBag.Area = Area;
                ViewBag.Shift = shift;
                ViewBag.datetime = datetime;
                GetAreas();
                ViewBag.Records = GetArea;

                return View();
            }
        }

        public ActionResult updatenewChoffer(string Username, string Fastcard, string Shift, string id)
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
                string updateQuery = "UPDATE RADAEmpire_AChoffer SET Username = @Username, " +
                " Fastcard = @Fastcard, Shift = @Shift  WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                {
                    DBSPP.Open();
                    command.Parameters.AddWithValue("@Username", Username.ToUpper());
                    command.Parameters.AddWithValue("@Fastcard", Fastcard.ToUpper());
                    command.Parameters.AddWithValue("@Shift", Shift.ToUpper());
                    command.Parameters.AddWithValue("@ID", id);
                    int rowsAffected = command.ExecuteNonQuery();
                    DBSPP.Close();
                }

                return RedirectToAction("NewChoffer", "Users");
            }
        }

        public ActionResult Area()
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
                GetAreas();
                ViewBag.Count = GetArea.Count.ToString();
                ViewBag.Records = GetArea;
                return View();
            }
        }

        public ActionResult RemoveArea(string id)
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
                string updateQuery = "UPDATE RADAEmpire_AAreas SET Active = @Active WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                {
                    DBSPP.Open();
                    command.Parameters.AddWithValue("@Active", false);
                    command.Parameters.AddWithValue("@ID", id);
                    int rowsAffected = command.ExecuteNonQuery();
                    DBSPP.Close();
                }

                return RedirectToAction("Area", "Users");
            }
        }

        public ActionResult Insert(string Name, string User)
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
                SqlCommand log = new SqlCommand("Select Name from RADAEmpire_AAreas where Active = '1' and Name = '" + Name + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drlog = log.ExecuteReader();
                if (drlog.HasRows)
                {
                    while (drlog.Read())
                    {
                        validation = drlog[0].ToString();
                    }
                }
                else
                {
                    validation = null;
                }
                DBSPP.Close();

                if (validation == null)
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
                    SqlCommand PalletControl = new SqlCommand("insert into RADAEmpire_AAreas" +
                        "(Who_create, Name, Datetime, Date, Active) values " +
                        "(@Who_create, @Name,@Datetime, @Date, @Active) ", DBSPP);
                    //--------------------------------------------------------------------------------------------
                    PalletControl.Parameters.AddWithValue("@Who_create", User.ToString());
                    PalletControl.Parameters.AddWithValue("@Name", Name.ToString());
                    PalletControl.Parameters.AddWithValue("@Date", usTime);
                    PalletControl.Parameters.AddWithValue("@Datetime", usTime);
                    PalletControl.Parameters.AddWithValue("@Active", true);
                    PalletControl.ExecuteNonQuery();
                    DBSPP.Close();
                    //--------------------------------------------------------------------------------------------
                    return RedirectToAction("Area", "Users");
                }
                else
                {
                    return RedirectToAction("Area", "Users");
                }
            }
        }

        public ActionResult GetUser(string Username, string Credentials, string Log, string Pass, string TypeUsers, string Email, string UserCreate)
        {
            string Validation = null;
            SqlCommand log = new SqlCommand("Select Username from RADAEmpire_AUsers where Active = '1' and Username = '" + Username + "' and Login_session = '" + Log + "'", DBSPP);
            DBSPP.Open();
            SqlDataReader drlog = log.ExecuteReader();
            if (drlog.HasRows)
            {
                while (drlog.Read())
                {
                    Validation = drlog[0].ToString();
                }
            }
            else
            {
                Validation = null;
            }
            DBSPP.Close();

            if (Validation == null)
            {
                // Obtener la fecha y hora actual en Alemania (zona horaria UTC+1 o UTC+2 dependiendo del horario de verano)
                DateTime germanTime = DateTime.UtcNow.AddHours(1);  // Alemania es UTC+1

                // Convertir la hora alemana a la hora en una zona horaria específica de EE. UU. (por ejemplo, Nueva York, UTC-5)
                TimeZoneInfo usEasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                DateTime usTime = TimeZoneInfo.ConvertTime(germanTime, usEasternTimeZone);

                // Formatear la fecha para que sea adecuada para la base de datos
                string formattedDate = usTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                //Guardar informacion a la base de datos del proyecto
                DBSPP.Open();
                SqlCommand PalletControl = new SqlCommand("insert into RADAEmpire_AUsers" +
                    "(Username, Employee_Number, Login_session, Password, Type_user, Email, DateAdded, DateRequest, Active,Createby) values " +
                    "(@Username, @Employee_Number, @Login_session, @Password, @Type_user, @Email, @DateAdded, @DateRequest, @Active,@Createby) ", DBSPP);
                //--------------------------------------------------------------------------------------------------------------------------------
                PalletControl.Parameters.AddWithValue("@Username", Username.ToUpper());
                PalletControl.Parameters.AddWithValue("@Employee_Number", Credentials.ToUpper());
                PalletControl.Parameters.AddWithValue("@Login_session", Log.ToUpper());
                PalletControl.Parameters.AddWithValue("@Password", Pass.ToString());
                PalletControl.Parameters.AddWithValue("@Type_user", TypeUsers.ToUpper());
                PalletControl.Parameters.AddWithValue("@Email", Email.ToString());
                PalletControl.Parameters.AddWithValue("@DateAdded", usTime);
                PalletControl.Parameters.AddWithValue("@DateRequest", usTime);
                //PalletControl.Parameters.AddWithValue("@DateAdded", DateTime.Now.ToString());
                //PalletControl.Parameters.AddWithValue("@DateRequest", DateTime.Now.ToString());
                PalletControl.Parameters.AddWithValue("@Active", true);
                PalletControl.Parameters.AddWithValue("@Createby", UserCreate);
                PalletControl.ExecuteNonQuery();
                DBSPP.Close();
                //--------------------------------------------------------------------------------------------------------------------------------
                ViewBag.message = "";
                message = "";
                GetUsuarios();
                ViewBag.Records = GetUsers;
                ViewBag.Count = GetUsers.Count.ToString();
                return RedirectToAction("NewUser", "Users");
            }
            else
            {
                ViewBag.message = "Message: The Employee you want to enter is already in the system";
                message = "Message: The Employee you want to enter is already in the system";
                GetUsuarios();
                ViewBag.Records = GetUsers;
                ViewBag.Count = GetUsers.Count.ToString();
                return RedirectToAction("NewUser", "Users");
            }
        }

        //public ActionResult RemoveUser(string ID)
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
        //        string updateQuery = "UPDATE RADAEmpire_AUsers SET Active = @Active WHERE ID = @ID";
        //        using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
        //        {
        //            DBSPP.Open();
        //            command.Parameters.AddWithValue("@Active", false);
        //            command.Parameters.AddWithValue("@ID", ID);
        //            int rowsAffected = command.ExecuteNonQuery();
        //            DBSPP.Close();
        //        }

        //        ViewBag.message = "";
        //        message = "";
        //        GetUsuarios();
        //        ViewBag.Records = GetUsers;
        //        ViewBag.Count = GetUsers.Count.ToString();
        //        return RedirectToAction("NewUser", "Users");
        //    }
        //}

        [HttpPost]
        public JsonResult RemoveUser(int ID)
        {
            if (Session["Username"] == null && Request.Cookies["UserCookie"] != null)
            {
                Session["Username"] = Request.Cookies["UserCookie"].Value;
            }

            if (Session.Count <= 0)
            {
                return Json(new { success = false, redirectUrl = Url.Action("LogIn", "Login") });
            }

            string updateQuery = "UPDATE RADAEmpire_AUsers SET Active = @Active WHERE ID = @ID";
            using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
            {
                DBSPP.Open();
                command.Parameters.AddWithValue("@Active", false);
                command.Parameters.AddWithValue("@ID", ID);
                int rowsAffected = command.ExecuteNonQuery();
                DBSPP.Close();
            }

            return Json(new { success = true });
        }

        private void getAsignacion(string name)
        {
            if (Asignacion.Count > 0)
            {
                Asignacion.Clear();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select * from RADAEmpire_ARoles where Active = '1' order by ID desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {// Filtrar usando la variable filtroArea
                    if (dr["Username"].ToString() == name)
                    {
                        Asignacion.Add(new AsignacionDeAreas()
                        {
                            ID = int.Parse(dr["ID"].ToString()),
                            Username = (dr["Username"].ToString()),
                            Who_create = (dr["Who_create"].ToString()),
                            Enterprise = (dr["Enterprise"].ToString()),
                            Areas = (dr["Areas"].ToString()),
                            Datetime = (dr["Datetime"].ToString()),
                        });
                    }
                }
                DBSPP.Close();
            }
        }

        private void getAsignacions(string name)
        {
            if (Asignacions.Count > 0)
            {
                Asignacions.Clear();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select * from RADAEmpire_ARolesChoffer where Active = '1' order by ID desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {// Filtrar usando la variable filtroArea
                    if (dr["Username"].ToString() == name)
                    {
                        Asignacions.Add(new AsignacionDeAreas()
                        {
                            ID = int.Parse(dr["ID"].ToString()),
                            Username = (dr["Username"].ToString()),
                            Who_create = (dr["Who_Create"].ToString()),
                            Enterprise = (dr["Enterprise"].ToString()),
                            Areas = (dr["Areas"].ToString()),
                            Datetime = (dr["Datetime"].ToString()),
                        });
                    }
                }
                DBSPP.Close();
            }
        }

        private void GetUsuarios()
        {
            if (GetUsers.Count > 0)
            {
                GetUsers.Clear();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select top (100) * from RADAEmpire_AUsers where Active = '1' order by ID desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetUsers.Add(new Usuarios()
                    {
                        ID = int.Parse(dr["ID"].ToString()),
                        Username = (dr["Username"].ToString()),
                        Employee_Number = (dr["Employee_Number"].ToString()),
                        Login_session = (dr["Login_session"].ToString()),
                        Password = (dr["Password"].ToString()),
                        Type_user = (dr["Type_user"].ToString()),
                        Email = (dr["Email"].ToString()),
                        DateAdded = Convert.ToDateTime(dr["DateRequest"]),
                    });
                }
                DBSPP.Close();
            }
        }

        private void GetAreas()
        {
            if (GetArea.Count > 0)
            {
                GetArea.Clear();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select top (100) * from RADAEmpire_AAreas where Active = '1' order by ID desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetArea.Add(new Areas()
                    {
                        id = (dr["ID"].ToString()),
                        Who_create = (dr["Who_create"].ToString()),
                        Name = (dr["Name"].ToString()),
                        Datetime = (dr["Datetime"].ToString()),
                    });
                }
                DBSPP.Close();
            }
        }

        private void Choffer()
        {
            if (GetChofers.Count > 0)
            {
                GetChofers.Clear();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select top (100) * from RADAEmpire_AChoffer where Active = '1' order by ID desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetChofers.Add(new Choferes()
                    {
                        ID = (dr["ID"].ToString()),
                        Who_create = (dr["Who_create"].ToString()),
                        Username = (dr["Username"].ToString()),
                        Fastcard = (dr["Fastcard"].ToString()),
                        Area = (dr["Area"].ToString()),
                        Shift = (dr["Shift"].ToString()),
                        Datetime = (dr["Datetime"].ToString()),
                        Date = (dr["Date"].ToString()),
                    });
                }
                DBSPP.Close();
            }
        }
    }
    
}