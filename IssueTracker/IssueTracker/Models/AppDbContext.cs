using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace IssueTracker.Models
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public virtual IDbSet <IssueState> IssueStates { get; set; }

        public virtual IDbSet<Issue> Issues { get; set; }

        public virtual IDbSet<Comment> Comments { get; set; }

        public static AppDbContext Create()
        {
            return new AppDbContext();
        }
    }
}