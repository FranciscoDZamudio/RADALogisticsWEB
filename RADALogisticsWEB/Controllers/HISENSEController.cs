using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RADALogisticsWEB.Models;

namespace RADALogisticsWEB.Controllers
{
    public class HISENSEController : Controller
    {
        List<Solicitud_Contenedores> GetListed = new List<Solicitud_Contenedores>();
        //connection SQL server (database)
        SqlConnection DBSPP = new SqlConnection("Data Source=RADAEmpire.mssql.somee.com ;Initial Catalog=RADAEmpire ;User ID=RooRada; password=rada1311");
        SqlCommand con = new SqlCommand();
        SqlDataReader dr;

        // GET: HISENSE
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(string ID)
        {
            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                string container = null, origins = null, destination = null, status = null, solicitud = null, confirmacion = null, entrega = null, request = null, choffer = null, comment = null, date = null;
                //create generate randoms int value
                SqlCommand conse = new SqlCommand("Select " +
                    " a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                    " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date  " +
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
                        container = drconse["Container"].ToString();//234 + 1216 + 
                        origins = drconse["Origen"].ToString();
                        destination = drconse["Destination"].ToString();
                        status = drconse["Status"].ToString();
                        solicitud = drconse["HSolicitud"].ToString();
                        confirmacion = drconse["HConfirm"].ToString();
                        entrega = drconse["HFinish"].ToString();
                        request = drconse["WhoRequest"].ToString();
                        choffer = drconse["Choffer"].ToString();
                        comment = drconse["Comment"].ToString();
                        date = drconse["Date"].ToString();
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

                return View();
            }
        }

        public ActionResult RequestContainer()
        {
            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                GetContainers();
                ViewBag.Records = GetListed;
                ViewBag.Count = GetListed.Count.ToString();
                return View();
            }
        }

        public ActionResult ProcessData(string User, string Type, string Container, string Origins, string Destination)
        {
            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
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

                //Guardar informacion a la base de datos del proyecto
                DBSPP.Open();
                SqlCommand PalletControl = new SqlCommand("insert into RADAEmpire_BRequestContainers" +
                    "(Folio, Who_Send, Container, Destination_Location, Origins_Location, Status, message, shift, Date, Datetime, Active) values " +
                    "(@Folio, @Who_Send, @Container, @Destination_Location, @Origins_Location, @Status, @message, @shift, @Date, @Datetime, @Active) ", DBSPP);
                //--------------------------------------------------------------------------------------------------------------------------------
                PalletControl.Parameters.AddWithValue("@Folio", Folio.ToString());
                PalletControl.Parameters.AddWithValue("@Who_Send", User.ToString());
                PalletControl.Parameters.AddWithValue("@Container", Container.ToString());
                PalletControl.Parameters.AddWithValue("@Destination_Location", Destination.ToString());
                PalletControl.Parameters.AddWithValue("@Origins_Location", Origins.ToString());
                PalletControl.Parameters.AddWithValue("@Status", Type.ToString());
                PalletControl.Parameters.AddWithValue("@message", "PENDING");
                PalletControl.Parameters.AddWithValue("@shift", "0");
                PalletControl.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd"));
                PalletControl.Parameters.AddWithValue("@Datetime", DateTime.Now.ToString("HH:mm:ss"));
                PalletControl.Parameters.AddWithValue("@Active", true);

                PalletControl.ExecuteNonQuery();
                DBSPP.Close();
                //--------------------------------------------------------------------------------------------------------------------------------

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
                RADAdocument.Parameters.AddWithValue("@Date", DateTime.Now.ToString("yyyy-MM-dd"));
                RADAdocument.Parameters.AddWithValue("@AreaWork", "RADALogistics");
                RADAdocument.Parameters.AddWithValue("@Active", true);
                RADAdocument.Parameters.AddWithValue("@Cancel", false);
                RADAdocument.ExecuteNonQuery();
                DBSPP.Close();
                //--------------------------------------------------------------------------------------------------------------------------------


                return RedirectToAction("RequestContainer", "HISENSE");
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
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select * from RADAEmpire_BRequestContainers where Active = '1' and Date = '" + DateTime.Now.ToString("yyyy-MM-dd") + "' order by ID desc";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetListed.Add(new Solicitud_Contenedores()
                    {
                        ID = int.Parse(dr["ID"].ToString()),
                        Folio = (dr["Folio"].ToString()),
                        Who_Send = (dr["Who_Send"].ToString()),
                        Container = (dr["Container"].ToString()),
                        Destination_Location = (dr["Destination_Location"].ToString()),
                        Origins_Location = ((dr["Origins_Location"].ToString())),
                        Status = (dr["Status"].ToString()),
                        shift = (dr["shift"].ToString()),
                        message = (dr["message"].ToString()),
                        Date = (Convert.ToDateTime(dr["Date"].ToString())),
                        Datetime = (Convert.ToDateTime(dr["Datetime"].ToString())),
                    });
                }
                DBSPP.Close();
            }
        }

        public ActionResult CancelContainer(string ID)
        {
            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
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
            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                //Guardar informacion a la base de datos del proyecto
                DBSPP.Open();
                SqlCommand PalletControl = new SqlCommand("insert into RADAEmpires_DRemoves" +
                    "(Folio, Reason, Datetime, Active,Company) values " +
                    "(@Folio, @Reason, @Datetime, @Active,@Company) ", DBSPP);
                //--------------------------------------------------------------------------------------------------------------------------------
                PalletControl.Parameters.AddWithValue("@Folio", ID.ToString());
                PalletControl.Parameters.AddWithValue("@Reason", Reason.ToString());
                PalletControl.Parameters.AddWithValue("@Datetime", DateTime.Now.ToString());
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