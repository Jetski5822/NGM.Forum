using NGM.Forum.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NGM.Forum.ViewModels
{
    public class RemoveInappropriateFlagViewModel
    {

        public ReportedPostRecord Report {get;set;}
        public String ReturnUrl { get;set;}
    }
}
