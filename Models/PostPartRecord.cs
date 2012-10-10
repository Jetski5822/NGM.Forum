using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace NGM.Forum.Models {
    public class PostPartRecord : ContentPartRecord {
        public virtual int ParentPostId { get; set; }

        [StringLengthMax]
        public virtual string Text { get; set; }

        public virtual string Format { get; set; }

        public virtual bool RequiresModeration { get; set; }
    }
}