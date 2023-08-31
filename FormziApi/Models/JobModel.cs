using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class JobModel
    {
        public int Id { get; set; }
        public byte[] Signature { get; set; }
        public System.DateTime Start { get; set; }
        public System.DateTime Finish { get; set; }
        public int CompletedBy { get; set; }
        public System.DateTime CompletedOn { get; set; }
        public Nullable<int> ApprovedBy { get; set; }
        public Nullable<System.DateTime> ApprovedOn { get; set; }
        public bool IsDeleted { get; set; }
        public long FormId { get; set; }
        public long EmployeeId { get; set; }
        public int LocationId { get; set; }

        public  List<DocumentModel> Documents { get; set; }
        public  EmployeeModel Employee { get; set; }
        public  FormModel Form { get; set; }
        public  LocationModel Location { get; set; }
    }
}