using SEA_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SEA_Application.Controllers
{
    public class SEAApiController : Controller
    {

        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: SEAApi
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

            int SessionID = db.AspNetSessions.Where(x => x.SessionName == SessionName).Select(x => x.Id).FirstOrDefault();


            var credentials = db.ruffdatas.Where(x => x.StudentUserName == ID).OrderByDescending(x => x.id).FirstOrDefault();

            var UserID = db.AspNetUsers.Where(x => x.UserName == ID).Select(x => x.Id).FirstOrDefault();

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
                    //var URL = "https://portalofficersacademy.com/SEAApi/StudentLogin?token=" + token.Token + "&SessionID=" + SessionID;
                    var URL = "http://localhost:1331/SEAApi/StudentLogin?token=" + token.Token + "&SessionID=" + SessionID;

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
                        db.TokenAuthentications.Add(token);
                        db.SaveChanges();
                        return Json(URL, JsonRequestBehavior.AllowGet);
                    }
                    else
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

        public ActionResult StudentLogin(string token, string SessionID)
        {
            var Token = db.TokenAuthentications.Where(x => x.Token == token).FirstOrDefault();

            if(Token.Expire == "True")
            {
                string reason = "Token is expired";
                return RedirectToAction("Expire", "Home", reason);
            }

            Token.Expire = "True";
            db.SaveChanges();

            var user = db.ruffdatas.Where(x => x.StudentUserName == Token.AspNetUser.UserName).OrderByDescending(x => x.id).FirstOrDefault();
            LoginViewModel model = new LoginViewModel();
            model.Password = user.StudentPassword;
            model.UserName = user.StudentUserName;
            model.SessionID = SessionID;
            model.RememberMe = false;

            return RedirectToAction("Login", "SEALogin", model);
        }
    }
}