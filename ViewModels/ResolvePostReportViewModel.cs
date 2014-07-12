using NGM.Forum.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NGM.Forum.ViewModels
{
    public class ResolvePostReportViewModel        
    {
        public ReportedPostRecord Report { get; set; }
        public String ReturnUrl { get; set; }
    }
}