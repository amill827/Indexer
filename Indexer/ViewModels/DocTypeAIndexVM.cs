using Indexer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Indexer.ViewModels
{
    public class DocTypeAIndexVM
    {
        public DocTypeAIndexVM()
        {
            IncompleteWorkItems = new List<WorkItem>();
            RecentlyCompletedWorkItems = new List<WorkItem>();
            RecentlyVerifiedWorkItems = new List<WorkItem>();
        }

        public List<WorkItem> IncompleteWorkItems { get; set; }
        public List<WorkItem> RecentlyCompletedWorkItems { get; set; }
        public List<WorkItem> RecentlyVerifiedWorkItems { get; set; }
        public int TotalWorkItemCount { get; set; }
        public int IncompleteWorkItemCount { get; set; }
        public int CompletedWorkItemCount { get; set; }
        public int VerifiedWorkItemCount { get; set; }

        
    }
}