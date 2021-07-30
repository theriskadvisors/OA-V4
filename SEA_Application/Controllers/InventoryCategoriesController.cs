using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SEA_Application.Models;

namespace SEA_Application.Controllers
{
    public class InventoryCategoriesController : Controller
    {
        private SEA_DatabaseEntities db = new SEA_DatabaseEntities();

        // GET: InventoryCategories
        public ActionResult Index()
        {
            return View(db.InventoryCategories.ToList());
        }

        // GET: InventoryCategories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryCategory inventoryCategory = db.InventoryCategories.Find(id);
            if (inventoryCategory == null)
            {
                return HttpNotFound();
            }
            return View(inventoryCategory);
        }

        public ActionResult InventoryCategoryList()
        {
            var InventoryCategoryList = db.InventoryCategories.Select(x => new { x.Id, x.Name }).ToList();

            string status = Newtonsoft.Json.JsonConvert.SerializeObject(InventoryCategoryList);

            return Content(status);

        }



        // GET: InventoryCategories/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: InventoryCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name")] InventoryCategory inventoryCategory)
        {
            if (ModelState.IsValid)
            {
                db.InventoryCategories.Add(inventoryCategory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(inventoryCategory);
        }

        // GET: InventoryCategories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryCategory inventoryCategory = db.InventoryCategories.Find(id);
            if (inventoryCategory == null)
            {
                return HttpNotFound();
            }
            return View(inventoryCategory);
        }

        // POST: InventoryCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name")] InventoryCategory inventoryCategory)
        {
            if (ModelState.IsValid)
            {
                db.Entry(inventoryCategory).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(inventoryCategory);
        }

        // GET: InventoryCategories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryCategory inventoryCategory = db.InventoryCategories.Find(id);
            if (inventoryCategory == null)
            {
                return HttpNotFound();
            }
            return View(inventoryCategory);
        }

        // POST: InventoryCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            InventoryCategory inventoryCategory = db.InventoryCategories.Find(id);
            db.InventoryCategories.Remove(inventoryCategory);
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
