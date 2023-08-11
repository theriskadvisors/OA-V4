using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using OfficeOpenXml;
using SEA_Application.CustomModel;
using SEA_Application.libs;
using SEA_Application.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using System.Web;
using System.Web.Mvc;

namespace SEA_Application.Controllers
{
    public class InventoryManagerController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
        //int SessionID = Convert.ToInt32(SessionIDStaticController.GlobalSessionID);
        // GET: InventoryManager
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            var CurrentUserId = User.Identity.GetUserId();
            var allMessages = (from a in db.AspNetMessages
                               join b in db.AspNetMessage_Receiver
                               on a.Id equals b.MessageID
                               where b.ReceiverID == CurrentUserId && b.Seen == "Not Seen"
                               join c in db.AspNetUsers
                              on a.SenderID equals c.Id
                               select new { a.Message, a.Time, c.Name }).ToList();
            List<Message> messages = new List<Message>();
            foreach (var item in allMessages)
            {
                Message m = new Message();
                m.Name = item.Name;
                m.message = item.Message;
                string monthName = item.Time.Value.ToString("MMM", CultureInfo.InvariantCulture);
                m.date = monthName + " " + item.Time.Value.Day + "," + item.Time.Value.Year;
                messages.Add(m);

            }
            ViewBag.Messages = messages;

            //var ty = (from classid in db.AspNetHomeworks
            //          join classname in db.AspNetClasses
            //          on classid.ClassId equals classname.Id
            //          where classid.PrincipalApproved_status == "Created" || classid.PrincipalApproved_status == "Edited"
            //          select new { classname.ClassName, classid.Date, classid.Id }).ToList().OrderByDescending(a => a.Date);
            //ViewBag.TotalStudents = (from uid in db.AspNetUsers
            //                         join sid in db.AspNetStudents
            //                         on uid.Id equals sid.StudentID
            //                         where uid.Status != "False"
            //                         select sid.StudentID).Count();


            //ViewBag.TotalSubjects = db.AspNetSubjects.Count();
            //ViewBag.TotalSessions = db.AspNetSessions.Count();
            //ViewBag.TotalTeachers = db.AspNetEmployees.Where(x => x.PositionAppliedFor == "TEACHER" && x.PositionAppliedFor == "Teacher").Count();

            ViewBag.TotalMessages = db.AspNetMessage_Receiver.Where(m => m.ReceiverID == CurrentUserId && m.Seen == "Not Seen").Count();
            ViewBag.TotalNotifications = db.AspNetNotification_User.Where(m => m.UserID == CurrentUserId && m.Seen == false).Count();

            var Classes1 = db.AspNetSessions.Select(x => x.SessionName).Distinct().ToList();
            List<string> classes = new List<string>();

            foreach (var clas in Classes1)
            {
                classes.Add(clas);
            }

            classes.Add("Old");
            ViewBag.AllClasses = classes;

            return View("BlankPage");
        }

        public class Message
        {
            public string Name { get; set; }
            public string message { get; set; }
            public string date { get; set; }
        }


    }
}