using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.Data.Conventions;

namespace NGM.Forum.Models {
    public class ForumPart : ContentPart<ForumPartRecord> {
        public string Title {
            get { return this.As<ITitleAspect>().Title; }
        }

        public string Description {
            get { return Record.Description; }
            set { Record.Description = value; }
        }

        public int ThreadCount {
            get { return Record.ThreadCount; }
            set { Record.ThreadCount = value; }
        }

        public int PostCount {
            get { return Record.PostCount; }
            set { Record.PostCount = value; }
        }

        public bool ThreadedPosts {
            get { return Record.ThreadedPosts; }
            set { Record.ThreadedPosts = value; }
        }

        public int ReplyCount {
            get { return PostCount >= ThreadCount ? PostCount - ThreadCount : 0; }
        }
    }

    public class ForumPartRecord : ContentPartRecord {
        [StringLengthMax]
        public virtual string Description { get; set; }

        public virtual int ThreadCount { get; set; }
        public virtual int PostCount { get; set; }

        public virtual bool ThreadedPosts { get; set; }
    }
}