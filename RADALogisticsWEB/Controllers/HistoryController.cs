using RADALogisticsWEB.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RADALogisticsWEB.Controllers
{
    public class HistoryController : Controller
    {
        List<Historial> GetRecords = new List<Historial>();
        List<Historial> GetRecordsQeury = new List<Historial>();
        //connection SQL server (database)
        SqlConnection DBSPP = new SqlConnection("Data Source=RADAEmpire.mssql.somee.com ;Initial Catalog=RADAEmpire ;User ID=RooRada; password=rada1311");
        SqlCommand con = new SqlCommand();
        SqlDataReader dr;
        // GET: History
        public ActionResult Records()
        {
            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                GetRecord();
                ViewBag.Records = GetRecords;
                ViewBag.Count = GetRecords.Count.ToString();
                return View();
            }
        }

        [HttpPost]
        public ActionResult Records(string Timeend, string TimeStart)
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
                    sqlTimeStart = " Where a.Date BETWEEN '" + TimeStart + "'";
                }

                if (Timeend == "")
                {
                    sqlTimeend = "";
                }
                else
                {
                    sqlTimeend = " and '" + Timeend + "'";
                }
            }
            else
            {
                count = count + 2;
            }

            if (count >= 2)
            {
                GetRecord();
                ViewBag.Records = GetRecords;
                ViewBag.Count = GetRecords.Count.ToString();
                return View();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "  Select top (1000) " +
                    " a.Folio as Folio, a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                    " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date " +
                    " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio " + sqlTimeStart + sqlTimeend + "";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetRecordsQeury.Add(new Historial()
                    {
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
                        Date = (Convert.ToDateTime(dr["Date"].ToString())),
                    });
                }
                DBSPP.Close();

                GetRecord();
                ViewBag.Records = GetRecordsQeury;
                ViewBag.Count = GetRecordsQeury.Count.ToString();
                return View();
            }
        }

        public ActionResult CancelContainerR(string ID)
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

                    if (TypeLog == "HISENSE")
                    {
                        return RedirectToAction("CancelContainerH", "History", new { ID = ID });
                    }
                    else
                    {
                        return View();
                    }
                }
                else
                {
                    return RedirectToAction("Records", "History");
                }
            }
        }

        public ActionResult CancelContainerH(string ID)
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

                    if (TypeLog == "HISENSE")
                    {
                        return View();
                    }
                    else
                    {
                        return RedirectToAction("CancelContainerR", "History", new { ID = ID });
                    }
                }
                else
                {
                    return RedirectToAction("Records", "History");
                }
            }
        }

        public ActionResult Delete(string ID, string Reason, string Company)
        {
            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                if (Company == "RADA")
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
                    PalletControl.Parameters.AddWithValue("@Company", "RADA");
                    PalletControl.ExecuteNonQuery();
                    DBSPP.Close();

                    //query message ------------------------------------------------------------------------------------------------------------------
                    string updateQuery = "UPDATE RADAEmpire_BRequestContainers SET message = @message WHERE Folio = @ID";
                    using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                    {
                        DBSPP.Open();
                        command.Parameters.AddWithValue("@message", "Canceled by Rada");
                        command.Parameters.AddWithValue("@ID", ID);
                        int rowsAffected = command.ExecuteNonQuery();
                        DBSPP.Close();
                    }
                    return RedirectToAction("Records", "History");
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
                    PalletControl.Parameters.AddWithValue("@Company", "HISENSE");
                    PalletControl.ExecuteNonQuery();
                    DBSPP.Close();

                    //query message ------------------------------------------------------------------------------------------------------------------
                    string updateQuery = "UPDATE RADAEmpire_BRequestContainers SET message = @message WHERE Folio = @ID";
                    using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                    {
                        DBSPP.Open();
                        command.Parameters.AddWithValue("@message", "Canceled by Hisense");
                        command.Parameters.AddWithValue("@ID", ID);
                        int rowsAffected = command.ExecuteNonQuery();
                        DBSPP.Close();
                    }
                    return RedirectToAction("Records", "History");
                }
            }
        }

        private void GetRecord()
        {
            if (GetRecords.Count > 0)
            {
                GetRecords.Clear();
            }
            else
            {
                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "  Select top (1000) " +
                    " a.Folio as Folio,a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                    " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.Date as Date " +
                    " from RADAEmpire_BRequestContainers as a inner join RADAEmpire_CEntryContrainers as b on b.Folio_Request = a.Folio";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetRecords.Add(new Historial()
                    {
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
                        Date = (Convert.ToDateTime(dr["Date"].ToString())),
                    });
                }
                DBSPP.Close();
            }
        }
    }
}