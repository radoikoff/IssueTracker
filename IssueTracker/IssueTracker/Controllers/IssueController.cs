using IssueTracker.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IssueTracker.Controllers
{
    public class IssueController : Controller
    {
        // GET: Issue
        public ActionResult Index()
        {
            return View();
        }

        //Get 
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

        //Get
        public ActionResult Create()
        {
            return View();
        }

        //Post
        [HttpPost]
        public ActionResult Create(Issue issue)
        {
            using(var db = new AppDbContext())
            {
                var currentUser = db.Users.FirstOrDefault(u => u.UserName.Equals(this.User.Identity.Name)); //chech if user is logged in
                var StateIdOfNew = db.IssueStates.FirstOrDefault(s => s.State.Equals("New")).Id;

                issue.StateId = StateIdOfNew; //New = id 1
                issue.AuthorId = currentUser.Id;
                issue.SubmissionDate = DateTime.Now;
                db.Issues.Add(issue);
                db.SaveChanges();
            }
            return RedirectToAction("List");
        }




    }
}