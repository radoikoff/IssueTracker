namespace IssueTracker.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;
    using Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.EntityFramework;

    internal sealed class Configuration : DbMigrationsConfiguration<IssueTracker.Models.AppDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            //AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(IssueTracker.Models.AppDbContext context)
        {
            if (!context.IssueStates.Any())
            {
                context.IssueStates.Add(new IssueState { State = "New"});
                context.IssueStates.Add(new IssueState { State = "Open"});
                context.IssueStates.Add(new IssueState { State = "Fixed"});
                context.IssueStates.Add(new IssueState { State = "Closed"});
                context.SaveChanges();
            }

            if (!context.Roles.Any())
            {
                this.CreateRole("Admin", context);
                this.CreateRole("Reviewer", context);
                this.CreateRole("EndUser", context);
            }

            if (!context.Users.Any())
            {
                this.CreateUser("admin@gmail.com", "Admin", "123", context);
                this.SetUserRole("admin@gmail.com", "Admin", context);

                this.CreateUser("vasko@gmail.com", "Vasil Radoykov", "1", context);
                this.SetUserRole("vasko@gmail.com", "Reviewer", context);
                this.SetUserRole("vasko@gmail.com", "EndUser", context);
            }
        }

        private void CreateRole(string roleName, AppDbContext context)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));

            var result = roleManager.Create(new IdentityRole(roleName));

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(",", result.Errors));
            }
        }

        private void SetUserRole(string email, string role, AppDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            var userId = context.Users.FirstOrDefault(u => u.Email.Equals(email)).Id;

            var result = userManager.AddToRole(userId, role);

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(",", result.Errors));
            }

        }

        private void CreateUser(string email, string fullName, string pass, AppDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            userManager.PasswordValidator = new PasswordValidator
            {
                RequireDigit = false,
                RequiredLength = 1,
                RequireLowercase = false,
                RequireNonLetterOrDigit = false,
                RequireUppercase = false
            };

            var user = new ApplicationUser()
            {
                Email = email,
                FullName = fullName,
                UserName = email
            };

            var result = userManager.Create(user, pass);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(",", result.Errors));
            }

        }
    }
}
