﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace IssueTracker.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MinLength(3)]
        [Display(Name = "Comment Text")]
        public string Text { get; set; }

        [ForeignKey("Issue")]
        public int IssueId { get; set; }

        public virtual Issue Issue { get; set; }

        [ForeignKey("Author")]
        public string AuthorId { get; set; }

        public virtual ApplicationUser Author { get; set; }

        [Required]
        public DateTime  CreatedDate { get; set; }

        [Required]
        public bool IsInternal { get; set; }

        public int? ChangeId { get; set; }
    }
}