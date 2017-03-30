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


        public ActionResult List()
        {

            var db = new AppDbContext();
            var issues = db.Issues
                .Include(i => i.Author)
                .Include(i => i.State)
                .ToList();

            return View(issues);
        }
    }
}