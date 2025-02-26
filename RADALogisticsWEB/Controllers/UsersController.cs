using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        //connection SQL server (database)
        SqlConnection DBSPP = new SqlConnection("Data Source=RADAEmpire.mssql.somee.com ;Initial Catalog=RADAEmpire ;User ID=RooRada; password=rada1311");
        SqlCommand con = new SqlCommand();
        SqlDataReader dr;
        public string message { get; set; }
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult UpdateProcess(string ID)
        {
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
                    command.Parameters.AddWithValue("@Username", Username);
                    command.Parameters.AddWithValue("@Employee_Number", Credentials);
                    command.Parameters.AddWithValue("@Login_session", Log);
                    command.Parameters.AddWithValue("@Password", Pass);
                    command.Parameters.AddWithValue("@Email", Email);
                    command.Parameters.AddWithValue("@Type_user", TypeUsers);
                    command.Parameters.AddWithValue("@ID", id);
                    int rowsAffected = command.ExecuteNonQuery();
                    DBSPP.Close();
                }

                return RedirectToAction("NewUser", "Users");
            }
        }

        public ActionResult NewUser()
        {
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
                //Guardar informacion a la base de datos del proyecto
                DBSPP.Open();
                SqlCommand PalletControl = new SqlCommand("insert into RADAEmpire_AUsers" +
                    "(Username, Employee_Number, Login_session, Password, Type_user, Email, DateAdded, DateRequest, Active,Createby) values " +
                    "(@Username, @Employee_Number, @Login_session, @Password, @Type_user, @Email, @DateAdded, @DateRequest, @Active,@Createby) ", DBSPP);
                //--------------------------------------------------------------------------------------------------------------------------------
                PalletControl.Parameters.AddWithValue("@Username", Username.ToString());
                PalletControl.Parameters.AddWithValue("@Employee_Number", Credentials.ToString());
                PalletControl.Parameters.AddWithValue("@Login_session", Log.ToString());
                PalletControl.Parameters.AddWithValue("@Password", Pass.ToString());
                PalletControl.Parameters.AddWithValue("@Type_user", TypeUsers.ToString());
                PalletControl.Parameters.AddWithValue("@Email", Email.ToString());
                PalletControl.Parameters.AddWithValue("@DateAdded", DateTime.Now.ToString());
                PalletControl.Parameters.AddWithValue("@DateRequest", DateTime.Now.ToString());
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

        public ActionResult RemoveUser(string ID)
        {
            ViewBag.User = Session["Username"];

            if (Session.Count <= 0)
            {
                return RedirectToAction("LogIn", "Login");
            }
            else
            {
                string updateQuery = "UPDATE RADAEmpire_AUsers SET Active = @Active WHERE ID = @ID";
                using (SqlCommand command = new SqlCommand(updateQuery, DBSPP))
                {
                    DBSPP.Open();
                    command.Parameters.AddWithValue("@Active", false);
                    command.Parameters.AddWithValue("@ID", ID);
                    int rowsAffected = command.ExecuteNonQuery();
                    DBSPP.Close();
                }

                ViewBag.message = "";
                message = "";
                GetUsuarios();
                ViewBag.Records = GetUsers;
                ViewBag.Count = GetUsers.Count.ToString();
                return RedirectToAction("NewUser", "Users");
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
                    con.CommandText = "Select top (1000) * from RADAEmpire_AUsers where Active = '1' order by ID desc";
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
                            DateAdded = Convert.ToDateTime(dr["DateRequest"].ToString()),
                        });
                    }
                    DBSPP.Close();
                }
            }
    }
    
}