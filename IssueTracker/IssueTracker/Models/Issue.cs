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

        public Issue()
        {
            comments = new HashSet<Comment>();
        }

        [Key]
        public int Id { get; set; }

        [ForeignKey ("State")]
        public int StateId { get; set; }

        public virtual IssueState State { get; set; }

        [Required]
        [MinLength(3)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        public virtual ApplicationUser Author { get; set; }

        [Required]
        public DateTime SubmissionDate { get; set; }

        public virtual ICollection<Comment> Comments
        {
            get { return this.comments; }
            set { this.comments = value; }
        }


    }
}