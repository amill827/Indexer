using Indexer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Indexer.ViewModels
{
    public class DocTypeAVM
    {
        public DocTypeAVM()
        {
            DocStatuses = Enumerable.Empty<SelectListItem>();
            Names = new List<DocTypeANameVM>();
        }


        public WorkItem WorkItem { get; set; }
        public int WorkItemID { get; set; }
        public int DocTypeAInstrumentID { get; set; }

        [Display(Name = "Not Workable")]
        public bool NotWorkable { get; set; }

        [StringLength(512, ErrorMessage = "Title field must be 512 characters or less")]
        public string Notes { get; set; }

        [Display(Name = "Application Date")]
        //[RequiredIf("NotWorkable==false", ErrorMessage = "Application Date is required")]
        [Required]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? ApplDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Filing Date")]
        public DateTime? MALDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Return Date")]
        public DateTime? MARDate { get; set; }

        [Display(Name = "License Return Status")]
        //[RequiredIf("NotWorkable==false", ErrorMessage = "License Return Status is required")]
        public int? SelectedDocStatus { get; set; }

        public List<DocTypeANameVM> Names { get; set; }

        

        public IEnumerable<SelectListItem> DocStatuses { get; set; }
        public IEnumerable<SelectListItem> NameTypes { get; set; }
        
        
    }
}