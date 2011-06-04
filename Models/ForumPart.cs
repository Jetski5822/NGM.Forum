using Orchard.ContentManagement;

namespace NGM.Forum.Models {
    public class ForumPart : ContentPart<ForumPartRecord> {
        public int ThreadCount {
            get { return Record.ThreadCount; }
            set { Record.ThreadCount = value; }
        }

        public int PostCount {
            get { return Record.PostCount; }
            set { Record.PostCount = value; }
        }
    }
}