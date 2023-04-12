using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.Office.Interop.Excel;
using SEA_Application.Models;
using static SEA_Application.Controllers.TermAssessmentControllers.TermAssessmentController;

namespace SEA_Application.Controllers
{
    public class ReceiptsController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
        int SessionID = Int32.Parse(SessionIDStaticController.GlobalSessionID);

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
                if (cashReceipt.Discount == null)
                {
                    cashReceipt.Discount = 0;
                }
                if (cashReceipt.Amount == null)
                {
                    cashReceipt.Amount = 0;
                }

                cashReceipt.CreationDate = GetLocalDateTime.GetLocalDateTimeFunction();
                db.CashReceipts.Add(cashReceipt);
                db.SaveChanges();

                var VoucherMsg = "";
                var AccRecDescription = "";
                var AdminDrawerDescription = "";
                var DiscountDescription = "";

                int? StudentId = null;
                string StudentName = null;
                string SessionName = null;

                if (cashReceipt.UserId != null)
                {
                  var Student = db.AspNetStudents.Where(x => x.StudentID == cashReceipt.UserId).FirstOrDefault();
                  var ClassId = Student.ClassID;
                  int? sessionId =  db.AspNetClasses.Where(x => x.Id == ClassId).FirstOrDefault().SessionID;
                  
                  SessionName = db.AspNetSessions.Where(x => x.Id == sessionId).FirstOrDefault().SessionName;

                  StudentId =   Student.Id;
                  StudentName = db.AspNetUsers.Where(x => x.Id == cashReceipt.UserId).FirstOrDefault().Name;
                  VoucherMsg = "Fee Received by Admin of Student " + StudentName + " Session Name " + SessionName;
                  AccRecDescription = "Fee Collected by student  (" + StudentName + ") (" + SessionName + ") ";
                  AdminDrawerDescription = "Fee received of Student (" + StudentName + ") (" + SessionName + ") ";
                  DiscountDescription = "Discount given to student (" + StudentName + ") (" + SessionName + ")  on payable fee ";

                }
                else
                {
                    VoucherMsg = cashReceipt.ReceivedFrom + " " + cashReceipt.Description;
                    AccRecDescription = cashReceipt.ReceivedFrom + " " + cashReceipt.Description;
                    AdminDrawerDescription = cashReceipt.ReceivedFrom + " " + cashReceipt.Description;
                    DiscountDescription = cashReceipt.ReceivedFrom + " " + cashReceipt.Description;
                }

                var idd = User.Identity.GetUserId();

                var username = db.AspNetUsers.Where(x => x.Id == idd).Select(x => x.Name).FirstOrDefault();
                Voucher voucher = new Voucher();


                voucher.Name = VoucherMsg;
                voucher.Notes = VoucherMsg;

                voucher.StudentId = StudentId;
              
                voucher.Date = GetLocalDateTime.GetLocalDateTimeFunction();
                voucher.CreatedBy = username;
                voucher.SessionID = SessionID;
                int? VoucherObj = db.Vouchers.Max(x => x.VoucherNo);
                voucher.VoucherNo = Convert.ToInt32(VoucherObj) + 1;
                db.Vouchers.Add(voucher);

                db.SaveChanges();


                var Leadger = db.Ledgers.Where(x => x.Name == "Account Receiveable").FirstOrDefault();
                int AccountReceiveableId = Leadger.Id;
                decimal? CurrentBalance = Leadger.CurrentBalance;
                decimal? AfterBalance = 0;
                VoucherRecord voucherRecord = new VoucherRecord();
                if (cashReceipt.Discount != 0)
                {

                    AfterBalance = CurrentBalance - Convert.ToDecimal(cashReceipt.Amount) - Math.Round(Convert.ToDecimal(cashReceipt.Discount));
                }
                else
                {
                    AfterBalance = CurrentBalance - Convert.ToDecimal(cashReceipt.Amount);
                }
                voucherRecord.LedgerId = AccountReceiveableId;
                voucherRecord.Type = "Cr";
                voucherRecord.Amount = Convert.ToDecimal(cashReceipt.Amount) + Math.Round(Convert.ToDecimal(cashReceipt.Discount));
                voucherRecord.CurrentBalance = CurrentBalance;

                voucherRecord.AfterBalance = AfterBalance;
                voucherRecord.VoucherId = voucher.Id;
                voucherRecord.Description = AccRecDescription;

                Leadger.CurrentBalance = AfterBalance;
                try
                {
                    db.VoucherRecords.Add(voucherRecord);
                    db.SaveChanges();

                }
                catch (Exception ex)
                {
                    var a = ex.Message;
                }

                var LeadgerAD = db.Ledgers.Where(x => x.Name == "Admin Drawer").FirstOrDefault();
                int AdminDrawerId = LeadgerAD.Id;
                decimal? CurrentBalanceAD = LeadgerAD.CurrentBalance;

                VoucherRecord voucherRecord1 = new VoucherRecord();
                decimal? AfterBalanceAD = CurrentBalanceAD + Convert.ToDecimal(cashReceipt.Amount);
                voucherRecord1.LedgerId = AdminDrawerId;
                voucherRecord1.Type = "Dr";
                voucherRecord1.Amount = Convert.ToDecimal(cashReceipt.Amount);
                voucherRecord1.CurrentBalance = CurrentBalanceAD;

                voucherRecord1.AfterBalance = AfterBalanceAD;
                voucherRecord1.VoucherId = voucher.Id;
                voucherRecord1.Description = AdminDrawerDescription;

                LeadgerAD.CurrentBalance = AfterBalanceAD;
                db.VoucherRecords.Add(voucherRecord1);
                db.SaveChanges();


                if (cashReceipt.Discount != 0 && cashReceipt.Discount != null)
                {
                    VoucherRecord voucherRecord3 = new VoucherRecord();

                    var LeadgerDiscount = db.Ledgers.Where(x => x.Name == "Discount").FirstOrDefault();

                    decimal? CurrentBalanceOfDiscount = LeadgerDiscount.CurrentBalance;
                    decimal? AfterBalanceOfDiscount = CurrentBalanceOfDiscount + Math.Round(Convert.ToDecimal(cashReceipt.Discount));
                    voucherRecord3.LedgerId = LeadgerDiscount.Id;
                    voucherRecord3.Type = "Dr";
                    voucherRecord3.Amount = Math.Round(Convert.ToDecimal(cashReceipt.Discount));
                    voucherRecord3.CurrentBalance = CurrentBalanceOfDiscount;
                    voucherRecord3.AfterBalance = AfterBalanceOfDiscount;
                    voucherRecord3.VoucherId = voucher.Id;
                    voucherRecord3.Description = DiscountDescription;  
                    LeadgerDiscount.CurrentBalance = AfterBalanceOfDiscount;

                    db.VoucherRecords.Add(voucherRecord3);
                    db.SaveChanges();


                }



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
