using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NGM.Forum.ViewModels
{
    public class ReportedPostRecordViewModel
    {
        //same as the ReportPostRecord model but when strings for the user names
        public int ReportId { get; set; }
        public int PostId { get; set; }
        public int PostByUserId { get; set; }
        public String PostByUserName { get; set; }
        public int ReportedByUserId { get; set; }
        public String ReportedByUserName { get; set; }
        public DateTime? ReportedDate { get; set; }
        public bool IsResolved { get; set; }
        public int ResolvedByUserId { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public String ResolvedByUserName { get; set; }
        public String Note { get; set; }
        public String ReasonReported { get; set; }
        public IContent Post { get; set; }

    }
}