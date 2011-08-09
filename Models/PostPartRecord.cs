using Orchard.ContentManagement.Records;

namespace NGM.Forum.Models {
    public class PostPartRecord : ContentPartRecord {
        public virtual int ParentPostId { get; set; }
        public virtual bool IsAnswer { get; set; }
    }
}