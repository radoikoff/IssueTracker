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
    public class CommentController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Comment
        public ActionResult Index()
        {
            var comments = db.Comments.Include(c => c.Author).Include(c => c.Issue);
            return View(comments.ToList());
        }

        // GET: Comment/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // get Create
        [Authorize]
        public ActionResult Create(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = new Comment();
            model.IssueId = (int)id;

            return View(model);
        }

        // post Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Comment comment)
        {
            if (ModelState.IsValid)
            {
                var currentUser = db.Users.FirstOrDefault(u => u.UserName.Equals(this.User.Identity.Name)); //to do: chech if user is logged in

                comment.AuthorId = currentUser.Id;
                comment.CreatedDate = DateTime.Now;

                db.Comments.Add(comment);
                db.SaveChanges();

                return RedirectToAction("Details", "Issue", new { id = comment.IssueId });
            }
            return View(comment);
        }

        // get Edit
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var comment = db.Comments.FirstOrDefault(c => c.Id == id);

            if (!this.IsUserAutorized(comment))
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
            }

            if (comment == null)
            {
                return HttpNotFound();
            }

            return View(comment);
        }

        // post Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Comment comment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(comment).State = EntityState.Modified;
                db.SaveChanges();

                return  RedirectToAction("Details", "Issue", new { id = comment.IssueId });
            }

            return View(comment);
        }

        // GET: Comment/Delete/5
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Comment comment = db.Comments.Find(id);
            if (comment == null)
            {
                return HttpNotFound();
            }
            return View(comment);
        }

        // POST: Comment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Comment comment = db.Comments.Find(id);
            db.Comments.Remove(comment);
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

        private bool IsUserAutorized(Comment comment)
        {
            bool isAuthor = comment.Author.UserName.Equals(User.Identity.Name);
            bool isAdmin = User.IsInRole("Admin");

            return isAuthor || isAdmin;
        }
    }
}
