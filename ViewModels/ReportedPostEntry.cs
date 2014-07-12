using NGM.Forum.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NGM.Forum.ViewModels
{
    public class ReportedPostEntry
    {
        public bool IsChecked { get; set; }
        public ReportedPostRecordViewModel ReportedPostRecordViewModel { get; set; }
    }
}