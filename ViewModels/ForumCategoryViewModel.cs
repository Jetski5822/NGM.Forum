using NGM.Forum.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NGM.Forum.ViewModels
{
    public class ForumCategoryViewModel
    {
        public String Title { get; set; }
        public String Description { get; set; }
        public int Weight { get; set; }
        public IList<ForumEntry> ForumEntries { get; set; }

    }

}