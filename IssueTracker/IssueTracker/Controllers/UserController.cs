using IssueTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace IssueTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        // GET: User
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        //Get List
        public ActionResult List()
        {
            using (var db = new AppDbContext())
            {
                List<ListUserViewModel> usersViewModel = new List<ListUserViewModel>();

                var users = db.Users.ToList();

                foreach (var user in users)
                {
                    var model = new ListUserViewModel();
                    model.Id = user.Id;
                    model.UserName = user.Email;
                    model.FullName = user.FullName;
                    model.Roles = GetUserRoles(user, db);

                    usersViewModel.Add(model);
                }
         
                ViewBag.Admins = GetAdmins(db);
                
                return View(usersViewModel);
            }
        }

        //get Edit
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var db = new AppDbContext())
            {
                var user = db.Users.FirstOrDefault(u => u.Id.Equals(id));
                if (user == null)
                {
                    return HttpNotFound();
                }

                var model = new EditUserViewModel();
                model.Email = user.Email;
                model.FullName = user.FullName;
                model.Roles = GetUserRoles(user, db);

                return View(model);
            }

            
        }

        private List<Role> GetUserRoles(ApplicationUser user, AppDbContext db)
        {
            var rolesInDatabase = db.Roles
                .Select(r => r.Name)
                .OrderBy(r => r)
                .ToList();

            List<Role> userRoles = new List<Role>();

            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            foreach (var roleName in rolesInDatabase)
            {
                Role role = new Role() { Name = roleName };
                if (userManager.IsInRole(user.Id, roleName))
                {
                    role.IsSelected = true;
                }
                userRoles.Add(role);
            }
            return userRoles;
        }

        private List<string> GetAdmins(AppDbContext db) //hashset
        {
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            var users = db.Users.ToList();

            var admins = new List<string>(); //hashset
            foreach (var user in users)
            {
                if (userManager.IsInRole(user.Id, "Admin"))
                {
                    admins.Add(user.Id);
                }
            }
            return admins;
        }
    }
}