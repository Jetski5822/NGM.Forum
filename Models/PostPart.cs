using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Data.Conventions;

namespace NGM.Forum.Models {
    public class PostPart : ContentPart<PostPartRecord> {
        public string Text {
            get { return this.As<BodyPart>().Text; }
            set { this.As<BodyPart>().Text = value; }
        }

        [CascadeAllDeleteOrphan]
        public ThreadPart ThreadPart {
            get { return this.As<ICommonPart>().Container.As<ThreadPart>(); }
            set { this.As<ICommonPart>().Container = value; }
        }

        public int ParentPostId {
            get { return Record.ParentPostId; }
            set { Record.ParentPostId = value; }
        }

        public bool IsAnswer {
            get { return Record.IsAnswer; }
            set { Record.IsAnswer = value; }
        }

        public bool IsParentThread() {
            return ParentPostId == 0;
        }
    }
}