using Indexer.Models;
using Indexer.Services;
using Indexer.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace Indexer.Controllers
{
    public class DocTypeAController : BaseController
    {
        private WorkItemService _workItemService = new WorkItemService();
        private DocTypeAService _docService = new DocTypeAService();
        private readonly int _docTypeID = Properties.Settings.Default.DocTypeAID;

        public ActionResult Index()
        {
            DocTypeAIndexVM vm =new DocTypeAIndexVM();

            var workItems = _workItemService.GetWorkItemsForDocType(_docTypeID).Where(x => !x.NotWorkable).ToList(); // Exclude any work items marked not workable
            vm.TotalWorkItemCount = workItems.Count();
            vm.IncompleteWorkItems = workItems.Where(x => x.VerifiedBy == null && x.LockedBy == null).ToList();
            vm.RecentlyCompletedWorkItems = workItems.Where(x => x.CompletedBy != null).ToList();
            vm.RecentlyVerifiedWorkItems = workItems.Where(x => x.VerifiedBy != null).ToList();
            vm.IncompleteWorkItemCount = vm.IncompleteWorkItems.Count();
            vm.CompletedWorkItemCount = workItems.Where(x => x.CompletedBy != null).Count();
            vm.VerifiedWorkItemCount = workItems.Where(x => x.VerifiedBy != null).Count();

            return View(vm);
        }

        public ActionResult IndexDocument(int? workItemID, bool assignToUser = false)
        {
            DocTypeAVM vm = new DocTypeAVM();
            //vm.SelectedDocType = _docTypeID;

            if (workItemID == null)
            {
                return RedirectToAction("Index");
            }

            WorkItem workItem = _workItemService.GetWorkItem(workItemID.GetValueOrDefault());
            if (workItem == null)
            {
                Log.Error($"User:{User.Identity.Name} Unable to find WorkItem for WorkItemID: {workItemID}.");
                ModelState.AddModelError("", "Unable to find the specified Work Item. Please contact IT if this problem persists.");
                return View(vm);
            }

            if (assignToUser)
            {//Assign/Lock work item to user so no others can edit
                bool success = _workItemService.TryLockWorkItem(workItemID.Value, User.Identity.Name, out string message);
                if (!success)
                {
                    ModelState.AddModelError("", message);
                }
            }

            LoadViewModel(vm, workItem);

            return View(vm);
        }

        [HttpPost]
        public ActionResult IndexDocument(DocTypeAVM viewModel)
        {
            try
            {
                viewModel.WorkItem = _workItemService.GetWorkItem(viewModel.WorkItemID);
                LoadDropDowns(viewModel);

                bool canBeCompleted = _workItemService.CanWorkItemBeCompleted(viewModel.WorkItem, User.Identity.Name, out string message);
                if (!canBeCompleted)
                {
                    ModelState.AddModelError("", message);
                    return View(viewModel);
                }

                if (!ModelState.IsValid)
                    return View(viewModel);

                _workItemService.UpdateWorkItem(viewModel.WorkItem.WorkItemID, viewModel.NotWorkable, viewModel.Notes, User.Identity.Name);
                if (!viewModel.NotWorkable)
                    AddOrUpdateInstrument(viewModel);
                
                _workItemService.MarkWorkItemAsComplete(viewModel.WorkItem.WorkItemID, User.Identity.Name);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"User: {User.Identity.Name} Encountered an error while saving data for Work Item ID: {viewModel.WorkItemID}. ");
                ModelState.AddModelError("", "Encountered an error while saving this information. Please try again. If this problem persists, please contact IT.");
            }

            return View(viewModel);
        }

        public ActionResult VerifyDocument(int? workItemID, bool assignToUser = false)
        {
            DocTypeAVM vm = new DocTypeAVM();

            if (workItemID == null)
                return View(vm);

            WorkItem workItem = _workItemService.GetWorkItem(workItemID.GetValueOrDefault());
            if (workItem == null)
            {
                Log.Error($"User:{User.Identity.Name}: Unable to find WorkItem for WorkItemID: {workItemID}.");
                ModelState.AddModelError("", "Unable to find the specified Work Item. Please contact IT if this problem persists.");
                return View(vm);
            }

            if (assignToUser)
            {//Assign/Lock work item to user so no others can edit
                bool success = _workItemService.TryLockWorkItem(workItemID.Value, User.Identity.Name, out string message);
                if (!success)
                { 
                    ModelState.AddModelError("", message);
                }
            }

            vm.WorkItem = workItem;
            LoadViewModel(vm, workItem);

            return View(vm);
        }

        [HttpPost]
        public ActionResult VerifyDocument(DocTypeAVM viewModel)
        {
            try
            {
                viewModel.WorkItem = _workItemService.GetWorkItem(viewModel.WorkItemID);
                LoadDropDowns(viewModel);


                bool canBeVerified = _workItemService.CanWorkItemBeVerified(viewModel.WorkItem, User.Identity.Name, out string message);
                if (!canBeVerified)
                {
                    ModelState.AddModelError("", message);
                    return View(viewModel);
                }

                if (!ModelState.IsValid)
                    return View(viewModel);

                
                _workItemService.UpdateWorkItem(viewModel.WorkItem.WorkItemID, viewModel.NotWorkable, viewModel.Notes, User.Identity.Name);
                if (!viewModel.NotWorkable)
                    AddOrUpdateInstrument(viewModel);
                _workItemService.MarkWorkItemAsVerified(viewModel.WorkItem.WorkItemID, User.Identity.Name);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"User: {User.Identity.Name} Encountered an error while submitting data for verify for Work Item ID: {viewModel.WorkItemID}. ");
                ModelState.AddModelError("", "Encountered an error while submitting this information. Please try again. If this problem persists, please contact IT.");
            }

            return View(viewModel);
        }

        public ActionResult Export()
        {
            ExportVM vm = new ExportVM();

            try
            {
                vm.ReadyExportCount = _workItemService.GetWorkItemCountReadyForExport(_docTypeID);
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"User: {User.Identity.Name} Encountered an error while getting an export count for DocTypeID: {_docTypeID}.");
                ModelState.AddModelError("", "Encountered an error while getting export information. Please try again. If this problem persists, please contact IT.");
            }

            return View(vm);
        }

        [HttpPost]
        public ActionResult Export(ExportVM vm)
        {
            try
            {
                _workItemService.ExportWorkItemsForDocType(_docTypeID, User.Identity.Name);
                vm.ExportSuccess = true;
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"User: {User.Identity.Name} Encountered an error while exporting data for DocTypeID:{_docTypeID} ");
                ModelState.AddModelError("", "Encountered an error while exporting data. Review logs for more details.");
            }
            
            vm.ReadyExportCount = _workItemService.GetWorkItemCountReadyForExport(_docTypeID);

            return View(vm);
        }

        private void AddOrUpdateInstrument(DocTypeAVM viewModel)
        {
            DocTypeAInstrument instrument;

            if (viewModel.DocTypeAInstrumentID != 0)
            {
                var names = GetDocTypeNamesFromViewModel(viewModel.Names);
                _docService.UpdateInstrument(viewModel, names,User.Identity.Name);
            }
            else
            {
                instrument = new DocTypeAInstrument();
                LoadInstrument(instrument, viewModel);
                _docService.AddInstrument(viewModel, instrument, User.Identity.Name);
            }
        }

        private void LoadInstrument(DocTypeAInstrument instrument, DocTypeAVM viewModel)
        {
            instrument.WorkItemID = viewModel.WorkItemID;
            instrument.ApplDate = (DateTime)viewModel.ApplDate;
            instrument.MALDate = viewModel.MALDate;
            instrument.MARDate = viewModel.MARDate;
            instrument.DocStatus = viewModel.SelectedDocStatus.Value;
            instrument.DocTypeANames = GetDocTypeNamesFromViewModel(viewModel.Names);
            instrument.AddedOn = DateTime.Now;
            instrument.AddedBy = User.Identity.Name;
        }
        private List<DocTypeAName> GetDocTypeNamesFromViewModel(List<DocTypeANameVM> names)
        {
            var docTypeNames = names.Select(x => new DocTypeAName
            {
                DocTypeANameID = x.DocTypeANameID,
                DocTypeAInstrumentID = x.DocTypeAInstrumentID,
                NameTypeID = x.NameTypeID,
                FirstName = x.FirstName,
                MiddleName = x.MiddleName,
                LastName = x.LastName,
                Age = x.Age,
                Suffix = x.Suffix
                
            }).ToList();
            //Remove empty malName items
            docTypeNames.RemoveAll(x => String.IsNullOrEmpty(x.FirstName) || String.IsNullOrEmpty(x.LastName));

            return docTypeNames;
        }

        private void LoadViewModel(DocTypeAVM viewModel, WorkItem workItem)
        {
            viewModel.WorkItem = workItem;
            viewModel.WorkItem.WorkItemImages = viewModel.WorkItem.WorkItemImages.OrderBy(x => x.ImageDescription).ToList();
            viewModel.WorkItemID = workItem.WorkItemID;
            viewModel.NotWorkable = workItem.NotWorkable;
            viewModel.Notes = workItem.Notes;

            var instrument = workItem.DocTypeAInstruments.FirstOrDefault();
            var nameTypes = _docService.GetNameTypes();
            //Create a list of empty name types, these will be filled later if they exist for this instrument
            foreach (var type in nameTypes)
            {   
                viewModel.Names.Add(new DocTypeANameVM(null, viewModel) { NameTypeID = type.NameTypeID });
            }

            if (instrument != null)
            {
                viewModel.DocTypeAInstrumentID = instrument.DocTypeAInstrumentID;
                viewModel.ApplDate = instrument.ApplDate;
                viewModel.MALDate = instrument.MALDate;
                viewModel.MARDate = instrument.MARDate;
                viewModel.SelectedDocStatus = instrument.DocStatus;
                var namesFromDB = instrument.DocTypeANames.Select(x => new DocTypeANameVM(x, viewModel))
                                                          .OrderBy(x => x.NameTypeID).ToList();
                //Load matching names from DB into empty names list
                for (int i = 0; i < viewModel.Names.Count; i++)
                {
                    var matchedName = namesFromDB.FirstOrDefault(x => x.NameTypeID == viewModel.Names[i].NameTypeID);
                    if (matchedName != null)
                        viewModel.Names[i] = matchedName;
                }
            }

            LoadDropDowns(viewModel);
        }

        private void LoadDropDowns(DocTypeAVM viewModel)
        {
            viewModel.DocStatuses = _docService.GetDocStatusesForDocType(_docTypeID)
                                        .Select(x => new SelectListItem() { Text = x.Description, Value = x.DocStatusID.ToString() });
            viewModel.NameTypes = _docService.GetNameTypes()
                                        .Select(x => new SelectListItem() { Text = x.Description, Value = x.NameTypeID.ToString() });
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        db.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}