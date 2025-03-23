using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElectronicHub.Controllers
{
    public class AdminController : Controller
    {
        //Qusery
        public readonly string Monthly_Wise_Sales_Query = @"SELECT TOP (12) DATENAME(MONTH, Date) AS 'Month', SUM(TRY_CONVERT(DECIMAL(18,2), [Sub_Total])) AS 'Revenue'
                                                            FROM[Order] WHERE TRY_CONVERT(DECIMAL(18,2), [Sub_Total]) IS NOT NULL
                                                            GROUP BY DATENAME(MONTH, Date)";

        public readonly string Orders_Status_Query = "SELECT [Status] ,Count([Order_ID]) as 'Count' FROM [Order] Group by Status";

        public readonly string Item_Wise_Quantity_Query = "SELECT [ItemName],[ItemQuantity] FROM [Items] Group by [ItemName],[ItemQuantity]";

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Add_Products_Admin()
        {
            return View();
        }
        public ActionResult AwaitingShipment()
        {
            return View();
        }
        public ActionResult AwaitingShipment_COD()
        {
            return View();
        }
        public ActionResult AwaitingShipment_Paypal()
        {
            return View();
        }
        public ActionResult Stock_Index()
        {
            return View();
        }
        public ActionResult View_Products_Admin()
        {
            return View();
        }
        public ActionResult CompletedShipment()
        {
            return View();
        }
        public ActionResult CompletedShipment_COD()
        {
            return View();
        }
        public ActionResult CompletedShipment_Paypal()
        {
            return View();
        }
        public ActionResult Category()
        {
            return View();
        }
        public ActionResult Add_Category()
        {
            return View();
        }
        public JsonResult Monthly_Wise_Sales()
        {
            try
            {
                string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                List<object> chartMonthly_Assigned_Tranning = new List<object>();
                chartMonthly_Assigned_Tranning.Add(new object[]
                                {
                            "Month","Rs."
                                });
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand(Monthly_Wise_Sales_Query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                chartMonthly_Assigned_Tranning.Add(new object[]
                                {
                                    sdr[0], sdr[1]
                                    });
                            }
                        }

                        con.Close();
                    }
                }

                return Json(chartMonthly_Assigned_Tranning);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return Json("No Data");
        }


        public JsonResult Orders_Status()
        {
            try
            {
                string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                List<object> chart_All_Status_Count_PieChart_Default = new List<object>();
                chart_All_Status_Count_PieChart_Default.Add(new object[]
                                {
                                 "State","Count"
                                });
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand(Orders_Status_Query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                chart_All_Status_Count_PieChart_Default.Add(new object[]
                                {
                                    sdr[0], sdr[1]
                                    });
                            }
                        }

                        con.Close();
                    }
                }

                return Json(chart_All_Status_Count_PieChart_Default);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Chart Load Failed...!!" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Item_Wise_Quantity()
        {
            try
            {
                string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                List<object> chartMonthly_Assigned_Tranning = new List<object>();
                chartMonthly_Assigned_Tranning.Add(new object[]
                                {
                            "Item","Quantity."
                                });
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand(Item_Wise_Quantity_Query))
                    {
                        cmd.Connection = con;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                chartMonthly_Assigned_Tranning.Add(new object[]
                                {
                                    sdr[0], sdr[1]
                                    });
                            }
                        }

                        con.Close();
                    }
                }

                return Json(chartMonthly_Assigned_Tranning);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return Json("No Data");
        }
    }
}