using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Data.Conventions;

namespace NGM.Forum.Models {
    public class PostPart : ContentPart<PostPartRecord> {
        public int ParentPostId {
            get { return Record.ParentPostId; }
            set { Record.ParentPostId = value; }
        }

        public string Text {
            get { return Record.Text; }
            set { this.Record.Text = value; }
        }

        public string Format {
            get { return Record.Format; }
            set { this.Record.Format = value; }
        }

        [CascadeAllDeleteOrphan]
        public ThreadPart ThreadPart {
            get { return this.As<ICommonPart>().Container.As<ThreadPart>(); }
            set { this.As<ICommonPart>().Container = value; }
        }

        public bool IsParentThread() {
            return ParentPostId == 0;
        }
    }
}