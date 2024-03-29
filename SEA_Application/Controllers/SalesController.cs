﻿using Microsoft.AspNet.Identity;
using Microsoft.Office.Interop.Excel;
using SEA_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using static iTextSharp.text.pdf.AcroFields;

namespace SEA_Application.Controllers
{
    public class SalesController : Controller
    {
        // GET: Sales
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        public ActionResult Index()
        {

            return View();
        }
        public ActionResult AllSaleOrdersList(DataTablesParam param)
        {
            try
            {

                int pageNo = 1;

                if (param.iDisplayStart >= param.iDisplayLength)
                {
                    pageNo = (param.iDisplayStart / param.iDisplayLength) + 1;
                }
                int totalCount = 0;


                if (param.sSearch != null)
                {
                    totalCount = db.SaleOrders
                                         .Select(x => new { x.Id, x.OrderNo, x.Description, x.Status, CustomerName = x.CustomerName,  x.Discount, Date = x.Date.Value, DiscountedPrice = x.DiscountedPrice, Total = x.Total.Value })
                                         .Where(x=>x.OrderNo.Value.ToString().Contains(param.sSearch) || x.Description.ToLower().ToLower().Contains(param.sSearch.ToLower()) || x.Status.ToLower().ToLower().Contains(param.sSearch.ToLower())  || x.Discount.Value.ToString().Contains(param.sSearch) 
                                          || string.Concat(x.Date.Month.ToString() + "/" + x.Date.Day.ToString() + "/" + x.Date.Year.ToString()).Contains(param.sSearch) || x.DiscountedPrice.Value.ToString().Contains(param.sSearch) || x.Total.ToString().Contains(param.sSearch))
                                         .OrderByDescending(x=>x.OrderNo).ToList().Count();
                    // totalCount = 

                    var SaleOrdersList = db.SaleOrders
                                         .Select(x => new { x.Id, x.OrderNo, x.Description, x.Status, CustomerName = x.CustomerName, x.Discount, Date = x.Date.Value, DiscountedPrice = x.DiscountedPrice, Total = x.Total.Value })
                                         .Where(x => x.OrderNo.Value.ToString().Contains(param.sSearch) || x.Description.ToLower().ToLower().Contains(param.sSearch.ToLower()) || x.Status.ToLower().ToLower().Contains(param.sSearch.ToLower()) || x.Discount.Value.ToString().Contains(param.sSearch)
                                          || string.Concat(x.Date.Month.ToString() + "/" + x.Date.Day.ToString() + "/" + x.Date.Year.ToString()).Contains(param.sSearch) || x.DiscountedPrice.Value.ToString().Contains(param.sSearch) || x.Total.ToString().Contains(param.sSearch)).OrderByDescending(x => x.OrderNo).Skip((pageNo - 1) * param.iDisplayLength).Take(param.iDisplayLength)
                                         .ToList();
 

                    List<SaleOrderCustom> CustomModelList = new List<SaleOrderCustom>();

                    foreach (var item in SaleOrdersList)
                    {
                        SaleOrderCustom obj = new SaleOrderCustom();

                        obj.Id = item.Id;
                        obj.OrderNo = item.OrderNo.Value;
                        obj.Status = item.Status;
                        obj.CustomerName = item.CustomerName;
                        obj.Discount = item.Discount.Value;

                        obj.DateTime = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(item.Date, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")), TimeZoneInfo.Local);
                        obj.Discount = item.Discount.Value;
                        obj.Total = item.Total;
                        obj.DiscountedPrice = item.DiscountedPrice.Value;
                        obj.Description = item.Description;

                        CustomModelList.Add(obj);

                    }

                    // return Json(CustomModelList, JsonRequestBehavior.AllowGet);


                    return Json(new
                    {
                        aaData = CustomModelList,
                        sEcho = param.sEcho,
                        iTotalDisplayRecords = totalCount,
                        iTotalRecords = totalCount

                    }, JsonRequestBehavior.AllowGet);


                }//end of if block
                else
                {

                    totalCount = db.SaleOrders
                                        .Select(x => new { x.Id, x.OrderNo, x.Description, x.Status, CustomerName = x.CustomerName, x.Discount, Date = x.Date.Value, DiscountedPrice = x.DiscountedPrice, Total = x.Total.Value })
                                         .OrderByDescending(x => x.OrderNo).ToList().Count();
                    
                    var SaleOrdersList = db.SaleOrders
                                         .Select(x => new { x.Id, x.OrderNo, x.Description, x.Status, CustomerName = x.CustomerName, x.Discount, Date = x.Date.Value, DiscountedPrice = x.DiscountedPrice, Total = x.Total.Value })
                                         .OrderByDescending(x => x.OrderNo).Skip((pageNo - 1) * param.iDisplayLength).Take(param.iDisplayLength).ToList();


                    List<SaleOrderCustom> CustomModelList = new List<SaleOrderCustom>();
                    foreach (var item in SaleOrdersList)
                    {
                        SaleOrderCustom obj = new SaleOrderCustom();

                        obj.Id = item.Id;
                        obj.OrderNo = item.OrderNo.Value;
                        obj.Status = item.Status;
                        obj.CustomerName = item.CustomerName;
                        obj.Discount = item.Discount.Value;
                        obj.DateTime = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(item.Date, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")), TimeZoneInfo.Local);
                        obj.Discount = item.Discount.Value;
                        obj.Total = item.Total;
                        obj.DiscountedPrice = item.DiscountedPrice.Value;
                        obj.Description = item.Description;

                        CustomModelList.Add(obj);

                    }


                    return Json(new
                    {
                        aaData = CustomModelList,
                        sEcho = param.sEcho,
                        iTotalDisplayRecords = totalCount,
                        iTotalRecords = totalCount

                    }, JsonRequestBehavior.AllowGet);


                }

                // CustomModelList= CustomModelList.OrderByDescending(x => x.OrderNo).ToList();

            }//end of try block
            catch (Exception ex)
            {
                var Msg = ex.Message;
                var inner = ex.InnerException.Message;
            }

            return Json("", JsonRequestBehavior.AllowGet);

        }

        public ActionResult ReturnSaleOrder()
        {
            var StudentList = (from user in db.AspNetUsers
                               join student in db.AspNetStudents on user.Id equals student.StudentID
                               select new
                               {
                                   StudentId = student.Id,
                                   StudentName = user.Name + "(" + user.UserName + ")",

                               }).Distinct().ToList();


            ViewBag.StudentID = new SelectList(StudentList, "StudentId", "StudentName");

            var id = User.Identity.GetUserId();
            var username = db.AspNetUsers.Where(x => x.Id == id).Select(x => x.Name).FirstOrDefault();

            ViewBag.UserName = username;


            return View();
        }
        public ActionResult SaveReturnSaleOrder(string Date, double GrandTotal,int SaleTypeId, string Cashier, List<SaleOrdersList> SaleOrdersList)
        {
            int? MaxId = 1000;
            int? GetID = db.SaleOrders.Select(x => x.OrderNo).Max();

            if (GetID != null)
            {
                MaxId = GetID + 1;
            }

            var Datetime = GetLocalDateTime.GetLocalDateTimeFunction();
            var Time = Datetime.Value.TimeOfDay;

            SaleOrder saleorder = new SaleOrder();
            //  saleorder.Date = Convert.ToDateTime(Date);
            // saleorder.Date = saleorder.Date.Value.Add(Time);
            saleorder.Date = GetLocalDateTime.GetLocalDateTimeFunction();
            saleorder.SaleTypeId = SaleTypeId;
            saleorder.Creator = Cashier;

            var TimeofDay = GetLocalDateTime.GetLocalDateTimeFunction().Value.TimeOfDay;
            //var TimeofDay = GetLocalDateTime.GetLocalDateTimeFunction().Valu;

            var TimeofDayHour = GetLocalDateTime.GetLocalDateTimeFunction().Value.Hour;


            if (TimeofDayHour >= 0 && TimeofDayHour <= 3)
            {
                //pervious date
                saleorder.Date = saleorder.Date.Value.AddDays(-1);
            }

            saleorder.CreationDate = GetLocalDateTime.GetLocalDateTimeFunction();
            saleorder.StudentId = null;
            saleorder.CustomerName = null;
            saleorder.Total = -GrandTotal;
            saleorder.Discount = 0;
            saleorder.DiscountedPrice = -GrandTotal;
            saleorder.OrderNo = MaxId;
            saleorder.Status = "Returned";

            db.SaleOrders.Add(saleorder);
            db.SaveChanges();


            foreach (var item in SaleOrdersList)
            {
                SaleDetail saleDetail = new SaleDetail();

                saleDetail.InventoryId = item.InventoryId;
                saleDetail.Quantity = item.Quantity;
                saleDetail.UnitSalePrice = item.UnitSalePrice;
                saleDetail.TotalPrice = -item.Cost;
                saleDetail.SaleOrderId = saleorder.Id;

                db.SaleDetails.Add(saleDetail);
                db.SaveChanges();

                var InventoryToUpdate = db.Inventories.Where(x => x.Id == item.InventoryId).FirstOrDefault();
                InventoryToUpdate.QuantityOnHand = InventoryToUpdate.QuantityOnHand + item.Quantity;


                //   var TotalCost = saleDetail.Quantity * InventoryToUpdate.AverageCost;

                ////update average cost and total cost 
                // InventoryToUpdate.TotalCost = InventoryToUpdate.TotalCost + TotalCost;
                //InventoryToUpdate.AverageCost = Math.Round((InventoryToUpdate.TotalCost.Value / InventoryToUpdate.QuantityOnHand.Value), 2);

                db.SaveChanges();

            }

            return Json("Returned", JsonRequestBehavior.AllowGet);

        }

        public ActionResult Summary()
        {
            return View();
        }

        public ActionResult GetSaleSummary(string FromDate, string ToDate, string CustomerName, string SaleType)
        {

            DateTime dateTimeFrom = Convert.ToDateTime(FromDate);


            DateTime dateTimeTo = Convert.ToDateTime(ToDate);

            dateTimeTo = dateTimeTo.AddDays(1);
            string toDateInString = dateTimeTo.ToString();

            List<SaleOrderCustom> SalesList = new List<SaleOrderCustom>();

            List<SaleOrder> SaleOrdersList = null;

            List<string> saleList = new List<string>();

            if (SaleType == "All")
            {
                saleList.Add("Cash");
                saleList.Add("Credit");
                saleList.Add("Food Panda");
            } else
            {

                saleList.Add(SaleType);
            }


            if (CustomerName == "All")
            {
              SaleOrdersList = db.SaleOrders.Where(x => x.Date >= dateTimeFrom && x.Date <= dateTimeTo && ( saleList.Contains(x.SaleType.Name) || x.SaleTypeId == null)).ToList();
               // SaleOrdersList = db.SaleOrders.Where(x => x.Date >= dateTimeFrom && x.Date <= dateTimeTo ).ToList();
            }
            else
            {
                SaleOrdersList = db.SaleOrders.Where(x => x.Date >= dateTimeFrom && x.Date <= dateTimeTo && x.CustomerName == CustomerName && (saleList.Contains(x.SaleType.Name) || x.SaleTypeId == null)).ToList();
               // SaleOrdersList = db.SaleOrders.Where(x => x.Date >= dateTimeFrom && x.Date <= dateTimeTo && x.CustomerName == CustomerName ).ToList();

            }

            foreach (var item in SaleOrdersList)
            {
                SaleOrderCustom obj = new SaleOrderCustom();
                obj.Id = item.Id;
                obj.OrderNo = item.OrderNo.Value;
                obj.Status = item.Status;

                if (item.CustomerName != null)
                {
                    obj.CustomerName = item.CustomerName;

                } else
                {
                    obj.CustomerName = "";

                }
                if (item.SaleType == null)
                {
                    obj.SaleType = "";

                }
                else
                {
                    obj.SaleType = item.SaleType.Name;

                }
                obj.Cashier = item.Creator;
                     
                obj.Discount = item.Discount.Value;
                obj.Total = item.Total.Value;
                obj.DiscountedPrice = item.DiscountedPrice.Value;
                var DiscountPercentage = item.DiscountPercentage;

                if (DiscountPercentage != null)
                {
                    DiscountPercentage = item.DiscountPercentage.Value;
                }
                else
                {
                    DiscountPercentage = Math.Round((item.Discount.Value * 100) / item.Total.Value, 2);
                }
                obj.DiscountPercentage = DiscountPercentage.Value;

                if (item.SaleDiscountId == null)
                {
                    obj.DiscountType = "Bismarck Discount";
                }
                else
                {
                    obj.DiscountType = item.SaleDiscount.Name;
                }

                if (obj.Status == "Returned")
                {
                    obj.DiscountType = "-";
                }

                obj.DateTime = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(item.Date.Value, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")), TimeZoneInfo.Local);

                SalesList.Add(obj);
            }

            return Json(SalesList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaleByQuantity()
        {
            return View();
        }

        public ActionResult GetSaleByQuantity(string FromDate, string ToDate)
        {
            DateTime dateTimeFrom = Convert.ToDateTime(FromDate);
            DateTime dateTimeTo = Convert.ToDateTime(ToDate);

            dateTimeTo = dateTimeTo.AddDays(1);
            string toDateInString = dateTimeTo.ToString();

            List<SaleOrderCustom> SalesList = new List<SaleOrderCustom>();

            List<SaleOrder> SaleOrdersList = db.SaleOrders.Where(x => x.Date >= dateTimeFrom && x.Date <= dateTimeTo).ToList();


            var AllSaleDetails = db.SaleDetails.Where(x => x.SaleOrder.Date >= dateTimeFrom && x.SaleOrder.Date <= dateTimeTo && x.SaleOrder.Status == "Paid").ToList();
            var SaleDistinctIds = AllSaleDetails.Select(x => x.InventoryId).Distinct();
            List<SaleDetailByQuantity> saleDetailByQuantity = new List<SaleDetailByQuantity>();

            foreach (var InventoryId in SaleDistinctIds)
            {
                SaleDetailByQuantity obj = new SaleDetailByQuantity();

                var SaleDetails = AllSaleDetails.Where(x => x.InventoryId == InventoryId).ToList();


                obj.InventoryId = InventoryId.Value;
                obj.ProductNo = SaleDetails.FirstOrDefault().Inventory.ItemNo.Value;
                obj.InventoryName = SaleDetails.FirstOrDefault().Inventory.Name;
                // obj.UnitSalePrice = SaleDetails.FirstOrDefault().UnitSalePrice.Value;
                obj.Quantity = SaleDetails.Select(x => x.Quantity.Value).Sum();

                obj.Total = SaleDetails.Select(x => x.TotalPrice.Value).Sum();

                saleDetailByQuantity.Add(obj);
            }



            var AllSaleReturnedDetails = db.SaleDetails.Where(x => x.SaleOrder.Date >= dateTimeFrom && x.SaleOrder.Date <= dateTimeTo && x.SaleOrder.Status == "Returned").ToList();

            var SaleDistinctReturnedIds = AllSaleReturnedDetails.Select(x => x.InventoryId).Distinct();

            foreach (var InventoryId in SaleDistinctReturnedIds)
            {
                SaleDetailByQuantity obj = new SaleDetailByQuantity();

                var SaleDetails = AllSaleReturnedDetails.Where(x => x.InventoryId == InventoryId).ToList();


                obj.InventoryId = InventoryId.Value;
                obj.ProductNo = SaleDetails.FirstOrDefault().Inventory.ItemNo.Value;
                obj.InventoryName = SaleDetails.FirstOrDefault().Inventory.Name;
                //obj.UnitSalePrice = SaleDetails.FirstOrDefault().UnitSalePrice.Value;
                obj.Quantity = -(SaleDetails.Select(x => x.Quantity.Value).Sum());

                obj.Total = SaleDetails.Select(x => x.TotalPrice.Value).Sum();

                saleDetailByQuantity.Add(obj);
            }


            var TotalDiscount = SaleOrdersList.Select(x => x.Discount.Value).Sum();
            var TotalDiscountedPrice = SaleOrdersList.Select(x => x.DiscountedPrice.Value).Sum();


            return Json(new { SaleDetailByQuantity = saleDetailByQuantity, TotalDiscount = TotalDiscount, TotalDiscountedPrice = TotalDiscountedPrice }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Create()
        {
            var StudentList = (from user in db.AspNetUsers
                               join student in db.AspNetStudents on user.Id equals student.StudentID
                               select new
                               {
                                   StudentId = student.Id,
                                   StudentName = user.Name + "(" + user.UserName + ")",

                               }).Distinct().ToList();


            var id = User.Identity.GetUserId();
            var username = db.AspNetUsers.Where(x => x.Id == id).Select(x => x.Name).FirstOrDefault();

            ViewBag.UserName = username;

            ViewBag.StudentID = new SelectList(StudentList, "StudentId", "StudentName");

            return View();
        }

        
        public ActionResult GetExternalAndProductionList()
        {
            var AllExternalAndProdutionList = db.Inventories.Where(x => x.Type == "ExternalProduct" || x.Type == "ProductProduction").Select(x => new { x.Id, Name = x.Name + "  (" + x.ItemNo + ")", x.UnitName, x.UnitPurchasePrice, x.UnitSalePrice, x.QuantityOnHand }).ToList();

            string status = Newtonsoft.Json.JsonConvert.SerializeObject(AllExternalAndProdutionList);

            return Content(status);
        }

        public ActionResult GetSaleTypeList()
        {
            var AllExternalAndProdutionList = db.SaleTypes.Select(x => new { x.Id, Name = x.Name }).ToList();

            string status = Newtonsoft.Json.JsonConvert.SerializeObject(AllExternalAndProdutionList);

            return Content(status);
        }

        public ActionResult GetCustomerList()
        {
            var CustomersList = db.SaleOrders.Where(x=>x.CustomerName.ToString().Trim() != "").Select(x=>x.CustomerName).Distinct().ToList();

            string status = Newtonsoft.Json.JsonConvert.SerializeObject(CustomersList);

            return Content(status);
        }


        [HttpPost]
        public ActionResult Create(int a)
        {





            return View();

        }
        //static
        public string base64Decode()
        {
            string sData = "AO51H2R6FJkWJaUh/P+4aG1ry/s5mPLF7AkXSfY/uQFMznWfxx+m52F+ftSl7JZI3Q==";
            try
            {
                var encoder = new System.Text.UTF8Encoding();
                System.Text.Decoder utf8Decode = encoder.GetDecoder();
                byte[] todecodeByte = Convert.FromBase64String(sData);
                int charCount = utf8Decode.GetCharCount(todecodeByte, 0, todecodeByte.Length);
                char[] decodedChar = new char[charCount];
                utf8Decode.GetChars(todecodeByte, 0, todecodeByte.Length, decodedChar, 0);
                string result = new String(decodedChar);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in base64Decode" + ex.Message);
            }
        }

        public ActionResult SaveSaleOrders(string CustomerName, string CustomerPhoneNo, string Date, string Description, double GrandTotal, float DiscountPercentage, int DiscountType, double Discount, double DiscountedPrice, int   SaleTypeId , string Cashier, List<SaleOrdersList> SaleOrdersList)
        {
            // return Json(new { OrderNo = 1000, data = "Created" }, JsonRequestBehavior.AllowGet);

            DateTime? SaleDate = null;

            int? MaxId = 1000;


            int? GetID = db.SaleOrders.Select(x => x.OrderNo).Max();


            if (GetID != null)

            {
                MaxId = GetID + 1;
            }


            var Datetime = GetLocalDateTime.GetLocalDateTimeFunction();
            var Time = Datetime.Value.TimeOfDay;

            SaleOrder saleorder = new SaleOrder();

            // saleorder.Date = Convert.ToDateTime(Date);
            //saleorder.Date = saleorder.Date.Value.Add(Time);

            saleorder.Date = GetLocalDateTime.GetLocalDateTimeFunction();
            saleorder.SaleTypeId = SaleTypeId;

            var TimeofDay = GetLocalDateTime.GetLocalDateTimeFunction().Value.TimeOfDay;
            //var TimeofDay = GetLocalDateTime.GetLocalDateTimeFunction().Valu;

            var TimeofDayHour = GetLocalDateTime.GetLocalDateTimeFunction().Value.Hour;


            if (TimeofDayHour >= 0 && TimeofDayHour <= 3)
            {
                //pervious date
                saleorder.Date = saleorder.Date.Value.AddDays(-1);
            }

            SaleDate = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(saleorder.Date.Value, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")), TimeZoneInfo.Local);
            saleorder.CreationDate = GetLocalDateTime.GetLocalDateTimeFunction();
            saleorder.StudentId = null;
            saleorder.CustomerName = null;
            saleorder.Total = GrandTotal;
            saleorder.Discount = Discount;
            saleorder.DiscountedPrice = DiscountedPrice;
            saleorder.OrderNo = MaxId;
            saleorder.Status = "Paid";
            saleorder.DiscountPercentage = DiscountPercentage;
            saleorder.SaleDiscountId = DiscountType;
            saleorder.Description = Description;
            saleorder.CustomerName = CustomerName;
            saleorder.CustomerPhoneNo = CustomerPhoneNo;
            saleorder.Creator = Cashier;

            db.SaleOrders.Add(saleorder);
            db.SaveChanges();


            foreach (var item in SaleOrdersList)
            {
                SaleDetail saleDetail = new SaleDetail();

                saleDetail.InventoryId = item.InventoryId;
                saleDetail.Quantity = item.Quantity;
                saleDetail.UnitSalePrice = item.UnitSalePrice;
                saleDetail.TotalPrice = item.Cost;
                saleDetail.SaleOrderId = saleorder.Id;

                db.SaleDetails.Add(saleDetail);
                db.SaveChanges();


                var InventoryToUpdate = db.Inventories.Where(x => x.Id == item.InventoryId).FirstOrDefault();
                InventoryToUpdate.QuantityOnHand = InventoryToUpdate.QuantityOnHand - item.Quantity;


                //var TotalCost = saleDetail.Quantity * InventoryToUpdate.AverageCost;

                //////update average cost and total cost 
                //InventoryToUpdate.TotalCost = InventoryToUpdate.TotalCost - TotalCost;

                //if (InventoryToUpdate.QuantityOnHand == 0)
                //{
                //    InventoryToUpdate.AverageCost = 0;
                //}
                //else
                //{
                //    InventoryToUpdate.AverageCost = Math.Round((InventoryToUpdate.TotalCost.Value / InventoryToUpdate.QuantityOnHand.Value), 2);

                //}

                db.SaveChanges();

            }


            return Json(new { Msg = "Created", OrderNo = MaxId, SaleDate = SaleDate }, JsonRequestBehavior.AllowGet);
        }

        public class SaleDetailByQuantity
        {
            public int Id { get; set; }
            public DateTime DateTime { get; set; }
            public int ProductNo { get; set; }
            public int InventoryId { get; set; }
            public string InventoryName { get; set; }
            public double UnitSalePrice { get; set; }
            public int Quantity { get; set; }
            public double Total { get; set; }


        }


        public ActionResult ViewSaleOrderDetails(int id)
        {
            ViewBag.SaleOrderID = id;

            var UserId = User.Identity.GetUserId();

            var UserRole = db.GetUserRoleById(UserId).FirstOrDefault();

            ViewBag.UserRole = UserRole;

            return View();
        }

        public ActionResult GetDiscountsList()
        {
            var SaleDiscountList = db.SaleDiscounts.Where(x => x.IsActive == true).Select(x => new { x.Id, x.Name, x.Percentage, x.Description }).ToList();

            string status = Newtonsoft.Json.JsonConvert.SerializeObject(SaleDiscountList);

            return Content(status);

        }

        public ActionResult GetSaleOrderDetails(int id)
        {
            var SaleOrder = db.SaleOrders.Where(x => x.Id == id).FirstOrDefault();
            var OrderDetails = db.SaleDetails.Where(x => x.SaleOrderId == SaleOrder.Id).ToList();

            SaleOrderCustom SaleOrderVM = new SaleOrderCustom();

           // SaleOrderVM.DateTime = SaleOrder.Date.Value;
            SaleOrderVM.DateTime = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(SaleOrder.Date.Value, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")), TimeZoneInfo.Local);
            //SaleOrder.Date.Value

            SaleOrderVM.Discount = SaleOrder.Discount.Value;
            var DiscountPercentage = SaleOrder.DiscountPercentage;

            if (DiscountPercentage != null)
            {
                DiscountPercentage = SaleOrder.DiscountPercentage.Value;
            }
            else
            {
                DiscountPercentage = Math.Round((SaleOrder.Discount.Value * 100) / SaleOrder.Total.Value, 2);
            }

            if (SaleOrder.SaleDiscountId == null)
            {
                SaleOrderVM.DiscountType = "Bismarck Discount";
            }
            else
            {
                SaleOrderVM.DiscountType = SaleOrder.SaleDiscount.Name;
            }
            SaleOrderVM.Cashier = SaleOrder.Creator == null ? SaleOrder.Creator = "The_Bismarck" : SaleOrder.Creator;
            SaleOrderVM.DiscountPercentage = DiscountPercentage.Value;
            SaleOrderVM.Status = SaleOrder.Status;
            //SaleOrderVM.StudentName = SaleOrder.AspNetStudent.AspNetUser.Name + "(" + SaleOrder.AspNetStudent.AspNetUser.UserName + ")";
            SaleOrderVM.CustomerName = SaleOrder.CustomerName;
            SaleOrderVM.CustomerPhoneNo = SaleOrder.CustomerPhoneNo;
            SaleOrderVM.Total = SaleOrder.Total.Value;
            SaleOrderVM.DiscountedPrice = SaleOrder.DiscountedPrice.Value;
            SaleOrderVM.OrderNo = SaleOrder.OrderNo.Value;
            SaleOrderVM.Id = SaleOrder.Id;

            foreach (var item in OrderDetails)
            {
                SaleOrdersList SaleDetail = new SaleOrdersList();

                SaleDetail.InventoryName = item.Inventory.Name;
                SaleDetail.InventoryId = item.InventoryId.Value;
                SaleDetail.Quantity = item.Quantity.Value;
                SaleDetail.UnitSalePrice = item.UnitSalePrice.Value;
                SaleDetail.Cost = item.TotalPrice.Value;

                SaleOrderVM.SaleOrderList.Add(SaleDetail);
            }

            return Json(SaleOrderVM, JsonRequestBehavior.AllowGet);
        }

        public class SaleOrderCustom
        {
            public int Id { get; set; }
            public int OrderNo { get; set; }
            public string Status { get; set; }
            public DateTime DateTime { get; set; }
            public string StudentName { get; set; }

            public string SaleType { get; set; }
            public string Cashier { get; set; }

            public string CustomerName { get; set; }
            public string CustomerPhoneNo { get; set; }
            public double Total { get; set; }
            public double Discount { get; set; }
            public double DiscountedPrice { get; set; }
            public string Description { get; set; }
            public double DiscountPercentage { get; set; }
            public string DiscountType { get; set; }

            public List<SaleOrdersList> SaleOrderList = new List<SaleOrdersList>();
        }

        public class SaleOrdersList
        {
            public int InventoryId { get; set; }
            public string InventoryName { get; set; }
            public int Quantity { get; set; }
            public double UnitSalePrice { get; set; }
            public double Cost { get; set; }
        }

    }

}