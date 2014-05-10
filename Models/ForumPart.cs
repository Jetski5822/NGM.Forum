using System;
using NGM.Forum.Extensions;
using NGM.Forum.Settings;
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
            get { return Retrieve(x => x.Description); }
            set { Store(x => x.Description, value); }
        }

        public int ThreadCount {
            get { return Retrieve(x => x.ThreadCount); }
            set { Store(x => x.ThreadCount, value); }
        }

        public int PostCount {
            get { return Retrieve(x => x.PostCount); }
            set { Store(x => x.PostCount, value); }
        }

        public bool ThreadedPosts {
            get { return Retrieve(x => x.ThreadedPosts); }
            set { Store(x => x.ThreadedPosts, value); }
        }

        public int Weight {
            get { return Retrieve(x => x.Weight); }
            set { Store(x => x.Weight, value); }
        }

        public int ReplyCount {
            get { return PostCount >= ThreadCount ? PostCount - ThreadCount : 0; }
        }

        public string PostType {
            get {
                var type = Settings.GetModel<ForumPartSettings>().PostType;
                return !String.IsNullOrWhiteSpace(type) ? type : Constants.Parts.Post;
            }
        }

        public string ThreadType {
            get {
                var type = Settings.GetModel<ForumPartSettings>().ThreadType;
                return !String.IsNullOrWhiteSpace(type) ? type : Constants.Parts.Thread;
            }
        }
    }

    public class ForumPartRecord : ContentPartRecord {
        [StringLengthMax]
        public virtual string Description { get; set; }

        public virtual int ThreadCount { get; set; }
        public virtual int PostCount { get; set; }

        public virtual bool ThreadedPosts { get; set; }

        public virtual int Weight { get; set; }
    }
}