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
    public class HISENSEController : Controller
    {
        List<Solicitud_Contenedores> GetListed = new List<Solicitud_Contenedores>();
        List<DetailsTable> GetDetails = new List<DetailsTable>();

        List<Areas> getArea = new List<Areas>();
        List<Areas> getAreaAll = new List<Areas>();
        //connection SQL server (database)
        SqlConnection DBSPP = new SqlConnection("Data Source=RADAEmpire.mssql.somee.com ;Initial Catalog=RADAEmpire ;User ID=RooRada; password=rada1311");
        SqlCommand con = new SqlCommand();
        SqlDataReader dr;

        // GET: HISENSE
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Details(string ID, string Record)
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
                string Area = null,container = null, origins = null, destination = null, status = null, solicitud = null, confirmacion = null, entrega = null, request = null, choffer = null, comment = null, date = null;
                //create generate randoms int value
                SqlCommand conse = new SqlCommand("Select " +
                    " a.Container as Container, a.Origins_Location as Origen, a.Destination_Location as Destination, a.Status as Status, a.Datetime as HSolicitud, " +
                    " b.Time_Confirm as HConfirm , b.Time_Finished as HFinish, a.Who_Send as WhoRequest, b.Choffer as Choffer, a.message as Comment, a.shift as Area, a.Date as Date  " +
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
                        Area = drconse["Area"].ToString();
                        comment = drconse["Comment"].ToString();
                        date = Convert.ToDateTime(drconse["Date"]).ToString("d");
                    }
                }
                DBSPP.Close();

                //create generate randoms int value
                string ReasonMT = null;
                SqlCommand reason = new SqlCommand("Select * from RADAEmpires_DRemoves where Active = '1' and Folio = '" + ID.ToString() + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drreason = reason.ExecuteReader();
                if (drreason.HasRows)
                {
                    while (drreason.Read())
                    {
                        ReasonMT = drreason["Reason"].ToString();
                    }
                }
                else
                {
                    ReasonMT = "Report not canceled";
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
                ViewBag.ReasonMT = ReasonMT;
                ViewBag.mot = Record;
                ViewBag.Areas = Area;

                GetDetailss(ID);
                ViewBag.Records = GetDetails;
                return View();
            }
        }

        public ActionResult RequestContainer()
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
                string val = null;
                //create generate randoms int value
                SqlCommand conse = new SqlCommand("Select Type_user from RADAEmpire_AUsers where Active = '1' and Username = '" + Session["Username"].ToString() + "'", DBSPP);
                DBSPP.Open();
                SqlDataReader drconse = conse.ExecuteReader();
                if (drconse.HasRows)
                {
                    while (drconse.Read())
                    {
                        val = drconse[0].ToString();
                    }
                }
                DBSPP.Close();

                if (val == "HISENSE")
                {
                    GetContainers();
                    GetAreas(name);
                    ViewBag.Records = GetListed;
                    ViewBag.listed = getArea;
                    ViewBag.Count = GetListed.Count.ToString();
                    return View();
                }
                else
                {
                    GetContainers();
                    GetAreasAll();
                    ViewBag.Records = GetListed;
                    ViewBag.listed = getAreaAll;
                    ViewBag.Count = GetListed.Count.ToString();
                    return View();
                }
            }
        }

        private void GetAreasAll()
        {
            if (getAreaAll.Count > 0)
            {
                getAreaAll.Clear();
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

                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select Name from RADAEmpire_AAreas where Active = '1'";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    getAreaAll.Add(new Areas()
                    {
                        Area = (dr["Name"].ToString()),
                    });
                }
                DBSPP.Close();
            }
        }

        private void GetAreas(string name)
        {
            if (getArea.Count > 0)
            {
                getArea.Clear();
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

                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select * from RADAEmpire_ARoles where Active = '1'";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    if (dr["Username"].ToString() == name)
                    {
                        getArea.Add(new Areas()
                        {
                            Username = (dr["Username"].ToString()),
                            Area = (dr["Areas"].ToString()),
                        });
                    }
                }
                DBSPP.Close();
            }
        }

        public PartialViewResult ActualizarTabla()
        {
            GetContainers();
            ViewBag.Records = GetListed; // Obtener nuevamente los datos
            return PartialView("table", ViewBag.Records);
        }

        public ActionResult ProcessData(string ActivoRampa , string User, string Type, string Container, string Origins, string Destination, string Area, string ActivoHidden)
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
                if (Area == null)
                {
                    return RedirectToAction("RequestContainer", "HISENSE");
                }
                else
                {
                    if (ActivoRampa == "true")
                    {
                        ActivoRampa = "SI";
                    }
                    else
                    {
                        ActivoRampa = "NO";
                    }
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
                    PalletControl.Parameters.AddWithValue("@Who_Send", User.ToString());
                    PalletControl.Parameters.AddWithValue("@Container", Container.ToUpper());
                    PalletControl.Parameters.AddWithValue("@Destination_Location", Destination.ToUpper());
                    PalletControl.Parameters.AddWithValue("@Origins_Location", Origins.ToUpper());
                    PalletControl.Parameters.AddWithValue("@Status", Type.ToString());
                    PalletControl.Parameters.AddWithValue("@message", "PENDING");
                    PalletControl.Parameters.AddWithValue("@shift", Area.ToString());
                    PalletControl.Parameters.AddWithValue("@Date", usTime.ToString());
                    PalletControl.Parameters.AddWithValue("@Datetime", usTime.ToString("HH:mm:ss"));
                    PalletControl.Parameters.AddWithValue("@Active", true);
                    PalletControl.Parameters.AddWithValue("@GruaRequest", ActivoHidden);
                    PalletControl.Parameters.AddWithValue("@RaRRequest", ActivoRampa.ToString());
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
                    RADAdocument.Parameters.AddWithValue("@Date", usTime.ToString());
                    RADAdocument.Parameters.AddWithValue("@AreaWork", "RADALogistics");
                    RADAdocument.Parameters.AddWithValue("@Active", true);
                    RADAdocument.Parameters.AddWithValue("@Cancel", false);
                    RADAdocument.ExecuteNonQuery();
                    DBSPP.Close();
                    //--------------------------------------------------------------------------------------------------------------------------------
                   
                    //Generar una lista por areas
                    List<string> lista = new List<string>();

                    if (Area == "ENVIOS" && Type == "CAR" && ActivoRampa == "NO")
                    {
                        lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                        lista.Add("CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                        lista.Add("CHOFER EN PROCESO DE ESTACIONAMIENTO CAJA");
                        lista.Add("CHOFER SOLTANDO CAJA");
                        lista.Add("CHOFER TERMINA MOVIMIENTO");
                    }
                    else
                    {
                        if (Area == "ENVIOS" && Type == "VAC" && ActivoRampa == "NO")
                        {
                            lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                            lista.Add("CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                            lista.Add("CHOFER EN RUTA A RAMPA DESTINO");
                            lista.Add("CHOFER SOLTANDO CAJA");
                            lista.Add("CHOFER TERMINA MOVIMIENTO");
                        }
                        else
                        {
                            if (Area == "PT" && Type == "CAR" && ActivoRampa == "NO")
                            {
                                lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                                lista.Add("CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                lista.Add("CHOFER EN PROCESO DE ESTACIONAMIENTO CAJA");
                                lista.Add("CHOFER SOLTANDO CAJA");
                                lista.Add("CHOFER TERMINA MOVIMIENTO");
                            }
                            else
                            {
                                if (Area == "PT" && Type == "VAC" && ActivoRampa == "NO")
                                {
                                    lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                                    lista.Add("CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                    lista.Add("CHOFER EN RUTA A RAMPA DESTINO");
                                    lista.Add("CHOFER SOLTANDO CAJA");
                                    lista.Add("CHOFER TERMINA MOVIMIENTO");
                                }
                            }
                        }
                    }

                    //TIPO DE MOVIMIENTO DE RAMPA A RAMPA
                    if (ActivoRampa == "SI" && ActivoHidden == "NO" && Type == "CAR")
                    {
                        lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                        lista.Add("CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                        lista.Add("CHOFER EN RUTA A RAMPA DESTINO");
                        lista.Add("CHOFER SOLTANDO CAJA");
                        lista.Add("CHOFER TERMINA MOVIMIENTO");
                    }
                    else
                    {
                        if (ActivoRampa == "SI" && ActivoHidden == "NO" && Type == "VAC")
                        {
                            lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                            lista.Add("CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                            lista.Add("CHOFER EN RUTA A RAMPA DESTINO");
                            lista.Add("CHOFER SOLTANDO CAJA");
                            lista.Add("CHOFER TERMINA MOVIMIENTO");
                        }
                    }


                    //AREA DE EMPAQUE
                    if (Area == "EMPAQUE" && Type == "CAR" && ActivoHidden == "NO" && ActivoRampa == "NO")
                    {
                        lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                        lista.Add("CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                        lista.Add("CHOFER EN RUTA A RAMPA DESTINO");
                        lista.Add("CHOFER SOLTANDO CAJA");
                        lista.Add("CHOFER TERMINA MOVIMIENTO");
                    }
                    else
                    {
                        if (Area == "EMPAQUE" && Type == "VAC" && ActivoHidden == "NO" && ActivoRampa == "NO")
                        {
                            lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                            lista.Add("CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                            lista.Add("CHOFER EN ESPERA DE SELLO");
                            lista.Add("CHOFER EN ESPERA DE AREA DE VACIO");
                            lista.Add("CHOFER TERMINA MOVIMIENTO");
                        }
                        else
                        {
                            if (Area == "EMPAQUE" && Type == "CAR" && ActivoHidden == "SI" && ActivoRampa == "NO")
                            {
                                lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                                lista.Add("CHOFER ENGANCHANDO CHASIS");
                                lista.Add("CHOFER EN ESPERA DE ACCESO A GRUA");
                                //lista.Add("CHOFER EN ESPERA DE MANIOBRA EN GRUA");
                                lista.Add("CHOFER EN ESPERA DE SALIDA DE GRUA");
                                lista.Add("CHOFER EN RUTA A RAMPA DESTINO");
                                lista.Add("CHOFER SOLTANDO CAJA");
                                lista.Add("CHOFER TERMINA MOVIMIENTO");
                            }
                            else
                            {
                                if (Area == "EMPAQUE" && Type == "VAC" && ActivoHidden == "SI" && ActivoRampa == "NO")
                                {
                                    lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                                    lista.Add("CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                    lista.Add("CHOFER EN ESPERA DE SELLO");
                                    lista.Add("CHOFER EN ESPERA DE ACCESO DE GRÚA");
                                    //lista.Add("CHOFER EN ESPERA DE MANIOBRA DE GRUA");
                                    //lista.Add("CHOFER EN ESPERA DE CARGADO DE GRUA");
                                    lista.Add("CHOFER EN ESPERA DE SALIDA DE GRUA");
                                    lista.Add("CHOFER SOLTANDO CHASIS");
                                    lista.Add("CHOFER TERMINA MOVIMIENTO");
                                }
                            }
                        }
                    }

                    //AREA DE GENERALES
                    if (Area == "GENERALES" && Type == "CAR" && ActivoHidden == "NO" && ActivoRampa == "NO")
                    {
                        lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                        lista.Add("CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                        lista.Add("CHOFER EN RUTA A RAMPA DESTINO");
                        lista.Add("CHOFER SOLTANDO CAJA");
                        lista.Add("CHOFER TERMINA MOVIMIENTO");
                    }
                    else
                    {
                        if (Area == "GENERALES" && Type == "VAC" && ActivoHidden == "NO" && ActivoRampa == "NO")
                        {
                            lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                            lista.Add("CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                            lista.Add("CHOFER EN ESPERA DE SELLO");
                            lista.Add("CHOFER EN ESPERA DE AREA DE VACIO");
                            lista.Add("CHOFER TERMINA MOVIMIENTO");
                        }
                        else
                        {
                            if (Area == "GENERALES" && Type == "CAR" && ActivoHidden == "SI" && ActivoRampa == "NO")
                            {
                                lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                                lista.Add("CHOFER ENGANCHANDO CHASIS");
                                lista.Add("CHOFER EN ESPERA DE ACCESO A GRUA");
                                //lista.Add("CHOFER EN ESPERA DE MANIOBRA EN GRUA");
                                lista.Add("CHOFER EN ESPERA DE SALIDA DE GRUA");
                                lista.Add("CHOFER EN RUTA A RAMPA DESTINO");
                                lista.Add("CHOFER SOLTANDO CAJA");
                                lista.Add("CHOFER TERMINA MOVIMIENTO");
                            }
                            else
                            {
                                if (Area == "GENERALES" && Type == "VAC" && ActivoHidden == "SI" && ActivoRampa == "NO")
                                {
                                    lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                                    lista.Add("CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                    lista.Add("CHOFER EN ESPERA DE SELLO");
                                    lista.Add("CHOFER EN ESPERA DE ACCESO DE GRÚA");
                                    //lista.Add("CHOFER EN ESPERA DE MANIOBRA DE GRUA");
                                    //lista.Add("CHOFER EN ESPERA DE CARGADO DE GRUA");
                                    lista.Add("CHOFER EN ESPERA DE SALIDA DE GRUA");
                                    lista.Add("CHOFER SOLTANDO CHASIS");
                                    lista.Add("CHOFER TERMINA MOVIMIENTO");
                                }
                            }
                        }
                    }

                    if (Area == "BODEGA 2" && Type == "CAR" && ActivoHidden == "NO" && ActivoRampa == "NO")
                    {
                        lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                        lista.Add("CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                        lista.Add("CHOFER EN RUTA A RAMPA DESTINO");
                        lista.Add("CHOFER SOLTANDO CAJA");
                        lista.Add("CHOFER TERMINA MOVIMIENTO");
                    }
                    else
                    {
                        if (Area == "BODEGA 2" && Type == "VAC" && ActivoHidden == "NO" && ActivoRampa == "NO")
                        {
                            lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                            lista.Add("CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                            lista.Add("CHOFER EN ESPERA DE SELLO");
                            lista.Add("CHOFER EN ESPERA DE AREA DE VACIO");
                            lista.Add("CHOFER TERMINA MOVIMIENTO");
                        }
                        else
                        {
                            if (Area == "BODEGA 2" && Type == "CAR" && ActivoHidden == "SI" && ActivoRampa == "NO")
                            {
                                lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                                lista.Add("CHOFER EN CAMINO A PLANTA");
                                lista.Add("CHOFER INGRESANDO A RECIBOS");
                                //lista.Add("CHOFER EN ESPERA DE MANIOBRA EN GRUA");
                                lista.Add("CHOFER ENGANCHANDO CHASIS");
                                lista.Add("CHOFER EN ESPERA DE ACCESO A GRUA");
                                //lista.Add("CHOFER EN ESPERA DE MANIOBRA EN GRUA");
                                lista.Add("CHOFER EN ESPERA DE SALIDA DE GRUA");
                                lista.Add("CHOFER EN RUTA A RAMPA DESTINO");
                                lista.Add("CHOFER SOLTANDO CAJA");
                                lista.Add("CHOFER TERMINA MOVIMIENTO");
                            }
                            else
                            {
                                if (Area == "BODEGA 2" && Type == "VAC" && ActivoHidden == "SI" && ActivoRampa == "NO")
                                {
                                    lista.Add("CHOFER ASIGNADO AL MOVIMIENTO");
                                    lista.Add("CHOFER EN PROCESO DE ENGANCHE DE CAJA");
                                    lista.Add("CHOFER EN ESPERA DE SELLO");
                                    lista.Add("CHOFER EN ESPERA DE ACCESO DE GRUA");
                                    //lista.Add("CHOFER EN ESPERA DE MANIOBRA DE GRUA");
                                    //lista.Add("CHOFER EN ESPERA DE CARGADO DE GRUA");
                                    lista.Add("CHOFER EN ESPERA DE MANIOBRA DE GRUA");
                                    lista.Add("CHOFER EN ESPERA DE CARGADO DE GRUA");
                                    lista.Add("CHOFER EN ESPERA DE SALIDA DE GRUA");
                                    lista.Add("CHOFER SOLTANDO CHASIS");
                                    lista.Add("CHOFER TERMINA MOVIMIENTO");
                                }
                            }
                        }
                    }

                    // Guardar información de cada paso en la base de datos
                    DBSPP.Open();
                    foreach (string paso in lista)
                    {
                        SqlCommand ProcessDB = new SqlCommand("INSERT INTO RADAEmpires_DZDetailsHisense " +
                            "(Folio, Type_StatusContainer, GruaMov, Process_Movement, End_date, Status, Comment, Date_Process, Activo) " +
                            "VALUES (@Folio, @Type_StatusContainer, @GruaMov, @Process_Movement, @End_date, @Status, @Comment, GETDATE(), @Activo)", DBSPP);

                        ProcessDB.Parameters.AddWithValue("@Folio", Folio.ToString());
                        ProcessDB.Parameters.AddWithValue("@Type_StatusContainer", Type.ToString());
                        ProcessDB.Parameters.AddWithValue("@GruaMov", ActivoHidden);
                        ProcessDB.Parameters.AddWithValue("@Process_Movement", paso); // Aquí se guarda cada paso de la lista
                        ProcessDB.Parameters.AddWithValue("@End_date", "00:00:00");
                        ProcessDB.Parameters.AddWithValue("@Status", "PENDIENTE");
                        ProcessDB.Parameters.AddWithValue("@Comment", "SIN COMENTARIOS");
                        ProcessDB.Parameters.AddWithValue("@Activo", true);

                        ProcessDB.ExecuteNonQuery();
                    }
                    DBSPP.Close();

                    return RedirectToAction("RequestContainer", "HISENSE");
                }
            }
        }

        private void GetDetailss(string ID)
        {
            if (GetDetails.Count > 0)
            {
                GetDetails.Clear();
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

                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select * from RADAEmpires_DZDetailsHisense where Activo = '1' and Folio = '" + ID + "' order by ID ASC";
                dr = con.ExecuteReader();
                while (dr.Read())
                {
                    GetDetails.Add(new DetailsTable()
                    {
                        Folio = (dr["Folio"].ToString()),
                        Type_Status = (dr["Type_StatusContainer"].ToString()),
                        GruaMov = (dr["GruaMov"].ToString()),
                        ProcessMovement = (dr["Process_Movement"].ToString()),
                        End_date = (dr["End_date"].ToString()),
                        Status = ((dr["Status"].ToString())),
                        Comment = (dr["Comment"].ToString()),
                        Date_Process = Convert.ToDateTime(dr["Date_Process"]).ToString("d"),
                    });
                }
                DBSPP.Close();
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
                // Obtener la fecha y hora actual en Alemania (zona horaria UTC+1 o UTC+2 dependiendo del horario de verano)
                DateTime germanTime = DateTime.UtcNow.AddHours(0);  // Alemania es UTC+1

                // Convertir la hora alemana a la hora en una zona horaria específica de EE. UU. (por ejemplo, Nueva York, UTC-5)
                TimeZoneInfo usEasternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
                DateTime usTime = TimeZoneInfo.ConvertTime(germanTime, usEasternTimeZone);

                // Formatear la fecha para que sea adecuada para la base de datos
                string formattedDate = usTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

                DBSPP.Open();
                con.Connection = DBSPP;
                con.CommandText = "Select * from RADAEmpire_BRequestContainers where Active = '1' and Date = '" + usTime.ToString("yyyy-MM-dd") + "' order by ID desc";
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
                SqlCommand PalletControl = new SqlCommand("insert into RADAEmpires_DRemoves" +
                    "(Folio, Reason, Datetime, Active,Company) values " +
                    "(@Folio, @Reason, @Datetime, @Active,@Company) ", DBSPP);
                //--------------------------------------------------------------------------------------------------------------------------------
                PalletControl.Parameters.AddWithValue("@Folio", ID.ToString());
                PalletControl.Parameters.AddWithValue("@Reason", Reason.ToString());
                PalletControl.Parameters.AddWithValue("@Datetime", usTime.ToString());
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