using IssueTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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

        //post Edit
        [HttpPost]
        public ActionResult Edit(string id, EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var db = new AppDbContext())
                {
                    var user = db.Users.FirstOrDefault(u => u.Id.Equals(id));

                    if (user == null)
                    {
                        return HttpNotFound();
                    }
                    user.Email = model.Email;
                    user.FullName = model.FullName;
                    user.UserName = model.Email;

                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        var passwordHasher = new PasswordHasher();
                        var newPasswordHash = passwordHasher.HashPassword(model.Password);
                        user.PasswordHash = newPasswordHash;
                    }

                    SetUserRoles(user, db, model);
                    db.Entry(user).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return RedirectToAction("List");
            }

            return View(model);
        }

        //get Delete
        public ActionResult Delete(string id)
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

                return View(user);
            }
        }

        //post Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
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

                db.Users.Remove(user);
                db.SaveChanges();
                return RedirectToAction("List");
            }
        }

        private void SetUserRoles(ApplicationUser user, AppDbContext db, EditUserViewModel model)
        {
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            foreach (var role in model.Roles)
            {
                if (role.IsSelected)
                {
                    userManager.AddToRole(user.Id, role.Name);
                }
                else if (!role.IsSelected)
                {
                    userManager.RemoveFromRole(user.Id, role.Name);
                }
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

    }
}