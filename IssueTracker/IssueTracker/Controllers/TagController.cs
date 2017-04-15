using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using IssueTracker.Models;

namespace IssueTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TagController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // get Index
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }


        //get List
        public ActionResult List()
        {
            List<Tag> tags = db.Tags.ToList();
            return View(tags);
        }

        // get Create
        public ActionResult Create()
        {
            return View();
        }

        // post Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Tag tag)
        {
            if (db.Tags.Any(t => t.Name.Equals(tag.Name)))
            {
                ModelState.AddModelError("Name", "Such tag name already exists!");
                return View(tag);
            }

            if (ModelState.IsValid)
            {
                db.Tags.Add(tag);
                db.SaveChanges();
                return RedirectToAction("List");
            }
            return View(tag);
        }

        // get Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tag tag = db.Tags.Find(id);
            if (tag == null)
            {
                return HttpNotFound();
            }
            return View(tag);
        }

        // post Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Tag tag)
        {
            if (db.Tags.Any(t => t.Name.Equals(tag.Name)))
            {
                ModelState.AddModelError("Name", "Such tag name already exists!");
                return View(tag);
            }

            if (ModelState.IsValid)
            {
                db.Entry(tag).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("List");
            }
            return View(tag);
        }

        // get Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Tag tag = db.Tags.Find(id);
            if (tag == null)
            {
                return HttpNotFound();
            }
            return View(tag);
        }

        // post Delete
        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Tag tag = db.Tags.Find(id);
            db.Tags.Remove(tag);
            db.SaveChanges();
            return RedirectToAction("List");
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
