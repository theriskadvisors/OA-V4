using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using SEA_Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.XPath;
using static iTextSharp.tool.xml.html.HTML;


namespace SEA_Application.Controllers
{
    public class SaleDiscountController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        public ActionResult Index()
        {
            return View(db.SaleDiscounts.Where(x=>x.Name != "Bismarck Discount").ToList());
        }
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Create(SaleDiscount saleDiscount)
        {

            string IsActiveString = Request.Form["IsActive"];

            bool isActive = false;
            if(IsActiveString == "on")
            {
                isActive = true;
            }

            saleDiscount.IsActive = isActive;
            db.SaleDiscounts.Add(saleDiscount);
            db.SaveChanges();

            return RedirectToAction("Index");
        }
        [HttpGet]
        public ActionResult Edit(int id)
        {
            SaleDiscount saleDiscount = db.SaleDiscounts.Find(id);

            return View(saleDiscount);

        }
        public ActionResult Edit (SaleDiscount saleDiscount)
        {

            string IsActiveString = Request.Form["IsActive"];

            bool isActive = false;
            if (IsActiveString == "on")
            {
                isActive = true;
            }

           var saleDiscountToUpdate =  db.SaleDiscounts.Where(x => x.Id == saleDiscount.Id).FirstOrDefault();


            saleDiscountToUpdate.IsActive = isActive;
            saleDiscountToUpdate.Name = saleDiscount.Name;
            saleDiscountToUpdate.Description = saleDiscount.Description;
            saleDiscountToUpdate.Percentage = saleDiscount.Percentage;
            db.SaveChanges();

            return RedirectToAction("Index");

        }


        
       


        




    }
}