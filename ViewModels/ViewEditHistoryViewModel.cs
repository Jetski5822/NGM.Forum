using NGM.Forum.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NGM.Forum.ViewModels
{
    public class ViewPostEditHistoryViewModel
    {
        public dynamic LastPost { get; set; }
        public IEnumerable<PostEditHistoryEntry> EditHistory { get; set; }
        public string ReturnUrl { get; set; }
    }
}