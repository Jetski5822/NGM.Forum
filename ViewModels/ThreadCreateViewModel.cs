using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NGM.Forum.ViewModels
{
    public class ThreadCreateViewModel
    {
        public int ForumId { get; set; }

        public String ThreadTitle { get; set; }

        public bool ShowIsSticky{get;set;}
        public bool isSticky { get; set; }

        public String Text { get; set; }  //this needs to match the Model users in the Post.Body.Editor.cshtml
        public String EditorFlavor { get; set; }  //this needs to match the Model users in the Post.Body.Editor.cshtml

        public String ReturnUrl { get; set; }
        
    }
}