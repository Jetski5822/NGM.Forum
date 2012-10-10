using Orchard.ContentManagement;

namespace NGM.Forum.Models {
    public class UserForumPart : ContentPart<UserForumPartRecord> {
        public bool RequiresModeration {
            get { return Record.RequiresModeration; }
            set { Record.RequiresModeration = value; }
        }
    }
}