using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace IssueTracker.Models
{
    public class IssueChange
    {
        public IssueChange()
        {
        }

        public IssueChange(int issueId, int stateId, string title, string description, string assigneeId, string changedById, DateTime changedAtDate)
        {
            this.IssueId = issueId;
            this.StateId = stateId;
            this.Title = title;
            this.Description = description;
            this.AssigneeId = assigneeId;
            this.ChangedById = changedById;
            this.ChangedAtDate = changedAtDate;
        }

        [Key]
        public int Id { get; set; }

        [ForeignKey("Issue")]
        public int IssueId { get; set; }

        public virtual Issue Issue { get; set; }

        [ForeignKey("State")]
        public int? StateId { get; set; }

        public virtual IssueState State { get; set; }

        [Required]
        [StringLength(250, MinimumLength = 3)]
        public string Title { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 3)]
        public string Description { get; set; }

        [ForeignKey("Assignee")]
        public string AssigneeId { get; set; }

        public virtual ApplicationUser Assignee { get; set; }

        [ForeignKey("ChangedBy")]
        public string ChangedById { get; set; }

        public virtual ApplicationUser ChangedBy { get; set; }

        [Required]
        public DateTime ChangedAtDate { get; set; }
    }
}