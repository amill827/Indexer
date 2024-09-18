using Antlr.Runtime;
using Indexer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;

namespace Indexer.ViewModels
{
    public class DocTypeANameVM
    {
        public DocTypeANameVM() 
        {
            NameTypes = Enumerable.Empty<SelectListItem>();
        }

        public DocTypeANameVM(DocTypeAName name, DocTypeAVM parentViewModel)
        {
            if (name != null)
            {
                DocTypeANameID = name.DocTypeANameID;
                DocTypeAInstrumentID = name.DocTypeAInstrumentID;
                NameTypeID = name.NameTypeID;
                FirstName = name.FirstName;
                MiddleName = name.MiddleName;
                LastName = name.LastName;
                Suffix = name.Suffix;
                Age = name.Age;
            }

            NotWorkable = parentViewModel.NotWorkable;

            NameTypes = Enumerable.Empty<SelectListItem>();
        }
        public int DocTypeANameID { get; set; }
        public int DocTypeAInstrumentID { get; set; }
        public int NameTypeID { get; set; }

        [StringLength(50, ErrorMessage = "First Name field must be 50 characters or less")]
        [Display(Name = "First Name")]
        //[RequiredIf("NotWorkable==false && (NType!='3' || (SelectedReturnStatus != 3 && SelectedReturnStatus != 2))", ErrorMessage = "First Name is required")]
        public string FirstName { get; set; }
        [StringLength(50, ErrorMessage = "Last Name field must be 50 characters or less")]
        [Display(Name = "Last Name")]
        //[RequiredIf("NotWorkable==false && (NType!='3' || (SelectedReturnStatus != 3 && SelectedReturnStatus != 2))", ErrorMessage = "Last Name is required")]
        public string LastName { get; set; }

        [StringLength(50, ErrorMessage = "Middle Name field must be 50 characters or less")]
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }

        [StringLength(10, ErrorMessage = "Suffix field must be 10 characters or less")]
        [Display(Name = "Suffix")]
        public string Suffix { get; set; }

        [Display(Name = "Age")]
        public Nullable<short> Age { get; set; }

        public bool NotWorkable { get; set; }

        public IEnumerable<SelectListItem> NameTypes { get; set; }
    }
}