using Orchard.ContentManagement;
using Orchard.ContentManagement.Records;

namespace NGM.Forum.Models {
    public class UserForumPart : ContentPart<UserForumPartRecord> {
        public bool RequiresModeration {
            get { return Record.RequiresModeration; }
            set { Record.RequiresModeration = value; }
        }
    }

    public class UserForumPartRecord : ContentPartRecord {
        public virtual bool RequiresModeration { get; set; }
    }
}