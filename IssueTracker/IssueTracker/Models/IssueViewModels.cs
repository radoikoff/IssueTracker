using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace IssueTracker.Models
{
    public class IssueSimpleModel
    {
        public int Id { get; set; }

        public string StateName { get; set; }

        public string Title { get; set; }

        public string AuthorFullName { get; set; }

        public DateTime SubmissionDate { get; set; }

        public int CommentsCount { get; set; }

        public List<Tag> Tags { get; set; }
    }


    public class IssueListViewModel
    {
        public List<IssueSimpleModel> Issues { get; set; }

        public List<IssueStateSimpleModel> IssueStates { get; set; }

        public int TotalIssueCount { get; set; }

        public string FilterMessage { get; set; }
    }


    public class IssueStateSimpleModel
    {
        public int Id { get; set; }

        public string StateName { get; set; }

        public int IssuesCount { get; set; }
    }


    public class ProgressIssueViewModel
    {
        public int IssueId { get; set; }

        public int IssueStateId { get; set; }

        public List<IssueState> IssueStates { get; set; }
    }

    public class CreateIssueViewModel
    {
        [Required]
        [StringLength(250, MinimumLength = 3)]
        public string Title { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 3)]
        public string Description { get; set; }

        public List<AssignedTag> AssignedTags { get; set; }
    }

    public class EditIssueViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [StringLength(250, MinimumLength = 3)]
        public string Title { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 3)]
        public string Description { get; set; }

        public List<AssignedTag> AssignedTags { get; set; }

        public string AssigneeId { get; set; }

        public List<SelectListItem> DropdownListItems { get; set; }
    }

    public class AssignedTag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}