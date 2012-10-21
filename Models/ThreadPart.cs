using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace NGM.Forum.Models {
    public class ThreadPart : ContentPart<ThreadPartRecord> {
        public string Title {
            get { return this.As<ITitleAspect>().Title; }
        }

        [CascadeAllDeleteOrphan]
        public ForumPart ForumPart {
            get { return this.As<ICommonPart>().Container.As<ForumPart>(); }
            set { this.As<ICommonPart>().Container = value; }
        }

        public int PostCount {
            get { return Record.PostCount; }
            set { Record.PostCount = value; }
        }

        public bool Approved {
            get { return Record.Approved; }
            set { Record.Approved = value; }
        }

        public int ReplyCount {
            get { return PostCount >= 1 ? PostCount - 1 : 0; }
        }
    }

    public class ThreadPartRecord : ContentPartRecord {
        public virtual int PostCount { get; set; }

        public virtual bool Approved { get; set; }
    }
}