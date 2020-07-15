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
    public class SEALoginController : Controller
    {

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        public SEALoginController()
        {
        }

        public SEALoginController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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
        // GET: SEALogin
        public ActionResult Index()
        {
            return View();
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

        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl = "")
        {
           
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true

            var result = await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:

                    var userID = SignInManager.AuthenticationManager.AuthenticationResponseGrant.Identity.GetUserId();
                    var startdate = DateTime.Now;
                    LogTime(startdate, userID);

                    SessionIDStaticController.GlobalSessionID = model.SessionID;

                    if (UserManager.IsInRole(userID, "Student"))
                    {
                        System.Web.HttpContext.Current.Session["StudentID"] = userID;
                        return RedirectToAction("Dashboard", "Student_Dashboard");
                    }

                    return RedirectToAction("");
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
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