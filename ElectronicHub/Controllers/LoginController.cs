using ElectronicHub.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElectronicHub.Controllers
{
    public class LoginController : Controller
    {

        public static string constring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        //Query
        public static string Login_Query = "select * from Login where Username = @UserName and Password = @Password";

        // GET: Login
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Register()
        {
            return View();
        }

        public ActionResult Login(User user)
        {
            try
            {
                string conn = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                SqlConnection sqlcon = new SqlConnection(conn);
                sqlcon.Open();
                SqlCommand cmd = new SqlCommand(Login_Query, sqlcon);

                cmd.Parameters.AddWithValue("@UserName", user.Username);
                cmd.Parameters.AddWithValue("@Password", user.Password);

                SqlDataReader sdr = cmd.ExecuteReader();

                if (sdr.Read())
                {

                    try
                    {
                        Session["username"] = user.Username.ToString();
                        Session["role"] = sdr[5].ToString();
                        Session["Name"] = sdr[4].ToString();
                        Session["UserId"] = sdr[0].ToString();

                        if (Session["role"].ToString() == "1")
                        {
                            return RedirectToAction("Index", "Customer");
                        }
                        else if (Session["role"].ToString() == "2")
                        {
                            return RedirectToAction("Index", "StoreHandler");
                        }
                        else if (Session["role"].ToString() == "3")
                        {
                            return RedirectToAction("Index", "Admin");
                        }
                        else
                        {
                            return RedirectToAction("Login", "Login");
                        }
                    }
                    catch (Exception e02)
                    {
                        Console.WriteLine("Error in  sdr" + e02);
                    }

                }
                else
                {
                    TempData["errorMsg"] = "<script>alert('Username or Password is Incorrect');</script>";
                }
                sqlcon.Close();
                return View();
            }
            catch (Exception e0)
            {
                Console.WriteLine("Error in  main" + e0);
                return View();
            }

        }

        public ActionResult Logout()
        {
            Session["username"] = null;
            Session["role"] = null;
            Session["location"] = null;
            Session.Abandon();
            return RedirectToAction("Login", "Login");
        }
    }
}