using System;
using NGM.Forum.Extensions;
using NGM.Forum.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace NGM.Forum.Models {
    public class ForumsHomePagePart : ContentPart<ForumsHomePagePartRecord>
    {
        public string Title {
            get { return this.As<ITitleAspect>().Title; }
        }
  
    }


    public class ForumsHomePagePartRecord : ContentPartRecord
    {
    }

}