using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SEA_Application.Models;
using Microsoft.AspNet.Identity;

namespace SEA_Application.Controllers
{
    public class CalendarController : Controller
    {

        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: Calendar
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetEvents()
        {
            try
            {
                var id = User.Identity.GetUserId();

                //var events = from x in db.Events
                //             where x.UserId == id
                //             select new { _id = x.EventID, description = x.Description, end = x.End, allDay = x.IsFullDay, textColor = "#ffffff", title = x.Subject, backgroundColor = x.ThemeColor, start = TimeZoneInfo.ConvertTimeFromUtc(x.Start, TimeZoneInfo.Local), x.IsPublic, instructor = x.AspNetUser.Name, subjectClass = x.SubjectClass, x.Url, type = "Appointment", calendar = "Sales", LessonName = x.AspnetLesson.Name, className = x.AspnetLesson.AspnetSubjectTopic.AspnetGenericBranchClassSubject.AspNetClass.Name, Section = x.AspnetLesson.AspnetSubjectTopic.AspnetGenericBranchClassSubject.AspNetSection.Name };

                var ThreeMonthBeforeDate = GetLocalDateTime.GetLocalDateTimeFunction().Value.AddMonths(-3);


                var events = db.Events.Where(x => x.UserId == id && x.Start >= ThreeMonthBeforeDate).Select(x => new
                { subjectName = x.AspnetLesson.AspnetSubjectTopic.GenericSubject.SubjectName, TimeTable = x.AspNetTimeTable.AspNetSubject.SubjectName, _id = x.EventID, description = x.Description, end = x.End, allDay = x.IsFullDay, textColor = "#ffffff", title = x.Subject, backgroundColor = x.ThemeColor, start = x.Start,LessonFullName = x.AspnetLesson.Name, x.IsPublic, instructor = x.AspNetUser.Name, type = "Appointment", calendar = "Sales", LessonName = x.Subject, className = x.AspNetSession.SessionName, subjectClass = x.AspNetSession.SessionName }).ToList();

                TimeZoneInfo PK_ZONE = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
                DateTime today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.Date, PK_ZONE);
                var eventstimatableList = new List<eventstimatable>();

                foreach (var item in events)
                {
                    var eventstimatable = new eventstimatable();
                    eventstimatable.title = item.title;
                    if (item.subjectName == null) { eventstimatable.className = "Old"; }
                    else { eventstimatable.className = item.subjectClass; }

                    if (item.subjectName == null) { eventstimatable.subjectClass = "Old"; }
                    else { eventstimatable.subjectClass = item.subjectClass; }

                    eventstimatable.LessonName = item.LessonName;
                    eventstimatable.LessonFullName = item.LessonFullName;
                    eventstimatable.calendar = item.calendar;
                    eventstimatable.instructor = item.instructor;
                    eventstimatable._id = item._id;
                    eventstimatable.description = item.description;
                    eventstimatable.end = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(item.end.Value, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")), TimeZoneInfo.Local);
                    eventstimatable.allDay = item.allDay;
                    eventstimatable.textColor = item.textColor;
                    eventstimatable.backgroundColor = item.backgroundColor;
                    eventstimatable.start = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(item.start, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")), TimeZoneInfo.Local);
                    eventstimatable.IsPublic = item.IsPublic;
                    eventstimatableList.Add(eventstimatable);
                }

                if (User.IsInRole("Student"))
                {
                    var eventstimatableStudentList = new List<eventstimatable>();
                    string subject = "";
                    foreach (var item in events)
                    {
                        var eventstimatable = new eventstimatable();
                        eventstimatable.title = item.title;
                        //eventstimatable.SecTitle = item.SecTitle;
                        //eventstimatable.Section = item.Section;
                        if (item.subjectName == null) { subject = item.TimeTable; }
                        else { subject = item.subjectName; }


                        eventstimatable.className = subject;
                        eventstimatable.LessonName = item.LessonName;
                        eventstimatable.LessonFullName = item.LessonFullName;
                        eventstimatable.calendar = item.calendar;
                        eventstimatable.subjectClass = subject;
                        eventstimatable.instructor = item.instructor;
                        eventstimatable._id = item._id;
                        eventstimatable.description = item.description;
                        eventstimatable.end = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(item.end.Value, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")), TimeZoneInfo.Local);
                        eventstimatable.allDay = item.allDay;
                        eventstimatable.textColor = item.textColor;
                        eventstimatable.backgroundColor = item.backgroundColor;
                        eventstimatable.start = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(item.start, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")), TimeZoneInfo.Local);
                        eventstimatable.IsPublic = item.IsPublic;
                        eventstimatableStudentList.Add(eventstimatable);
                    }
                    return new JsonResult { Data = eventstimatableStudentList, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
                }

                //var eventList = eventstimatableList.Select(x => new { title = x.title, className = x.className, LessonName = x.LessonName, calendar = x.calendar, subjectClass = x.subjectClass, instructor = x.instructor, _id = x._id, description = x.description, end = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(x.end.Value, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")), TimeZoneInfo.Local), allDay = x.allDay, textColor = x.textColor, backgroundColor = x.backgroundColor, start = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(x.start, TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time")), TimeZoneInfo.Local), IsPublic = x.IsPublic, });
                return new JsonResult { Data = eventstimatableList, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                var innerex = ex.InnerException.Message;
                var inner = ex.InnerException;
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public class eventstimatable
        {
            public string title { get; set; }
            public string Section { get; set; }
            public string className { get; set; }
            public string LessonName { get; set; }

            public string LessonFullName { get; set; }
            public string calendar { get; set; }
            public string Url { get; set; }
            public string subjectClass { get; set; }
            public string instructor { get; set; }
            public int _id { get; set; }
            public string description { get; set; }
            public string textColor { get; set; }
            public string backgroundColor { get; set; }
            public bool IsPublic { get; set; }
            public bool allDay { get; set; }
            public DateTime end { get; set; }
            public DateTime start { get; set; }

        }
    }
}