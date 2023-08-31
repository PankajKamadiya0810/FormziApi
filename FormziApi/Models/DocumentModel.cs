using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class DocumentModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Path { get; set; }
        public int JobId { get; set; }
        public long EmployeeId { get; set; }
      
    }
}