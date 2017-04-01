using IssueTracker.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace IssueTracker.Controllers
{
    public class IssueController : Controller
    {
        // GET: Issue
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        //Get List
        public ActionResult List()
        {
            using (var db = new AppDbContext())
            {
                var issues = db.Issues
                            .Include(i => i.Author)
                            .Include(i => i.State)
                            .ToList();

                return View(issues);
            }
        }

        //Get Create
        public ActionResult Create()
        {
            return View();
        }

        //Post: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Issue issue)
        {
            using (var db = new AppDbContext())
            {
                var currentUser = db.Users.FirstOrDefault(u => u.UserName.Equals(this.User.Identity.Name)); //to do: chech if user is logged in
                var StateIdOfNew = db.IssueStates.FirstOrDefault(s => s.State.Equals("New")).Id;

                issue.StateId = StateIdOfNew; //New = id 1
                issue.AuthorId = currentUser.Id;
                issue.SubmissionDate = DateTime.Now;
                db.Issues.Add(issue);
                db.SaveChanges();
            }
            return RedirectToAction("List");
        }


        //get Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new AppDbContext())
            {
                var issue = db.Issues.FirstOrDefault(i => i.Id == id);

                if (issue == null)
                {
                    return HttpNotFound();
                }
                return View(issue);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Issue issue)
        {
            if (ModelState.IsValid)
            {
                using (var db = new AppDbContext())
                {
                    db.Entry(issue).State = EntityState.Modified;
                    db.SaveChanges();

                    return RedirectToAction("List");
                }
            }
            return View(issue);
        }

        //get Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new AppDbContext())
            {
                var issue = db.Issues.FirstOrDefault(i => i.Id == id);

                if (issue == null)
                {
                    return HttpNotFound();
                }
                return View(issue);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public ActionResult DeletePOST (int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new AppDbContext())
            {
                var issue = db.Issues.FirstOrDefault(i => i.Id == id);

                if (issue == null)
                {
                    return HttpNotFound();
                }
                db.Issues.Remove(issue);
                db.SaveChanges();

                return RedirectToAction("List");
            }
        }
    }
}