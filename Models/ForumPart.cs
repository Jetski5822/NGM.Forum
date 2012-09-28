using Orchard.ContentManagement;
using Orchard.Core.Title.Models;

namespace NGM.Forum.Models {
    public class ForumPart : ContentPart<ForumPartRecord> {
        public string Title {
            get { return this.As<TitlePart>().Title; }
            set { this.As<TitlePart>().Title = value; }
        }

        public int ThreadCount {
            get { return Record.ThreadCount; }
            set { Record.ThreadCount = value; }
        }

        public int PostCount {
            get { return Record.PostCount; }
            set { Record.PostCount = value; }
        }

        public bool IsClosed {
            get { return Record.IsClosed; }
            set { Record.IsClosed = value; }
        }

        public bool UsePopularityAlgorithm {
            get { return Record.UsePopularityAlgorithm; }
            set { Record.UsePopularityAlgorithm = value; }
        }

        public int ReplyCount {
            get { return PostCount >= ThreadCount ? PostCount - ThreadCount : 0; }
        }
    }
}