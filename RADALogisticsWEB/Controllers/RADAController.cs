using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RADALogisticsWEB.Models;

namespace RADALogisticsWEB.Controllers
{
    public class RADAController : Controller
    {
        List<Inventario> GetInventary = new List<Inventario>();
        //connection SQL server (database)
        SqlConnection DBSPP = new SqlConnection("Data Source=RADAEmpire.mssql.somee.com ;Initial Catalog=RADAEmpire ;User ID=RooRada; password=rada1311");
        SqlCommand con = new SqlCommand();
        SqlDataReader dr;
        // GET: RADA
        public ActionResult Index()
        {
            return View();
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

        public ActionResult ProcessData(string User, string Container, string Location)
        {
            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
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
                PalletControl.Parameters.AddWithValue("@Date", DateTime.Now.ToString());
                PalletControl.Parameters.AddWithValue("@Datetime", DateTime.Now.ToString());
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
                string Username = null, Container = null, LocationCode = null, Status = null, Datetime = null;
                SqlCommand log = new SqlCommand("Select * from RADAEmpire_CInventoryControl where Active = '1' and ID = '" + ID + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drlog = log.ExecuteReader();
                if (drlog.HasRows)
                {
                    while (drlog.Read())
                    {
                        Username = drlog["Username"].ToString();
                        Container = drlog["Container"].ToString();
                        LocationCode = drlog["LocationCode"].ToString();
                        Status = drlog["Status"].ToString();
                        Datetime = drlog["Datetime"].ToString();

                    }
                }
                else
                {
                    Username = null;
                    Container = null;
                    LocationCode = null;
                    Status = null;
                    Datetime = null;
                }
                DBSPP.Close();

                ViewBag.User = Username;
                ViewBag.Container = Container;
                ViewBag.LocationCode = LocationCode;
                ViewBag.Status = Status;
                ViewBag.Datetime = Datetime;
                ViewBag.ID = ID;
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
                        Datetimes = (Convert.ToDateTime(dr["Datetime"].ToString())),
                    });
                }
                DBSPP.Close();
            }
        }
    }
}