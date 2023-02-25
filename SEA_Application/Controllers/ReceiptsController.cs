using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SEA_Application.Models;

namespace SEA_Application.Controllers
{
    public class ReceiptsController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        public ActionResult Index()
        {

            return View(db.CashReceipts.ToList());
        }
        public ActionResult Create()
        {
         

            var StudentList = (from user in db.AspNetUsers
                               join student in db.AspNetStudents on user.Id equals student.StudentID
                               select new
                               {
                                   UserId = user.Id,
                                   StudentName = user.Name + "(" + user.UserName + ")",

                               }).Distinct().ToList();

            ViewBag.UserId = new SelectList(StudentList, "UserId", "StudentName");

            return View();
        }

        public ActionResult getStudentList()
        {

            var StudentList = (from user in db.AspNetUsers
                               join student in db.AspNetStudents on user.Id equals student.StudentID
                               select new
                               {
                                   UserId = user.Id,
                                   StudentName = user.Name + "(" + user.UserName + ")",

                               }).Distinct().ToList();

            return Json(StudentList, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Create(CashReceipt cashReceipt)
        {
            try
            {
                cashReceipt.CreationDate =  GetLocalDateTime.GetLocalDateTimeFunction();
                db.CashReceipts.Add(cashReceipt);
                db.SaveChanges();
            }
            catch(Exception ex)
            {
                var msg = ex.Message;
            }

        

            return RedirectToAction("Index");
        }

        public ActionResult GetReceiptNo()
        {
            int? MaxId = 1000;


            int? GetID = db.CashReceiptNoes.FirstOrDefault().ReceiptSequenceNo;


            if (GetID != null)

            {
                MaxId = GetID + 1;
            }

            var cashReceiptToUpdate =  db.CashReceiptNoes.FirstOrDefault();
            cashReceiptToUpdate.ReceiptSequenceNo = MaxId;
            db.SaveChanges();

            return Json(MaxId, JsonRequestBehavior.AllowGet);


        }
        public ActionResult PrintReceipt (int id)
        {
            var cashReceipt =  db.CashReceipts.Where(x => x.Id == id).FirstOrDefault();

            return View(cashReceipt);
        }

        public ActionResult Edit(int id )
        {
            var cashReceiptToEdit  =   db.CashReceipts.Where(x => x.Id == id).FirstOrDefault();



            return View(cashReceiptToEdit);
        }
        [HttpPost]
        public ActionResult Edit(CashReceipt cashReceipt)
        {
            var cashReceiptToUpdate =  db.CashReceipts.Where(x => x.Id == cashReceipt.Id).FirstOrDefault();
            cashReceiptToUpdate.UserId = cashReceipt.UserId;
            //cashReceiptToUpdate.SessionId = cashReceipt.SessionId;
            cashReceiptToUpdate.ReceivedFrom = cashReceipt.ReceivedFrom;
            cashReceiptToUpdate.Description = cashReceipt.Description;
            cashReceiptToUpdate.Amount = cashReceipt.Amount;
            cashReceiptToUpdate.Dated = cashReceipt.Dated;
          ///  cashReceiptToUpdate.Course = cashReceipt.Course;
            cashReceiptToUpdate.ReceiptNo = cashReceipt.ReceiptNo;

            db.SaveChanges();

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            return View();
        }



    }
}
