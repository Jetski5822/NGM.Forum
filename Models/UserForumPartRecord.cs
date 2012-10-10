using Orchard.ContentManagement.Records;

namespace NGM.Forum.Models {
    public class UserForumPartRecord : ContentPartRecord {
        public virtual bool RequiresModeration { get; set; }
    }
}