using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace IssueTracker.Models
{
    public class IssueState
    {
        private ICollection<Issue> issues;

        public IssueState()
        {
            this.issues = new HashSet<Issue>();
        }
        
        [Key]
        public int Id { get; set; }

        [Required]
        public string State { get; set; }

        public virtual ICollection<Issue> Issues
        {
            get { return this.issues; }
            set { this.issues = value; }
        }
    }
}
