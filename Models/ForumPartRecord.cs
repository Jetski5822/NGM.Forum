using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace NGM.Forum.Models {
    public class ForumPartRecord : ContentPartRecord {
        [StringLengthMax]
        public virtual string Description { get; set; }

        public virtual int ThreadCount { get; set; }
        public virtual int PostCount { get; set; }
    }
}