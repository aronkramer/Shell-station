using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AmarMati.ReportModel.Model
{
    public class ProgressofFilesModel
    {
        public ProgressofFilesModel() { }

        public Int32? ProcurementID { get; set; }
        public String Procurement_FileNo { get; set; }
        public DateTime? Procurement_FilingDate { get; set; }
        public Int32? ProcurementProgressID { get; set; }
        public Int32? SectionID { get; set; }
        public String SectionName { get; set; }
        public Int32? StepID { get; set; }
        public String StepName { get; set; }
        public Int32? SubStepID { get; set; }
        public String SubStepName { get; set; }
        public bool? IsStarted { get; set; }
        public DateTime? StartDate { get; set; }
        public bool? IsEnded { get; set; }
        public DateTime? EndDate { get; set; }
        public Int32? DaysElapsed { get; set; }
        public Int32? MeanTime { get; set; }
        public Int32? StatusID { get; set; }
        public String Status { get; set; }
        public Int32? ReasonID { get; set; }
        public String Reason { get; set; }
        public String StatusBy { get; set; }
        public DateTime? StatusDate { get; set; }
        public String Remarks { get; set; }
        public String CreatedBy { get; set; }
        public DateTime? CreationDate { get; set; }
        public String UpdateBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}