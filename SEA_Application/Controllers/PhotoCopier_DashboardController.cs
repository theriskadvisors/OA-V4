using SEA_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SEA_Application.Controllers
{
    public class PhotoCopier_DashboardController : Controller
    {

        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();
        int SessionID = Int32.Parse(SessionIDStaticController.GlobalSessionID);


        // GET: PhotoCopier_Dashboard
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Dashboard()
        {

            ViewBag.TotalPhotoCopierPrice = db.ApprovedOrdersForPhotoCopier().Select(x => x.TotalPhotoCopierPrice).Sum();


            return View("BlankPage");
        }
        public ActionResult ApprovedOrders()
        {

            var result = db.ApprovedOrdersForPhotoCopier().ToList();




            return Json(result, JsonRequestBehavior.AllowGet);

        }

        public ActionResult BlankPage()
        {
            return View();
        }


    }
}