using Indexer.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace Indexer.Services
{
    public class WorkItemService
    {
        public WorkItemService() { }

        public List<WorkItem> GetWorkItemsForDocType(int docType)
        {
            using (IndexerDBEntities db = new IndexerDBEntities())
            {
                return db.WorkItems.Where(x => x.DocTypeID == docType).ToList();
            }
        }
        public WorkItem GetWorkItem(int workItemID)
        {
            using (IndexerDBEntities db = new IndexerDBEntities())
            {
                var result = db.WorkItems.Where(x => x.WorkItemID == workItemID)
                                     .Include(x => x.WorkItemImages)
                                     .Include(x=>x.DocTypeAInstruments.Select(y => y.DocTypeANames))
                                     .FirstOrDefault();
                return result;
            }
        }

        public bool TryLockWorkItem(int workItemID, string userName, out string message)
        {
            message = string.Empty;
            using (IndexerDBEntities db = new IndexerDBEntities())
            {
                var workItem = db.WorkItems.Find(workItemID);
                if (workItem.LockedBy!=null)
                {
                    Log.Error($"Cannot lock work item: {workItem.WorkItemID} to {userName} because it is already locked by another user.");
                    message = "Cannot assign this work item because it already locked by another user.";
                    return false;
                }

                workItem.LockedBy = userName;
                workItem.LockedOn = DateTime.Now;
                workItem.UpdatedOn = DateTime.Now;
                workItem.UpdatedBy = userName;

                db.SaveChanges();

                Log.Information($"User: {userName} Locked WorkItemID:{workItem.WorkItemID}.");
            }

            return true;
        }


        public bool CanWorkItemBeCompleted(WorkItem workItem, string userName, out string message)
        {
            message= string.Empty;
            if (!IsWorkItemAssignedToUser(workItem, userName))
            {
                message = "Unable to make changes to this Work Item because it is not locked by you.";
                Log.Information($"User: {userName} attempted to update work item assigned to user:{workItem.LockedBy ?? "null"} Work Item ID: {workItem.WorkItemID}");
                return false;
            }
            if (workItem.CompletedBy!=null)
            {
                message = "This Work Item has been marked as complete and cannot be updated.";
                Log.Information($"User: {userName} attempted to update an already completed Work Item ID: {workItem.WorkItemID}");
                return false;
            }

            return true;
        }

        public bool CanWorkItemBeVerified(WorkItem workItem, string userName, out string message)
        {
            message = string.Empty;
            if (!IsWorkItemAssignedToUser(workItem, userName))
            {
                message = "Unable to make changes to this Work Item because it is not locked by you.";
                Log.Information($"User: {userName} attempted to update work item assigned to user:{workItem.LockedBy ?? "null"} Work Item ID: {workItem.WorkItemID}");
                return false;
            }
            if (workItem.CompletedBy == null)
            {
                message = "You can't verify this work item because it hasn't been completed yet.";
                Log.Information($"User: {userName} attempted to verify Work Item ID: {workItem.WorkItemID}, but it hasn't been marked completed yet");
                return false;
            }
            if (workItem.VerifiedBy != null)
            {
                message = "This Work Item has been marked as verified and cannot be updated.";
                Log.Information($"User: {userName} attempted to update an already verified Work Item ID: {workItem.WorkItemID}");
                return false;
            }

            return true;
        }

        public bool IsWorkItemAssignedToUser(WorkItem workItem, string userName)
        {
            return workItem.LockedBy != null &&
                   workItem.LockedBy.Equals(userName, StringComparison.OrdinalIgnoreCase);
        }
        public void UpdateWorkItem(int workItemID, bool notWorkable, string notes, string userName)
        {
            using (IndexerDBEntities db = new IndexerDBEntities())
            {
                var workItem = db.WorkItems.Find(workItemID);
                workItem.NotWorkable = notWorkable;
                workItem.Notes = notWorkable ? notes : null;
                workItem.UpdatedOn = DateTime.Now;
                workItem.UpdatedBy = userName;
            
                db.SaveChanges();

                Log.Information($"User: {userName} updated Work Item ID: {workItem.WorkItemID}, NotWorkable:{workItem.NotWorkable} Notes: {workItem.Notes}");
            }
        }

        public void MarkWorkItemAsComplete(int workItemID, string userName)
        {
            using (IndexerDBEntities db = new IndexerDBEntities())
            {
                var workItem = db.WorkItems.Find(workItemID);
                if (workItem.CompletedBy != null)
                throw new Exception($"Cannot complete work item: {workItem.WorkItemID} as it is already marked as complete.");

                workItem.CompletedOn = DateTime.Now;
                workItem.CompletedBy = userName;
                workItem.UpdatedOn = DateTime.Now;
                workItem.UpdatedBy = userName;
                workItem.LockedOn = null; //Unlock the WorkItem so another user can verify it
                workItem.LockedBy = null;

                db.SaveChanges();

                Log.Information($"User: {userName} completed Work Item ID: {workItem.WorkItemID}");
            
            }
        }
        public void MarkWorkItemAsVerified(int workItemID, string userName)
        {
            using (IndexerDBEntities db = new IndexerDBEntities())
            {
                var workItem = db.WorkItems.Find(workItemID);
                if (workItem.VerifiedBy != null)
                    throw new Exception($"Cannot verify work item: {workItem.WorkItemID} as it is already marked as verified.");

                workItem.VerifiedOn = DateTime.Now;
                workItem.VerifiedBy = userName;
                workItem.UpdatedOn = DateTime.Now;
                workItem.UpdatedBy = userName;
                workItem.LockedOn = null; //Unlock the WorkItem so it can be exported
                workItem.LockedBy = null;

                db.SaveChanges();

                Log.Information($"User: {userName} verified Work Item ID: {workItem.WorkItemID}");
            }
        }

        public int GetWorkItemCountReadyForExport(int docTypeID)
        {
            using (IndexerDBEntities db = new IndexerDBEntities())
            {
                return GetWorkItemsForExport(docTypeID, db).Count();
            }
        }
        public List<WorkItem> GetWorkItemsReadyForExport(int docTypeID)
        {
            using (IndexerDBEntities db = new IndexerDBEntities())
            {
                return GetWorkItemsForExport(docTypeID, db).ToList();
            }
        }

        private IQueryable<WorkItem> GetWorkItemsForExport(int docTypeID, IndexerDBEntities db)
        {
            var results = db.WorkItems.Where(x => x.DocTypeID == docTypeID
                                            && x.LockedBy == null
                                            && x.VerifiedBy != null
                                            && x.ExportedOn == null
                                            && x.NotWorkable == false);
            
            return results;
        }

        //Simplified version of export. With higher item counts it will be necessary to use bulk update methods
        //instead of EF's way of updating one at a time.
        public void ExportWorkItemsForDocType(int docTypeID, string userName)
        {
            using (IndexerDBEntities db = new IndexerDBEntities())
            {
            var workItemsForExport = GetWorkItemsForExport(docTypeID, db).ToList();

                foreach (var item in workItemsForExport)
                {
                    //This is where the data would normally be exported to a different database, only
                    //marking the Work Items as exported here.
                    
                    if (item.ExportedOn != null)
                        throw new Exception($"Cannot export work item: {item.WorkItemID} as it is already exported.");

                    item.ExportedOn = DateTime.Now;
                    item.LockedBy = null;
                    item.LockedOn = DateTime.Now;
                    item.UpdatedOn = DateTime.Now;
                    item.UpdatedBy = userName;

                    Log.Information($"User: {userName} exporting Work Item ID: {item.WorkItemID}");
                }

                db.SaveChanges();
                Log.Information($"User: {userName} successfully exported {workItemsForExport.Count} workItems.");
            }

        }
    }
}