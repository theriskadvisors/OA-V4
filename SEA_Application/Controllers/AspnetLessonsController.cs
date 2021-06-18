using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using SEA_Application.Models;
using Microsoft.AspNet.Identity;
using System.Text.RegularExpressions;
using System.Linq.Dynamic;

namespace SEA_Application.Controllers
{
    public class AspnetLessonsController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: AspnetLessons
        public ActionResult Index()
        {
            var aspnetLessons = db.AspnetLessons.Include(a => a.AspnetSubjectTopic);
            return View(aspnetLessons.ToList());
        }

        // GET: AspnetLessons/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspnetLesson aspnetLesson = db.AspnetLessons.Find(id);
            if (aspnetLesson == null)
            {
                return HttpNotFound();
            }
            return View(aspnetLesson);
        }

        // GET: AspnetLessons/Create

        public ActionResult LoadSectionIdDDL()
        {
            var ClassList = db.AspNetSessions.OrderByDescending(x => x.Id).ToList().Select(x => new { x.Id, x.SessionName });

            string status = Newtonsoft.Json.JsonConvert.SerializeObject(ClassList);

            // return Json(SubjectsByClass, JsonRequestBehavior.AllowGet);
            return Content(status);

        }

        public ActionResult Create(int id)
        {
            if (id != 0)
            {
                AspnetSubjectTopic aspnetSubjectTopic = db.AspnetSubjectTopics.Find(id);


                int? SubjectId = aspnetSubjectTopic.SubjectId;
                GenericSubject Subject = db.GenericSubjects.Where(x => x.Id == SubjectId).FirstOrDefault();

                ViewBag.TopicExist = 1;

                ViewBag.TopicId = id;
                ViewBag.SubjectId = aspnetSubjectTopic.SubjectId;
                ViewBag.CTId = Subject.SubjectType;

                return View();

            }
            else
            {

                ViewBag.TopicId = null;
                ViewBag.SubjectId = null;
                ViewBag.CTId = null;
                ViewBag.TopicExist = 0;
                //   ViewBag.TopicId = new SelectList(db.AspnetSubjectTopics, "Id", "Name");
                // ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");

                return View();
            }

        }

        //public ActionResult CheckLessonOrderBy(string TopicId, string OrderBy)
        //{

        //    int TopId = Convert.ToInt32(TopicId);
        //    int OrderByValue = Convert.ToInt32(OrderBy);

        //    var TopicExist = "";
        //    AspnetLesson Lesson = db.AspnetLessons.Where(x => x.TopicId == TopId && x.OrderBy == OrderByValue).FirstOrDefault();

        //    if (Lesson == null)
        //    {
        //        TopicExist = "No";
        //    }
        //    else
        //    {
        //        TopicExist = "Yes";
        //    }


        //    return Json(TopicExist, JsonRequestBehavior.AllowGet);
        //}

        public ActionResult CheckLessonOrderBy(string TopicId, string OrderBy)
        {

            int TopId = Convert.ToInt32(TopicId);
            int OrderByValue = Convert.ToInt32(OrderBy);
            var Msg = "";
            var TopicExist = "";
            var Lessons = db.AspnetLessons.Where(x => x.TopicId == TopId && x.OrderBy == OrderByValue).ToList();

            if (Lessons.Count() == 0)
            {
                TopicExist = "No";
            }
            else
            {
                TopicExist = "Yes";
                var Names = "";
                foreach (var lesson in Lessons)
                {
                    Names = Names + lesson.Name + ",";
                }

                Names = Names.Substring(0, Names.Length - 1);

                Msg = "Selected order by value is already assigned to " + Names + " Lesson.";
            }

            return Json(new { TopicExist = TopicExist, Msg = Msg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewAttendance(int id, string BranchName, string ClassName, string SectionName, string SubjectName, string LessonName, string StartDate, string Type)
        {
            ViewBag.BranchName = BranchName;
            ViewBag.ClassName = ClassName;
            ViewBag.SectionName = SectionName;
            ViewBag.LessonID = id;
            ViewBag.Type = Type;
            ViewBag.SubjectName = SubjectName;
            ViewBag.LessonName = LessonName;
            ViewBag.StartDate = StartDate;
            return View();
        }
        public ActionResult GetLessonAttendance(int LessonID)
        {
            var lessonRecord = db.AspnetLessons.Where(x => x.Id == LessonID).FirstOrDefault();

            var data = (from track in db.StudentLessonTrackings.Where(x => x.LessonId == LessonID && x.IsCompleted != null)
                        join user in db.AspNetUsers on track.StudentId equals user.Id
                        select new
                        {
                            StudentName = user.Name,
                            LessonStatus = track.IsCompleted.ToString(),
                            LessonStartDate = track.StartDate.ToString()
                        }).ToList();
            var students = (from enrollment in db.Student_GenericSubjects
                            join lesson in db.AspnetLessons on enrollment.GenericSubjectId equals lesson.AspnetSubjectTopic.GenericSubject.Id
                            where enrollment.GenericSubject.SubjectType == lesson.AspnetSubjectTopic.GenericSubject.SubjectType

                            && lesson.Id == LessonID
                            select new { StudentName = enrollment.AspNetUser.Name, LessonStatus = "Absent", LessonStartDate = "" }).Distinct().ToList();

            foreach (var item in students)
            {
                var std = data.Where(x => x.StudentName.ToString() == item.StudentName.ToString()).FirstOrDefault();
                if (std == null)
                {
                    data.Add(item);
                }
            }
            return Json(data, JsonRequestBehavior.AllowGet);
            //return View();
        }

        public ActionResult GetSubjectsList(string CT)
        {



            //    Getall  db.GetSubjectsByType.

            var df = db.GetGenericSubjectsByType(CT).ToList();
            //  var df = db.GenericSubjects.Where(z=>z.SubjectType == CT).ToList();
            string status = Newtonsoft.Json.JsonConvert.SerializeObject(df);
            return Content(status);
        }
        // POST: AspnetLessons/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(LessonViewModel LessonViewModel)
        {
            AspnetLesson Lesson = new AspnetLesson();

            Lesson.Name = LessonViewModel.LessonName;
            Lesson.Video_Url = LessonViewModel.LessonVideoURL;
            Lesson.TopicId = LessonViewModel.TopicId;
            Lesson.DurationMinutes = LessonViewModel.LessonDuration;
            Lesson.IsActive = LessonViewModel.IsActive;
            Lesson.CreationDate = LessonViewModel.CreationDate;
            Lesson.Description = LessonViewModel.LessonDescription;
            Lesson.OrderBy = LessonViewModel.OrderBy;
            Lesson.CreationDate = DateTime.Now;
            Lesson.DaysAhead = LessonViewModel.DaysAhead;
            Lesson.DueDays = LessonViewModel.DueDays;

            string EncrID = Lesson.Name + Lesson.Description + Lesson.Id;
            Lesson.EncryptedID = Encrpt.Encrypt(EncrID, true);


            var newString = Regex.Replace(Lesson.EncryptedID, @"[^0-9a-zA-Z]+", "s");

            // Lesson.EncryptedID.Replace('/', 's').Replace('-','s').Replace('+','s').Replace('%','s').Replace('&','s');

            Lesson.EncryptedID = newString;
            db.AspnetLessons.Add(Lesson);
            db.SaveChanges();

            var LessonEncryption = db.AspnetLessons.Where(x => x.Id == Lesson.Id).FirstOrDefault();
            LessonEncryption.EncryptedID = LessonEncryption.EncryptedID + LessonEncryption.Id.ToString();
            db.SaveChanges();

            //Lesson_Session lessonSession = new Lesson_Session();
            //lessonSession.LessonId = Lesson.Id;
            //lessonSession.SessionId = LessonViewModel.SessionId;
            //lessonSession.StartDate = LessonViewModel.StartDate;
            //lessonSession.DueDate = LessonViewModel.DueDate;
            //db.Lesson_Session.Add(lessonSession);

            //db.SaveChanges();



            HttpPostedFileBase Assignment = Request.Files["Assignment"];
            HttpPostedFileBase Attachment1 = Request.Files["Attachment1"];
            HttpPostedFileBase Attachment2 = Request.Files["Attachment2"];
            HttpPostedFileBase Attachment3 = Request.Files["Attachment3"];

            if (Assignment.ContentLength > 0)
            {
                var fileName = Path.GetFileName(Assignment.FileName);
                Assignment.SaveAs(Server.MapPath("~/Content/StudentAssignments/") + fileName);
                AspnetStudentAssignment studentAssignment = new AspnetStudentAssignment();

                studentAssignment.FileName = fileName;

                studentAssignment.Name = LessonViewModel.AssignmentName;


                string DueDate = Convert.ToString(LessonViewModel.AssignmentDueDate);


                if (DueDate == "1/1/0001 12:00:00 AM")
                {
                    studentAssignment.DueDate = null;

                }
                else
                {

                    studentAssignment.DueDate = LessonViewModel.AssignmentDueDate;

                }


                studentAssignment.Description = LessonViewModel.AssignmentDescription;
                studentAssignment.CreationDate = DateTime.Now;
                studentAssignment.LessonId = Lesson.Id;

                db.AspnetStudentAssignments.Add(studentAssignment);
                db.SaveChanges();


            }

            if (Attachment1.ContentLength > 0)
            {
                var fileName = Path.GetFileName(Attachment1.FileName);
                Attachment1.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName);

                AspnetStudentAttachment studentAttachment1 = new AspnetStudentAttachment();

                studentAttachment1.Name = LessonViewModel.AttachmentName1;
                studentAttachment1.Path = fileName;
                studentAttachment1.CreationDate = DateTime.Now;
                studentAttachment1.LessonId = Lesson.Id;
                db.AspnetStudentAttachments.Add(studentAttachment1);
                db.SaveChanges();


            }
            if (Attachment2.ContentLength > 0)
            {

                var fileName = Path.GetFileName(Attachment2.FileName);
                Attachment2.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName);

                AspnetStudentAttachment studentAttachment2 = new AspnetStudentAttachment();

                studentAttachment2.Name = LessonViewModel.AttachmentName2;
                studentAttachment2.Path = fileName;
                studentAttachment2.CreationDate = DateTime.Now;
                studentAttachment2.LessonId = Lesson.Id;
                db.AspnetStudentAttachments.Add(studentAttachment2);

                db.SaveChanges();

            }

            if (Attachment3.ContentLength > 0)
            {

                var fileName = Path.GetFileName(Attachment3.FileName);
                Attachment3.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName);

                AspnetStudentAttachment studentAttachment3 = new AspnetStudentAttachment();

                studentAttachment3.Name = LessonViewModel.AttachmentName3;
                studentAttachment3.Path = fileName;
                studentAttachment3.CreationDate = DateTime.Now;
                studentAttachment3.LessonId = Lesson.Id;
                db.AspnetStudentAttachments.Add(studentAttachment3);
                db.SaveChanges();

            }

            if (LessonViewModel.LinkUrl1 != null)
            {
                AspnetStudentLink link1 = new AspnetStudentLink();

                link1.URL = LessonViewModel.LinkUrl1;
                link1.CreationDate = DateTime.Now;
                link1.LessonId = Lesson.Id;
                db.AspnetStudentLinks.Add(link1);
                db.SaveChanges();
            }

            if (LessonViewModel.LinkUrl2 != null)
            {
                AspnetStudentLink link2 = new AspnetStudentLink();

                link2.URL = LessonViewModel.LinkUrl2;
                link2.CreationDate = DateTime.Now;
                link2.LessonId = Lesson.Id;
                db.AspnetStudentLinks.Add(link2);
                db.SaveChanges();
            }


            if (LessonViewModel.LinkUrl3 != null)
            {
                AspnetStudentLink link3 = new AspnetStudentLink();

                link3.URL = LessonViewModel.LinkUrl3;
                link3.CreationDate = DateTime.Now;
                link3.LessonId = Lesson.Id;
                db.AspnetStudentLinks.Add(link3);
                db.SaveChanges();
            }



            return RedirectToAction("ViewTopicsAndLessons", "AspnetSubjectTopics");

        }
        public JsonResult SessionByLesson(int id)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var subs = (from session in db.Lesson_Session
                        where session.LessonId == id
                        select new { session.SessionId, session.LessonId }).ToList();


            return Json(subs, JsonRequestBehavior.AllowGet);

        }
        public ActionResult LessonSessionView()
        {


            return View("LessonSessionView");
        }

        public ActionResult GetLessonSessions(DataTablesParam param)
        {
            // var StartDateList =  db.Lesson_Session.Select( x => x.StartDate..ToString());

            //var StartDateList =  db.Lesson_Session.Select(x => string.Concat( x.StartDate.Value.Month.ToString() +"/"+x.StartDate.Value.Day.ToString()+"/"+x.StartDate.Value.Year.ToString()));


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


                    totalCount = (from lesson in db.AspnetLessons
                                  join lessonsesion in db.Lesson_Session on lesson.Id equals lessonsesion.LessonId
                                  select new
                                  {
                                      lessonsesion.Id,
                                      lesson.Name,
                                      lessonsesion.AspNetSession.SessionName,
                                      lessonsesion.StartDate,
                                      lessonsesion.DueDate,
                                      lesson.AspnetSubjectTopic.GenericSubject.SubjectName,
                                  }).Where(x => x.Name.ToLower().Contains(param.sSearch.ToLower()) || x.SessionName.ToLower().Contains(param.sSearch.ToLower()) || x.SubjectName.ToLower().Contains(param.sSearch.ToLower()) || string.Concat(x.StartDate.Value.Month.ToString() + "/" + x.StartDate.Value.Day.ToString() + "/" + x.StartDate.Value.Year.ToString()).Contains(param.sSearch) || string.Concat(x.DueDate.Value.Month.ToString() + "/" + x.DueDate.Value.Day.ToString() + "/" + x.DueDate.Value.Year.ToString()).Contains(param.sSearch) /*x.DueDate.ToString().Contains(param.sSearch)*/).ToList().Count();


                    var AllLessonSessions = (from lesson in db.AspnetLessons
                                             join lessonsesion in db.Lesson_Session on lesson.Id equals lessonsesion.LessonId
                                             select new
                                             {
                                                 lessonsesion.Id,
                                                 lesson.Name,
                                                 lessonsesion.AspNetSession.SessionName,
                                                 lessonsesion.StartDate,
                                                 lessonsesion.DueDate,
                                                 lesson.AspnetSubjectTopic.GenericSubject.SubjectName,
                                             }).Where(x => x.Name.ToLower().Contains(param.sSearch.ToLower()) || x.SessionName.ToLower().Contains(param.sSearch.ToLower()) || x.SubjectName.ToLower().Contains(param.sSearch.ToLower()) || string.Concat(x.StartDate.Value.Month.ToString() + "/" + x.StartDate.Value.Day.ToString() + "/" + x.StartDate.Value.Year.ToString()).Contains(param.sSearch) || string.Concat(x.DueDate.Value.Month.ToString() + "/" + x.DueDate.Value.Day.ToString() + "/" + x.DueDate.Value.Year.ToString()).Contains(param.sSearch)).OrderBy(x => x.Name).Skip((pageNo - 1) * param.iDisplayLength).Take(param.iDisplayLength).ToList();

                    return Json(new
                    {
                        aaData = AllLessonSessions,
                        sEcho = param.sEcho,
                        iTotalDisplayRecords = totalCount,
                        iTotalRecords = totalCount

                    }, JsonRequestBehavior.AllowGet);




                }
                else
                {

                    totalCount = (from lesson in db.AspnetLessons
                                  join lessonsesion in db.Lesson_Session on lesson.Id equals lessonsesion.LessonId
                                  select new
                                  {
                                      lessonsesion.Id,
                                      lesson.Name,
                                      lessonsesion.AspNetSession.SessionName,
                                      lessonsesion.StartDate,
                                      lessonsesion.DueDate,
                                      lesson.AspnetSubjectTopic.GenericSubject.SubjectName,
                                  }).ToList().Count();


                    var AllLessonSessions = (from lesson in db.AspnetLessons
                                             join lessonsesion in db.Lesson_Session on lesson.Id equals lessonsesion.LessonId
                                             select new
                                             {
                                                 lessonsesion.Id,
                                                 lesson.Name,
                                                 lessonsesion.AspNetSession.SessionName,
                                                 lessonsesion.StartDate,
                                                 lessonsesion.DueDate,
                                                 lesson.AspnetSubjectTopic.GenericSubject.SubjectName,
                                             }).ToList().Skip((pageNo - 1) * param.iDisplayLength).Take(param.iDisplayLength).ToList();

                    return Json(new
                    {
                        aaData = AllLessonSessions,
                        sEcho = param.sEcho,
                        iTotalDisplayRecords = totalCount,
                        iTotalRecords = totalCount

                    }, JsonRequestBehavior.AllowGet);

                }

            }
            catch (Exception ex)
            {
                var Msg = ex.Message;
                var inner = ex.InnerException.Message;
            }

            return Json("", JsonRequestBehavior.AllowGet);



        }
        [HttpGet]
        public ActionResult EditLessonSession(int id)
        {
            ViewBag.Id = id;
            var LS = db.Lesson_Session.Where(x => x.Id == id).FirstOrDefault();
            if (LS != null)
            {
                int? TopicId = db.AspnetLessons.Where(x => x.Id == LS.LessonId).FirstOrDefault().TopicId;
                int? SubjectId = db.AspnetSubjectTopics.Where(x => x.Id == TopicId).FirstOrDefault().SubjectId;
                string SubjectType = db.GenericSubjects.Where(x => x.Id == SubjectId).FirstOrDefault().SubjectType;
                //   ViewBag.SubId = new SelectList(db.GenericSubjects.Where(x => x.SubjectType == SubjectType), "Id", "SubjectName", SubjectId);

                ViewBag.SubId = new SelectList(db.GenericSubjects.Where(x => x.SubjectType == SubjectType), "Id", "SubjectName", SubjectId);
                ViewBag.TopicId = new SelectList(db.AspnetSubjectTopics.Where(x => x.SubjectId == SubjectId), "Id", "Name", TopicId);
                ViewBag.LessonId = new SelectList(db.AspnetLessons.Where(x => x.TopicId == TopicId), "Id", "Name", LS.LessonId);
                ViewBag.SessionId = new SelectList(db.AspNetSessions, "Id", "SessionName", LS.SessionId);
                ViewBag.CTId = SubjectType;
                var StartDate = Convert.ToDateTime(LS.StartDate);
                var StartDateInString = StartDate.ToString("yyyy-MM-dd");
                ViewBag.LessonStartDate = StartDateInString;
                ////Due Date
                var DueDate = Convert.ToDateTime(LS.DueDate);
                var DueDateInString = DueDate.ToString("yyyy-MM-dd");
                ViewBag.LessonDueDate = DueDateInString;
            }
            return View();
        }
        [HttpPost]
        public ActionResult EditLessonSession(int SessionId, int TopicId)
        {
            var LessonSessionId = Convert.ToInt64(Request.Form["LessonSessionId"]);
            var LessonSessionToDelete = db.Lesson_Session.Where(x => x.Id == LessonSessionId).FirstOrDefault();
            db.Lesson_Session.Remove(LessonSessionToDelete);
            db.SaveChanges();

            var SessionId1 = Request.Form["SessionId"];
            var LessonId = Request.Form["LessonId"];
            var StartDate = Request.Form["StartDate"];
            var DueDate = Request.Form["DueDate"];
            Lesson_Session ls = new Lesson_Session();
            ls.LessonId = Convert.ToInt32(LessonId);
            ls.SessionId = Convert.ToInt32(SessionId1);
            ls.StartDate = Convert.ToDateTime(StartDate);
            ls.DueDate = Convert.ToDateTime(DueDate);
            db.Lesson_Session.Add(ls);
            db.SaveChanges();

            var events = db.Events.Where(x => x.LessonID == ls.LessonId).ToList();

            foreach (var item in events)
            {
                item.Start = ls.StartDate.Value.AddHours(18);
                item.End = ls.StartDate.Value.AddHours(21);
                db.SaveChanges();
            }

            return RedirectToAction("LessonSessionView");
        }

        public ActionResult CreateLessonSession()
        {

            ViewBag.TopicId = new SelectList(db.AspnetSubjectTopics, "Id", "Name");

            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");

            ViewBag.LessonId = new SelectList(db.AspnetLessons, "Id", "Name");

            return View();

        }
        public ActionResult ScheduleTopic()
        {
            ViewBag.TopicId = new SelectList(db.AspnetSubjectTopics, "Id", "Name");

            ViewBag.ClassID = new SelectList(db.AspNetClasses, "Id", "ClassName");

            ViewBag.LessonId = new SelectList(db.AspnetLessons, "Id", "Name");


            return View();
        }

        [HttpPost]
        public ActionResult ScheduleTopic(int SessionId)
        {

            var SessionId1 = Request.Form["SessionId"];
            var LessonId = Request.Form["LessonId"];
            var StartDate = Request.Form["StartDate"];
            var DueDate = Request.Form["DueDate"];
            var TopId = Request.Form["TopId"];

            var TopicIdInt = Convert.ToInt32(TopId);

            var AllLessonIds = db.AspnetLessons.Where(x => x.TopicId == TopicIdInt).Select(x => x.Id).ToList();

            List<int> LessonIds = new List<int>();

            foreach (var id in AllLessonIds)
            {
                var LessonSession = db.Lesson_Session.Where(x => x.LessonId == id && x.SessionId == SessionId).FirstOrDefault();

                if (LessonSession != null)
                {
                    db.Lesson_Session.Remove(LessonSession);
                    db.SaveChanges();

                }
                //Event

                var newevent = db.Events.Where(x => x.LessonID == id && x.SessionID == SessionId).ToList();

                db.Events.RemoveRange(newevent);
                db.SaveChanges();

                //if (LessonSession == null)
                //{
                //    LessonIds.Add(id);
                //}
            }

            foreach (var lessonId in AllLessonIds)
            {
                Lesson_Session ls = new Lesson_Session();
                ls.LessonId = Convert.ToInt32(lessonId);
                ls.SessionId = Convert.ToInt32(SessionId1);

                ls.StartDate = Convert.ToDateTime(StartDate);
                ls.DueDate = Convert.ToDateTime(DueDate);

                db.Lesson_Session.Add(ls);
                db.SaveChanges();

                var lesson = db.AspnetLessons.Where(x => x.Id == ls.LessonId).FirstOrDefault();
                var Users = (from std in db.Student_GenericSubjects.Where(x => x.GenericSubject.Id == lesson.AspnetSubjectTopic.GenericSubject.Id)
                             join session in db.AspNetStudent_Session_class on std.StudentId equals session.AspNetStudent.StudentID
                             where session.SessionID == ls.SessionId && session.AspNetStudent.StudentID == std.StudentId && std.AspNetUser.Status != "False"
                             select std.StudentId).Distinct().ToList();
                //var Users = db.Student_GenericSubjects.Where(x => x.GenericSubject.Id == lesson.AspnetSubjectTopic.GenericSubject.Id).Select(x=> x.StudentId).ToList();
                string[] color = { "Red", "Blue", "Green", "Yellow", "Orange", "Purple", "Aqua" };

                var AdminsAndStaff = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Admin") || x.AspNetRoles.Select(y => y.Name).Contains("Staff")).Select(x => x.Id).ToList();

                foreach (var item in AdminsAndStaff)
                {
                    Users.Add(item);
                }

                foreach (var items in Users)
                {
                    Random random = new Random();
                    int colorcode = random.Next(1, 5);
                    var newEvent = new Event();
                    newEvent.UserId = items;
                    newEvent.IsFullDay = false;
                    newEvent.IsPublic = false;
                    newEvent.Subject = lesson.AspnetSubjectTopic.GenericSubject.SubjectName + "-" + lesson.AspnetSubjectTopic.GenericSubject.SubjectType;
                    newEvent.Description = lesson.Description;
                    newEvent.SessionID = db.AspNetUsers_Session.Where(x => x.UserID == items).Select(x => x.SessionID).FirstOrDefault();
                    newEvent.ThemeColor = color[colorcode];
                    newEvent.Start = ls.StartDate.Value; //Convert.ToDateTime(day[0] + " " + starttime);
                    newEvent.End = ls.StartDate.Value.AddDays(5);  //Convert.ToDateTime(day[0] + " " + Endtime);

                    newEvent.Start = ls.StartDate.Value; //.Value.AddDays(5); //Convert.ToDateTime(day[0] + " " + starttime);
                    newEvent.End = ls.StartDate.Value.AddDays(5);  //Convert.ToDateTime(day[0] + " " + Endtime);

                    newEvent.TimeTableId = null; // item.Id;
                    newEvent.LessonID = lesson.Id;
                    newEvent.SessionID = ls.SessionId;
                    db.Events.Add(newEvent);
                }
            }//added by me
            db.SaveChanges();
            return RedirectToAction("LessonSessionView");
        }



        [HttpPost]
        public ActionResult CreateLessonSession(int SessionId)
        {
            //var SessionId1 = Request.Form["SessionId"];

            var SessionId1 = Request.Form["SessionId"].Split(',').ToList();

            var LessonId = Request.Form["LessonId"];
            var StartDate = Request.Form["StartDate"];
            var DueDate = Request.Form["DueDate"];

            // var sessionId = Convert.ToInt32(SessionId1);
            var lessonId = Convert.ToInt32(LessonId);

            foreach (var item in SessionId1)
            {
                int sessionId = Convert.ToInt32(item);

                var LessonSession = db.Lesson_Session.Where(x => x.LessonId == lessonId && x.SessionId == sessionId).FirstOrDefault();

                if (LessonSession != null)
                {
                    db.Lesson_Session.Remove(LessonSession);
                    db.SaveChanges();
                }
            }

            foreach (var item1 in SessionId1)
            {
                int sessionId = Convert.ToInt32(item1);

                Lesson_Session ls = new Lesson_Session();
                ls.LessonId = Convert.ToInt32(LessonId);
                ls.SessionId = sessionId;
                ls.StartDate = Convert.ToDateTime(StartDate);
                ls.DueDate = Convert.ToDateTime(DueDate);

                db.Lesson_Session.Add(ls);
                db.SaveChanges();



                var lesson = db.AspnetLessons.Where(x => x.Id == ls.LessonId).FirstOrDefault();
                var Users = (from std in db.Student_GenericSubjects.Where(x => x.GenericSubject.Id == lesson.AspnetSubjectTopic.GenericSubject.Id)
                             join session in db.AspNetStudent_Session_class on std.StudentId equals session.AspNetStudent.StudentID
                             where session.SessionID == ls.SessionId
                             select std.StudentId).ToList();
                //var Users = db.Student_GenericSubjects.Where(x => x.GenericSubject.Id == lesson.AspnetSubjectTopic.GenericSubject.Id).Select(x=> x.StudentId).ToList();
                string[] color = { "Red", "Blue", "Green", "Yellow", "Orange", "Purple", "Aqua" };

                var AdminsAndStaff = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Admin") || x.AspNetRoles.Select(y => y.Name).Contains("Staff")).Select(x => x.Id).ToList();

                foreach (var item in AdminsAndStaff)
                {
                    Users.Add(item);
                }

                foreach (var items in Users)
                {
                    Random random = new Random();
                    int colorcode = random.Next(1, 5);
                    var newEvent = new Event();
                    newEvent.UserId = items;
                    newEvent.IsFullDay = false;
                    newEvent.IsPublic = false;
                    newEvent.Subject = lesson.AspnetSubjectTopic.GenericSubject.SubjectName + "-" + lesson.AspnetSubjectTopic.GenericSubject.SubjectType;
                    newEvent.Description = lesson.Description;
                    newEvent.SessionID = db.AspNetUsers_Session.Where(x => x.UserID == items).Select(x => x.SessionID).FirstOrDefault();
                    newEvent.ThemeColor = color[colorcode];
                    newEvent.Start = ls.StartDate.Value.AddHours(10); //Convert.ToDateTime(day[0] + " " + starttime);
                    newEvent.End = ls.DueDate.Value.AddHours(13);  //Convert.ToDateTime(day[0] + " " + Endtime);
                    newEvent.TimeTableId = null; // item.Id;
                    newEvent.LessonID = lesson.Id;
                    newEvent.SessionID = ls.SessionId;
                    db.Events.Add(newEvent);
                }


                db.SaveChanges();

            }

            return RedirectToAction("LessonSessionView");
        }
        public ActionResult DeleteLessonSession(int id)
        {
            var LessonSessionToDelete = db.Lesson_Session.Where(x => x.Id == id).FirstOrDefault();

            db.Lesson_Session.Remove(LessonSessionToDelete);
            db.SaveChanges();

            return RedirectToAction("LessonSessionView");

        }

        public ActionResult CheckLessonAlreadyScheduled(int TopicId, int SessionId)
        {
            var LessonExist = "";
            var ErrorMsg = "";
            var Button = "Disabled";

            List<int> LessonIds = db.AspnetLessons.Where(x => x.TopicId == TopicId).Select(x => x.Id).ToList();
            var LessonIdsCount = LessonIds.Count();

            if (LessonIdsCount != 0)
            {
                LessonExist = "Yes";

                Button = "Enabled";
                //var LessonSession = db.Lesson_Session.Where(x => x.LessonId == Lesson.Id && x.SessionId == SessionId).FirstOrDefault();

                var AllLessonSessionCount = (from lessonSession in db.Lesson_Session
                                             where lessonSession.SessionId == SessionId && LessonIds.Contains(lessonSession.LessonId.Value)
                                             select lessonSession).Count();

                if (LessonIdsCount != 0 && LessonIdsCount > AllLessonSessionCount)
                {
                    Button = "Enabled";

                    var count = LessonIdsCount - AllLessonSessionCount;

                    if (LessonIdsCount != count)
                    {

                        ErrorMsg = "New lesson found in selected topic and Session";

                    }

                    // ErrorMsg = "New lesson found in selected topic and section";
                }

                else if (LessonIdsCount != 0 && LessonIdsCount == AllLessonSessionCount)
                {
                    Button = "Enabled";
                    ErrorMsg = "Lessons are already scheduled for the selected topic and selected session. if you click continue system will delete the previous ones of this topic for the selected session.";
                }
                else
                {
                    ErrorMsg = "";
                }
            }
            else
            {
                Button = "Disabled";

                LessonExist = "No";
            }

            return Json(new { LessonExist = LessonExist, ErrorMsg = ErrorMsg, Button = Button }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult CheckLessonAlreadyScheduled1(int LessonId, string SessionId)
        {
            //var Error = "";

            List<string> Errors = new List<string>();

            var ListItems = SessionId.Split(',').ToList();

            foreach (var item in ListItems)
            {
                int sessionId = Convert.ToInt32(item);

                var LessonSession = db.Lesson_Session.Where(x => x.LessonId == LessonId && x.SessionId == sessionId).FirstOrDefault();

                if (LessonSession != null)
                {
                    var FromDate = LessonSession.StartDate.Value.ToString("MM/dd/yyyy");
                    var ToDate = LessonSession.DueDate.Value.ToString("MM/dd/yyyy");

                    var Error = "";

                    Error = "The selected lesson in already scheduled from " + FromDate + " to " + ToDate + " in " + LessonSession.AspNetSession.SessionName;

                    Errors.Add(Error);
                }
            }

            //. On Creation," +
            //  " the pervious schedule will automatically delete.

            if (Errors.Count() != 0)
            {

                Errors.Add("On Creation the pervious schedule lessons will be automatically deleted.");

            }

            //   return Json(new { Error, JsonRequestBehavior.AllowGet });

            return Json(Errors, JsonRequestBehavior.AllowGet);

        }


        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AspnetLesson aspnetLesson = db.AspnetLessons.Find(id);
            if (aspnetLesson == null)
            {
                return HttpNotFound();
            }

            AspnetStudentAssignment studentAssignment = db.AspnetStudentAssignments.Where(x => x.LessonId == aspnetLesson.Id).FirstOrDefault();
            List<AspnetStudentAttachment> studentAttachments = db.AspnetStudentAttachments.Where(x => x.LessonId == aspnetLesson.Id).ToList();
            List<AspnetStudentLink> studentLinks = db.AspnetStudentLinks.Where(x => x.LessonId == aspnetLesson.Id).ToList();
            LessonViewModel lessonViewModel = new LessonViewModel();
            lessonViewModel.LessonDescription = aspnetLesson.Description;
            lessonViewModel.LessonVideoURL = aspnetLesson.Video_Url;
            lessonViewModel.LessonName = aspnetLesson.Name;
            lessonViewModel.LessonDuration = aspnetLesson.DurationMinutes;
            lessonViewModel.DaysAhead = aspnetLesson.DaysAhead;
            lessonViewModel.DueDays = aspnetLesson.DueDays;
            //    Lesson_Session LessonSession = db.Lesson_Session.Where(x => x.LessonId == id).FirstOrDefault();

            lessonViewModel.IsActive = Convert.ToBoolean(aspnetLesson.IsActive);

            //var StartDate = Convert.ToDateTime(LessonSession.StartDate);

            //var StartDateInString = StartDate.ToString("yyyy-MM-dd");

            //ViewBag.LessonStartDate = StartDateInString;

            ////Due Date
            //var DueDate = Convert.ToDateTime(LessonSession.DueDate);

            //var DueDateInString = DueDate.ToString("yyyy-MM-dd");


            //ViewBag.LessonDueDate = DueDateInString;


            int? TopicId = aspnetLesson.TopicId;

            ViewBag.LessonDuration = aspnetLesson.DurationMinutes;

            int? SubjectId = db.AspnetSubjectTopics.Where(x => x.Id == TopicId).FirstOrDefault().SubjectId;
            GenericSubject Subject = db.GenericSubjects.Where(x => x.Id == SubjectId).FirstOrDefault();



            var CourseType = Subject.SubjectType;

            lessonViewModel.Id = aspnetLesson.Id;
            if (studentAssignment != null)
            {
                lessonViewModel.AssignmentName = studentAssignment.Name;
                lessonViewModel.AssignmentDescription = studentAssignment.Description;
                DateTime Date = Convert.ToDateTime(studentAssignment.DueDate);
                string date = Date.ToString("yyyy-MM-dd");

                ViewBag.AssignmentFileName = studentAssignment.FileName;

                lessonViewModel.AssignmentDueDate = studentAssignment.DueDate;
                ViewBag.Date = date;


            }


            int count = 1;

            foreach (var link in studentLinks)
            {

                if (count == 1)
                {

                    lessonViewModel.LinkUrl1 = link.URL;

                }
                else if (count == 2)
                {

                    lessonViewModel.LinkUrl2 = link.URL;

                }
                else if (count == 3)
                {
                    lessonViewModel.LinkUrl3 = link.URL;


                }
                else
                {

                }

                count++;

            }

            count = 1;
            foreach (var attachment in studentAttachments)
            {

                if (count == 1)
                {

                    lessonViewModel.AttachmentName1 = attachment.Name;
                    ViewBag.Attachment1FileName = attachment.Path;

                }
                else if (count == 2)
                {

                    lessonViewModel.AttachmentName2 = attachment.Name;
                    ViewBag.Attachment2FileName = attachment.Path;


                }
                else if (count == 3)
                {
                    lessonViewModel.AttachmentName3 = attachment.Name;
                    ViewBag.Attachment3FileName = attachment.Path;



                }
                else
                {

                }

                count++;

            }


            //  ViewBag.SecId = new SelectList(db.AspNetClasses, "Id", "ClassName", ClassId);
            // ViewBag.SubId = new SelectList(db.GenericSubjects.Where(x => x.SubjectType == Subject.SubjectType), "Id", "SubjectName", SubjectId);

            ViewBag.SubId = new SelectList(db.GenericSubjects.Where(x => x.SubjectType == Subject.SubjectType), "Id", "SubjectName", SubjectId);
            ViewBag.TopicId = new SelectList(db.AspnetSubjectTopics.Where(x => x.SubjectId == SubjectId), "Id", "Name", aspnetLesson.TopicId);
            // ViewBag.SessionId = new SelectList(db.AspNetSessions, "Id", "SessionName", LessonSession.SessionId);

            ViewBag.CTId = Subject.SubjectType;

            ViewBag.OrderBy = aspnetLesson.OrderBy;

            return View(lessonViewModel);
        }

        // POST: AspnetLessons/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(LessonViewModel LessonViewModel)
        {

            AspnetLesson Lesson = db.AspnetLessons.Where(x => x.Id == LessonViewModel.Id).FirstOrDefault();
            Lesson.Name = LessonViewModel.LessonName;
            Lesson.Video_Url = LessonViewModel.LessonVideoURL;
            Lesson.TopicId = LessonViewModel.TopicId;
            Lesson.DurationMinutes = LessonViewModel.LessonDuration;
            Lesson.Description = LessonViewModel.LessonDescription;
            Lesson.IsActive = LessonViewModel.IsActive;
            Lesson.OrderBy = LessonViewModel.OrderBy;
            Lesson.DaysAhead = LessonViewModel.DaysAhead;
            Lesson.DueDays = LessonViewModel.DueDays;
            db.SaveChanges();


            HttpPostedFileBase Assignment = Request.Files["Assignment"];
            HttpPostedFileBase Attachment1 = Request.Files["Attachment1"];
            HttpPostedFileBase Attachment2 = Request.Files["Attachment2"];
            HttpPostedFileBase Attachment3 = Request.Files["Attachment3"];

            var fileName = "";
            if (Assignment.ContentLength > 0)
            {
                fileName = Path.GetFileName(Assignment.FileName);
                Assignment.SaveAs(Server.MapPath("~/Content/StudentAssignments/") + fileName);

            }
            AspnetStudentAssignment studentAssignment = db.AspnetStudentAssignments.Where(x => x.LessonId == Lesson.Id).FirstOrDefault();

            if (studentAssignment != null)
            {

                if (fileName != "")
                {

                    studentAssignment.FileName = fileName;

                }

                studentAssignment.Name = LessonViewModel.AssignmentName;
                string DueDate = Convert.ToString(LessonViewModel.AssignmentDueDate);

                if (DueDate == "1/1/0001 12:00:00 AM")
                {
                    studentAssignment.DueDate = null;
                }
                else
                {
                    studentAssignment.DueDate = LessonViewModel.AssignmentDueDate;

                }

                studentAssignment.Description = LessonViewModel.AssignmentDescription;
                db.SaveChanges();
            }
            else
            {
                if (Assignment.ContentLength > 0)
                {

                    AspnetStudentAssignment studentAssignment1 = new AspnetStudentAssignment();

                    studentAssignment1.FileName = fileName;

                    studentAssignment1.Name = LessonViewModel.AssignmentName;


                    string DueDate = Convert.ToString(LessonViewModel.AssignmentDueDate);


                    if (DueDate == "1/1/0001 12:00:00 AM")
                    {
                        studentAssignment1.DueDate = null;

                    }
                    else
                    {

                        studentAssignment1.DueDate = LessonViewModel.AssignmentDueDate;

                    }


                    studentAssignment1.Description = LessonViewModel.AssignmentDescription;
                    studentAssignment1.CreationDate = DateTime.Now;
                    studentAssignment1.LessonId = Lesson.Id;

                    db.AspnetStudentAssignments.Add(studentAssignment1);
                    db.SaveChanges();



                }

            }


            List<AspnetStudentAttachment> studentAttachments = db.AspnetStudentAttachments.Where(x => x.LessonId == Lesson.Id).ToList();
            List<AspnetStudentLink> studentLinks = db.AspnetStudentLinks.Where(x => x.LessonId == Lesson.Id).ToList();



            //db.AspnetStudentAttachments.RemoveRange(studentAttachments);
            //db.SaveChanges();

            db.AspnetStudentLinks.RemoveRange(studentLinks);
            db.SaveChanges();

            SEA_DatabaseEntities db1 = new SEA_DatabaseEntities();

            List<AspnetStudentAttachment> studentAttachments1 = db1.AspnetStudentAttachments.Where(x => x.LessonId == Lesson.Id).ToList();

            int TotalAttachments = studentAttachments1.Count;

            if (TotalAttachments == 0)
            {
                if (Attachment1.ContentLength > 0)
                {
                    var fileName1 = Path.GetFileName(Attachment1.FileName);
                    Attachment1.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName1);

                    AspnetStudentAttachment studentAttachment1 = new AspnetStudentAttachment();

                    studentAttachment1.Name = LessonViewModel.AttachmentName1;
                    studentAttachment1.Path = fileName1;
                    studentAttachment1.CreationDate = DateTime.Now;
                    studentAttachment1.LessonId = Lesson.Id;
                    db.AspnetStudentAttachments.Add(studentAttachment1);
                    db.SaveChanges();

                }
                if (Attachment2.ContentLength > 0)
                {

                    var fileName1 = Path.GetFileName(Attachment2.FileName);
                    Attachment2.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName1);

                    AspnetStudentAttachment studentAttachment2 = new AspnetStudentAttachment();

                    studentAttachment2.Name = LessonViewModel.AttachmentName2;
                    studentAttachment2.Path = fileName1;
                    studentAttachment2.CreationDate = DateTime.Now;
                    studentAttachment2.LessonId = Lesson.Id;
                    db.AspnetStudentAttachments.Add(studentAttachment2);

                    db.SaveChanges();

                }

                if (Attachment3.ContentLength > 0)
                {

                    var fileName1 = Path.GetFileName(Attachment3.FileName);
                    Attachment3.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName1);

                    AspnetStudentAttachment studentAttachment3 = new AspnetStudentAttachment();

                    studentAttachment3.Name = LessonViewModel.AttachmentName3;
                    studentAttachment3.Path = fileName1;
                    studentAttachment3.CreationDate = DateTime.Now;
                    studentAttachment3.LessonId = Lesson.Id;
                    db.AspnetStudentAttachments.Add(studentAttachment3);
                    db.SaveChanges();

                }


            }
            else
            {

                if (TotalAttachments == 1)
                {
                    var FirstElement = studentAttachments1.ElementAt(0);
                    FirstElement.Name = LessonViewModel.AttachmentName1;

                    var FileName = FirstElement.Path;

                    if (Attachment1.ContentLength > 0)
                    {
                        var fileName1 = Path.GetFileName(Attachment1.FileName);
                        FileName = fileName1;
                        Attachment1.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName1);

                    }
                    FirstElement.Path = FileName;
                    db1.SaveChanges();

                    if (Attachment2.ContentLength > 0)
                    {

                        var fileName1 = Path.GetFileName(Attachment2.FileName);
                        Attachment2.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName1);

                        AspnetStudentAttachment studentAttachment2 = new AspnetStudentAttachment();

                        studentAttachment2.Name = LessonViewModel.AttachmentName2;
                        studentAttachment2.Path = fileName1;
                        studentAttachment2.CreationDate = DateTime.Now;
                        studentAttachment2.LessonId = Lesson.Id;
                        db.AspnetStudentAttachments.Add(studentAttachment2);

                        db.SaveChanges();

                    }

                    if (Attachment3.ContentLength > 0)
                    {

                        var fileName1 = Path.GetFileName(Attachment3.FileName);
                        Attachment3.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName1);

                        AspnetStudentAttachment studentAttachment3 = new AspnetStudentAttachment();

                        studentAttachment3.Name = LessonViewModel.AttachmentName3;
                        studentAttachment3.Path = fileName1;
                        studentAttachment3.CreationDate = DateTime.Now;
                        studentAttachment3.LessonId = Lesson.Id;
                        db.AspnetStudentAttachments.Add(studentAttachment3);
                        db.SaveChanges();

                    }

                }

                else if (TotalAttachments == 2)
                {

                    var FirstElement = studentAttachments1.ElementAt(0);
                    FirstElement.Name = LessonViewModel.AttachmentName1;

                    var FileName0 = FirstElement.Path;

                    if (Attachment1.ContentLength > 0)
                    {
                        var fileName1 = Path.GetFileName(Attachment1.FileName);
                        FileName0 = fileName1;
                        Attachment1.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName1);

                    }
                    FirstElement.Path = FileName0;
                    db1.SaveChanges();


                    var SecondElement = studentAttachments1.ElementAt(1);
                    SecondElement.Name = LessonViewModel.AttachmentName2;

                    var FileName1 = SecondElement.Path;

                    if (Attachment2.ContentLength > 0)
                    {
                        var fileName2 = Path.GetFileName(Attachment2.FileName);
                        FileName1 = fileName2;
                        Attachment2.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName2);

                    }
                    SecondElement.Path = FileName1;
                    db1.SaveChanges();



                    if (Attachment3.ContentLength > 0)
                    {

                        var fileName1 = Path.GetFileName(Attachment3.FileName);
                        Attachment3.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName1);

                        AspnetStudentAttachment studentAttachment3 = new AspnetStudentAttachment();

                        studentAttachment3.Name = LessonViewModel.AttachmentName3;
                        studentAttachment3.Path = fileName1;
                        studentAttachment3.CreationDate = DateTime.Now;
                        studentAttachment3.LessonId = Lesson.Id;
                        db.AspnetStudentAttachments.Add(studentAttachment3);
                        db.SaveChanges();

                    }

                }

                else
                {

                    var FirstElement = studentAttachments1.ElementAt(0);
                    FirstElement.Name = LessonViewModel.AttachmentName1;

                    var FileName0 = FirstElement.Path;

                    if (Attachment1.ContentLength > 0)
                    {
                        var fileName1 = Path.GetFileName(Attachment1.FileName);
                        FileName0 = fileName1;
                        Attachment1.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName1);

                    }
                    FirstElement.Path = FileName0;
                    db1.SaveChanges();


                    var SecondElement = studentAttachments1.ElementAt(1);
                    SecondElement.Name = LessonViewModel.AttachmentName2;

                    var FileName1 = SecondElement.Path;

                    if (Attachment2.ContentLength > 0)
                    {
                        var fileName2 = Path.GetFileName(Attachment2.FileName);
                        FileName1 = fileName2;
                        Attachment2.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName2);

                    }
                    SecondElement.Path = FileName1;
                    db1.SaveChanges();


                    var ThirdElement = studentAttachments1.ElementAt(2);
                    ThirdElement.Name = LessonViewModel.AttachmentName3;

                    var FileName2 = ThirdElement.Path;

                    if (Attachment3.ContentLength > 0)
                    {
                        var fileName3 = Path.GetFileName(Attachment3.FileName);
                        FileName2 = fileName3;
                        Attachment2.SaveAs(Server.MapPath("~/Content/StudentAttachments/") + fileName3);

                    }
                    ThirdElement.Path = FileName2;
                    db1.SaveChanges();


                }


            }



            if (LessonViewModel.LinkUrl1 != null)
            {
                AspnetStudentLink link1 = new AspnetStudentLink();

                link1.URL = LessonViewModel.LinkUrl1;
                link1.CreationDate = DateTime.Now;
                link1.LessonId = Lesson.Id;
                db.AspnetStudentLinks.Add(link1);
                db.SaveChanges();
            }

            if (LessonViewModel.LinkUrl2 != null)
            {
                AspnetStudentLink link2 = new AspnetStudentLink();

                link2.URL = LessonViewModel.LinkUrl2;
                link2.CreationDate = DateTime.Now;
                link2.LessonId = Lesson.Id;
                db.AspnetStudentLinks.Add(link2);
                db.SaveChanges();
            }


            if (LessonViewModel.LinkUrl3 != null)
            {
                AspnetStudentLink link3 = new AspnetStudentLink();

                link3.URL = LessonViewModel.LinkUrl3;
                link3.CreationDate = DateTime.Now;
                link3.LessonId = Lesson.Id;
                db.AspnetStudentLinks.Add(link3);
                db.SaveChanges();
            }


            return RedirectToAction("ViewTopicsAndLessons", "AspnetSubjectTopics");
        }

        public ActionResult SessionPrePlan()
        {



            return View();
        }
        public JsonResult GetTopicLessonScheduled(int SubID, string Date)
        {


            var FirstDate = Convert.ToDateTime(Date);

            var AllTopics = db.AspnetSubjectTopics.Where(x => x.SubjectId == SubID).OrderBy(x => x.OrderBy).Select(x => new { x.Id, x.Name, x.OrderBy }).ToList();

            List<ScheduledLessonsOfTopic> List = new List<ScheduledLessonsOfTopic>();
            int count = 0;
            var PerviousDate = FirstDate;

            foreach (var Topic in AllTopics)
            {
                ScheduledLessonsOfTopic Schedule = new ScheduledLessonsOfTopic();

                Schedule.TopicId = Topic.Id;
                Schedule.TopicName = Topic.Name;

                var AllLessons = db.AspnetLessons.Where(x => x.TopicId == Topic.Id).OrderBy(x => x.OrderBy).ToList();


                foreach (var Lesson in AllLessons)
                {
                    ScheduledLesson ScheduleLesson = new ScheduledLesson();


                    if (count == 0)
                    {
                        ScheduleLesson.LessonId = Lesson.Id;
                        ScheduleLesson.LessonName = Lesson.Name;
                        ScheduleLesson.OrderBy = Lesson.OrderBy;
                        // ScheduleLesson.Date = FirstDate.ToString();
                        string[] DateArray = FirstDate.ToString().Split('/');
                        string[] yeararray = DateArray[2].Split(' ');
                        ScheduleLesson.Date = yeararray[0] + "-" + DateArray[0] + "-" + DateArray[1];

                        //DueDate Section
                        var DueDays = Lesson.DueDays;

                        var DueDate = FirstDate;

                        if (DueDays != null)
                        {
                            DueDate = FirstDate.AddDays(Lesson.DueDays.Value);
                        }
                        else
                        {
                            DueDate = FirstDate.AddDays(0);

                        }


                        string[] DueDateArray = DueDate.ToString().Split('/');
                        string[] DueDateyeararray = DateArray[2].Split(' ');

                        ScheduleLesson.DueDate = DueDateyeararray[0] + "-" + DueDateArray[0] + "-" + DueDateArray[1];


                        count++;
                    }


                    else
                    {

                        ScheduleLesson.LessonId = Lesson.Id;
                        ScheduleLesson.LessonName = Lesson.Name;
                        ScheduleLesson.OrderBy = Lesson.OrderBy;
                        //  ScheduleLesson.Date = PerviousDate.AddDays(Lesson.DaysAhead.Value).ToString();

                        var DaysAhead = Lesson.DaysAhead;
                        if (DaysAhead != null)
                        {

                            PerviousDate = PerviousDate.AddDays(Lesson.DaysAhead.Value);

                        }
                        else
                        {
                            PerviousDate = PerviousDate.AddDays(0);

                        }

                        string[] DateArray = PerviousDate.ToString().Split('/');
                        string[] yeararray = DateArray[2].Split(' ');

                        ScheduleLesson.Date = yeararray[0] + "-" + DateArray[0] + "-" + DateArray[1];


                        //DueDate Section
                        var DueDays = Lesson.DueDays;

                        var DueDate = PerviousDate;

                        if (DueDays != null)
                        {
                            DueDate = PerviousDate.AddDays(Lesson.DueDays.Value);
                        }
                        else
                        {
                            DueDate = PerviousDate.AddDays(0);

                        }


                        string[] DueDateArray = DueDate.ToString().Split('/');
                        string[] DueDateyeararray = DateArray[2].Split(' ');

                        ScheduleLesson.DueDate = DueDateyeararray[0] + "-" + DueDateArray[0] + "-" + DueDateArray[1];



                        //  PerviousDate = PerviousDate.AddDays(Lesson.DaysAhead.Value);

                    }

                    Schedule.ScheduledLessons.Add(ScheduleLesson);

                }


                List.Add(Schedule);

            }


            return Json(List, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveLessonsSchedule(int SessionId, int SubjectId, List<ScheduledLesson> LessonsList)
        {
            string Msg = "Success";

            try
            {
                var AllLessonSessionToDelete = db.Lesson_Session.Where(x => x.AspnetLesson.AspnetSubjectTopic.GenericSubject.Id == SubjectId && x.SessionId == SessionId).ToList();

                if (AllLessonSessionToDelete.Count() != 0)
                {


                    db.Lesson_Session.RemoveRange(AllLessonSessionToDelete);
                    db.SaveChanges();


                    var newevents = db.Events.Where(x => x.AspnetLesson.AspnetSubjectTopic.GenericSubject.Id == SubjectId && x.SessionID == SessionId).ToList();

                    db.Events.RemoveRange(newevents);
                    db.SaveChanges();
                    
                }

                List<Lesson_Session> LessonSessionList = new List<Lesson_Session>();

                foreach (var item in LessonsList)
                {

                    Lesson_Session LessonSession = new Lesson_Session();


                    LessonSession.LessonId = item.LessonId;
                    LessonSession.StartDate = Convert.ToDateTime(item.Date);
                    LessonSession.DueDate = Convert.ToDateTime(item.DueDate);
                    LessonSession.SessionId = SessionId;

                    db.Lesson_Session.Add(LessonSession);
                    db.SaveChanges();

                   


                    var lesson = db.AspnetLessons.Where(x => x.Id == LessonSession.LessonId).FirstOrDefault();
                    var Users = (from std in db.Student_GenericSubjects.Where(x => x.GenericSubject.Id == lesson.AspnetSubjectTopic.GenericSubject.Id)
                                 join session in db.AspNetStudent_Session_class on std.StudentId equals session.AspNetStudent.StudentID
                                 where session.SessionID == LessonSession.SessionId
                                 select std.StudentId).ToList();
                    //var Users = db.Student_GenericSubjects.Where(x => x.GenericSubject.Id == lesson.AspnetSubjectTopic.GenericSubject.Id).Select(x=> x.StudentId).ToList();
                    string[] color = { "Red", "Blue", "Green", "Yellow", "Orange", "Purple", "Aqua" };

                    var AdminsAndStaff = db.AspNetUsers.Where(x => x.AspNetRoles.Select(y => y.Name).Contains("Admin") || x.AspNetRoles.Select(y => y.Name).Contains("Staff")).Select(x => x.Id).ToList();

                    foreach (var item1 in AdminsAndStaff)
                    {
                        Users.Add(item1);
                    }

                    foreach (var items in Users)
                    {
                        Random random = new Random();
                        int colorcode = random.Next(1, 5);
                        var newEvent = new Event();
                        newEvent.UserId = items;
                        newEvent.IsFullDay = false;
                        newEvent.IsPublic = false;
                        newEvent.Subject = lesson.AspnetSubjectTopic.GenericSubject.SubjectName + "-" + lesson.AspnetSubjectTopic.GenericSubject.SubjectType;
                        newEvent.Description = lesson.Description;
                        newEvent.SessionID = db.AspNetUsers_Session.Where(x => x.UserID == items).Select(x => x.SessionID).FirstOrDefault();
                        newEvent.ThemeColor = color[colorcode];
                        newEvent.Start = LessonSession.StartDate.Value.AddHours(10); //Convert.ToDateTime(day[0] + " " + starttime);
                        newEvent.End = LessonSession.DueDate.Value.AddHours(13);  //Convert.ToDateTime(day[0] + " " + Endtime);
                        newEvent.TimeTableId = null; // item.Id;
                        newEvent.LessonID = lesson.Id;
                        newEvent.SessionID = LessonSession.SessionId;
                        db.Events.Add(newEvent);
                    }


                    db.SaveChanges();

                    // LessonSessionList.Add(LessonSession);

                }
                // LessonSessionList.AddRange(LessonSessionList);

                //db.Lesson_Session.AddRange(LessonSessionList);
                ///db.SaveChanges();



            }
            catch (Exception ex)
            {
                Msg = "Error";
            }



            return Json(Msg, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckSessionAlreadyScheduled(int SessionId, int SubjectId)
        {

            //  var AllSubjectTopics =  db.AspnetSubjectTopics.Where(x => x.Id == SubjectId).Select(x=>x.Id).ToList();
            var Msg = "Success";
            var AllLessonSession = db.Lesson_Session.Where(x => x.AspnetLesson.AspnetSubjectTopic.GenericSubject.Id == SubjectId && x.SessionId == SessionId).ToList();

            if (AllLessonSession.Count() != 0)
            {
                //    Msg = "Selected session is already pre-planned for the selected subject if continue it will automatically delete the pervious lessons planned for the selected subject of selected session .";
                 Msg = "The selected session is already pre-planned for the selected subject, if you click on create, the system will delete the previous lessons planned for the selected subject of selected session.";
            }

            return Json(Msg, JsonRequestBehavior.AllowGet);
        }

        public class ScheduledLessonsOfTopic
        {
            public int TopicId { get; set; }
            public string TopicName { get; set; }

            public List<ScheduledLesson> ScheduledLessons = new List<ScheduledLesson>();
        }
        public class ScheduledLesson
        {
            public int LessonId { get; set; }
            public string LessonName { get; set; }

            public int? OrderBy { get; set; }
            public string Date { get; set; }
            public string DueDate { get; set; }
        }


        // GET: AspnetLessons/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AspnetLesson aspnetLesson = db.AspnetLessons.Find(id);
            if (aspnetLesson == null)
            {
                return HttpNotFound();
            }
            return View(aspnetLesson);
        }

        // POST: AspnetLessons/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AspnetLesson aspnetLesson = db.AspnetLessons.Find(id);
            db.AspnetLessons.Remove(aspnetLesson);
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

        public ActionResult DeleteLessons(int? id)
        {
            int? LessonId = id;

            AspnetLesson LessonToDelete = db.AspnetLessons.Where(x => x.Id == LessonId).FirstOrDefault();

            if (LessonToDelete != null)
            {
                IEnumerable<AspnetComment> CommentsToDelete = db.AspnetComment_Head.Where(x => x.LessonId == LessonId).SelectMany(x => x.AspnetComments);
                db.AspnetComments.RemoveRange(CommentsToDelete);
                db.SaveChanges();

                List<AspnetComment_Head> ListCommentHeadToDelete = db.AspnetComment_Head.Where(x => x.LessonId == LessonId).ToList();
                db.AspnetComment_Head.RemoveRange(ListCommentHeadToDelete);
                db.SaveChanges();

                var AssignmentToDelete = db.AspnetStudentAssignments.Where(x => x.LessonId == LessonId).FirstOrDefault();
                if (AssignmentToDelete != null)
                {

                    db.AspnetStudentAssignments.Remove(AssignmentToDelete);
                    db.SaveChanges();
                }

                List<AspnetStudentAttachment> StudentAttachmentListToDelete = db.AspnetStudentAttachments.Where(x => x.LessonId == LessonId).ToList();
                db.AspnetStudentAttachments.RemoveRange(StudentAttachmentListToDelete);
                db.SaveChanges();


                List<AspnetStudentLink> StudentLinkListToDelete = db.AspnetStudentLinks.Where(x => x.LessonId == LessonId).ToList();
                db.AspnetStudentLinks.RemoveRange(StudentLinkListToDelete);
                db.SaveChanges();

                List<AspnetStudentAssignmentSubmission> StudentAssignmentSubmissionListToDelete = db.AspnetStudentAssignmentSubmissions.Where(x => x.LessonId == LessonId).ToList();
                db.AspnetStudentAssignmentSubmissions.RemoveRange(StudentAssignmentSubmissionListToDelete);
                db.SaveChanges();

                List<StudentLessonTracking> StudentLessonTrackingListToDelete = db.StudentLessonTrackings.Where(x => x.LessonId == LessonId).ToList();
                db.StudentLessonTrackings.RemoveRange(StudentLessonTrackingListToDelete);
                db.SaveChanges();

                List<Student_Quiz_Scoring> StudentQuizScoringToDelete = db.AspnetQuestions.Where(x => x.LessonId == LessonId).SelectMany(x => x.Student_Quiz_Scoring).ToList();
                db.Student_Quiz_Scoring.RemoveRange(StudentQuizScoringToDelete);
                db.SaveChanges();

                List<Lesson_Session> LessonSessionToDelete = db.Lesson_Session.Where(x => x.LessonId == LessonId).ToList();
                db.Lesson_Session.RemoveRange(LessonSessionToDelete);
                db.SaveChanges();

                List<Quiz_Topic_Questions> QuizTopicQuestionnsToDelete = db.AspnetQuestions.Where(x => x.LessonId == LessonId).SelectMany(x => x.Quiz_Topic_Questions).ToList();
                db.Quiz_Topic_Questions.RemoveRange(QuizTopicQuestionnsToDelete);
                db.SaveChanges();

                List<AspnetQuestion> QuestionListToDelete = db.AspnetQuestions.Where(x => x.LessonId == LessonId).ToList();
                db.AspnetQuestions.RemoveRange(QuestionListToDelete);
                db.SaveChanges();

                List<Event> Event = db.Events.Where(x => x.LessonID == LessonId).ToList();
                db.Events.RemoveRange(Event);
                db.SaveChanges();

                db.AspnetLessons.Remove(LessonToDelete);
                db.SaveChanges();

            }

            return RedirectToAction("ViewTopicsAndLessons", "AspnetSubjectTopics");

        }
    }
}
