﻿using IssueTracker.Models;
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
        //get Index
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        //get List
        public ActionResult List(int? stateId, int? tagId, string searchStr)
        {
            List<Issue> dbIssues = null;
            string filterMsg = null;

            using (var db = new AppDbContext())
            {
                if (stateId == null && tagId == null && searchStr == null)
                {
                    dbIssues = db.Issues
                            .Include(i => i.Author)
                            .Include(i => i.State)
                            .Include(i => i.Assignee)
                            .Include(i => i.Tags).ToList();
                }
                else if (stateId != null && stateId >= 1 && stateId <= db.IssueStates.Count())
                {
                    dbIssues = db.Issues.Where(i => i.StateId == stateId).ToList();
                    filterMsg = "Filtering by Issue State is active. Click to clear the filter";
                }

                else if (tagId != null && tagId >= 1 && tagId <= db.Tags.Count())
                {
                    dbIssues = db.Tags.Where(t => t.Id == tagId).Select(t => t.Issues).FirstOrDefault().ToList();
                    filterMsg = "Filtering by Tag is active. Click to clear the filter";
                }
                else if (searchStr != null)
                {
                    dbIssues = db.Issues.Where(i => i.Title.Contains(searchStr)).ToList();
                    filterMsg = "Filtering by Issue Title is active. Click to clear the filter";
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
                    if (dbIssue.Assignee != null)
                    {
                        issue.AssigneeName = dbIssue.Assignee.FullName;
                    }
                    issue.CommentsCount = dbIssue.Comments.Count();
                    issue.Tags = dbIssue.Tags.ToList();

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
                model.FilterMessage = filterMsg;

                return View(model);
            }
        }

        //get Details
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
                    .Include(i => i.Assignee)
                    .Include(i => i.Comments.Select(c => c.Author))
                    .Include(i => i.Tags)
                    .Include(i => i.Changes.Select(c => c.State))
                    .FirstOrDefault();

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
            var model = new CreateIssueViewModel();
            using (var db = new AppDbContext())
            {
                model.AssignedTags = GetIssueTags(null, db);
            }
            return View(model);
        }

        //Post: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateIssueViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new AppDbContext())
                {
                    Issue issue = new Issue();
                    string currentUserId = db.Users.FirstOrDefault(u => u.UserName.Equals(this.User.Identity.Name)).Id; //to do: chech if user is logged in
                    int NewStateId = db.IssueStates.FirstOrDefault(s => s.State.Equals("New")).Id;

                    issue.Title = model.Title;
                    issue.Description = model.Description;
                    issue.StateId = NewStateId; //New = id 1
                    issue.AuthorId = currentUserId;
                    issue.SubmissionDate = DateTime.Now;
                    SetIssueTags(issue, db, model);

                    db.Issues.Add(issue);
                    db.SaveChanges();

                    AddNewChangeLogEntry(issue, db);
                }
                return RedirectToAction("List");
            }
            return View(model);
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

                if (issue == null)
                {
                    return HttpNotFound();
                }

                if (!(this.IsUserAutorized(issue) || User.IsInRole("Owner")))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }

                var model = new EditIssueViewModel();
                model.Id = issue.Id;
                model.Title = issue.Title;
                model.Description = issue.Description;
                model.AssignedTags = GetIssueTags(issue, db);
                model.AssigneesDropdownList = GetDropdownListForAssignees(db, issue.AssigneeId);
                SetViewRights(model, issue);

                return View(model);
            }
        }

        //post Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditIssueViewModel model)
        {
            using (var db = new AppDbContext())
            {
                var issue = db.Issues.FirstOrDefault(i => i.Id == model.Id);

                if (ModelState.IsValid)
                {
                    issue.Title = model.Title;
                    issue.Description = model.Description;
                    SetIssueTags(issue, db, model);
                    issue.AssigneeId = model.AssigneeId;

                    db.Entry(issue).State = EntityState.Modified;
                    db.SaveChanges();

                    AddNewChangeLogEntry(issue, db);

                    return RedirectToAction("Details", new { id = model.Id });
                }

                model.AssigneesDropdownList = GetDropdownListForAssignees(db, issue.AssigneeId);
                if (model.AssigneeId != null) //clear default selected user and assing the user returned from the post
                {
                    model.AssigneesDropdownList.FirstOrDefault(s => s.Selected = true).Selected = false;
                    model.AssigneesDropdownList.FirstOrDefault(s => s.Value.Equals(model.AssigneeId)).Selected = true;
                }
                return View(model);
            }
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
                var issue = db.Issues.Include(i => i.Tags).FirstOrDefault(i => i.Id == id);

                if (issue == null)
                {
                    return HttpNotFound();
                }

                if (!this.IsUserAutorized(issue))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
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

                if (issue == null)
                {
                    return HttpNotFound();
                }

                var model = new ProgressIssueViewModel();

                model.IssueId = issue.Id;
                model.IssueStateId = issue.StateId;
                model.IssueStateName = issue.State.State;
                model.IssueTitle = issue.Title;
                model.AllowedIssueStates = SetAllowedStates(issue, db);
                model.AssignedTags = GetIssueTags(issue, db);
                model.AssigneeId = issue.AssigneeId;
                model.AssigneesDropdownList = GetDropdownListForAssignees(db, issue.AssigneeId);

                return View(model);
            }
        }

        //post Progress
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Progress(ProgressIssueViewModel model)
        {
            using (var db = new AppDbContext())
            {
                var issue = db.Issues.FirstOrDefault(i => i.Id == model.IssueId);
                if (ModelState.IsValid)
                {
                    issue.StateId = (int)model.IssueStateId;
                    issue.AssigneeId = model.AssigneeId;
                    SetIssueTags(issue, db, model);
                    db.Entry(issue).State = EntityState.Modified;
                    db.SaveChanges();

                    AddNewChangeLogEntry(issue, db);
                    AddInternalComment(model.IssueId, db);

                    return RedirectToAction("Details", "Issue", new { id = issue.Id });
                }

                model.AllowedIssueStates = SetAllowedStates(issue, db);
                model.AssigneesDropdownList = GetDropdownListForAssignees(db, issue.AssigneeId);
                if (model.AssigneeId != null) //clear default selected user and assing the user returned from the post
                {
                    model.AssigneesDropdownList.FirstOrDefault(s => s.Selected = true).Selected = false;
                    model.AssigneesDropdownList.FirstOrDefault(s => s.Value.Equals(model.AssigneeId)).Selected = true;
                }

                return View(model);
            }
        }


        public ActionResult History(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new AppDbContext())
            {
                var changes = db.Changes.Where(c => c.IssueId == id)
                                .Include(c=>c.ChangedBy)
                                .Include(c=>c.Assignee)
                                .Include(c=>c.State)
                                .OrderByDescending(c => c.ChangedAtDate)
                                .ToList();

                if (!changes.Any())
                {
                    return HttpNotFound();
                }

                return View(changes);
            }
        }


        private void AddInternalComment(int issueId, AppDbContext db)
        {
            var comment = new Comment();
            var currentUser = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
            int latestChangeEntryId = db.Changes.Where(c => c.IssueId == issueId).OrderByDescending(c => c.ChangedAtDate).First().Id;

            comment.IssueId = issueId;
            comment.AuthorId = currentUser.Id;
            comment.CreatedDate = DateTime.Now;
            comment.IsInternal = true;
            comment.ChangeId = latestChangeEntryId;
            comment.Text = "internal";

            db.Comments.Add(comment);
            db.SaveChanges();
        }

        private bool IsUserAutorized(Issue issue)
        {
            bool isAuthor = issue.Author.UserName.Equals(User.Identity.Name);
            bool isAdmin = User.IsInRole("Admin");

            return isAuthor || isAdmin;
        }

        private void SetViewRights(EditIssueViewModel model, Issue issue)
        {
            bool isAuthor = issue.Author.UserName.Equals(User.Identity.Name);
            bool isAdmin = User.IsInRole("Admin");
            bool isOwner = User.IsInRole("Owner");

            if (isAuthor || isAdmin)
            {
                model.HasEditRights = true;
            }

            if (isOwner || isAdmin)
            {
                model.HasProgressRights = true;
            }
        }

        private void SetIssueTags(Issue issue, AppDbContext db, CreateIssueViewModel model)
        {
            foreach (var tag in model.AssignedTags)
            {
                if (tag.IsSelected)
                {
                    Tag dbTag = db.Tags.FirstOrDefault(t => t.Id == tag.Id);
                    issue.Tags.Add(dbTag);
                }
            }
        }

        private void SetIssueTags(Issue issue, AppDbContext db, EditIssueViewModel model)
        {
            issue.Tags.Clear();
            foreach (var tag in model.AssignedTags)
            {
                if (tag.IsSelected)
                {
                    Tag dbTag = db.Tags.FirstOrDefault(t => t.Id == tag.Id);
                    issue.Tags.Add(dbTag);
                }
            }
        }

        private void SetIssueTags(Issue issue, AppDbContext db, ProgressIssueViewModel model)
        {
            issue.Tags.Clear();
            foreach (var tag in model.AssignedTags)
            {
                if (tag.IsSelected)
                {
                    Tag dbTag = db.Tags.FirstOrDefault(t => t.Id == tag.Id);
                    issue.Tags.Add(dbTag);
                }
            }
        }

        private List<AssignedTag> GetIssueTags(Issue issue, AppDbContext db)
        {
            List<AssignedTag> assignedTags = new List<AssignedTag>();
            List<int> issueTagsIds = new List<int>();

            if (issue != null)
            {
                issueTagsIds = issue.Tags.Select(t => t.Id).ToList();
            }

            foreach (var tag in db.Tags)
            {
                var assignedTag = new AssignedTag();
                assignedTag.Id = tag.Id;
                assignedTag.Name = tag.Name;
                if (issueTagsIds.Contains(tag.Id))
                {
                    assignedTag.IsSelected = true;
                }
                assignedTags.Add(assignedTag);
            }
            return assignedTags;
        }

        private List<SelectListItem> GetDropdownListForAssignees(AppDbContext db, string assigneeUserId)
        {
            var dropdownListItems = new List<SelectListItem>();

            foreach (var user in db.Users)
            {
                var currentListItem = new SelectListItem { Value = user.Id, Text = user.FullName };
                if (user.Id.Equals(assigneeUserId))
                {
                    currentListItem.Selected = true;
                }
                dropdownListItems.Add(currentListItem);
            }
            return dropdownListItems;
        }

        private List<AllowedIssueState> SetAllowedStates(Issue issue, AppDbContext db)
        {
            var allowedStates = new List<AllowedIssueState>();

            if (User.IsInRole("Admin"))
            {
                foreach (var state in db.IssueStates)
                {
                    allowedStates.Add(new AllowedIssueState
                    {
                        Id = state.Id,
                        Name = state.State,
                        HintText = state.State
                    });
                }
                return allowedStates;
            }

            if (issue.State.State.Equals("New") && User.IsInRole("Owner"))
            {
                IssueState state = db.IssueStates.FirstOrDefault(s => s.State.Equals("Open"));
                allowedStates.Add(new AllowedIssueState
                {
                    Id = state.Id,
                    Name = state.State,
                    HintText = "Accept the issue (goes to Open state)"
                });

                state = db.IssueStates.FirstOrDefault(s => s.State.Equals("Closed"));
                allowedStates.Add(new AllowedIssueState
                {
                    Id = state.Id,
                    Name = state.State,
                    HintText = "Reject the issue (goes to Closed state)"
                });
                return allowedStates;
            }

            if (issue.State.State.Equals("Open") && (User.IsInRole("Owner") || issue.Assignee.UserName.Equals(User.Identity.Name)))
            {
                var state = db.IssueStates.FirstOrDefault(s => s.State.Equals("Fixed"));
                allowedStates.Add(new AllowedIssueState
                {
                    Id = state.Id,
                    Name = state.State,
                    HintText = "Confirm issue has been fixed (goes to Fixed state)"
                });

                state = db.IssueStates.FirstOrDefault(s => s.State.Equals("Closed"));
                allowedStates.Add(new AllowedIssueState
                {
                    Id = state.Id,
                    Name = state.State,
                    HintText = "Issue will not be fixed (goes to Closed state)"
                });
                return allowedStates;
            }
            return allowedStates;
        }

        private void AddNewChangeLogEntry(Issue issue, AppDbContext db)
        {
            var currentUser = db.Users.FirstOrDefault(u => u.UserName.Equals(User.Identity.Name));
            var changeEntry = new IssueChange(issue.Id, issue.StateId, issue.Title, issue.Description, issue.AssigneeId, currentUser.Id, DateTime.Now);

            db.Changes.Add(changeEntry);
            db.SaveChanges();
        }
    }
}