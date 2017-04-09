using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

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
    }


    public class IssueListViewModel
    {
        public List<IssueSimpleModel> Issues { get; set; }

        public List<IssueStateSimpleModel> IssueStates { get; set; }

        public int TotalIssueCount { get; set; }
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
}