using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RADALogisticsWEB.Models;
using System.Globalization;

namespace RADALogisticsWEB.Controllers
{
    public class HomeController : Controller
    {    //connection SQL server (database)
        List<MovimientosEliminados> GetDelleted = new List<MovimientosEliminados>();
        List<MovimientosEliminados> GetDelletedquery = new List<MovimientosEliminados>();
        SqlConnection DBSPP = new SqlConnection("Data Source=RADAEmpire.mssql.somee.com ;Initial Catalog=RADAEmpire ;User ID=RooRada; password=rada1311");
        SqlCommand con = new SqlCommand();
        SqlDataReader dr;
        public string Username { get; set; }

        public ActionResult PingSession()
        {
            var user = Session["Username"]; // Solo accede a la sesión para que no expire
            return new HttpStatusCodeResult(200); // OK
        }
        public static string ConvertirFechaAlemanaAUsa(string fechaHora)
        {
            try
            {
                // Definir la zona horaria de Alemania y EE.UU. (Ejemplo: Eastern Time)
                TimeZoneInfo zonaAlemania = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
                TimeZoneInfo zonaUSA = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"); // Cambia según la zona de EE.UU.

                // Convertir la fecha de string a DateTime en formato alemán
                DateTime fechaUtc = DateTime.ParseExact(fechaHora, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                // Convertir la fecha a UTC (desde Alemania)
                DateTime fechaEnUtc = TimeZoneInfo.ConvertTimeToUtc(fechaUtc, zonaAlemania);

                // Convertir la fecha desde UTC a la zona horaria de EE.UU.
                DateTime fechaUsa = TimeZoneInfo.ConvertTimeFromUtc(fechaEnUtc, zonaUSA);

                // Devolver la fecha en formato USA
                return fechaUsa.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                return "Error: " + ex.Message;
            }
        }

        public ActionResult AdministratorHome()
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
                string username = Session["Username"].ToString();
                string TypeLog = null;
                SqlCommand log = new SqlCommand("Select Type_user from RADAEmpire_AUsers where Active = '1' and Username = '" + username + "'", DBSPP);
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

                if (TypeLog == "ADMINISTRATOR")
                {
                    return View();
                }
                else
                {
                    if (TypeLog == "HISENSE")
                    {
                        return RedirectToAction("HisenseHome", "Home");
                    }
                    else
                    {
                        return RedirectToAction("RadaHome", "Home");
                    }
                }
            }
        }

        public ActionResult RadaHome()
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
                string username = Session["Username"].ToString();
                string TypeLog = null;
                SqlCommand log = new SqlCommand("Select Type_user from RADAEmpire_AUsers where Active = '1' and Username = '" + username + "'", DBSPP);
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

                if (TypeLog == "ADMINISTRATOR")
                {
                    return RedirectToAction("AdministratorHome", "Home");
                }
                else
                {
                    if (TypeLog == "HISENSE")
                    {
                        return RedirectToAction("HisenseHome", "Home");
                    }
                    else
                    {
                        return View();
                    }
                }
            }
        }

        public ActionResult HisenseHome()
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
                string username = Session["Username"].ToString();
                string TypeLog = null;
                SqlCommand log = new SqlCommand("Select Type_user from RADAEmpire_AUsers where Active = '1' and Username = '" + username + "'", DBSPP);
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

                if (TypeLog == "ADMINISTRATOR")
                {
                    return RedirectToAction("AdministratorHome", "Home");
                }
                else
                {
                    if (TypeLog == "HISENSE")
                    {
                        return View();
                    }
                    else
                    {
                        return RedirectToAction("RadaHome", "Home");
                    }
                }
            }
        }

        public ActionResult Restore(string ID)
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
                string Folio_Request = null;
                //create generate randoms int value
                SqlCommand conse = new SqlCommand("Select Folio_Request from RADAEmpire_CEntryContrainers where Active = '1' and Cancel = '1' and Folio_Request = '" + ID.ToString() + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drconse = conse.ExecuteReader();
                if (drconse.HasRows)
                {
                    while (drconse.Read())
                    {
                        Folio_Request = drconse["Folio_Request"].ToString();
                    }
                }
                DBSPP.Close();

                //query message ------------------------------------------------------------------------------------------------------------------
                string updateQuery = "UPDATE RADAEmpire_CEntryContrainers SET Cancel = @Cancel WHERE Folio_Request = @ID";
                using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                {
                    DBSPP.Open();
                    command.Parameters.AddWithValue("@Cancel", false);
                    command.Parameters.AddWithValue("@ID", Folio_Request.ToString());
                    int rowsAffected = command.ExecuteNonQuery();
                    DBSPP.Close();
                }

                //query message ------------------------------------------------------------------------------------------------------------------
                string updateQuery2 = "UPDATE RADAEmpires_DRemoves SET Active = @Active WHERE Folio = @ID";
                using (SqlCommand command2 = new SqlCommand(updateQuery2, DBSPP))
                {
                    DBSPP.Open();
                    command2.Parameters.AddWithValue("@Active", false);
                    command2.Parameters.AddWithValue("@ID", Folio_Request.ToString());
                    int rowsAffected = command2.ExecuteNonQuery();
                    DBSPP.Close();
                }

                //query message ------------------------------------------------------------------------------------------------------------------
                string updateQuery12 = "UPDATE RADAEmpire_BRequestContainers SET message = @message WHERE Folio = @ID";
                using (SqlCommand command2 = new SqlCommand(updateQuery12, DBSPP))
                {
                    DBSPP.Open();
                    command2.Parameters.AddWithValue("@message", "");
                    command2.Parameters.AddWithValue("@ID", Folio_Request.ToString());
                    int rowsAffected = command2.ExecuteNonQuery();
                    DBSPP.Close();
                }

                return RedirectToAction("Restore", "Home");
            }
        }

        public ActionResult Cancellation()
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
                GetCancelled();
                ViewBag.Records = GetDelleted;
                ViewBag.Count = GetDelleted.Count.ToString();
                return View();
            }
        }

        [HttpPost]
        public ActionResult Cancellation(string Timeend, string TimeStart)
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
                        sqlTimeStart = " and CAST(Datetime AS DATE) BETWEEN '" + TimeStart + "'";
                    }

                    if (Timeend == "")
                    {
                        sqlTimeend = "";
                    }
                    else
                    {
                        sqlTimeend = " and '" + Timeend + "' order by ID desc";
                    }
                }
                else
                {
                    count = count + 2;
                }

                if (count >= 2)
                {
                    GetCancelled();
                    ViewBag.Records = GetDelleted;
                    ViewBag.Count = GetDelleted.Count.ToString();
                    return View();
                }
                else
                {
                    DBSPP.Open();
                    con.Connection = DBSPP;
                    con.CommandText = "Select top (1000) * from RADAEmpires_DRemoves where Active = '1'" + sqlTimeStart + sqlTimeend + "";
                    dr = con.ExecuteReader();
                    while (dr.Read())
                    {
                        GetDelletedquery.Add(new MovimientosEliminados()
                        {
                            Folio = (dr["Folio"].ToString()),
                            Reason = (dr["Reason"].ToString()),
                            Company = (dr["Company"].ToString()),
                            Datetime = Convert.ToDateTime(dr["Datetime"].ToString()),
                        });
                    }
                    DBSPP.Close();

                    ViewBag.Records = GetDelletedquery;
                    ViewBag.Count = GetDelletedquery.Count.ToString();
                    return View();
                }
            }
        }

        private void GetCancelled()
        {
            if (GetDelleted.Count > 0)
            {
                GetDelleted.Clear();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select top (1000) * from RADAEmpires_DRemoves where Active = '1' order by ID desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetDelleted.Add(new MovimientosEliminados()
                    {
                        Folio = (dr["Folio"].ToString()),
                        Reason = (dr["Reason"].ToString()),
                        Company = (dr["Company"].ToString()),
                        Datetime = Convert.ToDateTime(dr["Datetime"].ToString()),
                    });
                }
                DBSPP.Close();
            }
        }

    }
}