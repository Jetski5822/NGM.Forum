using Orchard;
using Orchard.ContentManagement.Records;
using System;
using System.Collections.Generic;

namespace NGM.Forum.Models {	

    public class ForumCategoryPartRecord : ContentPartRecord {
        public virtual String Description { get; set; }
        public virtual int Weight { get; set; }        
    }

}