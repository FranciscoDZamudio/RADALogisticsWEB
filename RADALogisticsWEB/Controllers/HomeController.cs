using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RADALogisticsWEB.Models;

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

        public ActionResult AdministratorHome()
        {
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

        public ActionResult Cancellation()
        {
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