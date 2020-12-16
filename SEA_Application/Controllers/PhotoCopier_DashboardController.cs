using SEA_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SEA_Application.Controllers
{
    public class PhotoCopier_DashboardController : Controller
    {

        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
        int SessionID = Int32.Parse(SessionIDStaticController.GlobalSessionID);


        // GET: PhotoCopier_Dashboard
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Dashboard()
        {

            DateTime? dateTime = GetLocalDateTime.GetLocalDateTimeFunction();

            DateTime? RemoveTimePart = dateTime.Value.Date;

            ViewBag.TotalPhotoCopierPrice = db.PhotoCopierTotal().Where(x => x.PublishDate == RemoveTimePart).Select(x => x.TotalPhotoCopierPrice).Sum();


            return View("BlankPage");
        }
        public ActionResult ApprovedOrders()
        {
            //var result = db.ApprovedOrdersForPhotoCopier().ToList();

            var GrandTotalPhotoCopier = 0.0;
            DateTime? dateTime = GetLocalDateTime.GetLocalDateTimeFunction();
            DateTime? RemoveTimePart = dateTime.Value.Date;

            // var result = db.PhotoCopierTotal().Where(x => x.PublishDate == RemoveTimePart).ToList();

            // var TodayOrdersList = db.AspNetOrders.Where(x => x.ApproveDate == RemoveTimePart).ToList();

            var OrderNotesList = db.AspNetNotesOrders.Where(x => x.AspNetOrder.ApproveDate == RemoveTimePart && x.Status== "Paid").ToList();
            var NotesIdsList = OrderNotesList.Select(x => x.NotesID).Distinct();

            List<PhotoCopierSummary> photoCopierSummaryList = new List<PhotoCopierSummary>();

            foreach (var item in NotesIdsList)
            {
                int QuantityCounter = 0;
                double PhotoCopierUnitPrice = 0.0;
                PhotoCopierSummary photoCopierSummary = new PhotoCopierSummary();
                photoCopierSummary.NotesName = db.AspNetNotes.Where(x => x.Id == item).Select(x => x.Title).FirstOrDefault();


                foreach (var OrderNote in OrderNotesList)
                {

                    if (item == OrderNote.NotesID)
                    {
                        QuantityCounter = QuantityCounter + OrderNote.Quantity.Value;
                        PhotoCopierUnitPrice = OrderNote.PhotoCopierPrice.Value;
                    }
                }

                photoCopierSummary.Quantity = QuantityCounter;
                photoCopierSummary.PhotoCopierUnitPrice = PhotoCopierUnitPrice;
                photoCopierSummary.PhotoCopierTotal = Convert.ToDouble(photoCopierSummary.Quantity * photoCopierSummary.PhotoCopierUnitPrice);

                GrandTotalPhotoCopier = GrandTotalPhotoCopier + photoCopierSummary.PhotoCopierTotal;
                
                photoCopierSummaryList.Add(photoCopierSummary);

            }


           // var ReturnedNotesIdsList = OrderNotesList.Where(x => x.Status == "Returned").Select(x => x.NotesID).Distinct();

            var ReturnedOrderNotesList = db.AspNetNotesOrders.Where(x => x.AspNetOrder.ApproveDate == RemoveTimePart && x.Status == "Returned").ToList();
            var ReturnedNotesIdsList = ReturnedOrderNotesList.Select(x => x.NotesID).Distinct();
            foreach (var item in ReturnedNotesIdsList)
            {
                int QuantityCounter = 0;
                double PhotoCopierUnitPrice = 0.0;
                PhotoCopierSummary photoCopierSummary = new PhotoCopierSummary();
                photoCopierSummary.NotesName = db.AspNetNotes.Where(x => x.Id == item).Select(x => x.Title).FirstOrDefault();


                foreach (var OrderNote in ReturnedOrderNotesList)
                {

                    if (item == OrderNote.NotesID)
                    {
                        QuantityCounter = QuantityCounter + OrderNote.Quantity.Value;
                        PhotoCopierUnitPrice = OrderNote.PhotoCopierPrice.Value;
                    }
                }

                photoCopierSummary.Quantity = QuantityCounter;
                photoCopierSummary.PhotoCopierUnitPrice = PhotoCopierUnitPrice;
                photoCopierSummary.PhotoCopierTotal = Convert.ToDouble(photoCopierSummary.Quantity * photoCopierSummary.PhotoCopierUnitPrice);

                GrandTotalPhotoCopier = GrandTotalPhotoCopier + photoCopierSummary.PhotoCopierTotal;

                photoCopierSummaryList.Add(photoCopierSummary);

            }



            return Json( new { photoCopierSummaryList = photoCopierSummaryList, GrandTotalPhotoCopier = GrandTotalPhotoCopier }, JsonRequestBehavior.AllowGet);

        }

        public class PhotoCopierSummary
        {
            public string NotesName { get; set; }
            public string NotesType { get; set; }
            public int Quantity { get; set; }
            public double PhotoCopierUnitPrice { get; set; }
            public double PhotoCopierTotal { get; set; }

        }

        public ActionResult ApprovedOrdersByDate(string FromDate, string ToDate)
        {
            var GrandTotalPhotoCopier = 0.0;

            DateTime dateTimeFrom = Convert.ToDateTime(FromDate);
            DateTime dateTimeTo = Convert.ToDateTime(ToDate);

            // var result = db.PhotoCopierTotal().Where(x => x.PublishDate >= dateTimeFrom && x.PublishDate <= dateTimeTo).ToList();
            
            var OrderNotesList = db.AspNetNotesOrders.Where(x => x.AspNetOrder.ApproveDate >= dateTimeFrom && x.AspNetOrder.ApproveDate <= dateTimeTo && x.Status=="Paid").ToList();
            var NotesIdsList = OrderNotesList.Select(x => x.NotesID).Distinct();

            List<PhotoCopierSummary> photoCopierSummaryList = new List<PhotoCopierSummary>();

            foreach (var item in NotesIdsList)
            {
                int QuantityCounter = 0;
                double PhotoCopierUnitPrice = 0.0;
                PhotoCopierSummary photoCopierSummary = new PhotoCopierSummary();
                photoCopierSummary.NotesName = db.AspNetNotes.Where(x => x.Id == item).Select(x => x.Title).FirstOrDefault();


                foreach (var OrderNote in OrderNotesList)
                {

                    if (item == OrderNote.NotesID)
                    {
                        QuantityCounter = QuantityCounter + OrderNote.Quantity.Value;
                        PhotoCopierUnitPrice = OrderNote.PhotoCopierPrice.Value;
                    }
                }

                photoCopierSummary.Quantity = QuantityCounter;
                photoCopierSummary.PhotoCopierUnitPrice = PhotoCopierUnitPrice;
                photoCopierSummary.PhotoCopierTotal = Convert.ToDouble(photoCopierSummary.Quantity * photoCopierSummary.PhotoCopierUnitPrice);

                GrandTotalPhotoCopier = GrandTotalPhotoCopier + photoCopierSummary.PhotoCopierTotal;

                photoCopierSummaryList.Add(photoCopierSummary);

            }



            var ReturnedOrderNotesList = db.AspNetNotesOrders.Where(x => x.AspNetOrder.ApproveDate >= dateTimeFrom && x.AspNetOrder.ApproveDate <= dateTimeTo && x.Status == "Returned").ToList();

            var ReturnedNotesIdsList = ReturnedOrderNotesList.Select(x => x.NotesID).Distinct();
            foreach (var item in ReturnedNotesIdsList)
            {
                int QuantityCounter = 0;
                double PhotoCopierUnitPrice = 0.0;
                PhotoCopierSummary photoCopierSummary = new PhotoCopierSummary();
                photoCopierSummary.NotesName = db.AspNetNotes.Where(x => x.Id == item).Select(x => x.Title).FirstOrDefault();


                foreach (var OrderNote in ReturnedOrderNotesList)
                {

                    if (item == OrderNote.NotesID)
                    {
                        QuantityCounter = QuantityCounter + OrderNote.Quantity.Value;
                        PhotoCopierUnitPrice = OrderNote.PhotoCopierPrice.Value;
                    }
                }

                photoCopierSummary.Quantity = QuantityCounter;
                photoCopierSummary.PhotoCopierUnitPrice = PhotoCopierUnitPrice;
                photoCopierSummary.PhotoCopierTotal = Convert.ToDouble(photoCopierSummary.Quantity * photoCopierSummary.PhotoCopierUnitPrice);

                GrandTotalPhotoCopier = GrandTotalPhotoCopier + photoCopierSummary.PhotoCopierTotal;

                photoCopierSummaryList.Add(photoCopierSummary);

            }



            //   return Json(photoCopierSummaryList, JsonRequestBehavior.AllowGet);

            return Json(new { photoCopierSummaryList = photoCopierSummaryList, GrandTotalPhotoCopier = GrandTotalPhotoCopier }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult BlankPage()
        {
            return View();
        }


    }
}