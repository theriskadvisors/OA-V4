using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SEA_Application.Models;
using System.Collections.Generic;
using OfficeOpenXml;
using System.Web.UI.WebControls;
using System.IO;
using System.Web.UI;
using Hangfire;
using System.Net;
using System.Text;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SEA_Application.Controllers
{
    public class ApiController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;


        public ApiController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            //var abc = 0;
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        // GET: Api
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetSessionList()
        {
            var session = db.AspNetSessions.Select(x => x.SessionName).Distinct().ToList();
            return Json(session, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Login(string ID, string Password, string SessionName, string MAC)
        {

            TimeZoneInfo PK_ZONE = TimeZoneInfo.FindSystemTimeZoneById("Pakistan Standard Time");
            DateTime today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, PK_ZONE);

           int SessionID = db.AspNetSessions.Where(x => x.SessionName == SessionName).Select(x=> x.Id).FirstOrDefault();


            var credentials = db.ruffdatas.Where(x=>x.StudentUserName == ID).OrderByDescending(x=>x.id).FirstOrDefault();

            var UserID = db.AspNetUsers.Where(x => x.UserName == ID).Select(x=> x.Id).FirstOrDefault();

            if (credentials == null)
                return Json("Invalid Login Attempt.", JsonRequestBehavior.AllowGet);

            if (credentials.StudentUserName == ID && credentials.StudentPassword == Password)
            {
                AspNetUsers_Session aus = db.AspNetUsers_Session.Where(x => x.SessionID == SessionID && x.UserID == UserID).FirstOrDefault();
                if (aus == null)
                {
                    ViewBag.SessionID = db.AspNetSessions.ToList().Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.SessionName,
                        Selected = (x.Status == "Active")
                    });
                    return Json("Kindly select your enrolled session to login.", JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var user = db.AspNetUsers.Where(x => x.UserName == ID).Select(x => x).FirstOrDefault();
                    Guid g = Guid.NewGuid();
                    string GuidString = Convert.ToBase64String(g.ToByteArray());
                    GuidString = GuidString.Replace("=", "");
                    GuidString = GuidString.Replace("+", "");

                    var token = new TokenAuthentication();

                    token.Expire = "False";
                    token.StudentID = user.Id;
                    token.CreationDate = today;
                    token.Token = GuidString;
                    var URL = "https://portalofficersacademy.com/Account/StudentLogin?token=" + token.Token + "&SessionID=" + SessionID;

                    if (user.PC_MAC == null && user.Mobile_MAC == null)
                    {
                        user.PC_MAC = MAC;
                        db.TokenAuthentications.Add(token);
                        db.SaveChanges();

                        return Json(URL, JsonRequestBehavior.AllowGet);
                    }
                    else if (user.PC_MAC != MAC && user.PC_MAC != null)
                    {
                        return Json("Kindly login with registered device", JsonRequestBehavior.AllowGet);
                    }
                    else if (user.Mobile_MAC != MAC && user.Mobile_MAC != null)
                    {
                        return Json("Kindly login with registered device", JsonRequestBehavior.AllowGet);
                    }
                    else if (user.PC_MAC == MAC || user.Mobile_MAC == MAC)
                    {
                        return Json(URL, JsonRequestBehavior.AllowGet);
                    }else
                    {
                        return Json("Kindly login with registered device", JsonRequestBehavior.AllowGet);
                    }
                }
            }
            else
            {
                return Json("invalid login Attempt", JsonRequestBehavior.AllowGet);
            }
        }

        public void LogTime(DateTime date, string userID)
        {
            var id = db.AspNetLoginTimes.Where(x => x.UserId == userID && x.StartTime.Day == date.Day).Select(x => x.ID).FirstOrDefault();
            if (id != 0)
            {
                db.AspNetLoginTimes.Where(x => x.ID == id).FirstOrDefault().StartTime = date;
                db.SaveChanges();
            }
            else
            {
                var username = db.AspNetUsers.Where(x => x.Id == userID).Select(x => x.Name).FirstOrDefault();
                AspNetLoginTime logintime = new AspNetLoginTime();
                logintime.UserName = username;
                logintime.UserId = userID;
                logintime.StartTime = date;
                db.AspNetLoginTimes.Add(logintime);
                db.SaveChanges();
            }
            return;
        }

        public async Task<ActionResult> StudentLogin(string token, string SessionID)
        {
            var Token = db.TokenAuthentications.Where(x => x.Token == token).FirstOrDefault();
            Token.Expire = "True";

            var user = db.ruffdatas.Where(x => x.StudentUserName == Token.AspNetUser.UserName).OrderByDescending(x => x.id).FirstOrDefault();
            LoginViewModel model = new LoginViewModel();
            model.Password = user.StudentPassword;
            model.UserName = user.StudentUserName;
            model.SessionID = SessionID;
            model.RememberMe = false;

            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:

                    var userID = SignInManager.AuthenticationManager.AuthenticationResponseGrant.Identity.GetUserId();
                    //AspNetUser res = db.AspNetUsers.Where(x => x.Id == userID).Select(x => x).FirstOrDefault();
                    //if (res.Status == "False")
                    //{
                    //    AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                    //    // return RedirectToAction("Index", "Home");
                    //    SessionIDStaticController.GlobalSessionID = model.SessionID;


                    //    ViewBag.SessionID = db.AspNetSessions.ToList().Select(x => new SelectListItem
                    //    {
                    //        Value = x.Id.ToString(),
                    //        Text = x.SessionName,
                    //        Selected = (x.Status == "Active")
                    //    });
                    //    ModelState.AddModelError("", "Admin has disabled your account.");
                    //    return View(model);
                    //}
                    var startdate = DateTime.Now;
                    LogTime(startdate, userID);
                    SessionIDStaticController.GlobalSessionID = model.SessionID;

                    //else { 
                    //    }
                    if (UserManager.IsInRole(userID, "Student"))
                    {
                        System.Web.HttpContext.Current.Session["StudentID"] = userID;
                        return RedirectToAction("Dashboard", "Student_Dashboard");

                    }else
                    {
                        return RedirectToAction("Login", "Account", new { returnUrl = ""});
                    }

                    //return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = "", RememberMe = model.RememberMe });
                case SignInStatus.Failure:
                default:
                    ViewBag.SessionID = db.AspNetSessions.ToList().Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.SessionName,
                        Selected = (x.Status == "Active")
                    });
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }
    }
}