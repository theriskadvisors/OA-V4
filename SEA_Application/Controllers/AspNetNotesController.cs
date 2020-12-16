using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Microsoft.AspNet.Identity;
using SEA_Application.Models;

namespace SEA_Application.Controllers
{
    public class AspNetNotesController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
        int SessionID = Int32.Parse(SessionIDStaticController.GlobalSessionID);

        // GET: AspNetNotes
        public ActionResult Index()
        {
            //foreach (var day in last7Days)
            //{
            //    var day1 = day;
            //}
            //Console.WriteLine($"{day:yyyy-MM-dd}"); // Any manipulations with days go here


            var aspNetNotes = db.AspNetNotes.Include(a => a.AspNetSubject);
            return View(aspNetNotes.ToList());
        }

        public ActionResult ApproveOrders(int OrderId, string OrderType, double DiscountedPrice, double Discount, List<ChangeNotesPrice> ChangeNotesPrice)
        {
            //return Json("", JsonRequestBehavior.AllowGet);

            var dbTransaction = db.Database.BeginTransaction();
            try
            {
                AspNetOrder OrderToModify = db.AspNetOrders.Where(x => x.Id == OrderId).FirstOrDefault();

                if (OrderToModify.Status == "Pending")
                {

                    OrderToModify.Status = "Paid";
                    OrderToModify.Discount = Discount;
                    OrderToModify.ApproveDate = GetLocalDateTime.GetLocalDateTimeFunction();
                    OrderToModify.TotalAmount = Convert.ToInt32(DiscountedPrice);
                    db.Entry(OrderToModify).State = EntityState.Modified;
                    db.SaveChanges();


                    List<AspNetNotesOrder> NotesOrderToModify = db.AspNetNotesOrders.Where(x => x.OrderId == OrderId).ToList();

                    foreach (var NotesOrder in NotesOrderToModify)
                    {
                        var ChangeNotesP = ChangeNotesPrice.Where(x => x.OrderNotesId == NotesOrder.Id).FirstOrDefault();

                        NotesOrder.Status = "Paid";
                        NotesOrder.OAPrice = ChangeNotesP.OAPrice;
                        NotesOrder.PhotoCopierPrice = ChangeNotesP.PhotoCopierPrice;

                        db.Entry(NotesOrder).State = EntityState.Modified;

                        db.SaveChanges();

                    }
                    var result = from Notes in db.AspNetNotes
                                 join OrderNotes in db.AspNetNotesOrders on Notes.Id equals OrderNotes.NotesID
                                 join Order in db.AspNetOrders on OrderNotes.OrderId equals Order.Id
                                 join Student in db.AspNetStudents on OrderNotes.StudentID equals Student.Id
                                 join User in db.AspNetUsers on Student.StudentID equals User.Id
                                 where Order.Id == OrderId
                                 select new
                                 {
                                     classId = Student.ClassID,
                                     Title = Notes.Title,
                                     Quantity = OrderNotes.Quantity,
                                     GrandTotal = Notes.GrandTotal,
                                     PhotoCopier = Notes.PhotoCopierPrice,
                                     NameOfStudent = User.Name,
                                     StudentId = Student.Id,

                                     // TotalPhotoCopierPrice = (Notes.PhotoCopierPrice * OrderNotes.Quantity)
                                     TotalPhotoCopierPrice = (OrderNotes.PhotoCopierPrice * OrderNotes.Quantity)
                                 };

                    var NameOfStudent = result.FirstOrDefault().NameOfStudent;
                    var classIdd = result.FirstOrDefault().classId;
                    var SessionIdd = db.AspNetClasses.Where(x => x.Id == classIdd).FirstOrDefault().SessionID;
                    var SessionName = db.AspNetSessions.Where(x => x.Id == SessionIdd).FirstOrDefault().SessionName;

                    double TotalPriceOfPhotoCopier = 0;

                    foreach (var item in result)
                    {
                        TotalPriceOfPhotoCopier = Convert.ToDouble(TotalPriceOfPhotoCopier + item.TotalPhotoCopierPrice);
                    }

                    var id = User.Identity.GetUserId();
                    var username = db.AspNetUsers.Where(x => x.Id == id).Select(x => x.Name).FirstOrDefault();
                    Voucher voucher = new Voucher();

                    voucher.Name = "Notes paid by Student " + NameOfStudent + " Session Name " + SessionName;
                    voucher.Notes = "Cash received for notes";
                    voucher.Date = GetLocalDateTime.GetLocalDateTimeFunction();
                    voucher.StudentId = result.FirstOrDefault().StudentId;

                    voucher.CreatedBy = username;
                    voucher.SessionID = SessionID;
                    int? VoucherObj = db.Vouchers.Max(x => x.VoucherNo);

                    voucher.VoucherNo = Convert.ToInt32(VoucherObj) + 1;
                    db.Vouchers.Add(voucher);
                    db.SaveChanges();

                    if (OrderType == "Postpaid")
                    {

                        var Leadger = db.Ledgers.Where(x => x.Name == "Admin Drawer").FirstOrDefault();
                        int AdminDrawerId = Leadger.Id;
                        decimal? CurrentBalance = Leadger.CurrentBalance;
                        VoucherRecord voucherRecord = new VoucherRecord();
                        decimal? AfterBalance = CurrentBalance + Convert.ToDecimal(DiscountedPrice); //OrderToModify.TotalAmount;
                        voucherRecord.LedgerId = AdminDrawerId;
                        voucherRecord.Type = "Dr";
                        voucherRecord.Amount = OrderToModify.TotalAmount;
                        voucherRecord.CurrentBalance = CurrentBalance;

                        voucherRecord.AfterBalance = AfterBalance;
                        voucherRecord.VoucherId = voucher.Id;
                        voucherRecord.Description = "Notes paid by student (" + NameOfStudent + ") (" + SessionName + ")";
                        Leadger.CurrentBalance = AfterBalance;
                        db.VoucherRecords.Add(voucherRecord);
                        db.SaveChanges();

                        VoucherRecord voucherRecord1 = new VoucherRecord();

                        var LeadgerNotes = db.Ledgers.Where(x => x.Name == "Notes").FirstOrDefault();

                        decimal? CurrentBalanceOfNotes = LeadgerNotes.CurrentBalance;
                        decimal? AfterBalanceOfNotes = CurrentBalanceOfNotes + Convert.ToDecimal(DiscountedPrice);//OrderToModify.TotalAmount;
                        voucherRecord1.LedgerId = LeadgerNotes.Id;
                        voucherRecord1.Type = "Cr";
                        voucherRecord1.Amount = OrderToModify.TotalAmount;
                        voucherRecord1.CurrentBalance = CurrentBalanceOfNotes;
                        voucherRecord1.AfterBalance = AfterBalanceOfNotes;
                        voucherRecord1.VoucherId = voucher.Id;
                        voucherRecord1.Description = "Notes against order ID " + OrderId;
                        LeadgerNotes.CurrentBalance = AfterBalanceOfNotes;

                        db.VoucherRecords.Add(voucherRecord1);
                        db.SaveChanges();

                    }//end of if

                    VoucherRecord voucherRecord2 = new VoucherRecord();

                    var IdofLedger = from Ledger in db.Ledgers
                                     join LedgerHd in db.LedgerHeads on Ledger.LedgerHeadId equals LedgerHd.Id
                                     where LedgerHd.Name == "Liabilities" && Ledger.Name == "Photocopier"
                                     select new
                                     {
                                         Ledger.Id

                                     };


                    int photoCopierId = Convert.ToInt32(IdofLedger.FirstOrDefault().Id);
                    var LeadgerPhotoCopierL = db.Ledgers.Where(x => x.Id == photoCopierId).FirstOrDefault();

                    decimal? CurrentBalanceOfPhotoCopiter = LeadgerPhotoCopierL.CurrentBalance;
                    decimal? AfterBalanceOfPhotoCopier = CurrentBalanceOfPhotoCopiter + Convert.ToDecimal(TotalPriceOfPhotoCopier);
                    voucherRecord2.LedgerId = LeadgerPhotoCopierL.Id;
                    voucherRecord2.Type = "Cr";
                    voucherRecord2.Amount = Convert.ToDecimal(TotalPriceOfPhotoCopier);
                    voucherRecord2.CurrentBalance = CurrentBalanceOfPhotoCopiter;
                    voucherRecord2.AfterBalance = AfterBalanceOfPhotoCopier;
                    voucherRecord2.VoucherId = voucher.Id;
                    voucherRecord2.Description = "Notes against order ID " + OrderId;
                    LeadgerPhotoCopierL.CurrentBalance = AfterBalanceOfPhotoCopier;
                    db.VoucherRecords.Add(voucherRecord2);

                    db.SaveChanges();

                    VoucherRecord voucherRecord3 = new VoucherRecord();

                    var IdofLedger1 = from Ledger in db.Ledgers
                                      join LedgerHd in db.LedgerHeads on Ledger.LedgerHeadId equals LedgerHd.Id
                                      where LedgerHd.Name == "Expense" && Ledger.Name == "Photocopier"
                                      select new
                                      {
                                          Ledger.Id
                                      };

                    int photoCopierIdOfExpense = Convert.ToInt32(IdofLedger1.FirstOrDefault().Id);
                    var LeadgerPhotoCopierE = db.Ledgers.Where(x => x.Id == photoCopierIdOfExpense).FirstOrDefault();

                    decimal? CurrentBalanceOfPhotoCopiterE = LeadgerPhotoCopierE.CurrentBalance;
                    decimal? AfterBalanceOfPhotoCopierE = CurrentBalanceOfPhotoCopiterE + Convert.ToDecimal(TotalPriceOfPhotoCopier);
                    voucherRecord3.LedgerId = LeadgerPhotoCopierE.Id;
                    voucherRecord3.Type = "Dr";
                    voucherRecord3.Amount = Convert.ToDecimal(TotalPriceOfPhotoCopier);
                    voucherRecord3.CurrentBalance = CurrentBalanceOfPhotoCopiterE;
                    voucherRecord3.AfterBalance = AfterBalanceOfPhotoCopierE;
                    voucherRecord3.VoucherId = voucher.Id;
                    voucherRecord3.Description = "Notes against order ID " + OrderId;
                    LeadgerPhotoCopierE.CurrentBalance = AfterBalanceOfPhotoCopierE;

                    db.VoucherRecords.Add(voucherRecord3);
                    db.SaveChanges();

                    if (Discount != 0)
                    {
                        VoucherRecord voucherRecord5 = new VoucherRecord();
                        var LeadgerDiscount = db.Ledgers.Where(x => x.Name == "Discount").FirstOrDefault();

                        decimal? CurrentBalanceOfDiscount = LeadgerDiscount.CurrentBalance;
                        decimal? AfterBalanceOfDiscount = CurrentBalanceOfDiscount + Convert.ToDecimal(Discount);
                        voucherRecord5.LedgerId = LeadgerDiscount.Id;
                        voucherRecord5.Type = "Dr";
                        voucherRecord5.Amount = Convert.ToDecimal(Discount);
                        voucherRecord5.CurrentBalance = CurrentBalanceOfDiscount;
                        voucherRecord5.AfterBalance = AfterBalanceOfDiscount;
                        voucherRecord5.VoucherId = voucher.Id;
                        voucherRecord5.Description = "Discont given to student " + OrderId;
                        LeadgerDiscount.CurrentBalance = AfterBalanceOfDiscount;
                        db.VoucherRecords.Add(voucherRecord5);
                        db.SaveChanges();
                    }

                }
                dbTransaction.Commit();

            }//end of try block
            catch (Exception ex)
            {
                var msg = ex.Message;
                dbTransaction.Dispose();

            }


            return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult PlaceOrder()
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
        public ActionResult NotesPrint()
        {
            List<string> CourseTypeList = new List<string>() { "CSS", "PMS" };

            List<Subject> SubjectList = new List<Subject>();

            foreach (var c in CourseTypeList)
            {
                var AllCSSNotesList = db.AspNetNotes.Where(x => x.CourseType == c).ToList();

                var list = AllCSSNotesList.GroupBy(x => x.SubjectID).Select(x => x.Key).ToList();

                foreach (var item in list)
                {

                    var SubjectName = db.AspNetSubjects.Where(x => x.Id == item).FirstOrDefault().SubjectName;
                    var CourseType1 = c;
                    var SubjectId = item;

                    Subject subject = new Subject();

                    subject.SubjectName = SubjectName;
                    subject.CourseType = CourseType1;
                    subject.SubjectId = SubjectId.Value;

                    var AllNotesList = db.AspNetNotes.Where(x => x.SubjectID == item).ToList();

                    foreach (var item1 in AllNotesList)
                    {
                        SubjectNotes subjectNote = new SubjectNotes();
                        subjectNote.SubjectId = item1.SubjectID.Value;
                        subjectNote.NotesTitle = item1.Title;
                        subjectNote.NotesPrice = item1.GrandTotal.Value;
                        subjectNote.Description = item1.Description;

                        subject.SubjectNotes.Add(subjectNote);
                    }
                    SubjectList.Add(subject);

                }

            }

            List<CourseType> courseTypeList = new List<CourseType>();

            foreach (var item in CourseTypeList)
            {
                CourseType obj = new CourseType();
                obj.CourseTypeName = item;

                obj.SubjectNotes.AddRange(SubjectList.Where(x => x.CourseType == item).ToList());

                courseTypeList.Add(obj);

            }

            return View(courseTypeList);
        }
        public ActionResult ReturnOrder()
        {
            //  List<int> OrderIdList = db.AspNetOrders.Where(x => x.Status == "Paid").Select(x => x.Id).ToList();

            // ViewBag.OrderId = new SelectList(db.AspNetOrders.Where(x => x.Status == "Paid" && x.OrderType == "Postpaid").ToList(), "Id", "Id");

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
        public ActionResult OrderList(int OrderId)
        {

            var NotesOrder = db.AspNetNotesOrders.Where(x => x.OrderId == OrderId).ToList();

            List<PlaceNotes> PlaceNotesList = new List<PlaceNotes>();

            foreach (var item in NotesOrder)
            {
                PlaceNotes PlaceNote = new PlaceNotes();
                var Note = db.AspNetNotes.Where(x => x.Id == item.NotesID).FirstOrDefault();

                PlaceNote.NotesType = Note.NotesType;
                PlaceNote.NoteName = Note.Title;
                PlaceNote.NotesId = item.NotesID.Value;
                PlaceNote.OrderNotesId = item.Id;
                PlaceNote.Price = (item.PhotoCopierPrice.Value + item.OAPrice.Value);
                PlaceNote.Quantity = item.Quantity.Value;
                PlaceNote.Total = Convert.ToInt32(PlaceNote.Price * PlaceNote.Quantity);

                PlaceNotesList.Add(PlaceNote);
            }

            return Json(PlaceNotesList, JsonRequestBehavior.AllowGet);
        }






        public ActionResult GetAllNotes()
        {

            var AllNotesList = db.AspNetNotes.Select(x => new { x.Id, x.Title }).ToList();
            return Json(AllNotesList, JsonRequestBehavior.AllowGet);

        }

        public ActionResult GetNoteDetail(int NoteId)
        {
            var GetNoteDetails = db.AspNetNotes.Where(x => x.Id == NoteId).Select(x => new { x.Id, x.NotesType, x.AspNetSubject.SubjectName, x.CourseType, x.Price, x.PhotoCopierPrice, x.OAPrice, x.GrandTotal }).FirstOrDefault();

            return Json(GetNoteDetails, JsonRequestBehavior.AllowGet);

        }

        public class CourseType
        {
            public string CourseTypeName { get; set; }
            public List<Subject> SubjectNotes = new List<Subject>();

        }

        public class Subject
        {
            public int SubjectId { get; set; }
            public string SubjectName { get; set; }
            public string CourseType { get; set; }
            public List<SubjectNotes> SubjectNotes = new List<SubjectNotes>();
        }
        public class SubjectNotes
        {
            public int SubjectId { get; set; }

            public string NotesTitle { get; set; }
            public string Description { get; set; }
            public double NotesPrice { get; set; }
        }

        public class ChangeNotesPrice
        {
            public int OrderNotesId { get; set; }
            public double PhotoCopierPrice { get; set; }
            public double OAPrice { get; set; }


        }
        // GET: AspNetNotes/Details/5
        public ActionResult Details(string id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetNote aspNetNote = db.AspNetNotes.Where(x => x.EncryptedID == id).FirstOrDefault();
            if (aspNetNote == null)
            {
                // return HttpNotFound();
                var aspNetNotes = db.AspNetNotes.Include(a => a.AspNetSubject);
                return View(aspNetNotes.ToList());
            }
            return View(aspNetNote);
        }

        public ActionResult RecentOrders()
        {
            //  var CurrentUserId = User.Identity.GetUserId();

            //   int StudentId = db.AspNetStudents.Where(x => x.StudentID == CurrentUserId).FirstOrDefault().Id;

            //var result = from Notes in db.AspNetNotes
            //             join OrderNotes in db.AspNetNotesOrders on Notes.Id equals OrderNotes.NotesID

            //             select new
            //             {
            //                 Id = OrderNotes.Id,
            //                 Title = Notes.Title,
            //                 Discription = Notes.Description,
            //                 CourseType = Notes.CourseType,
            //                 Price = Notes.Price,
            //                 Quantity = OrderNotes.Quantity,
            //                 Status = OrderNotes.Status,

            //             };

            var result = db.StudentOrderDetails().Where(x => x.OrderStatus == "Pending").ToList();



            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult ApprovedOrders()
        {
            //  var result = db.StudentOrderDetails().Where(x => x.OrderStatus == "Paid").ToList();

            DateTime[] last7Days = Enumerable.Range(0, 7)
            .Select(i => DateTime.Now.Date.AddDays(-i))
            .ToArray();

            DateTime lastDate = last7Days.LastOrDefault();
            var FromDate = lastDate.ToString("yyyy/MM/dd");
            DateTime FirstDate = last7Days.FirstOrDefault();
            var ToDate = FirstDate.ToString("yyyy/MM/dd");

            var FromDateInDateTime = Convert.ToDateTime(FromDate);
            var ToDateInDateTime = Convert.ToDateTime(ToDate);


            var result = (from Order in db.AspNetOrders
                          join OrderNotes in db.AspNetNotesOrders on Order.Id equals OrderNotes.OrderId
                          join student in db.AspNetStudents on OrderNotes.StudentID equals student.Id
                          where Order.Status == "Paid" && Order.PublishDate >= FromDateInDateTime && Order.PublishDate <= ToDateInDateTime
                          select new
                          {
                              OrderID = Order.Id,
                              // NotesOrderID = OrderNotes.Id,
                              OrderType = Order.OrderType,
                              TotalAmount = Order.TotalAmount,
                              OrderStatus = Order.Status,
                              StudentName = student.AspNetUser.Name,
                              CourseType = student.CourseType,
                              student.AspNetUser.UserName,
                              student.AspNetClass.ClassName,
                              StudentId = student.Id,
                              UsrId = student.AspNetUser.Id
                          }).Distinct().ToList();


            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ApprovedOrdersByDate(string FromDate, string ToDate)
        {

            DateTime dateTimeFrom = Convert.ToDateTime(FromDate);
            DateTime dateTimeTo = Convert.ToDateTime(ToDate);


            var result = (from Order in db.AspNetOrders
                          join OrderNotes in db.AspNetNotesOrders on Order.Id equals OrderNotes.OrderId
                          join student in db.AspNetStudents on OrderNotes.StudentID equals student.Id
                          where Order.Status == "Paid" && Order.PublishDate >= dateTimeFrom && Order.PublishDate <= dateTimeTo
                          select new
                          {
                              OrderID = Order.Id,
                              // NotesOrderID = OrderNotes.Id,
                              OrderType = Order.OrderType,
                              TotalAmount = Order.TotalAmount,
                              OrderStatus = Order.Status,
                              StudentName = student.AspNetUser.Name,
                              CourseType = student.CourseType,
                              student.AspNetUser.UserName,
                              student.AspNetClass.ClassName,
                              StudentId = student.Id,
                              UsrId = student.AspNetUser.Id
                          }).Distinct().ToList();

            return Json(result, JsonRequestBehavior.AllowGet);

            // return Json("", JsonRequestBehavior.AllowGet);
        }

        public ActionResult PendingOrdersDetails(int OrderId)
        {
            var result = (from Notes in db.AspNetNotes
                          join OrderNotes in db.AspNetNotesOrders on Notes.Id equals OrderNotes.NotesID
                          where OrderNotes.OrderId == OrderId
                          select new
                          {
                              Id = OrderNotes.Id,
                              Title = Notes.Title,
                              Discription = Notes.Description,
                              CourseType = Notes.CourseType,
                              Price = Notes.GrandTotal,
                              Quantity = OrderNotes.Quantity,
                              Status = OrderNotes.Status,
                              Total = Notes.GrandTotal,
                              //PhotoCopierPrice = Notes.PhotoCopierPrice,
                              // OAPrice = Notes.OAPrice,
                              PhotoCopierPrice = OrderNotes.PhotoCopierPrice,
                              OAPrice = OrderNotes.OAPrice,

                          }).ToList();


            List<OrderDetails> OrderDetailsList = new List<OrderDetails>();

            ViewBag.OrderId = OrderId;
            ViewBag.OrderType = db.AspNetOrders.Where(x => x.Id == OrderId).FirstOrDefault().OrderType;
            ViewBag.OrderStatus = db.AspNetOrders.Where(x => x.Id == OrderId).FirstOrDefault().Status;

            ViewBag.Discount = db.AspNetOrders.Where(x => x.Id == OrderId).FirstOrDefault().Discount;

            foreach (var item in result)
            {

                OrderDetails orderDetail = new OrderDetails();

                orderDetail.NotesOrderId = item.Id;
                orderDetail.OrderTitle = item.Title;
                orderDetail.Quantity = item.Quantity.Value;
                orderDetail.PhotoCopierPrice = item.PhotoCopierPrice.Value;
                orderDetail.OAPrice = item.OAPrice.Value;
                orderDetail.Total = (orderDetail.PhotoCopierPrice + orderDetail.OAPrice) * item.Quantity.Value;
                OrderDetailsList.Add(orderDetail);


            }

            return View(OrderDetailsList);
        }




        public class OrderDetails
        {

            public int NotesOrderId { get; set; }
            public string OrderTitle { get; set; }
            public int Quantity { get; set; }
            public double OAPrice { get; set; }
            public double Discount { get; set; }
            public double PhotoCopierPrice { get; set; }
            public double Total { get; set; }

        }

        public ActionResult RecentOrdersDetails(int OrderId)
        {

            var result = from Notes in db.AspNetNotes
                         join OrderNotes in db.AspNetNotesOrders on Notes.Id equals OrderNotes.NotesID
                         where OrderNotes.OrderId == OrderId
                         select new
                         {
                             Id = OrderNotes.Id,
                             Title = Notes.Title,
                             Discription = Notes.Description,
                             CourseType = Notes.CourseType,
                             Price = Notes.GrandTotal,
                             Quantity = OrderNotes.Quantity,
                             Status = OrderNotes.Status,
                             Total = Notes.GrandTotal * OrderNotes.Quantity,
                             PhotoCopierPrice = Notes.PhotoCopierPrice * OrderNotes.Quantity,
                             OAPrice = Notes.OAPrice * OrderNotes.Quantity,

                         };


            return Json(result, JsonRequestBehavior.AllowGet);
        }
        // GET: AspNetNotes/Create
        public ActionResult Create()
        {
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects.Where(x => x.ClassID == 1), "Id", "SubjectName");

            return View();
        }

        public ActionResult DeleteOrder(int OrderId)
        {
            var AllNotesOrderToDelete = db.AspNetNotesOrders.Where(x => x.OrderId == OrderId).ToList();

            db.AspNetNotesOrders.RemoveRange(AllNotesOrderToDelete);
            db.SaveChanges();

            AspNetOrder OrderToDelete = db.AspNetOrders.Where(x => x.Id == OrderId).FirstOrDefault();

            db.AspNetOrders.Remove(OrderToDelete);
            db.SaveChanges();

            return Json("", JsonRequestBehavior.AllowGet);
        }

        // POST: AspNetNotes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Title,Description,SubjectID,CourseType,NotesType,Price,CreationDate,Pages,PerPagePrice,photoCopierHidden")] AspNetNote aspNetNote)
        {
            if (aspNetNote.NotesType == "Notes")
            {
                aspNetNote.NotesType = Request.Form["NotesType"];

                aspNetNote.CourseType = Request.Form["CourseType"];
                aspNetNote.Pages = Convert.ToDouble(Request.Form["Pages"]);
                aspNetNote.PerPagePrice = Convert.ToDouble(Request.Form["perPagePrice"]);
                aspNetNote.BindingPrice = Convert.ToDouble(Request.Form["bindingPrice"]);
                aspNetNote.GrandTotal = Convert.ToDouble(Request.Form["grandTotalHidden"]);
                aspNetNote.OAPrice = Convert.ToDouble(Request.Form["oAHidden"]);
                aspNetNote.PhotoCopierPrice = Convert.ToDouble(Request.Form["photoCopierHidden"]);
                aspNetNote.CreationDate = GetLocalDateTime.GetLocalDateTimeFunction();

                if (ModelState.IsValid)
                {
                    string EncrID = aspNetNote.Id + aspNetNote.SubjectID + aspNetNote.Price.ToString();
                    aspNetNote.EncryptedID = Encrpt.Encrypt(EncrID, true);

                    //aspNetNote.EncryptedID.Replace('/', 's').Replace('-', 's').Replace('+', 's').Replace('%', 's').Replace('&', 's');

                    var newString = Regex.Replace(aspNetNote.EncryptedID, @"[^0-9a-zA-Z]+", "s");

                    aspNetNote.EncryptedID = newString;
                    db.AspNetNotes.Add(aspNetNote);
                    db.SaveChanges();

                    var NotesEncryption = db.AspNetNotes.Where(x => x.Id == aspNetNote.Id).FirstOrDefault();
                    NotesEncryption.EncryptedID = NotesEncryption.EncryptedID + aspNetNote.Id.ToString();
                    db.SaveChanges();

                    return RedirectToAction("Index");
                }
            }
            else if (aspNetNote.NotesType == "Books")
            {

                aspNetNote.NotesType = Request.Form["NotesType"];
                aspNetNote.OABookPercentage = Convert.ToDouble(Request.Form["OABookPercentage"]);
                aspNetNote.BindingPrice = Convert.ToDouble(Request.Form["bindingPrice"]);
                aspNetNote.OAPrice = Convert.ToDouble(Request.Form["oAHidden"]);
                aspNetNote.PhotoCopierPrice = Convert.ToDouble(Request.Form["photoCopierHidden"]);
                aspNetNote.GrandTotal = Convert.ToDouble(Request.Form["grandTotalHidden"]);

                aspNetNote.CourseType = Request.Form["CourseType"];

                string EncrID = aspNetNote.Id + aspNetNote.SubjectID + aspNetNote.Price.ToString();
                aspNetNote.EncryptedID = Encrpt.Encrypt(EncrID, true);


                var newString = Regex.Replace(aspNetNote.EncryptedID, @"[^0-9a-zA-Z]+", "s");

                // Lesson.EncryptedID.Replace('/', 's').Replace('-','s').Replace('+','s').Replace('%','s').Replace('&','s');
                aspNetNote.EncryptedID = newString;



                //   aspNetNote.EncryptedID.Replace('/', 's').Replace('-', 's').Replace('+', 's').Replace('%', 's').Replace('&', 's');



                aspNetNote.CreationDate = GetLocalDateTime.GetLocalDateTimeFunction();
                if (ModelState.IsValid)
                {
                    db.AspNetNotes.Add(aspNetNote);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }


            } //else if 
            else
            {
                ModelState.AddModelError("NotesType", "Please Select Notes Type");
            }

            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetNote.SubjectID);
            return View(aspNetNote);
        }
        //public ActionResult SaveReturnOrder(List<PlaceNotes> PlaceNotes, int StudentId )
        //{

        //    var dbTransaction = db.Database.BeginTransaction();
        //    try
        //    {
        //        var CheckOrder = db.AspNetOrders.Where(x => x.Id == OrderId).FirstOrDefault();
        //        if (CheckOrder.OrderType == "Postpaid")
        //        {


        //            var AllOrderNotes = db.AspNetNotesOrders.Where(x => x.OrderId == OrderId).ToList();
        //            var TotalToModify = 0.0;
        //            var TotalPhotoCopierToModify = 0.0;
        //            foreach (var item in PlaceNotes)
        //            {

        //                var OrderNoteToModify = AllOrderNotes.Where(x => x.Id == item.OrderNotesId).FirstOrDefault();

        //                if (item.Quantity == OrderNoteToModify.Quantity)
        //                { }
        //                else
        //                {
        //                    var Quantity = OrderNoteToModify.Quantity - item.Quantity;

        //                    var TotalPriceToModify = (OrderNoteToModify.PhotoCopierPrice + OrderNoteToModify.OAPrice) * Quantity;
        //                    var TotalPhotoCopierPriceToModify = OrderNoteToModify.PhotoCopierPrice * Quantity;

        //                    TotalToModify = TotalToModify + TotalPriceToModify.Value;
        //                    TotalPhotoCopierToModify = TotalPhotoCopierToModify + TotalPhotoCopierPriceToModify.Value;

        //                    OrderNoteToModify.Quantity = item.Quantity;
        //                    db.SaveChanges();

        //                    var OrderToModify = db.AspNetOrders.Where(x => x.Id == OrderId).FirstOrDefault();
        //                    OrderToModify.TotalAmount = OrderToModify.TotalAmount - Convert.ToInt32(TotalPriceToModify);
        //                    db.SaveChanges();
        //                }

        //                //   db.AspNetOrders.Where(x=>x.Id == OrderId)


        //            }//end of for each

        //            if (TotalToModify != 0.0 || TotalPhotoCopierToModify != 0.0)
        //            {


        //                var Student = db.AspNetNotesOrders.Where(x => x.OrderId == OrderId).FirstOrDefault().AspNetStudent;

        //                var NameOfStudent = Student.AspNetUser.Name;
        //                var classIdd = Student.AspNetClass.Id;
        //                var SessionIdd = db.AspNetClasses.Where(x => x.Id == classIdd).FirstOrDefault().SessionID;
        //                var SessionName = db.AspNetSessions.Where(x => x.Id == SessionIdd).FirstOrDefault().SessionName;

        //                var id = User.Identity.GetUserId();
        //                var username = db.AspNetUsers.Where(x => x.Id == id).Select(x => x.Name).FirstOrDefault();
        //                Voucher voucher = new Voucher();

        //                voucher.Name = "Notes returned by Student " + NameOfStudent + " Session Name " + SessionName;
        //                voucher.Notes = "Cash received for notes";
        //                voucher.Date = GetLocalDateTime.GetLocalDateTimeFunction();
        //                voucher.StudentId = Student.Id;

        //                voucher.CreatedBy = username;
        //                voucher.SessionID = SessionID;
        //                int? VoucherObj = db.Vouchers.Max(x => x.VoucherNo);

        //                voucher.VoucherNo = Convert.ToInt32(VoucherObj) + 1;
        //                db.Vouchers.Add(voucher);
        //                db.SaveChanges();



        //                var Leadger = db.Ledgers.Where(x => x.Name == "Admin Drawer").FirstOrDefault();
        //                int AdminDrawerId = Leadger.Id;
        //                decimal? CurrentBalance = Leadger.CurrentBalance;
        //                VoucherRecord voucherRecord = new VoucherRecord();
        //                decimal? AfterBalance = CurrentBalance - Convert.ToDecimal(TotalToModify); //OrderToModify.TotalAmount;
        //                voucherRecord.LedgerId = AdminDrawerId;
        //                voucherRecord.Type = "Cr";
        //                voucherRecord.Amount = Convert.ToDecimal(TotalToModify);
        //                voucherRecord.CurrentBalance = CurrentBalance;

        //                voucherRecord.AfterBalance = AfterBalance;
        //                voucherRecord.VoucherId = voucher.Id;
        //                voucherRecord.Description = "Notes returned by student (" + NameOfStudent + ") (" + SessionName + ")";
        //                Leadger.CurrentBalance = AfterBalance;
        //                db.VoucherRecords.Add(voucherRecord);
        //                db.SaveChanges();

        //                VoucherRecord voucherRecord1 = new VoucherRecord();

        //                var LeadgerNotes = db.Ledgers.Where(x => x.Name == "Notes").FirstOrDefault();

        //                decimal? CurrentBalanceOfNotes = LeadgerNotes.CurrentBalance;
        //                decimal? AfterBalanceOfNotes = CurrentBalanceOfNotes - Convert.ToDecimal(TotalToModify);//OrderToModify.TotalAmount;
        //                voucherRecord1.LedgerId = LeadgerNotes.Id;
        //                voucherRecord1.Type = "Dr";
        //                voucherRecord1.Amount = Convert.ToDecimal(TotalToModify);
        //                voucherRecord1.CurrentBalance = CurrentBalanceOfNotes;
        //                voucherRecord1.AfterBalance = AfterBalanceOfNotes;
        //                voucherRecord1.VoucherId = voucher.Id;
        //                voucherRecord1.Description = "Notes against order ID " + OrderId;
        //                LeadgerNotes.CurrentBalance = AfterBalanceOfNotes;

        //                db.VoucherRecords.Add(voucherRecord1);
        //                db.SaveChanges();



        //                VoucherRecord voucherRecord2 = new VoucherRecord();

        //                var IdofLedger = from Ledger in db.Ledgers
        //                                 join LedgerHd in db.LedgerHeads on Ledger.LedgerHeadId equals LedgerHd.Id
        //                                 where LedgerHd.Name == "Liabilities" && Ledger.Name == "Photocopier"
        //                                 select new
        //                                 {
        //                                     Ledger.Id

        //                                 };


        //                int photoCopierId = Convert.ToInt32(IdofLedger.FirstOrDefault().Id);
        //                var LeadgerPhotoCopierL = db.Ledgers.Where(x => x.Id == photoCopierId).FirstOrDefault();

        //                decimal? CurrentBalanceOfPhotoCopiter = LeadgerPhotoCopierL.CurrentBalance;
        //                decimal? AfterBalanceOfPhotoCopier = CurrentBalanceOfPhotoCopiter - Convert.ToDecimal(TotalPhotoCopierToModify);
        //                voucherRecord2.LedgerId = LeadgerPhotoCopierL.Id;
        //                voucherRecord2.Type = "Dr";
        //                voucherRecord2.Amount = Convert.ToDecimal(TotalPhotoCopierToModify);
        //                voucherRecord2.CurrentBalance = CurrentBalanceOfPhotoCopiter;
        //                voucherRecord2.AfterBalance = AfterBalanceOfPhotoCopier;
        //                voucherRecord2.VoucherId = voucher.Id;
        //                voucherRecord2.Description = "Notes against order ID " + OrderId;
        //                LeadgerPhotoCopierL.CurrentBalance = AfterBalanceOfPhotoCopier;
        //                db.VoucherRecords.Add(voucherRecord2);

        //                db.SaveChanges();

        //                VoucherRecord voucherRecord3 = new VoucherRecord();

        //                var IdofLedger1 = from Ledger in db.Ledgers
        //                                  join LedgerHd in db.LedgerHeads on Ledger.LedgerHeadId equals LedgerHd.Id
        //                                  where LedgerHd.Name == "Expense" && Ledger.Name == "Photocopier"
        //                                  select new
        //                                  {
        //                                      Ledger.Id
        //                                  };

        //                int photoCopierIdOfExpense = Convert.ToInt32(IdofLedger1.FirstOrDefault().Id);
        //                var LeadgerPhotoCopierE = db.Ledgers.Where(x => x.Id == photoCopierIdOfExpense).FirstOrDefault();

        //                decimal? CurrentBalanceOfPhotoCopiterE = LeadgerPhotoCopierE.CurrentBalance;
        //                decimal? AfterBalanceOfPhotoCopierE = CurrentBalanceOfPhotoCopiterE - Convert.ToDecimal(TotalPhotoCopierToModify);
        //                voucherRecord3.LedgerId = LeadgerPhotoCopierE.Id;
        //                voucherRecord3.Type = "Cr";
        //                voucherRecord3.Amount = Convert.ToDecimal(TotalPhotoCopierToModify);
        //                voucherRecord3.CurrentBalance = CurrentBalanceOfPhotoCopiterE;
        //                voucherRecord3.AfterBalance = AfterBalanceOfPhotoCopierE;
        //                voucherRecord3.VoucherId = voucher.Id;
        //                voucherRecord3.Description = "Notes against order ID " + OrderId;
        //                LeadgerPhotoCopierE.CurrentBalance = AfterBalanceOfPhotoCopierE;

        //                db.VoucherRecords.Add(voucherRecord3);
        //                db.SaveChanges();
        //                dbTransaction.Commit();
        //            } //end of if statement of checking total 

        //        }



        //    }//end of try block
        //    catch (Exception ex)
        //    {
        //        var msg = ex.Message;
        //        dbTransaction.Dispose();
        //    }


        //    return Json("", JsonRequestBehavior.AllowGet);

        //}

        public ActionResult SaveReturnOrder(List<PlaceNotes> PlaceNotes, int StudentId)
        {


            var dbTransaction = db.Database.BeginTransaction();
            try
            {
                var Order = PlaceNotes.FirstOrDefault();

                AspNetOrder NewOrder = new AspNetOrder();
                NewOrder.TotalAmount = -Order.Total;
                NewOrder.Status = "Returned";
                NewOrder.PublishDate = GetLocalDateTime.GetLocalDateTimeFunction();
                NewOrder.ApproveDate = GetLocalDateTime.GetLocalDateTimeFunction();
                NewOrder.OrderType = null;
                NewOrder.Discount = 0;

                db.AspNetOrders.Add(NewOrder);
                db.SaveChanges();

                AspNetNotesOrder NewNotesOrder = new AspNetNotesOrder();

                NewNotesOrder.NotesID = Order.NotesId;
                NewNotesOrder.StudentID = StudentId;
                NewNotesOrder.Quantity = Order.Quantity;
                NewNotesOrder.Status = "Returned";
                NewNotesOrder.CreationDate = GetLocalDateTime.GetLocalDateTimeFunction();
                NewNotesOrder.OrderId = NewOrder.Id;
                NewNotesOrder.OrderType = null;
                NewNotesOrder.PhotoCopierPrice = -Order.PhotoCopierPrice;
                NewNotesOrder.OAPrice = -Order.OAPrice;

                db.AspNetNotesOrders.Add(NewNotesOrder);
                db.SaveChanges();


                var TotalPhotoCopierToDelete = Order.PhotoCopierPrice * Order.Quantity;
                var TotalOAToDelete = Order.OAPrice * Order.Quantity;

                var Student = db.AspNetStudents.Where(x=>x.Id == StudentId).FirstOrDefault();


                var NameOfStudent = Student.AspNetUser.Name;
                var classIdd = Student.AspNetClass.Id;
                var SessionIdd = db.AspNetClasses.Where(x => x.Id == classIdd).FirstOrDefault().SessionID;
                var SessionName = db.AspNetSessions.Where(x => x.Id == SessionIdd).FirstOrDefault().SessionName;

                var id = User.Identity.GetUserId();
                var username = db.AspNetUsers.Where(x => x.Id == id).Select(x => x.Name).FirstOrDefault();


                Voucher voucher = new Voucher();

                voucher.Name = "Notes returned by Student " + NameOfStudent + " Session Name " + SessionName;
                voucher.Notes = "Cash received for notes";
                voucher.Date = GetLocalDateTime.GetLocalDateTimeFunction();
                voucher.StudentId = Student.Id;

                voucher.CreatedBy = username;
                voucher.SessionID = SessionID;
                int? VoucherObj = db.Vouchers.Max(x => x.VoucherNo);

                voucher.VoucherNo = Convert.ToInt32(VoucherObj) + 1;
                db.Vouchers.Add(voucher);
                db.SaveChanges();



                var Leadger = db.Ledgers.Where(x => x.Name == "Admin Drawer").FirstOrDefault();
                int AdminDrawerId = Leadger.Id;
                decimal? CurrentBalance = Leadger.CurrentBalance;
                VoucherRecord voucherRecord = new VoucherRecord();
                decimal? AfterBalance = CurrentBalance - Convert.ToDecimal(Order.Total); //OrderToModify.TotalAmount;
                voucherRecord.LedgerId = AdminDrawerId;
                voucherRecord.Type = "Cr";
                voucherRecord.Amount = Convert.ToDecimal(Order.Total);
                voucherRecord.CurrentBalance = CurrentBalance;

                voucherRecord.AfterBalance = AfterBalance;
                voucherRecord.VoucherId = voucher.Id;
                voucherRecord.Description = "Notes returned by student (" + NameOfStudent + ") (" + SessionName + ")";
                Leadger.CurrentBalance = AfterBalance;
                db.VoucherRecords.Add(voucherRecord);
                db.SaveChanges();

                VoucherRecord voucherRecord1 = new VoucherRecord();

                var LeadgerNotes = db.Ledgers.Where(x => x.Name == "Notes").FirstOrDefault();

                decimal? CurrentBalanceOfNotes = LeadgerNotes.CurrentBalance;
                decimal? AfterBalanceOfNotes = CurrentBalanceOfNotes - Convert.ToDecimal(Order.Total);//OrderToModify.TotalAmount;
                voucherRecord1.LedgerId = LeadgerNotes.Id;
                voucherRecord1.Type = "Dr";
                voucherRecord1.Amount = Convert.ToDecimal(Order.Total);
                voucherRecord1.CurrentBalance = CurrentBalanceOfNotes;
                voucherRecord1.AfterBalance = AfterBalanceOfNotes;
                voucherRecord1.VoucherId = voucher.Id;
                voucherRecord1.Description = "Notes against order ID " + NewNotesOrder.OrderId;
                LeadgerNotes.CurrentBalance = AfterBalanceOfNotes;

                db.VoucherRecords.Add(voucherRecord1);
                db.SaveChanges();



                VoucherRecord voucherRecord2 = new VoucherRecord();

                var IdofLedger = from Ledger in db.Ledgers
                                 join LedgerHd in db.LedgerHeads on Ledger.LedgerHeadId equals LedgerHd.Id
                                 where LedgerHd.Name == "Liabilities" && Ledger.Name == "Photocopier"
                                 select new
                                 {
                                     Ledger.Id

                                 };


                int photoCopierId = Convert.ToInt32(IdofLedger.FirstOrDefault().Id);
                var LeadgerPhotoCopierL = db.Ledgers.Where(x => x.Id == photoCopierId).FirstOrDefault();

                decimal? CurrentBalanceOfPhotoCopiter = LeadgerPhotoCopierL.CurrentBalance;
                decimal? AfterBalanceOfPhotoCopier = CurrentBalanceOfPhotoCopiter - Convert.ToDecimal(TotalPhotoCopierToDelete);
                voucherRecord2.LedgerId = LeadgerPhotoCopierL.Id;
                voucherRecord2.Type = "Dr";
                voucherRecord2.Amount = Convert.ToDecimal(TotalPhotoCopierToDelete);
                voucherRecord2.CurrentBalance = CurrentBalanceOfPhotoCopiter;
                voucherRecord2.AfterBalance = AfterBalanceOfPhotoCopier;
                voucherRecord2.VoucherId = voucher.Id;
                voucherRecord2.Description = "Notes against order ID " + NewNotesOrder.OrderId;
                LeadgerPhotoCopierL.CurrentBalance = AfterBalanceOfPhotoCopier;
                db.VoucherRecords.Add(voucherRecord2);

                db.SaveChanges();

                VoucherRecord voucherRecord3 = new VoucherRecord();

                var IdofLedger1 = from Ledger in db.Ledgers
                                  join LedgerHd in db.LedgerHeads on Ledger.LedgerHeadId equals LedgerHd.Id
                                  where LedgerHd.Name == "Expense" && Ledger.Name == "Photocopier"
                                  select new
                                  {
                                      Ledger.Id
                                  };

                int photoCopierIdOfExpense = Convert.ToInt32(IdofLedger1.FirstOrDefault().Id);
                var LeadgerPhotoCopierE = db.Ledgers.Where(x => x.Id == photoCopierIdOfExpense).FirstOrDefault();

                decimal? CurrentBalanceOfPhotoCopiterE = LeadgerPhotoCopierE.CurrentBalance;
                decimal? AfterBalanceOfPhotoCopierE = CurrentBalanceOfPhotoCopiterE - Convert.ToDecimal(TotalPhotoCopierToDelete);
                voucherRecord3.LedgerId = LeadgerPhotoCopierE.Id;
                voucherRecord3.Type = "Cr";
                voucherRecord3.Amount = Convert.ToDecimal(TotalPhotoCopierToDelete);
                voucherRecord3.CurrentBalance = CurrentBalanceOfPhotoCopiterE;
                voucherRecord3.AfterBalance = AfterBalanceOfPhotoCopierE;
                voucherRecord3.VoucherId = voucher.Id;
                voucherRecord3.Description = "Notes against order ID " + NewNotesOrder.OrderId;
                LeadgerPhotoCopierE.CurrentBalance = AfterBalanceOfPhotoCopierE;

                db.VoucherRecords.Add(voucherRecord3);
                db.SaveChanges();

                dbTransaction.Commit();




            }//end of try block
            catch (Exception ex)
            {
                var msg = ex.Message;
                dbTransaction.Dispose();
            }


            return Json("", JsonRequestBehavior.AllowGet);

        }
        public ActionResult SavePlaceOrder(List<PlaceNotes> PlaceNotes, int StudentId, double GrandTotal)
        {
            // return Json("", JsonRequestBehavior.AllowGet);

            var dbTransaction = db.Database.BeginTransaction();
            try
            {

                AspNetOrder order = new AspNetOrder();
                order.TotalAmount = Convert.ToInt32(GrandTotal);
                order.Status = "Pending";
                order.PublishDate = GetLocalDateTime.GetLocalDateTimeFunction();
                order.OrderType = "Postpaid";
                order.Discount = 0;

                db.AspNetOrders.Add(order);
                db.SaveChanges();


                foreach (var item in PlaceNotes)
                {
                    AspNetNotesOrder NotesOrder = new AspNetNotesOrder();

                    NotesOrder.NotesID = item.NotesId;
                    NotesOrder.StudentID = StudentId;
                    NotesOrder.Quantity = item.Quantity;
                    NotesOrder.Status = "Pending";
                    NotesOrder.CreationDate = GetLocalDateTime.GetLocalDateTimeFunction();
                    NotesOrder.OrderId = order.Id;
                    NotesOrder.OrderType = "Postpaid";
                    NotesOrder.PhotoCopierPrice = db.AspNetNotes.Where(x => x.Id == item.NotesId).FirstOrDefault().PhotoCopierPrice;
                    NotesOrder.OAPrice = db.AspNetNotes.Where(x => x.Id == item.NotesId).FirstOrDefault().OAPrice;

                    db.AspNetNotesOrders.Add(NotesOrder);
                    db.SaveChanges();

                }
                dbTransaction.Commit();
            }//end of try block
            catch (Exception ex)
            {
                var msg = ex.Message;
                dbTransaction.Dispose();

            }


            return Json("", JsonRequestBehavior.AllowGet);
        }
        public class PlaceNotes
        {
            public int NotesId { get; set; }
            public int OrderNotesId { get; set; }
            public string NotesType { get; set; }
            public double Price { get; set; }
            public double OAPrice { get; set; }
            public double PhotoCopierPrice { get; set; }
            public int Quantity { get; set; }
            public int Total { get; set; }

            public string NoteName { get; set; }

        }

        public ActionResult GetNotesSummary(string FromDate, string ToDate)
        {
            DateTime dateTimeFrom = Convert.ToDateTime(FromDate);


            DateTime dateTimeTo = Convert.ToDateTime(ToDate);

            // dateTimeTo = dateTimeTo.AddDays(1);
            // string toDateInString = dateTimeTo.ToString();

            var result = (from Notes in db.AspNetNotes
                          join OrderNotes in db.AspNetNotesOrders on Notes.Id equals OrderNotes.NotesID
                          join Order in db.AspNetOrders on OrderNotes.OrderId equals Order.Id
                          join student in db.AspNetStudents on OrderNotes.StudentID equals student.Id
                          //where Order.PublishDate >= dateTimeFrom && Order.PublishDate <= dateTimeTo
                          where Order.ApproveDate >= dateTimeFrom && Order.ApproveDate <= dateTimeTo
                          select new
                          {
                              OrderId = Order.Id,
                              OrderNotesId = OrderNotes.Id,
                              Title = Notes.Title,
                              Discription = Notes.Description,
                              CourseType = Notes.CourseType,
                              Price = Notes.GrandTotal,
                              Quantity = OrderNotes.Quantity,
                              Discount = Order.Discount,
                              Status = OrderNotes.Status,
                              StudentName = student.AspNetUser.Name,
                              ClassName = student.AspNetClass.ClassName,
                              PhotoCopierPrice = OrderNotes.PhotoCopierPrice,
                              OAPrice = OrderNotes.OAPrice,

                          }).Distinct().ToList();

            var AllOrders = db.AspNetOrders.Where(x => x.ApproveDate >= dateTimeFrom && x.ApproveDate <= dateTimeTo).ToList();

            List<SaleReport> SaleReportList = new List<SaleReport>();

            foreach (var item in AllOrders)
            {
                var SearchedOrders = result.Where(x => x.OrderId == item.Id).ToList();

                SaleReport saleReport = new SaleReport();
                saleReport.OrderId = item.Id;
                saleReport.StudentName = SearchedOrders.FirstOrDefault().StudentName;
                saleReport.ClassName = SearchedOrders.FirstOrDefault().ClassName;
                saleReport.Status = item.Status;
                if (item.Discount == null)
                {

                    saleReport.Discount = 0;
                }
                else
                {
                    saleReport.Discount = item.Discount.Value;

                }

                var TotalPhotoCopier = 0.0;
                var TotalOAPrice = 0.0;
                var Total = 0.0;
                var TotalDiscounted = 0.0;
                var TotalDiscountedOAPrice = 0.0;

                foreach (var item1 in SearchedOrders)
                {

                    TotalPhotoCopier = TotalPhotoCopier + (item1.PhotoCopierPrice.Value * item1.Quantity.Value);
                    TotalOAPrice = TotalOAPrice + (item1.OAPrice.Value * item1.Quantity.Value);
                    //  Total = Total +( TotalPhotoCopier + TotalOAPrice);

                }
                Total = TotalPhotoCopier + TotalOAPrice;
                TotalDiscounted = Total - saleReport.Discount;
                //  TotalDiscounted = Total - item.Discount.Value;
                //saleReport.Discount
                //TotalDiscountedOAPrice = TotalOAPrice - item.Discount.Value;
                TotalDiscountedOAPrice = TotalOAPrice - saleReport.Discount;


                saleReport.TotalPhotoCopier = TotalPhotoCopier;
                saleReport.TotalOA = TotalOAPrice;
                saleReport.Total = Total;
                saleReport.TotalDiscounted = TotalDiscounted;
                saleReport.TotalDiscountedOAPrice = TotalDiscountedOAPrice;


                SaleReportList.Add(saleReport);

            }

            return Json(SaleReportList, JsonRequestBehavior.AllowGet);

        }

        public class SaleReport
        {
            public int OrderId { get; set; }
            public string StudentName { get; set; }
            public string ClassName { get; set; }
            public string NotesTitle { get; set; }
            public string Descriptioin { get; set; }
            public string Status { get; set; }
            //public string CourseType { get; set; }
            public double TotalPhotoCopier { get; set; }
            public double TotalOA { get; set; }
            public double Total { get; set; }
            public double Discount { get; set; }

            public double TotalDiscounted { get; set; }
            public double TotalDiscountedOAPrice { get; set; }


        }


        // GET: AspNetNotes/Edit/5
        public ActionResult Edit(int id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetNote aspNetNote = db.AspNetNotes.Where(x => x.Id == id).FirstOrDefault();
            if (aspNetNote == null)
            {
                return HttpNotFound();
            }
            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetNote.SubjectID);
            return View(aspNetNote);
        }

        // POST: AspNetNotes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Title,Description,SubjectID,CourseType,Price,CreationDate,NotesType")] AspNetNote aspNetNote)
        {
            if (aspNetNote.NotesType == "Notes")

            {
                aspNetNote.NotesType = Request.Form["NotesType"];

                aspNetNote.CourseType = Request.Form["CourseType"];
                aspNetNote.Pages = Convert.ToDouble(Request.Form["Pages"]);
                aspNetNote.PerPagePrice = Convert.ToDouble(Request.Form["perPagePrice"]);
                aspNetNote.BindingPrice = Convert.ToDouble(Request.Form["bindingPrice"]);
                aspNetNote.GrandTotal = Convert.ToDouble(Request.Form["grandTotalHidden"]);
                aspNetNote.OAPrice = Convert.ToDouble(Request.Form["oAHidden"]);
                aspNetNote.PhotoCopierPrice = Convert.ToDouble(Request.Form["photoCopierHidden"]);
                aspNetNote.CreationDate = GetLocalDateTime.GetLocalDateTimeFunction();

                if (ModelState.IsValid)
                {
                    db.Entry(aspNetNote).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

            }
            else if (aspNetNote.NotesType == "Books")
            {
                aspNetNote.NotesType = Request.Form["NotesType"];
                aspNetNote.OABookPercentage = Convert.ToDouble(Request.Form["OABookPercentage"]);
                aspNetNote.BindingPrice = Convert.ToDouble(Request.Form["bindingPrice"]);
                aspNetNote.OAPrice = Convert.ToDouble(Request.Form["oAHidden"]);
                aspNetNote.PhotoCopierPrice = Convert.ToDouble(Request.Form["photoCopierHidden"]);
                aspNetNote.GrandTotal = Convert.ToDouble(Request.Form["grandTotalHidden"]);

                aspNetNote.CourseType = Request.Form["CourseType"];

                aspNetNote.CreationDate = GetLocalDateTime.GetLocalDateTimeFunction();
                if (ModelState.IsValid)
                {
                    db.Entry(aspNetNote).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

            }
            else
            {
                ModelState.AddModelError("NotesType", "Please Select Notes Type");
            }



            ViewBag.SubjectID = new SelectList(db.AspNetSubjects, "Id", "SubjectName", aspNetNote.SubjectID);
            return View(aspNetNote);
        }

        // GET: AspNetNotes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspNetNote aspNetNote = db.AspNetNotes.Find(id);
            if (aspNetNote == null)
            {
                return HttpNotFound();
            }
            return View(aspNetNote);
        }

        // POST: AspNetNotes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspNetNote aspNetNote = db.AspNetNotes.Find(id);
            db.AspNetNotes.Remove(aspNetNote);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
