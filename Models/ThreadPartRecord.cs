using Orchard.ContentManagement.Records;

namespace NGM.Forum.Models {
    public class ThreadPartRecord : ContentPartRecord {
        public virtual bool IsSticky { get; set; }
        public virtual bool IsClosed { get; set; }
        public virtual int PostCount { get; set; }
        public virtual ThreadType Type { get; set; }
    }
}