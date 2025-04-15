using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RADALogisticsWEB.Controllers
{
    public class LoginController : Controller
    {
        //connection SQL server (database)
        SqlConnection DBSPP = new SqlConnection("Data Source=RADAEmpire.mssql.somee.com ;Initial Catalog=RADAEmpire ;User ID=RooRada; password=rada1311");
        SqlCommand con = new SqlCommand();
        SqlDataReader dr;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LogIn()
        {
            ViewBag.message = "";
            return View();
        }

        [HttpPost]
        public ActionResult LogIn(string Username, string password)
        {
            if (Username != "" && password != "")
            {
                //query search username to employee enter to credentials
                string Usernames = null;
                SqlCommand sqlUsername = new SqlCommand("Select Username from RADAEmpire_AUsers where Active = '1' and Login_session = '" + Username + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drsqlUsername = sqlUsername.ExecuteReader();
                if (drsqlUsername.HasRows)
                {
                    while (drsqlUsername.Read())
                    {
                        Usernames = drsqlUsername[0].ToString();
                    }
                }
                else
                {
                    Usernames = null;
                }
                DBSPP.Close();

                if (Usernames == null)
                {
                    ViewBag.message = "Message: User no found !";
                    return View();
                }
                else
                {
                    //query search password to employee enter to credentials
                    string pass = null;
                    SqlCommand sqlPassword = new SqlCommand("Select Username from RADAEmpire_AUsers where Active = '1' and Login_session = '" + Username + "' and Password = '" + password + "'", DBSPP);
                    DBSPP.Open();
                    SqlDataReader drsqlPassword = sqlPassword.ExecuteReader();
                    if (drsqlPassword.HasRows)
                    {
                        while (drsqlPassword.Read())
                        {
                            pass = drsqlPassword[0].ToString();
                        }
                    }
                    else
                    {
                        pass = null;
                    }
                    DBSPP.Close();

                    if (pass == null)
                    {
                        ViewBag.message = "Message: User no found !";
                        return View();
                    }
                    else
                    {
                        string TypeLog = null;
                        SqlCommand log = new SqlCommand("Select Type_user from RADAEmpire_AUsers where Active = '1' and Login_session = '" + Username + "' and Password = '" + password + "'", DBSPP);
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

                        // Crear cookie persistente
                        HttpCookie userCookie = new HttpCookie("UserCookie", Usernames);
                        userCookie.Expires = DateTime.Now.AddHours(8); // duración de la cookie
                        Response.Cookies.Add(userCookie);

                        if (TypeLog == "ADMINISTRATOR")
                        {
                            //Go to next page (Menu Scroll)
                            //HomeController Frm = new HomeController();
                            //Frm.Username = Username;
                            Session["Username"] = Usernames;
                            Session["Type"] = TypeLog;
                            return RedirectToAction("AdministratorHome", "Home");
                        }
                        else
                        {
                            if (TypeLog == "HISENSE")
                            {
                                //Go to next page (Menu Scroll)
                                //HomeController Frm = new HomeController();
                                //Frm.Username = Username;
                                Session["Username"] = Usernames;
                                Session["Type"] = TypeLog;
                                return RedirectToAction("HisenseHome", "Home");
                            }
                            else
                            {
                                //View pages RADA
                                //Go to next page (Menu Scroll)
                                //HomeController Frm = new HomeController();
                                //Frm.Username = Username;
                                Session["Username"] = Usernames;
                                Session["Type"] = TypeLog;
                                return RedirectToAction("RadaHome", "Home");
                            }
                        }
                    }
                }
            }
            else
            {
                ViewBag.message = "Message: Error Credentials ! ";
                return View();
            }
        }
    }
}