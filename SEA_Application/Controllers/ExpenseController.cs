using Microsoft.AspNet.Identity;
using SEA_Application.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Data;
using System.Web;
using System.Web.Mvc;

using System.Data.Entity.Core.Objects;

namespace SEA_Application.Controllers
{
    public class ExpenseController : Controller
    {
        SEA_DatabaseEntities db = new SEA_DatabaseEntities();
        int SessionID = Int32.Parse(SessionIDStaticController.GlobalSessionID);

        // GET: Expense
        public ActionResult Index()
        {

            ViewBag.CurrentBalance = db.Ledgers.Where(x => x.Name == "Admin Drawer").FirstOrDefault().CurrentBalance;

            return View();
        }
        //public ActionResult ExpenseList()
        //{
        //    var VoucherRecordsList = db.VoucherRecords.Where(x => x.Ledger.LedgerHead.Name == "Expense" && x.Ledger.Name != "Discount" && x.Ledger.Name != "Photocopier").GroupBy(x => x.VoucherId).ToList();

        //    var FirstElement = VoucherRecordsList.FirstOrDefault();

        //    List<ExpenseViewModel> ExpenseList = new List<ExpenseViewModel>();

        //    foreach (var group in VoucherRecordsList)
        //    {
        //        ExpenseViewModel ExpenseViewModel = new ExpenseViewModel();
        //        ExpenseViewModel.VoucherId = group.Key.Value;

        //                          //  db.Vouchers.Where(x=>x.Id == )

        //        double Sum = 0;
        //        var groupKey = group.Key;
        //        foreach (var groupedItem in group)
        //        {

        //            Sum = Sum + Convert.ToDouble(groupedItem.Amount.Value);

        //            ExpenseViewModel.Date = groupedItem.Voucher.Date.ToString();
        //        }
        //        ExpenseViewModel.TotalAmount = Sum;

        //        ExpenseList.Add(ExpenseViewModel);


        //    }

        //    //ViewBag.VoucherRecordsList = VoucherRecordsList;

        //    return View(ExpenseList);
        //}

        public ActionResult ExpenseList()
        {

            DateTime? Datetime = GetLocalDateTime.GetLocalDateTimeFunction();

            var Day = "";
            var Month = "";
            var Year = "";

            Day = Datetime.Value.Day.ToString();
            Month = Datetime.Value.Month.ToString();
            Year = Datetime.Value.Year.ToString();

            //   DateTime? dateTimeTo = Datetime.Value.AddDays(1);

            //&& x.Voucher.Date == DateTime.Today


            var VoucherRecordsList = db.VoucherRecords.Where(x => x.Ledger.LedgerHead.Name == "Expense" && x.Ledger.Name != "Discount" && x.Ledger.Name != "Photocopier"&&  x.Voucher.Date.Value.Day.ToString() == Day && x.Voucher.Date.Value.Month.ToString() == Month & x.Voucher.Date.Value.Year.ToString() == Year).ToList();



            return View(VoucherRecordsList);
        }


        [HttpGet]
        public ActionResult EditExpense(int id)
        {
            ViewBag.CurrentBalance = db.Ledgers.Where(x => x.Name == "Admin Drawer").FirstOrDefault().CurrentBalance;

            // var VoucherObj = db.Vouchers.Where(x => x.Id == id).FirstOrDefault();

            /// ViewBag.Name = VoucherObj.Notes;
            // ViewBag.Notes = VoucherObj.Notes;

            // var VoucherRecordsList = db.VoucherRecords.Where(x => x.Ledger.LedgerHead.Name == "Expense" && x.Ledger.Name != "Discount" && x.VoucherId == id).ToList();

            ViewBag.VoucherId = id;

            return View();
        }
        public ActionResult EditCashVoucher(_Voucher Vouchers)
        {
            var dbTransaction = db.Database.BeginTransaction();


            try
            {

                var VoucherIDToDelete = Vouchers.VoucherDeleteId;
                List<VoucherRecord> VoucherRecordList = db.VoucherRecords.Where(x => x.VoucherId == VoucherIDToDelete).ToList();
                foreach (var voucherRecord in VoucherRecordList)
                {
                    var LedgerHeadName = db.Ledgers.Where(x => x.Id == voucherRecord.LedgerId).Select(x => x.LedgerHead.Name).FirstOrDefault();

                    var LedgerType = voucherRecord.Type;
                    if (LedgerHeadName == "Assets" || LedgerHeadName == "Expense")
                    {
                        var LedgerToModify = db.Ledgers.Where(x => x.Id == voucherRecord.LedgerId).FirstOrDefault();

                        decimal LedgerAmountToModify = LedgerToModify.CurrentBalance.Value;

                        if (LedgerType == "Dr")
                        {
                            LedgerToModify.CurrentBalance = LedgerAmountToModify - voucherRecord.Amount;
                            db.SaveChanges();
                        }
                        else
                        {
                            LedgerToModify.CurrentBalance = LedgerAmountToModify + voucherRecord.Amount;
                            db.SaveChanges();
                        }

                    }
                    else if (LedgerHeadName == "Income" || LedgerHeadName == "Liabilities")
                    {

                        var LedgerToModify = db.Ledgers.Where(x => x.Id == voucherRecord.LedgerId).FirstOrDefault();

                        decimal LedgerAmountToModify = LedgerToModify.CurrentBalance.Value;

                        if (LedgerType == "Dr")
                        {
                            LedgerToModify.CurrentBalance = LedgerAmountToModify + voucherRecord.Amount;
                            db.SaveChanges();
                        }
                        else
                        {
                            LedgerToModify.CurrentBalance = LedgerAmountToModify - voucherRecord.Amount;
                            db.SaveChanges();
                        }

                    }
                    else
                    {
                    }

                }


                db.VoucherRecords.RemoveRange(VoucherRecordList);
                db.SaveChanges();


                Voucher VoucherToDelete = db.Vouchers.Where(x => x.Id == VoucherIDToDelete).FirstOrDefault();

                db.Vouchers.Remove(VoucherToDelete);
                db.SaveChanges();


                var LeadgerAdminDrawer = db.Ledgers.Where(x => x.Name == "Admin Drawer").FirstOrDefault();

                decimal? CurrentBalanceOfAdminDrawer = LeadgerAdminDrawer.CurrentBalance;

                decimal Total = Convert.ToDecimal(Vouchers.uppertotal);

                if (CurrentBalanceOfAdminDrawer >= Total)
                {

                    string[] D4 = Vouchers.Time.Split('-');
                    Vouchers.Time = D4[2] + "/" + D4[1] + "/" + D4[0];
                    Voucher v = new Voucher();

                    var localtime = DateTime.Now.ToLocalTime().ToString();
                    var time = localtime.Split(' ');
                    var timestamps = time[1].Split(':');
                    if (timestamps[0].Count() == 1)
                    {
                        timestamps[0] = "0" + timestamps[0];
                    }
                    if (timestamps[1].Count() == 1)
                    {
                        timestamps[1] = "0" + timestamps[1];
                    }
                    if (timestamps[2].Count() == 1)
                    {
                        timestamps[2] = "0" + timestamps[2];
                    }
                    time[1] = timestamps[0] + ":" + timestamps[1] + ":" + timestamps[2];

                    var date = Vouchers.Time + " " + time[1] + " " + time[2];

                    //  DateTime dt = //DateTime.ParseExact(date, "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                    v.Date = GetLocalDateTime.GetLocalDateTimeFunction();
                    v.Notes = Vouchers.Narration;
                    v.VoucherNo = Vouchers.VoucherNo;
                    v.Name = Vouchers.VoucherName;
                    var id = User.Identity.GetUserId();
                    v.SessionID = SessionID;

                    var username = db.AspNetUsers.Where(x => x.Id == id).Select(x => x.Name).FirstOrDefault();
                    v.CreatedBy = username;
                    db.Vouchers.Add(v);
                    db.SaveChanges();

                    foreach (Voucher_Detail voucher in Vouchers.VoucherDetail)
                    {
                        VoucherRecord VR = new VoucherRecord();
                        int idd = Convert.ToInt32(voucher.Code);

                        var Ledger = db.Ledgers.Where(x => x.Id == idd).FirstOrDefault();
                        decimal? CurrentBlance = Ledger.CurrentBalance;
                        decimal? AfterBalanceOfLedger = CurrentBlance + Convert.ToDecimal(voucher.Credit);
                        VR.LedgerId = idd;
                        VR.Type = "Dr";
                        VR.Amount = Convert.ToDecimal(voucher.Credit);
                        VR.CurrentBalance = CurrentBlance;
                        VR.AfterBalance = AfterBalanceOfLedger;
                        VR.VoucherId = v.Id;
                        VR.Description = voucher.Transaction;
                        Ledger.CurrentBalance = AfterBalanceOfLedger;

                        db.VoucherRecords.Add(VR);
                        db.SaveChanges();

                    }

                    var LeadgerAdminDrawer1 = db.Ledgers.Where(x => x.Name == "Admin Drawer").FirstOrDefault();

                    decimal? CurrentBalanceOfAdminDrawer1 = LeadgerAdminDrawer1.CurrentBalance;


                    decimal? AfterBalanceOfAdminDrawer1 = CurrentBalanceOfAdminDrawer1 - Convert.ToDecimal(Vouchers.uppertotal);
                    VoucherRecord voucherRecord1 = new VoucherRecord();

                    voucherRecord1.LedgerId = LeadgerAdminDrawer.Id;
                    voucherRecord1.Type = "Cr";
                    voucherRecord1.Amount = Convert.ToDecimal(Vouchers.uppertotal);
                    voucherRecord1.CurrentBalance = CurrentBalanceOfAdminDrawer;
                    voucherRecord1.AfterBalance = AfterBalanceOfAdminDrawer1;
                    voucherRecord1.VoucherId = v.Id;
                    voucherRecord1.Description = "Expense Credited";
                    LeadgerAdminDrawer1.CurrentBalance = AfterBalanceOfAdminDrawer1;

                    db.VoucherRecords.Add(voucherRecord1);
                    db.SaveChanges();
                    dbTransaction.Commit();

                    var result = "yes";

                    return Json(result, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    dbTransaction.Dispose();

                    var result = "No";
                    return Json(result, JsonRequestBehavior.AllowGet);

                }

            }//end of try block
            catch (Exception ex)
            {
                var msg = ex.Message;
                dbTransaction.Dispose();
            }

            //  return RedirectToAction("Cash");


            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteExpense(int id)
        {

            var dbTransaction = db.Database.BeginTransaction();
            try
            {

                List<VoucherRecord> VoucherRecordList = db.VoucherRecords.Where(x => x.VoucherId == id).ToList();
                foreach (var voucherRecord in VoucherRecordList)
                {
                    var LedgerHeadName = db.Ledgers.Where(x => x.Id == voucherRecord.LedgerId).Select(x => x.LedgerHead.Name).FirstOrDefault();

                    var LedgerType = voucherRecord.Type;
                    if (LedgerHeadName == "Assets" || LedgerHeadName == "Expense")
                    {
                        var LedgerToModify = db.Ledgers.Where(x => x.Id == voucherRecord.LedgerId).FirstOrDefault();

                        decimal LedgerAmountToModify = LedgerToModify.CurrentBalance.Value;

                        if (LedgerType == "Dr")
                        {
                            LedgerToModify.CurrentBalance = LedgerAmountToModify - voucherRecord.Amount;
                            db.SaveChanges();
                        }
                        else
                        {
                            LedgerToModify.CurrentBalance = LedgerAmountToModify + voucherRecord.Amount;
                            db.SaveChanges();
                        }

                    }
                    else if (LedgerHeadName == "Income" || LedgerHeadName == "Liabilities")
                    {

                        var LedgerToModify = db.Ledgers.Where(x => x.Id == voucherRecord.LedgerId).FirstOrDefault();

                        decimal LedgerAmountToModify = LedgerToModify.CurrentBalance.Value;

                        if (LedgerType == "Dr")
                        {
                            LedgerToModify.CurrentBalance = LedgerAmountToModify + voucherRecord.Amount;
                            db.SaveChanges();
                        }
                        else
                        {
                            LedgerToModify.CurrentBalance = LedgerAmountToModify - voucherRecord.Amount;
                            db.SaveChanges();
                        }

                    }
                    else
                    {
                    }

                }

                db.VoucherRecords.RemoveRange(VoucherRecordList);
                db.SaveChanges();


                Voucher Voucher = db.Vouchers.Where(x => x.Id == id).FirstOrDefault();
                db.Vouchers.Remove(Voucher);        
                db.SaveChanges();

            }// end of try block 

            catch (Exception ex)
            {
                var msg = ex.Message;
                dbTransaction.Dispose();
            }

            dbTransaction.Commit();


            return Json("success", JsonRequestBehavior.AllowGet);
        }

        public JsonResult ExpenseRecordsToEdit(int id)
        {

            //  ViewBag.CurrentBalance = db.Ledgers.Where(x => x.Name == "Admin Drawer").FirstOrDefault().CurrentBalance;

            var VoucherObj = db.Vouchers.Where(x => x.Id == id).FirstOrDefault();

            //   ViewBag.Name = VoucherObj.Notes;
            // ViewBag.Notes = VoucherObj.Notes;

            var VoucherRecordsList = db.VoucherRecords.Where(x => x.Ledger.LedgerHead.Name == "Expense" && x.Ledger.Name != "Discount" && x.Ledger.Name != "Photocopier" && x.VoucherId == id).Select(x => new { x.AfterBalance, x.CurrentBalance, x.Amount, x.Description, x.Id, x.Type, x.Ledger.Name, x.LedgerId }).ToList();

            var Name = VoucherObj.Name;
            var Notes = VoucherObj.Notes;
            var Date = VoucherObj.Date;
            var VoucherNo = VoucherObj.VoucherNo;

            return Json(new { Name = Name,Date = Date, Notes = Notes, VoucherNo = VoucherNo,  VoucherRecordsList = VoucherRecordsList }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditExpense()
        {

            return View();
        }
        public class ExpenseViewModel
        {
            public int VoucherId { get; set; }
            public string VoucherDetail { get; set; }
            public string NotesDetails { get; set; }
            public double TotalAmount { get; set; }
            public string Date { get; set; }
        }



        public JsonResult FindVoucherNo()
        {
            int No;
            try
            {
                No = (int)db.Vouchers.Select(x => x.VoucherNo).Max();
                No++;

            }
            catch
            {
                No = 1;
            }

            return Json(No, JsonRequestBehavior.AllowGet);

        }
        public class _Voucher
        {
            public string VoucherName { get; set; }
            public string VoucherDescription { get; set; }
            public string Narration { get; set; }
            public string Time { get; set; }
            public string upperdesc { get; set; }
            public int uppercode { get; set; }
            public string accounttype { get; set; }
            public string uppertotal { get; set; }
            public int VoucherNo { get; set; }

            public int VoucherDeleteId { get; set; }
            public List<Voucher_Detail> VoucherDetail { set; get; }
        }
        public ActionResult AddCashVoucher(_Voucher Vouchers)
        {
            var dbTransaction = db.Database.BeginTransaction();


            try
            {

                var LeadgerAdminDrawer = db.Ledgers.Where(x => x.Name == "Admin Drawer").FirstOrDefault();

                decimal? CurrentBalanceOfAdminDrawer = LeadgerAdminDrawer.CurrentBalance;

                decimal Total = Convert.ToDecimal(Vouchers.uppertotal);

                if (CurrentBalanceOfAdminDrawer >= Total)
                {

                    string[] D4 = Vouchers.Time.Split('-');
                    Vouchers.Time = D4[2] + "/" + D4[1] + "/" + D4[0];
                    Voucher v = new Voucher();

                    var localtime = DateTime.Now.ToLocalTime().ToString();
                    var time = localtime.Split(' ');
                    var timestamps = time[1].Split(':');
                    if (timestamps[0].Count() == 1)
                    {
                        timestamps[0] = "0" + timestamps[0];
                    }
                    if (timestamps[1].Count() == 1)
                    {
                        timestamps[1] = "0" + timestamps[1];
                    }
                    if (timestamps[2].Count() == 1)
                    {
                        timestamps[2] = "0" + timestamps[2];
                    }
                    time[1] = timestamps[0] + ":" + timestamps[1] + ":" + timestamps[2];

                    var date = Vouchers.Time + " " + time[1] + " " + time[2];

                    //  DateTime dt = //DateTime.ParseExact(date, "dd/MM/yyyy hh:mm:ss tt", CultureInfo.InvariantCulture);

                    v.Date = GetLocalDateTime.GetLocalDateTimeFunction();
                    v.Notes = Vouchers.Narration;
                    v.VoucherNo = Vouchers.VoucherNo;
                    v.Name = Vouchers.VoucherName;
                    var id = User.Identity.GetUserId();
                    v.SessionID = SessionID;

                    var username = db.AspNetUsers.Where(x => x.Id == id).Select(x => x.Name).FirstOrDefault();
                    v.CreatedBy = username;
                    db.Vouchers.Add(v);
                    db.SaveChanges();

                    foreach (Voucher_Detail voucher in Vouchers.VoucherDetail)
                    {
                        VoucherRecord VR = new VoucherRecord();
                        int idd = Convert.ToInt32(voucher.Code);

                        var Ledger = db.Ledgers.Where(x => x.Id == idd).FirstOrDefault();
                        decimal? CurrentBlance = Ledger.CurrentBalance;
                        decimal? AfterBalanceOfLedger = CurrentBlance + Convert.ToDecimal(voucher.Credit);
                        VR.LedgerId = idd;
                        VR.Type = "Dr";
                        VR.Amount = Convert.ToDecimal(voucher.Credit);
                        VR.CurrentBalance = CurrentBlance;
                        VR.AfterBalance = AfterBalanceOfLedger;
                        VR.VoucherId = v.Id;
                        VR.Description = voucher.Transaction;
                        Ledger.CurrentBalance = AfterBalanceOfLedger;

                        db.VoucherRecords.Add(VR);
                        db.SaveChanges();

                    }

                    var LeadgerAdminDrawer1 = db.Ledgers.Where(x => x.Name == "Admin Drawer").FirstOrDefault();

                    decimal? CurrentBalanceOfAdminDrawer1 = LeadgerAdminDrawer1.CurrentBalance;


                    decimal? AfterBalanceOfAdminDrawer1 = CurrentBalanceOfAdminDrawer1 - Convert.ToDecimal(Vouchers.uppertotal);
                    VoucherRecord voucherRecord1 = new VoucherRecord();

                    voucherRecord1.LedgerId = LeadgerAdminDrawer.Id;
                    voucherRecord1.Type = "Cr";
                    voucherRecord1.Amount = Convert.ToDecimal(Vouchers.uppertotal);
                    voucherRecord1.CurrentBalance = CurrentBalanceOfAdminDrawer;
                    voucherRecord1.AfterBalance = AfterBalanceOfAdminDrawer1;
                    voucherRecord1.VoucherId = v.Id;
                    voucherRecord1.Description = "Expense Credited";
                    LeadgerAdminDrawer1.CurrentBalance = AfterBalanceOfAdminDrawer1;

                    db.VoucherRecords.Add(voucherRecord1);
                    db.SaveChanges();

                    dbTransaction.Commit();
                    var result = "yes";
                    return Json(result, JsonRequestBehavior.AllowGet);

                }
                else
                {
                    dbTransaction.Dispose();
                    var result = "No";
                    return Json(result, JsonRequestBehavior.AllowGet);

                }

            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                dbTransaction.Dispose();
            }



            return RedirectToAction("Cash");
        }
        public class Voucher_Detail
        {
            public string Type { get; set; }
            public string VoucherNo { get; set; }
            public string Time { get; set; }
            public string Code { get; set; }
            public string Transaction { get; set; }
            public string Credit { get; set; }
            public string Debit { get; set; }
            public double balance { get; set; }
            public int BranchId { get; set; }


        }
        public JsonResult SelectListLedgers()
        {

            var ledger = db.Ledgers.Where(x => x.LedgerGroup.Name == "Cash").ToList();
            List<Ledger> List = new List<Ledger>();

            foreach (var item in ledger)
            {
                Ledger Ledger = new Ledger();
                Ledger.Id = item.Id;
                Ledger.Name = item.Name;
                List.Add(Ledger);
            }
            return Json(List, JsonRequestBehavior.AllowGet);
        }
        public JsonResult CheckaccountHead(int id)
        {
            var flag = false;
            var headName = db.Ledgers.Where(x => x.Id == id).Select(x => x.LedgerHead.Name).FirstOrDefault();
            if (headName == "Income" || headName == "Expense")
            {
                flag = true;
            }
            return Json(flag, JsonRequestBehavior.AllowGet);
        }
        public class HeadList
        {
            public int HeadId { get; set; }
            public string HeadName { get; set; }
            public List<AccountsList> accountlist { get; set; }
        }
        public class AccountsList
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
        public JsonResult SelectAllLedgers()
        {

            var headlist = db.LedgerHeads.Where(x => x.Name == "Expense").ToList();

            List<HeadList> Head_list = new List<HeadList>();
            foreach (var item in headlist)
            {
                HeadList hl = new HeadList();
                hl.HeadId = item.Id;
                hl.HeadName = item.Name;

                var ledger = db.Ledgers.Where(x => x.LedgerGroup.Name != "Cash" && x.LedgerGroup.Name != "Bank" && x.LedgerHeadId == item.Id && x.Name != "Discount" && x.Name != "Photocopier").ToList();

                hl.accountlist = new List<AccountsList>();
                foreach (var l_item in ledger)
                {
                    AccountsList Ledger = new AccountsList();
                    Ledger.Id = l_item.Id;
                    Ledger.Name = l_item.Name;
                    hl.accountlist.Add(Ledger);
                }
                Head_list.Add(hl);


            }
            return Json(Head_list, JsonRequestBehavior.AllowGet);
        }
        public ActionResult Report()
        {


            return View();
        }
        public ActionResult ReportRecords()
        {

            //var Date = DateTime.Now.ToString("d/M/yyyy");
            //var result = from Voucher in db.Vouchers
            //             join VoucherRecord in db.VoucherRecords on Voucher.Id equals VoucherRecord.VoucherId
            //             join Leadger in db.Ledgers on VoucherRecord.LedgerId equals Leadger.Id
            //             where VoucherRecord.Ledger.Name == "Admin Drawer" && Voucher.Date.ToString() == Date
            //             select new
            //             {
            //                 VoucherRecord.Type,
            //                 Voucher.Date,
            //                 VoucherRecord.Amount,
            //                 Voucher.Id,
            //                 VoucherRecord.Description,
            //             };

            //var x = 0;

            var result = db.TodayExpense();

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DebitRecords()
        {

            var result = db.TodayDebitList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }



    }
}