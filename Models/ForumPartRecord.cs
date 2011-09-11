using Orchard.ContentManagement.Records;

namespace NGM.Forum.Models {
    public class ForumPartRecord : ContentPartRecord {
        public virtual bool IsClosed { get; set; }
        public virtual int ThreadCount { get; set; }
        public virtual int PostCount { get; set; }

        public virtual bool UsePopularityAlgorithm { get; set; }

        public virtual string Category { get; set; }
    }
}