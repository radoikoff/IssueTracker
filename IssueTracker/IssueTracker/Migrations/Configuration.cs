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
                context.IssueStates.Add(new IssueState { State = "New" });
                context.IssueStates.Add(new IssueState { State = "Open" });
                context.IssueStates.Add(new IssueState { State = "Fixed" });
                context.IssueStates.Add(new IssueState { State = "Closed" });
                context.SaveChanges();
            }

            if (!context.Roles.Any())
            {
                this.CreateRole("Admin", context);
                this.CreateRole("Owner", context);
                this.CreateRole("EndUser", context);
            }

            if (!context.Users.Any())
            {
                this.CreateUser("admin@gmail.com", "Admin Adminov", "123", context);
                this.SetUserRoles("admin@gmail.com", new string[] { "Admin", "EndUser" }, context);

                this.CreateUser("vasko@gmail.com", "Vasil Radoykov", "1", context);
                this.SetUserRoles("vasko@gmail.com", new string[] { "Owner", "EndUser" }, context);

                this.CreateUser("pesho@gmail.com", "Pesho Goshov", "1", context);
                this.SetUserRoles("pesho@gmail.com", new string[] { "EndUser" }, context);
            }

            if (!context.Tags.Any())
            {
                context.Tags.Add(new Tag { Name = "Bug" });
                context.Tags.Add(new Tag { Name = "Enhancement" });
                context.Tags.Add(new Tag { Name = "Duplicate" });
                context.Tags.Add(new Tag { Name = "Not a Bug" });
                context.Tags.Add(new Tag { Name = "Wontfix" });
                context.SaveChanges();
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

        private void SetUserRoles(string email, string[] roles, AppDbContext context)
        {
            var userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

            var userId = context.Users.FirstOrDefault(u => u.Email.Equals(email)).Id;

            var result = userManager.AddToRoles(userId, roles);

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
