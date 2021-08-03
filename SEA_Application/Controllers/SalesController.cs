using SEA_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

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
        public ActionResult AllSaleOrdersList()
        {
            //TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(x.PurchaseDate.Value, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")), TimeZoneInfo.Local) 
            var SaleOrdersList = db.SaleOrders.Select(x => new { x.Id, x.OrderNo, x.Status, CustomerName = x.CustomerName, /*StudentName = x.AspNetStudent.AspNetUser.Name,*/ x.Discount,Date = x.Date.Value, DiscountedPrice = x.DiscountedPrice, Total = x.Total.Value }).ToList();

            List<SaleOrderCustom> CustomModelList = new List<SaleOrderCustom>();


            foreach(var item in SaleOrdersList)
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


                CustomModelList.Add(obj);
            }




            return Json(CustomModelList, JsonRequestBehavior.AllowGet);
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



            return View();
        }
        public ActionResult SaveReturnSaleOrder(string Date,double GrandTotal, List<SaleOrdersList> SaleOrdersList)
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
            saleorder.Date = Convert.ToDateTime(Date);
            saleorder.Date = saleorder.Date.Value.Add(Time);
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

        public ActionResult GetSaleSummary(string FromDate, string ToDate)
        {

            DateTime dateTimeFrom = Convert.ToDateTime(FromDate);


            DateTime dateTimeTo = Convert.ToDateTime(ToDate);

            dateTimeTo = dateTimeTo.AddDays(1);
            string toDateInString = dateTimeTo.ToString();

            List<SaleOrderCustom> SalesList = new List<SaleOrderCustom>();
            List<SaleOrder> SaleOrdersList= db.SaleOrders.Where(x => x.Date >= dateTimeFrom && x.Date <= dateTimeTo).ToList();

            foreach(var item in SaleOrdersList)
            {
                SaleOrderCustom obj = new SaleOrderCustom();
                obj.Id = item.Id;
                obj.OrderNo = item.OrderNo.Value;
                obj.Status = item.Status;
                obj.CustomerName = item.CustomerName;
                obj.Discount = item.Discount.Value;
                obj.Total = item.Total.Value;
                obj.DiscountedPrice = item.DiscountedPrice.Value;
                obj.DateTime = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(item.Date.Value, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")), TimeZoneInfo.Local);

                SalesList.Add(obj);
            }

            return Json(SalesList, JsonRequestBehavior.AllowGet);
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


            ViewBag.StudentID = new SelectList(StudentList, "StudentId", "StudentName");

            return View();
        }
        public ActionResult GetExternalAndProductionList()
        {
            var AllExternalAndProdutionList = db.Inventories.Where(x => x.Type == "ExternalProduct" || x.Type == "ProductProduction").Select(x => new { x.Id, Name = x.Name + "  (" + x.ItemNo + ")", x.UnitName, x.UnitPurchasePrice, x.UnitSalePrice, x.QuantityOnHand }).ToList();

            string status = Newtonsoft.Json.JsonConvert.SerializeObject(AllExternalAndProdutionList);

            return Content(status);
        }


        [HttpPost]
        public ActionResult Create(int a)
        {





            return View();

        }
        public ActionResult SaveSaleOrders(string Date,  double GrandTotal, double Discount, double DiscountedPrice, List<SaleOrdersList> SaleOrdersList)
        {
            // return Json(new { OrderNo = 1000, data = "Created" }, JsonRequestBehavior.AllowGet);

            int? MaxId = 1000;


            int? GetID = db.SaleOrders.Select(x => x.OrderNo).Max();


            if (GetID != null)

            {
                MaxId = GetID + 1;
            }


            var Datetime = GetLocalDateTime.GetLocalDateTimeFunction();
            var Time = Datetime.Value.TimeOfDay;

            SaleOrder saleorder = new SaleOrder();
            saleorder.Date = Convert.ToDateTime(Date);
            saleorder.Date = saleorder.Date.Value.Add(Time);
            saleorder.CreationDate = GetLocalDateTime.GetLocalDateTimeFunction();
            saleorder.StudentId = null;
            saleorder.CustomerName = null;
            saleorder.Total = GrandTotal;
            saleorder.Discount = Discount;
            saleorder.DiscountedPrice = DiscountedPrice;
            saleorder.OrderNo = MaxId;
            saleorder.Status = "Paid";

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


            return Json(new { Msg = "Created", OrderNo = MaxId }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewSaleOrderDetails(int id)
        {
            ViewBag.SaleOrderID = id;

            return View();
        }

        public ActionResult GetSaleOrderDetails(int id)
        {
            var SaleOrder = db.SaleOrders.Where(x => x.Id == id).FirstOrDefault();
            var OrderDetails = db.SaleDetails.Where(x => x.SaleOrderId == SaleOrder.Id).ToList();

            SaleOrderCustom SaleOrderVM = new SaleOrderCustom();

            SaleOrderVM.DateTime = SaleOrder.Date.Value;
            SaleOrderVM.Discount = SaleOrder.Discount.Value;
            SaleOrderVM.Status = SaleOrder.Status;
            //SaleOrderVM.StudentName = SaleOrder.AspNetStudent.AspNetUser.Name + "(" + SaleOrder.AspNetStudent.AspNetUser.UserName + ")";
            SaleOrderVM.CustomerName = SaleOrder.CustomerName;
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

            public string CustomerName { get; set; }
            public double Total { get; set; }
            public double Discount { get; set; }
            public double DiscountedPrice { get; set; }

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