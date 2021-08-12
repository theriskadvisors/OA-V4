using SEA_Application.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace SEA_Application.Controllers
{
    public class InventoryController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: Inventory
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Inventory()
        {
            return View();
        }


        public ActionResult GetRawProducts()
        {

            var AllRawProducts = db.Inventories.Where(x => x.Type == "RawProduct").Select(x => new { x.Id, x.UnitName, x.Name, x.QuantityOnHand, x.UnitPurchasePrice, x.UnitSalePrice, x.Type, x.Description, x.AverageCost, x.TotalCost }).ToList();


            return Json(AllRawProducts, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetExternalProducts()
        {

            var AllExternalProducts = db.Inventories.Where(x => x.Type == "ExternalProduct").Select(x => new { x.Id, x.Name, Category = x.InventoryCategory.Name, x.QuantityOnHand, x.UnitName, x.ItemNo, x.UnitPurchasePrice, x.UnitSalePrice, x.Type, x.Description, x.AverageCost, x.TotalCost }).ToList();

            return Json(AllExternalProducts, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetProductProductions()
        {
            var AllProductProductions = db.Inventories.Where(x => x.Type == "ProductProduction").Select(x => new { x.Id, x.UnitName, x.Name, x.QuantityOnHand, x.UnitPurchasePrice, x.UnitSalePrice, x.Type, x.Description, x.AverageCost, x.TotalCost }).ToList();
            return Json(AllProductProductions, JsonRequestBehavior.AllowGet);

        }
        public ActionResult AddStockPurchase()
        {
            ViewBag.SupplierId = new SelectList(db.Suppliers, "Id", "Name");

            var MaxId = 1000;

            try
            {
                var GetID = db.StockOrders.Select(x => x.Id).Max();

                MaxId = GetID + 1;
            }
            catch (Exception Ex)
            {

            }

            ViewBag.MaxId = MaxId;

            return View();
        }


        public ActionResult AddProductProduction()
        {
            var MaxId = 1000;

            try
            {
                var GetID = db.ProductOrders.Select(x => x.Id).Max();

                MaxId = GetID + 1;
            }
            catch (Exception Ex)
            {

            }

            ViewBag.MaxId = MaxId;

            return View();

        }

        public ActionResult PrintAllInventoryByCategory()
        {

            var ItemsByCategory = db.Inventories.GroupBy(x => x.InventoryCategory.Name).OrderBy(x => x.Key).ToList();

            //foreach (var group in ItemsByCategory)
            //{
            //    var groupKey = group.Key;
            //    foreach (var groupedItem in group)

            //    {
            //        var ItemsName = groupedItem.Name;
            //    }

            //      //  DoSomethingWith(groupKey, groupedItem);
            //}

            return View(ItemsByCategory);

        }


        public ActionResult SaveProductProduction(string Date, int FinishedItem, double UnitCostPrice, double TotalCostOfFinishedItem, int TotalFinishedQuantity, List<ProductProductionList> ProductProductionList)
        {

            int? MaxId = 1000;
            int? GetID = db.ProductOrders.Select(x => x.OrderNo).Max();

            if (GetID != null)

            {
                MaxId = GetID + 1;
            }

            var Datetime = GetLocalDateTime.GetLocalDateTimeFunction();
            var Time = Datetime.Value.TimeOfDay;

            ProductOrder productOrder = new ProductOrder();

            productOrder.OrderDate = Convert.ToDateTime(Date);
            productOrder.OrderDate = productOrder.OrderDate.Value.Add(Time);

            productOrder.InventoryId = FinishedItem;
            productOrder.UnitCostPrice = UnitCostPrice;
            productOrder.TotalCost = TotalCostOfFinishedItem;
            productOrder.OrderQuantity = TotalFinishedQuantity;
            productOrder.CreationDate = GetLocalDateTime.GetLocalDateTimeFunction();
            productOrder.OrderNo = MaxId;

            db.ProductOrders.Add(productOrder);
            db.SaveChanges();


            var ProductionInventoryToUpdae = db.Inventories.Where(x => x.Id == FinishedItem).FirstOrDefault();
            ProductionInventoryToUpdae.QuantityOnHand = ProductionInventoryToUpdae.QuantityOnHand + TotalFinishedQuantity;
            ProductionInventoryToUpdae.UnitPurchasePrice = UnitCostPrice;

            ProductionInventoryToUpdae.TotalCost = ProductionInventoryToUpdae.TotalCost + TotalCostOfFinishedItem;
            ProductionInventoryToUpdae.AverageCost = Math.Round((ProductionInventoryToUpdae.TotalCost.Value / ProductionInventoryToUpdae.QuantityOnHand.Value), 2);

            db.SaveChanges();


            foreach (var item in ProductProductionList)
            {
                ProductProduction productProduction = new ProductProduction();

                productProduction.Cost = item.Cost;
                productProduction.StockQuantity = item.Quantity;
                productProduction.InventoryId = item.InventoryId;
                productProduction.ProductOrderId = productOrder.Id;

                db.ProductProductions.Add(productProduction);
                db.SaveChanges();


                var RawIItemsToUpdate = db.Inventories.Where(x => x.Id == item.InventoryId).FirstOrDefault();
                RawIItemsToUpdate.QuantityOnHand = RawIItemsToUpdate.QuantityOnHand - item.Quantity;


                var TotalCost = productProduction.StockQuantity * RawIItemsToUpdate.AverageCost;

                ////update average cost and total cost 
                RawIItemsToUpdate.TotalCost = RawIItemsToUpdate.TotalCost - TotalCost;
                RawIItemsToUpdate.AverageCost = Math.Round((RawIItemsToUpdate.TotalCost.Value / RawIItemsToUpdate.QuantityOnHand.Value), 2);

                db.SaveChanges();
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }
        public class ProductProductionList
        {
            public int InventoryId { get; set; }
            public string InventoryName { get; set; }
            public double UnitPrice { get; set; }
            public int Quantity { get; set; }
            public double Cost { get; set; }
        }

        public ActionResult SaveNewPurchasedStock(string Date, string Note, List<NewStocksList> NewStocksList)
        {
            try
            {
                int? MaxId = 1000;
                int? GetID = db.StockOrders.Select(x => x.OrderNo).Max();

                if (GetID != null)

                {
                    MaxId = GetID + 1;
                }

                var Datetime = GetLocalDateTime.GetLocalDateTimeFunction();
                var Time = Datetime.Value.TimeOfDay;

                StockOrder stockOrder = new StockOrder();
                stockOrder.Notes = Note;
                stockOrder.PurchaseDate = Convert.ToDateTime(Date);
                stockOrder.PurchaseDate = stockOrder.PurchaseDate.Value.Add(Time);
                stockOrder.OrderDate = GetLocalDateTime.GetLocalDateTimeFunction();
                // stockOrder.Id = OrderNo;
                stockOrder.OrderNo = MaxId;

                stockOrder.Status = "Paid";

                db.StockOrders.Add(stockOrder);
                db.SaveChanges();

                foreach (var item in NewStocksList)
                {
                    StockPurchase stockPurchase = new StockPurchase();


                    stockPurchase.InventoryId = item.InventoryId;
                    stockPurchase.PurchasePrice = 0;
                    stockPurchase.VendorId = item.VendorId;
                    stockPurchase.Quantity = item.Quantity;
                    stockPurchase.StockOrderId = stockOrder.Id;


                    db.StockPurchases.Add(stockPurchase);
                    db.SaveChanges();


                    var InventoryToUpdate = db.Inventories.Where(x => x.Id == stockPurchase.InventoryId).FirstOrDefault();

                    InventoryToUpdate.UnitPurchasePrice = stockPurchase.PurchasePrice;

                    if (InventoryToUpdate.QuantityOnHand == null)
                    {
                        InventoryToUpdate.QuantityOnHand = 0;
                    }
                    InventoryToUpdate.QuantityOnHand = InventoryToUpdate.QuantityOnHand + stockPurchase.Quantity;

                    // var TotalCost = stockPurchase.Quantity * stockPurchase.PurchasePrice;

                    //update average cost and total cost 
                    // InventoryToUpdate.TotalCost = InventoryToUpdate.TotalCost + TotalCost;
                    //  InventoryToUpdate.AverageCost = Math.Round((InventoryToUpdate.TotalCost.Value / InventoryToUpdate.QuantityOnHand.Value), 2);

                    db.SaveChanges();


                }
            }
            catch (Exception Ex)
            {
                var msg = Ex.Message;

                return Json("Exception", JsonRequestBehavior.AllowGet);


            }

            return Json("Created", JsonRequestBehavior.AllowGet);
        }


        public ActionResult SaveReturnPurchaseOrder(string Date, string Note, List<NewStocksList> NewStocksList)
        {
            try
            {
                int? MaxId = 1000;
                int? GetID = db.StockOrders.Select(x => x.OrderNo).Max();

                if (GetID != null)

                {
                    MaxId = GetID + 1;
                }

                var Datetime = GetLocalDateTime.GetLocalDateTimeFunction();
                var Time = Datetime.Value.TimeOfDay;

                StockOrder stockOrder = new StockOrder();
                stockOrder.Notes = Note;
                stockOrder.PurchaseDate = Convert.ToDateTime(Date);
                stockOrder.PurchaseDate = stockOrder.PurchaseDate.Value.Add(Time);
                stockOrder.OrderDate = GetLocalDateTime.GetLocalDateTimeFunction();
                stockOrder.Status = "Returned";
                stockOrder.OrderNo = MaxId;

                db.StockOrders.Add(stockOrder);
                db.SaveChanges();

                foreach (var item in NewStocksList)
                {
                    StockPurchase stockPurchase = new StockPurchase();


                    stockPurchase.InventoryId = item.InventoryId;
                    stockPurchase.PurchasePrice = item.UnitPrice;
                    stockPurchase.VendorId = item.VendorId;
                    stockPurchase.Quantity = -item.Quantity;
                    stockPurchase.StockOrderId = stockOrder.Id;



                    db.StockPurchases.Add(stockPurchase);
                    db.SaveChanges();


                    var InventoryToUpdate = db.Inventories.Where(x => x.Id == stockPurchase.InventoryId).FirstOrDefault();

                    // InventoryToUpdate.UnitPurchasePrice = stockPurchase.PurchasePrice;

                    if (InventoryToUpdate.QuantityOnHand == null)
                    {
                        InventoryToUpdate.QuantityOnHand = 0;
                    }
                    InventoryToUpdate.QuantityOnHand = InventoryToUpdate.QuantityOnHand + stockPurchase.Quantity;

                    // var TotalCost = stockPurchase.Quantity * stockPurchase.PurchasePrice;

                    //update average cost and total cost 
                    // InventoryToUpdate.TotalCost = InventoryToUpdate.TotalCost + TotalCost;

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
            }
            catch (Exception Ex)
            {
                var msg = Ex.Message;
                return Json("Exception", JsonRequestBehavior.AllowGet);

            }

            return Json("Returned", JsonRequestBehavior.AllowGet);
        }

        public ActionResult PurchasedOrderSummary()
        {


            return View();
        }

        public ActionResult GetPurchasedSummary(string FromDate, string ToDate)
        {

            DateTime dateTimeFrom = Convert.ToDateTime(FromDate);


            DateTime dateTimeTo = Convert.ToDateTime(ToDate);

            dateTimeTo = dateTimeTo.AddDays(1);
            string toDateInString = dateTimeTo.ToString();


            List<PurchaseSummary> PurchaseListPaid = new List<PurchaseSummary>();
            List<PurchaseSummary> PurchaseListReturned = new List<PurchaseSummary>();
            List<StockPurchase> StockPurchaseList = db.StockPurchases.Where(x => x.StockOrder.PurchaseDate >= dateTimeFrom && x.StockOrder.PurchaseDate <= dateTimeTo).ToList();



            var AllPaidPurchaedList = StockPurchaseList.Where(x => x.StockOrder.Status == "Paid").ToList();

            var AllReturnedPurchaedList = StockPurchaseList.Where(x => x.StockOrder.Status == "Returned").ToList();


            var PaidIds = AllPaidPurchaedList.Select(x => x.InventoryId).Distinct();
            var ReturnedIds = AllReturnedPurchaedList.Select(x => x.InventoryId).Distinct();


            foreach (var item in PaidIds)
            {
                PurchaseSummary obj = new PurchaseSummary();

                var inventory = db.Inventories.Where(x => x.Id == item).FirstOrDefault();


                obj.ProductNo = inventory.ItemNo.Value;
                obj.ProductName = inventory.Name;
                obj.Status = "Paid";
                //  obj.DateTime = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(item.StockOrder.PurchaseDate.Value, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")), TimeZoneInfo.Local);

                obj.Quantity = AllPaidPurchaedList.Where(x => x.InventoryId == item).Sum(x => x.Quantity.Value);


                PurchaseListPaid.Add(obj);

            }



            foreach (var item in ReturnedIds)
            {
                PurchaseSummary obj = new PurchaseSummary();

                var inventory = db.Inventories.Where(x => x.Id == item).FirstOrDefault();


                obj.ProductNo = inventory.ItemNo.Value;
                obj.ProductName = inventory.Name;
                obj.Status = "Returned";
                //  obj.DateTime = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(item.StockOrder.PurchaseDate.Value, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")), TimeZoneInfo.Local);

                obj.Quantity = AllReturnedPurchaedList.Where(x => x.InventoryId == item).Sum(x => x.Quantity.Value);


                PurchaseListReturned.Add(obj);

            }


            List<PurchaseSummary> PaidAndReturned = new List<PurchaseSummary>();
            PaidAndReturned.AddRange(PurchaseListPaid);
            PaidAndReturned.AddRange(PurchaseListReturned);


            var Summary = PaidAndReturned.OrderBy(x => x.ProductNo).ThenBy(x => x.Status);


            return Json(Summary, JsonRequestBehavior.AllowGet);
        }

        public class PurchaseSummary
        {
            public int OrderId { get; set; }
            public int ProductNo { get; set; }
            public string ProductName { get; set; }
            public string Status { get; set; }
            public int Quantity { get; set; }
            public DateTime DateTime { get; set; }


        }




        public ActionResult GetAllSuppliers()
        {

            var AllSuppliersList = db.Suppliers.Select(x => new { x.Id, x.Name }).ToList();

            string status = Newtonsoft.Json.JsonConvert.SerializeObject(AllSuppliersList);

            // return Json(SubjectsByClass, JsonRequestBehavior.AllowGet);
            return Content(status);

        }
        public ActionResult GetAllInventoryItems()
        {

            var InventoryProducts = db.Inventories.Where(x => x.Type != "ProductProduction").Select(x => new { x.Id, Name = x.Name + "  (" + x.ItemNo + ")", x.QuantityOnHand, x.TotalCost, x.AverageCost }).ToList();

            string status = Newtonsoft.Json.JsonConvert.SerializeObject(InventoryProducts);

            return Content(status);


        }

        public ActionResult RawProductsList()
        {
            var AllRawProductsList = db.Inventories.Where(x => x.Type == "RawProduct").Select(x => new { x.Id, x.Name, x.UnitPurchasePrice, x.QuantityOnHand, x.AverageCost }).ToList();
            string status = Newtonsoft.Json.JsonConvert.SerializeObject(AllRawProductsList);
            return Content(status);

        }
        public ActionResult GetFinishedProductsList()
        {
            var AllRawProductsList = db.Inventories.Where(x => x.Type == "ProductProduction").Select(x => new { x.Id, x.Name }).ToList();

            string status = Newtonsoft.Json.JsonConvert.SerializeObject(AllRawProductsList);
            return Content(status);

        }
        public ActionResult PurchaseOrdersList()
        {

            return View();
        }

        public ActionResult ViewStockDeatils()
        {
            return View();
        }

        public ActionResult StockOrderList()
        {
            var StockOrdersList = db.StockOrders.Select(x => new { x.Id, PurchaseDate = x.PurchaseDate, x.Notes, x.OrderNo, x.Status }).ToList();

            List<StockOrderVM> StockOrderListForView = new List<StockOrderVM>();

            foreach (var item in StockOrdersList)
            {
                StockOrderVM obj = new StockOrderVM();

                obj.Id = item.Id;
                obj.PurchaseDate = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(item.PurchaseDate.Value, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")), TimeZoneInfo.Local);
                obj.OrderNo = item.OrderNo.Value;
                obj.Notes = item.Notes;
                obj.Status = item.Status;

                StockOrderListForView.Add(obj);
            }

            return Json(StockOrderListForView, JsonRequestBehavior.AllowGet);
        }
        public class StockOrderVM
        {
            public int Id { get; set; }
            public DateTime PurchaseDate { get; set; }

            public string Notes { set; get; }
            public int OrderNo { set; get; }
            public string Status { set; get; }

        }

        public ActionResult ListProductionOrders()
        {

            return View();
        }

        public ActionResult ViewProductionDetails()
        {

            return View();
        }

        public ActionResult ProductionOrdersList()
        {
            var ProductOrderList = db.ProductOrders.Select(x => new { x.Id, x.Inventory.Name, x.OrderQuantity, x.UnitCostPrice, x.TotalCost, x.OrderDate, x.OrderNo }).ToList();

            return Json(ProductOrderList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult InventoryProductDetails(int InventoryId)
        {
            var InventoryItem = db.Inventories.Where(x => x.Id == InventoryId).FirstOrDefault();

            ViewBag.InventoryName = InventoryItem.Name;
            ViewBag.InventoryId = InventoryId;


            return View();
        }
        public ActionResult InventoryProductionDetails(int InventoryId)
        {
            var InventoryItem = db.Inventories.Where(x => x.Id == InventoryId).FirstOrDefault();

            ViewBag.InventoryName = InventoryItem.Name;
            ViewBag.InventoryId = InventoryId;


            return View();

        }
        public ActionResult InventoryProductionDetailsList(int InventoryId)
        {
            var ProductOrderList = db.ProductOrders.Where(x => x.InventoryId == InventoryId).Select(x => new { x.Id, x.OrderQuantity, x.UnitCostPrice, x.TotalCost, x.CreationDate, x.OrderDate, x.Status, InventoryName = x.Inventory.Name, TranscationName = "Production Order" }).ToList();
            var AllSaleOrders = db.SaleDetails.Where(x => x.InventoryId == InventoryId).Select(x => new { x.SaleOrder.Id, x.Quantity, x.SaleOrder.Date, Transcataions = "Sale Order", InventoryName = x.Inventory.Name, PurchasePrice = 0, x.Inventory.AverageCost, StudentName = x.SaleOrder.AspNetStudent.AspNetUser.Name }).ToList();

            List<InventoryDetails> InventoryDetailList = new List<InventoryDetails>();
            foreach (var Item in ProductOrderList)
            {
                InventoryDetails inventoryDetails = new InventoryDetails();
                inventoryDetails.Id = Item.Id;
                inventoryDetails.CreationDate = Item.OrderDate.Value;
                inventoryDetails.InventoryName = Item.InventoryName;
                inventoryDetails.PurchasePrice = Item.UnitCostPrice.Value;
                inventoryDetails.QuantityOnHand = Item.OrderQuantity.Value;
                inventoryDetails.StudentName = "";
                inventoryDetails.TranscationName = Item.TranscationName;
                // inventoryDetails.TotalCost = Item.TotalCost.Value;
                inventoryDetails.Balance = 0;

                InventoryDetailList.Add(inventoryDetails);
            }


            foreach (var Item in AllSaleOrders)
            {
                InventoryDetails inventoryDetails = new InventoryDetails();
                inventoryDetails.Id = Item.Id;
                inventoryDetails.CreationDate = Item.Date.Value;
                inventoryDetails.InventoryName = Item.InventoryName;
                inventoryDetails.PurchasePrice = 0;
                inventoryDetails.QuantityOnHand = -Item.Quantity.Value;
                inventoryDetails.StudentName = Item.StudentName;
                inventoryDetails.TranscationName = Item.Transcataions;
                inventoryDetails.Balance = 0;

                // inventoryDetails.TotalAmountBalance = inventoryDetails.QuantityOnHand * Item.AverageCost.Value;
                //inventoryDetails.AverageCost = inventoryDetails.TotalAmountBalance / Item.Quantity.Value;

                InventoryDetailList.Add(inventoryDetails);

            }

            var InventoryDetails = InventoryDetailList.OrderBy(x => x.CreationDate).ToList();

            var RememberQuantity = 0;
            var RememberTotalCost = 0.0;
            int count = 1;

            foreach (var item in InventoryDetails)
            {
                item.Order = count;

                if (item.TranscationName == "Production Order")
                {
                    item.Balance = item.QuantityOnHand + RememberQuantity;

                    RememberQuantity = item.Balance;

                    item.AverageCost = item.PurchasePrice;
                    item.Amount = item.PurchasePrice * item.QuantityOnHand;

                    RememberTotalCost = RememberTotalCost + item.Amount;

                    item.TotalAmountBalance = RememberTotalCost;

                }
                else
                {
                    item.AverageCost = Math.Round((RememberTotalCost / RememberQuantity), 2);

                    item.Balance = item.QuantityOnHand + RememberQuantity; // In case Quantity in minus(-)
                    RememberQuantity = item.Balance;

                    item.Amount = item.AverageCost * item.QuantityOnHand;

                    RememberTotalCost = RememberTotalCost + item.Amount;
                    item.TotalAmountBalance = RememberTotalCost;

                    //item.Balance = item.QuantityOnHand + RememberQuantity;

                    //  RememberQuantity = item.Balance;

                }
                count++;
            }


            return Json(InventoryDetails, JsonRequestBehavior.AllowGet);

        }

        public ActionResult InventoryProductDetailsList(int InventoryId)
        {
            var AllStockPaidOrder = db.StockPurchases.Where(x => x.InventoryId == InventoryId && x.StockOrder.Status == "Paid").Select(x => new { x.StockOrder.Id, x.Quantity, x.StockOrder.OrderDate, SupplierName = x.Supplier.Name, Transcataions = "Purchase Order (Paid)", x.StockOrder.PurchaseDate, InventoryName = x.Inventory.Name, PurchasePrice = x.PurchasePrice, x.Inventory.AverageCost }).ToList();
            //var AllStockReturnedOrder = db.StockPurchases.Where(x => x.InventoryId == InventoryId && x.StockOrder.Status == "Returned").Select(x => new { x.StockOrder.Id, x.Quantity, x.StockOrder.OrderDate, SupplierName = x.Supplier.Name, Transcataions = "Purchase Order (Returned)", x.StockOrder.PurchaseDate, InventoryName = x.Inventory.Name, PurchasePrice = x.PurchasePrice, x.Inventory.AverageCost }).ToList();
            var AllStockReturnedOrder = db.StockPurchases.Where(x => x.InventoryId == InventoryId && x.StockOrder.Status == "Returned").Select(x => new { x.StockOrder.Id, x.Quantity, x.StockOrder.OrderDate, SupplierName = x.Supplier.Name, Transcataions = "Purchase Order (Returned)", x.StockOrder.PurchaseDate, InventoryName = x.Inventory.Name, PurchasePrice = x.PurchasePrice, x.Inventory.AverageCost }).ToList();

            // var AllProductionProduction = db.ProductProductions.Where(x => x.InventoryId == InventoryId).Select(x => new { x.ProductOrder.Id, x.StockQuantity, x.ProductOrder.CreationDate, Transcataions = "Production Order", x.ProductOrder.OrderDate, InventoryName = x.Inventory.Name, PurchasePrice = 0, x.Inventory.AverageCost }).ToList();

            var AllSaleOrdersPaid = db.SaleDetails.Where(x => x.InventoryId == InventoryId && x.SaleOrder.Status == "Paid").Select(x => new { x.SaleOrder.Id, x.Quantity, x.SaleOrder.Date, Transcataions = "Sale Order (Paid)", InventoryName = x.Inventory.Name, PurchasePrice = 0, x.Inventory.AverageCost }).ToList();

            //  var AllSaleOrdersReturn = db.SaleDetails.Where(x => x.InventoryId == InventoryId && x.SaleOrder.Status == "Returned").Select(x => new { x.SaleOrder.Id, x.Quantity, x.SaleOrder.Date, Transcataions = "Sale Order (Returned)", InventoryName = x.Inventory.Name, PurchasePrice = 0, x.Inventory.AverageCost }).ToList();

            var AllSaleOrdersReturned = db.SaleDetails.Where(x => x.InventoryId == InventoryId && x.SaleOrder.Status == "Returned").Select(x => new { x.SaleOrder.Id, x.Quantity, x.SaleOrder.Date, Transcataions = "Sale Order (Returned)", InventoryName = x.Inventory.Name, PurchasePrice = 0, x.Inventory.AverageCost }).ToList();

            List<InventoryDetails> InventoryDetailList = new List<InventoryDetails>();

            //var RememberQuantity = 0.0;

            foreach (var Item in AllStockPaidOrder)
            {
                InventoryDetails inventoryDetails = new InventoryDetails();
                inventoryDetails.Id = Item.Id;
                inventoryDetails.CreationDate = Item.PurchaseDate.Value;
                inventoryDetails.InventoryName = Item.InventoryName;
                inventoryDetails.PurchasePrice = Item.PurchasePrice.Value;
                inventoryDetails.QuantityOnHand = Item.Quantity.Value;
                inventoryDetails.SupplierName = Item.SupplierName;
                inventoryDetails.TranscationName = Item.Transcataions;

                inventoryDetails.Balance = 0;

                //  inventoryDetails.AverageCost = inventoryDetails.UnitCost;
                //inventoryDetails.Amount = inventoryDetails.UnitCost * inventoryDetails.QuantityOnHand;
                // inventoryDetails.TotalAmountBalance = inventoryDetails.QuantityOnHand * Item.AverageCost.Value;


                InventoryDetailList.Add(inventoryDetails);
            }


            foreach (var Item in AllStockReturnedOrder)
            {
                InventoryDetails inventoryDetails = new InventoryDetails();
                inventoryDetails.Id = Item.Id;
                inventoryDetails.CreationDate = Item.PurchaseDate.Value;
                inventoryDetails.InventoryName = Item.InventoryName;
                inventoryDetails.PurchasePrice = Item.PurchasePrice.Value;
                inventoryDetails.QuantityOnHand = Item.Quantity.Value;
                inventoryDetails.SupplierName = Item.SupplierName;
                inventoryDetails.TranscationName = Item.Transcataions;

                inventoryDetails.Balance = 0;

                //  inventoryDetails.AverageCost = inventoryDetails.UnitCost;
                //inventoryDetails.Amount = inventoryDetails.UnitCost * inventoryDetails.QuantityOnHand;
                // inventoryDetails.TotalAmountBalance = inventoryDetails.QuantityOnHand * Item.AverageCost.Value;


                InventoryDetailList.Add(inventoryDetails);
            }

            foreach (var Item in AllSaleOrdersPaid)
            {

                InventoryDetails inventoryDetails = new InventoryDetails();
                inventoryDetails.Id = Item.Id;
                inventoryDetails.CreationDate = Item.Date.Value;
                inventoryDetails.InventoryName = Item.InventoryName;
                inventoryDetails.PurchasePrice = 0;
                inventoryDetails.QuantityOnHand = -Item.Quantity.Value;
                inventoryDetails.SupplierName = "";
                inventoryDetails.TranscationName = Item.Transcataions;
                inventoryDetails.Balance = 0;

                // inventoryDetails.TotalAmountBalance = inventoryDetails.QuantityOnHand * Item.AverageCost.Value;
                //inventoryDetails.AverageCost = inventoryDetails.TotalAmountBalance / Item.Quantity.Value;

                InventoryDetailList.Add(inventoryDetails);

            }



            foreach (var Item in AllSaleOrdersReturned)
            {

                InventoryDetails inventoryDetails = new InventoryDetails();
                inventoryDetails.Id = Item.Id;
                inventoryDetails.CreationDate = Item.Date.Value;
                inventoryDetails.InventoryName = Item.InventoryName;
                inventoryDetails.PurchasePrice = 0;
                inventoryDetails.QuantityOnHand = Item.Quantity.Value;
                inventoryDetails.SupplierName = "";
                inventoryDetails.TranscationName = Item.Transcataions;
                inventoryDetails.Balance = 0;

                // inventoryDetails.TotalAmountBalance = inventoryDetails.QuantityOnHand * Item.AverageCost.Value;
                //inventoryDetails.AverageCost = inventoryDetails.TotalAmountBalance / Item.Quantity.Value;

                InventoryDetailList.Add(inventoryDetails);

            }

            var InventoryDetails = InventoryDetailList.OrderBy(x => x.CreationDate).ToList();

            var RememberQuantity = 0;
            //var RememberTotalCost = 0.0;

            int count = 1;

            foreach (var item in InventoryDetails)
            {
                item.Order = count;

                if (item.TranscationName == "Purchase Order (Paid)")
                {
                    item.Balance = item.QuantityOnHand + RememberQuantity;
                    RememberQuantity = item.Balance;

                    // item.AverageCost = item.PurchasePrice;
                    // item.Amount = item.PurchasePrice * item.QuantityOnHand;

                    // RememberTotalCost = RememberTotalCost + item.Amount;

                    // item.TotalAmountBalance = RememberTotalCost;
                }



                else  //(item.TranscationName == "Sale Order") //Paid or Returned Both
                {
                    //item.AverageCost = Math.Round((RememberTotalCost / RememberQuantity), 2);

                    item.Balance = item.QuantityOnHand + RememberQuantity; // Both Quantity in minus(-) and +
                    RememberQuantity = item.Balance;

                    // item.Amount = item.AverageCost * item.QuantityOnHand;

                    // RememberTotalCost = RememberTotalCost + item.Amount;
                    // item.TotalAmountBalance = RememberTotalCost;


                }


                count++;
            }

            return Json(InventoryDetails, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PurchaseOrderDetails(int id)
        {
            ViewBag.PurchaseOrderId = id;

            return View();
        }

        public ActionResult EditStockPurchase(int id)
        {

            ViewBag.StockOrderId = id;



            return View();
        }
        public ActionResult ListStockOrderForEdit(int StockOrderId)
        {

            var StockOrder = db.StockOrders.Where(x => x.Id == StockOrderId).FirstOrDefault();

            var Description = StockOrder.Notes;
            var PurchaseDate = StockOrder.PurchaseDate;

            var StockPurchaseList = db.StockPurchases.Where(x => x.StockOrderId == StockOrderId).ToList();

            List<NewStocksList> StockList = new List<NewStocksList>();


            foreach (var item in StockPurchaseList)
            {
                NewStocksList stock = new NewStocksList();

                stock.InventoryId = item.InventoryId;
                stock.VendorId = item.VendorId;
                stock.Quantity = item.Quantity.Value;
                StockList.Add(stock);

            }

            return Json(new { Description = Description, PurchaseDate = PurchaseDate, StockList = StockList }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UpdateStockPurchase()
        {


            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetPurchasedOrderDetails(int id)
        {
            var PurchasedStockList = db.StockPurchases.Where(x => x.StockOrderId == id).ToList();

            List<NewStocksList> NewStockList = new List<NewStocksList>();

            foreach (var item in PurchasedStockList)
            {
                NewStocksList obj = new NewStocksList();

                obj.InventoryId = item.InventoryId;
                obj.ProductName = item.Inventory.Name;
                obj.Quantity = item.Quantity.Value;
                obj.UnitPrice = item.PurchasePrice.Value;
                obj.VendorName = item.Supplier.Name;

                var Total = item.PurchasePrice.Value * item.Quantity.Value;
                obj.Total = Total;

                NewStockList.Add(obj);
            }

            var GrandTotal = NewStockList.Sum(x => x.Total);

            return Json(new { GrandTotal = GrandTotal, StockList = NewStockList }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProductionOrderDetails(int id)
        {
            ViewBag.ProductionOrderId = id;

            return View();
        }
        public ActionResult ReturnPurchasedOrder()
        {

            return View();
        }
        public ActionResult GetProductionOrderDetails(int id)
        {
            List<ProductProductionList> ProductProductionList = new List<ProductProductionList>();

            var ProductProduction = db.ProductProductions.Where(x => x.ProductOrderId == id).ToList();

            foreach (var item in ProductProduction)
            {
                ProductProductionList obj = new ProductProductionList();

                obj.InventoryId = item.InventoryId.Value;
                obj.InventoryName = item.Inventory.Name;
                obj.UnitPrice = item.Inventory.AverageCost.Value;
                obj.Quantity = item.StockQuantity.Value;
                obj.Cost = item.Cost.Value;

                ProductProductionList.Add(obj);
            }

            var GrandTotal = 0.0;
            GrandTotal = ProductProductionList.Sum(x => x.Cost);

            return Json(new { GrandTotal = GrandTotal, ProductionStock = ProductProductionList }, JsonRequestBehavior.AllowGet);
        }


        public class InventoryDetails
        {
            public int Order { get; set; }
            public int Id { get; set; }
            public DateTime Date { get; set; }
            public string InventoryName { get; set; }
            public double PurchasePrice { get; set; }
            public DateTime CreationDate { get; set; }
            public string TranscationName { get; set; }
            public string SupplierName { get; set; }
            public int QuantityOnHand { get; set; }
            public int Balance { get; set; }
            public string StudentName { get; set; }
            public double UnitCost { get; set; }
            public double TotalCost { get; set; }

            public double AverageCost { get; set; }
            public double Amount { get; set; }

            public double TotalAmountBalance { get; set; }
        }


        public class NewStocksList
        {
            public int InventoryId { get; set; }
            public double UnitPrice { get; set; }
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public int VendorId { get; set; }
            public double Total { get; set; }

            public string VendorName { get; set; }
        }

        [HttpGet]
        public ActionResult Create(string InventoryType)
        {

            ViewBag.InventoryType = InventoryType;


            int? MaxId = 1;
            int? GetID = db.Inventories.Select(x => x.ItemNo).Max();

            if (GetID != null)

            {
                MaxId = GetID + 1;
            }

            ViewBag.ItemNo = MaxId;


            return View();
        }
        [HttpPost]
        public ActionResult Create(Inventory Inventory)
        {
            var ItemNoExist = db.Inventories.Where(x => x.ItemNo == Inventory.ItemNo).FirstOrDefault();

            if (ItemNoExist == null)
            {
                Inventory.CreationDate = GetLocalDateTime.GetLocalDateTimeFunction();
                Inventory.AverageCost = 0;
                Inventory.TotalCost = 0;

                db.Inventories.Add(Inventory);
                db.SaveChanges();
                return RedirectToAction("Inventory");

            }
            ViewBag.InventoryType = "ExternalProduct";
            ViewBag.ItemNo = Inventory.ItemNo;
            ViewBag.Error = "Product No Already Exist.";

            return View(Inventory);

        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            var InventoryToEdit = db.Inventories.Where(x => x.Id == id).FirstOrDefault();

            return View(InventoryToEdit);
        }

        [HttpPost]
        public ActionResult Edit(Inventory Inventory)
        {

            if (ModelState.IsValid)
            {
                db.Entry(Inventory).State = EntityState.Modified;
                db.Entry(Inventory).Property(x => x.CreationDate).IsModified = false;
                db.Entry(Inventory).Property(x => x.AverageCost).IsModified = false;
                db.Entry(Inventory).Property(x => x.TotalCost).IsModified = false;
                db.Entry(Inventory).Property(x => x.UnitPurchasePrice).IsModified = false;
                db.SaveChanges();

            }

            return RedirectToAction("Inventory", "Inventory");
            //return RedirectToAction("Inventory", new{controller= "Inventory", area=string.Empty});
        }

    }
}