using NGM.Forum.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NGM.Forum.ViewModels
{
    public class ForumEntry    
    {
        public ForumPartRecord ForumPartRecord { get; set; }
        public String Title { get; set; }
        //public bool IsChecked { get; set; }
    }
}