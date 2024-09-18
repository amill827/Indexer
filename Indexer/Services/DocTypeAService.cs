using Indexer.Models;
using Indexer.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Indexer.Services
{
    public class DocTypeAService
    {
        public DocTypeAService()
        {
            
        }
        public void AddInstrument(DocTypeAVM viewModel, DocTypeAInstrument instrument, string userName)
        {
            using (IndexerDBEntities db = new IndexerDBEntities())
            {
                
                db.DocTypeAInstruments.Add(instrument);

                db.SaveChanges();
                Log.Information($"{userName} successfully added DocTypeAInstrument ID: {instrument.DocTypeAInstrumentID}");
            }
        }

        public void UpdateInstrument(DocTypeAVM viewModel, List<DocTypeAName> names, string userName)
        {
            using (IndexerDBEntities db = new IndexerDBEntities())
            {
                var instrument = db.DocTypeAInstruments.Find(viewModel.DocTypeAInstrumentID);
                instrument.WorkItemID = viewModel.WorkItemID;
                instrument.ApplDate = viewModel.ApplDate.GetValueOrDefault();
                instrument.MALDate = viewModel.MALDate;
                instrument.MARDate = viewModel.MARDate;
                instrument.DocStatus = viewModel.SelectedDocStatus.GetValueOrDefault();
                instrument.UpdatedOn = DateTime.Now;
                instrument.UpdatedBy = userName;

                foreach (var name in names)
                {
                    var docTypeName = db.DocTypeANames.Find(name.DocTypeANameID);

                    if (docTypeName != null)
                    {
                        docTypeName.FirstName = name.FirstName;
                        docTypeName.MiddleName = name.MiddleName;
                        docTypeName.LastName = name.LastName;
                        docTypeName.Suffix = name.Suffix;
                        docTypeName.Age = name.Age;
                    }
                    else
                        instrument.DocTypeANames.Add(name);
                }

                db.SaveChanges();
                Log.Information($"{userName} successfully updated DocTypeAInstrument ID: {instrument.DocTypeAInstrumentID}");
            }

                
        }

        public List<NameType> GetNameTypes()
        {
            using (var db = new IndexerDBEntities()) { 
                return db.NameTypes.ToList();
            }
        }

        internal List<DocStatus> GetDocStatusesForDocType(int docTypeID)
        {
            using (var db = new IndexerDBEntities())
            {
                return db.DocStatuses.Where(x=>x.DocTypeID==docTypeID).ToList();
            }
        }

        
    }

    
}