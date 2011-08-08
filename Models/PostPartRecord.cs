using Orchard.ContentManagement.Records;

namespace NGM.Forum.Models {
    public class PostPartRecord : ContentPartRecord {
        public virtual int ParentPostId { get; set; }
        public virtual int MarkedAs { get; set; }
    }
}