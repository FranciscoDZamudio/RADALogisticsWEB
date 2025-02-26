using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RADALogisticsWEB.Controllers
{
    public class HomeController : Controller
    {    //connection SQL server (database)
        SqlConnection DBSPP = new SqlConnection("Data Source=RADAEmpire.mssql.somee.com ;Initial Catalog=RADAEmpire ;User ID=RooRada; password=rada1311");
        SqlCommand con = new SqlCommand();
        SqlDataReader dr;
        public string Username { get; set; }

        public ActionResult AdministratorHome()
        {
            ViewBag.User = Session["Username"];
            Username = Session["Username"].ToString();

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
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

                if (TypeLog == "ADMINISTRATOR")
                {
                    //Go to next page (Menu Scroll)
                    Session["Username"] = Username;
                    return View();
                }
                else
                {
                    if (TypeLog == "HISENSE")
                    {
                        Session["Username"] = Username;
                        return RedirectToAction("HisenseHome", "Home");
                    }
                    else
                    {
                        //View pages RADA
                        //Go to next page (Menu Scroll)
                        Session["Username"] = Username;
                        return RedirectToAction("RadaHome", "Home");
                    }
                }
            }
        }

        public ActionResult RadaHome()
        {
            ViewBag.User = Session["Username"];
            Username = Session["Username"].ToString();

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
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

                if (TypeLog == "ADMINISTRATOR")
                {
                    //Go to next page (Menu Scroll)
                    Session["Username"] = Username;
                    return RedirectToAction("AdministratorHome", "Home");
                }
                else
                {
                    if (TypeLog == "HISENSE")
                    {
                        //Go to next page (Menu Scroll)
                        Session["Username"] = Username;
                        return RedirectToAction("HisenseHome", "Home");
                    }
                    else
                    {
                        //View pages RADA
                        //Go to next page (Menu Scroll)
                        Session["Username"] = Username;
                        return View();
                    }
                }
            }
        }

        public ActionResult HisenseHome()
        {
            ViewBag.User = Session["Username"];
            Username = Session["Username"].ToString();

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
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

                if (TypeLog == "ADMINISTRATOR")
                {
                    //Go to next page (Menu Scroll)
                    Session["Username"] = Username;
                    return RedirectToAction("AdministratorHome", "Home");
                }
                else
                {
                    if (TypeLog == "HISENSE")
                    {
                        //Go to next page (Menu Scroll)
                        Session["Username"] = Username;
                        return View();
                    }
                    else
                    {
                        //View pages RADA
                        //Go to next page (Menu Scroll)
                        Session["Username"] = Username;
                        return RedirectToAction("RadaHome", "Home");
                    }
                }
            }
        }
    }
}