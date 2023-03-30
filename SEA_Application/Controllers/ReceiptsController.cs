using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.Office.Interop.Excel;
using SEA_Application.Models;
using static SEA_Application.Controllers.TermAssessmentControllers.TermAssessmentController;

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
                cashReceipt.CreationDate = GetLocalDateTime.GetLocalDateTimeFunction();
                db.CashReceipts.Add(cashReceipt);
                db.SaveChanges();
            }
            catch (Exception ex)
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

            var cashReceiptToUpdate = db.CashReceiptNoes.FirstOrDefault();
            cashReceiptToUpdate.ReceiptSequenceNo = MaxId;
            db.SaveChanges();

            return Json(MaxId, JsonRequestBehavior.AllowGet);


        }
        public ActionResult PrintReceipt(int id)
        {
            var cashReceipt = db.CashReceipts.Where(x => x.Id == id).FirstOrDefault();

            return View(cashReceipt);
        }
        public ActionResult PrintFeeSummary(string id)
        {
            var cashReceiptsList = db.CashReceipts.Where(x => x.UserId == id).ToList();

            var Student = db.AspNetStudents.Where(x => x.StudentID == id).FirstOrDefault();
            if (Student != null)
            {

                ViewBag.StudentClass = Student.AspNetClass.ClassName;
                ViewBag.StudentName = Student.AspNetUser.Name;
                ViewBag.RollNo = Student.AspNetUser.UserName;
          
            } else
            {
                ViewBag.StudentClass = null;
                ViewBag.StudentName = null;
                ViewBag.RollNo = null;
            }

            if (Student != null)
            {

            var StudentFeeDetails = db.StudentFeeMonths.Where(x => x.StudentId == Student.Id).OrderBy(x => x.Id).FirstOrDefault();

            ViewBag.FeeType = null;
            ViewBag.TotalFee = null;

            if (StudentFeeDetails != null)
            {
                ViewBag.FeeType = StudentFeeDetails.FeeType;
                ViewBag.TotalFee = StudentFeeDetails.FeePayable;
            }

            }
            else
            {
                ViewBag.FeeType = null;
                ViewBag.TotalFee = null;

            }



            return View(cashReceiptsList);


        }

        public ActionResult Edit(int id)
        {
            var cashReceiptToEdit = db.CashReceipts.Where(x => x.Id == id).FirstOrDefault();



            return View(cashReceiptToEdit);
        }
        [HttpPost]
        public ActionResult Edit(CashReceipt cashReceipt)
        {
            var cashReceiptToUpdate = db.CashReceipts.Where(x => x.Id == cashReceipt.Id).FirstOrDefault();
         //   cashReceiptToUpdate.UserId = cashReceipt.UserId;
            //cashReceiptToUpdate.SessionId = cashReceipt.SessionId;
            cashReceiptToUpdate.ReceivedFrom = cashReceipt.ReceivedFrom;
            cashReceiptToUpdate.Description = cashReceipt.Description;
            cashReceiptToUpdate.Amount = cashReceipt.Amount;
            cashReceiptToUpdate.Dated = cashReceipt.Dated;
            cashReceiptToUpdate.Discount =  cashReceipt.Discount;
            cashReceiptToUpdate.DueDate = cashReceipt.DueDate;
            ///  cashReceiptToUpdate.Course = cashReceipt.Course;
            cashReceiptToUpdate.ReceiptNo = cashReceipt.ReceiptNo;
            

            db.SaveChanges();

            return RedirectToAction("Index");
        }
        public ActionResult GetStudentFeeDetails(string StudentId)
        {
            double TotalFeeAmount = 0;
            var TotalFee = "";
            double? TotalFeeCollected = 0;
            double? TotalDiscount = 0;
            var Student = db.AspNetStudents.Where(x => x.StudentID == StudentId).FirstOrDefault();

            if (Student != null)
            {
                var StudentFeeDetails = db.StudentFeeMonths.Where(x => x.StudentId == Student.Id).OrderBy(x => x.Id).FirstOrDefault();

                TotalFee = StudentFeeDetails.FeePayable + " - " + StudentFeeDetails.FeeType;
                TotalFeeAmount = StudentFeeDetails.FeePayable.Value;
            }


            var StudentCashReceipt = db.CashReceipts.Where(x => x.UserId == StudentId).ToList();

            if (StudentCashReceipt.Count() > 0)
            {
                //double TotalDiscount = 0;
                TotalFeeCollected = StudentCashReceipt.Sum(x => x.Amount);
                TotalDiscount = StudentCashReceipt.Sum(x => x.Discount);

                if (TotalFeeCollected == null)
                {
                    TotalFeeCollected = 0;
                }
                if (TotalDiscount == null)
                {
                    TotalDiscount = 0;
                }

               // TotalDiscount = StudentCashReceipt.Sum(x => x.Discount.Value);
               //  TotalFeeCollected = TotalFeeCollected - TotalDiscount;

            }

            return Json(new { TotalFee = TotalFee, TotalFeeCollected = TotalFeeCollected, TotalFeeAmount = TotalFeeAmount, TotalDiscount = TotalDiscount }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult FeeSummary()
        {
            //var list = (from fee in db.StudentFeeMonths
            //            join std in db.AspNetStudents on fee.StudentId equals std.Id
            //            join user in db.AspNetUsers on std.StudentID equals user.Id


            var list = (
                        from std in db.AspNetStudents
                        join user in db.AspNetUsers on std.StudentID equals user.Id

                        select new
                        {
                            Id = user.Id,
                            RollNo = user.UserName,
                            Name = user.Name,
                            Session = std.AspNetClass.ClassName,
                            Invoices = user.CashReceipts.Count(),
                            TotalFee = std.StudentFeeMonths.FirstOrDefault().FeePayable,
                            FeeReceived = user.CashReceipts.Sum(x => x.Amount),
                            TotalDiscount = user.CashReceipts.Sum(x => x.Discount)

                        }).Distinct().ToList();

            //db.AspNetStudents.Where(x=>x.AspNetUser)


            List<StudentFeeSummary> feeSummaryList = new List<StudentFeeSummary>();

            foreach (var item in list)
            {
                StudentFeeSummary obj = new StudentFeeSummary();
                obj.Id = item.Id;
                obj.RollNo = item.RollNo;
                obj.Name = item.Name;
                obj.Session = item.Session;
                obj.Invoices = item.Invoices;
                obj.TotalFee = item.TotalFee;
                obj.FeeReceived = item.FeeReceived;
                obj.TotalDiscount = item.TotalDiscount;

                feeSummaryList.Add(obj);
            }

            return View(feeSummaryList);


        }
        public ActionResult AllDefaulterStudents()
        {
            var TodayDate = DateTime.Now.Date;

            //var CashReceipts = db.CashReceipts.Where(x => x.UserId != null && x.DueDate > TodayDate).Select(x => new
            //{

            //    Id = x.AspNetUser.Id,
            //    RollNo = x.AspNetUser.UserName,
            //    Name = x.AspNetUser.Name,
            //    DueDate = x.DueDate,

            //  //  TotalFee = std.StudentFeeMonths.FirstOrDefault().FeePayable,
            //    //FeeReceived = user.CashReceipts.Sum(x => x.Amount),
            //    //TotalDiscount = user.CashReceipts.Sum(x => x.Discount)

            //}).ToList();

            var CashReceipts = db.CashReceipts.Where(x => x.UserId != null && TodayDate > x.DueDate).ToList();
            
            var list = (
                        from std in db.AspNetStudents
                        join user in db.AspNetUsers on std.StudentID equals user.Id

                        select new
                        {
                            Id = user.Id,
                            RollNo = user.UserName,
                            Name = user.Name,
                            Session = std.AspNetClass.ClassName,
                            Invoices = user.CashReceipts.Count(),
                            TotalFee = std.StudentFeeMonths.FirstOrDefault().FeePayable,
                            FeeReceived = user.CashReceipts.Sum(x => x.Amount),
                            TotalDiscount = user.CashReceipts.Sum(x => x.Discount),
                            

                        }).Distinct().ToList();


                    var receiptsList =   (from l in list 
                    join receipt in db.CashReceipts.Where(x => x.UserId != null &&  TodayDate > x.DueDate) on l.Id equals receipt.UserId
                    select new
                    {
                        Id = l.Id,
                        RollNo = l.RollNo,
                        Name = l.Name,
                        Session = l.Session,
                        Invoices = l.Invoices,
                        TotalFee = l.TotalFee,
                        FeeReceived = l.FeeReceived,
                        TotalDiscount = l.TotalDiscount,
                        DueDate =  receipt.DueDate
                       
                        
                    }).Distinct().ToList();


                        //var list = (
                        //            from std in db.AspNetStudents
                        //            join user in db.AspNetUsers on std.StudentID equals user.Id
                        //            join receipt in db.CashReceipts.Where(x => x.UserId != null && x.DueDate > TodayDate) on user.Id equals receipt.UserId
                        //            select new
                        //            {
                        //                Id = user.Id,
                        //                RollNo = user.UserName,
                        //                Name = user.Name,
                        //                Session = std.AspNetClass.ClassName,
                        //                Invoices = user.CashReceipts.Count(),
                        //                TotalFee = std.StudentFeeMonths.FirstOrDefault().FeePayable,
                        //                FeeReceived = user.CashReceipts.Sum(x => x.Amount),
                        //                TotalDiscount = user.CashReceipts.Sum(x => x.Discount)

                        //            }).Distinct().ToList();




                        //var list = (
                        //             from std in db.AspNetStudents
                        //             join user in db.AspNetUsers on std.StudentID equals user.Id
                        //            // join cashReceipt in db.CashReceipts on user.Id equals cashReceipt.UserId
                        //             select new
                        //             {
                        //                 Id = user.Id,
                        //                 RollNo = user.UserName,
                        //                 Name = user.Name,
                        //                 Session = std.AspNetClass.ClassName,
                        //                 Invoices = user.CashReceipts.Count(),
                        //                 TotalFee = std.StudentFeeMonths.FirstOrDefault().FeePayable,
                        //                 FeeReceived = user.CashReceipts.Sum(x => x.Amount),
                        //                 TotalDiscount = user.CashReceipts.Sum(x => x.Discount)

                        //             }).Distinct().ToList();



            return Json(receiptsList, JsonRequestBehavior.AllowGet);
        }

        public class StudentFeeSummary
        {
            public string Id { get; set; }
            public string RollNo { get; set; }
            public string Name { get; set; }
            public string Session { get; set; }

            public int Invoices { get; set; }

            public double? TotalFee { get; set; }

            public double? FeeReceived { get; set; }
            public double? TotalDiscount { get; set; }
        }
        public ActionResult FeeHistory(string Id)
        {
            var cashReceiptsList = db.CashReceipts.Where(x => x.UserId == Id).ToList();

            return View(cashReceiptsList);
        }
        public ActionResult Delete(int id)
        {
            return View();
        }



    }
}
