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
        public ActionResult List(int? id)
        {
            using (var db = new AppDbContext())
            {
                List<Issue> dbIssues;

                if (id == null || id == 0)
                {
                    dbIssues = db.Issues
                            .Include(i => i.Author)
                            .Include(i => i.State)
                            .ToList();
                }
                else if (id >= 1 && id <= db.IssueStates.Count())
                {
                    dbIssues = db.Issues
                            .Include(i => i.Author)
                            .Include(i => i.State)
                            .Where(i => i.StateId == id)
                            .ToList();
                }
                else
                {
                    return HttpNotFound();
                }

                var issues = new List<IssueSimpleModel>();

                foreach (var dbIssue in dbIssues)
                {
                    var issue = new IssueSimpleModel();

                    issue.Id = dbIssue.Id;
                    issue.AuthorFullName = dbIssue.Author.FullName;
                    issue.Title = dbIssue.Title;
                    issue.SubmissionDate = dbIssue.SubmissionDate;
                    issue.StateName = dbIssue.State.State;
                    issue.CommentsCount = dbIssue.Comments.Count();

                    issues.Add(issue);
                }

                var issueStates = new List<IssueStateSimpleModel>();

                var dbIssueStates = db.IssueStates.ToList();
                foreach (var dbIssueState in dbIssueStates)
                {
                    var state = new IssueStateSimpleModel();
                    state.Id = dbIssueState.Id;
                    state.StateName = dbIssueState.State;
                    state.IssuesCount = dbIssueState.Issues.Count();

                    issueStates.Add(state);
                }

                var model = new IssueListViewModel();
                model.Issues = issues;
                model.IssueStates = issueStates;
                model.TotalIssueCount = db.Issues.Count();

                return View(model);
            }
        }

        //get details
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new AppDbContext())
            {
                var issue = db.Issues
                    .Where(i => i.Id == id)
                    .Include(i => i.Author)
                    .Include(i => i.State)
                    .Include(i => i.Comments)
                    .Include(i => i.Comments.Select(c => c.Author))
                    .First();

                if (issue == null)
                {
                    return HttpNotFound();
                }
                return View(issue);
            }
        }

        //Get Create
        [Authorize]
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
        [Authorize]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new AppDbContext())
            {
                var issue = db.Issues.FirstOrDefault(i => i.Id == id);

                if (!this.IsUserAutorized(issue))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                if (issue == null)
                {
                    return HttpNotFound();
                }
                return View(issue);
            }
        }

        //post Edit
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
        [Authorize]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new AppDbContext())
            {
                var issue = db.Issues.FirstOrDefault(i => i.Id == id);

                if (!this.IsUserAutorized(issue))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                if (issue == null)
                {
                    return HttpNotFound();
                }
                return View(issue);
            }
        }

        //post Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public ActionResult DeletePOST(int? id)
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

        //get Progress
        [Authorize]
        public ActionResult Progress(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new AppDbContext())
            {
                var issue = db.Issues.FirstOrDefault(i => i.Id == id);

                if (!this.IsUserAutorized(issue))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                if (issue == null)
                {
                    return HttpNotFound();
                }

                var model = new ProgressIssueViewModel();

                model.IssueId = (int)id;
                model.IssueStateId = issue.StateId;
                model.IssueStates = db.IssueStates.ToList();

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Progress")]
        public ActionResult ProgressConfirmed(ProgressIssueViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new AppDbContext())
                {
                    //update issue state
                    var issue = db.Issues.FirstOrDefault(i => i.Id == model.IssueId);
                    issue.StateId = model.IssueStateId;
                    db.Entry(issue).State = EntityState.Modified;
                    db.SaveChanges();

                    //create automatic comment
                    var comment = new Comment();
                    var currentUser = db.Users.FirstOrDefault(u => u.UserName.Equals(this.User.Identity.Name)); //to do: chech if user is logged in
                    comment.IssueId = model.IssueId;
                    comment.AuthorId = currentUser.Id;
                    comment.CreatedDate = DateTime.Now;
                    comment.Text =$"{currentUser.FullName} move this issue to {issue.State.State} state on {comment.CreatedDate}";

                    db.Comments.Add(comment);
                    db.SaveChanges();

                    //update model in case MmodelState is not valid. The model does not have issue states list
                    model.IssueStates = db.IssueStates.ToList(); //in case 

                    return RedirectToAction("Details", "Issue", new { id = issue.Id });     
                }
            } 
             return View(model);
        }

        private bool IsUserAutorized(Issue issue)
        {
            bool isAuthor = issue.Author.UserName.Equals(User.Identity.Name);
            bool isAdmin = User.IsInRole("Admin");

            return isAuthor || isAdmin;
        }
    }
}