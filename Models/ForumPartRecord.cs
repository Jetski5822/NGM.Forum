using System;
using Orchard.ContentManagement.Records;

namespace NGM.Forum.Models {
    public class ForumPartRecord : ContentPartRecord {
        public virtual int Position { get; set; }
        
        public virtual bool IsClosed { get; set; }
        public virtual int ThreadCount { get; set; }
        public virtual int PostCount { get; set; }

        public virtual bool UsePopularityAlgorithm { get; set; }

        //public virtual string CategoryTitle { get; set; }
        //public virtual int CategoryPosition { get; set; }
    }
}