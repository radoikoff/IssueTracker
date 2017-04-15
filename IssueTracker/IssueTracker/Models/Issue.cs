using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace IssueTracker.Models
{
    public class Issue
    {
        private ICollection<Comment> comments;
        private ICollection<Tag> tags;

        public Issue()
        {
            comments = new HashSet<Comment>();
            tags = new HashSet<Tag>();
        }

        [Key]
        public int Id { get; set; }

        [ForeignKey ("State")]
        public int StateId { get; set; }

        public virtual IssueState State { get; set; }

        [Required]
        [StringLength(250, MinimumLength = 3)]
        public string Title { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 3)]
        public string Description { get; set; }

        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        public virtual ApplicationUser Author { get; set; }

        [ForeignKey("Assignee")]
        public string AssigneeId { get; set; }

        public virtual ApplicationUser Assignee { get; set; }

        [Required]
        public DateTime SubmissionDate { get; set; }

        public virtual ICollection<Comment> Comments
        {
            get { return this.comments; }
            set { this.comments = value; }
        }

        public virtual ICollection<Tag> Tags
        {
            get { return this.tags; }
            set { this.tags = value; }
        }

    }
}