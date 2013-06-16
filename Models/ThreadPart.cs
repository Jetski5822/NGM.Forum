using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Utilities;
using Orchard.Security;

namespace NGM.Forum.Models {
    public class ThreadPart : ContentPart<ThreadPartRecord>, IThreadPart {
        private readonly LazyField<IUser> _closedBy = new LazyField<IUser>();
        private readonly LazyField<PostPart> _firstPost = new LazyField<PostPart>();
        private readonly LazyField<PostPart> _latestPost = new LazyField<PostPart>();

        public LazyField<IUser> ClosedByField { get { return _closedBy; } }
        public LazyField<PostPart> FirstPostField { get { return _firstPost; } }
        public LazyField<PostPart> LatestPostField { get { return _firstPost; } }

        public string Title {
            get { return this.As<ITitleAspect>().Title; }
        }

        public ForumPart ForumPart {
            get { return this.As<ICommonPart>().Container.As<ForumPart>(); }
            set { this.As<ICommonPart>().Container = value; }
        }

        public int PostCount {
            get { return Record.PostCount; }
            set { Record.PostCount = value; }
        }

        public bool IsSticky {
            get { return Record.IsSticky; }
            set { Record.IsSticky = value; }
        }

        public DateTime? ClosedOnUtc {
            get { return Record.ClosedOnUtc; }
            set { Record.ClosedOnUtc = value; }
        }

        public IUser ClosedBy {
            get { return _closedBy.Value; }
            set { _closedBy.Value = value; }
        }

        public PostPart FirstPost {
            get { return _firstPost.Value; }
            set { _firstPost.Value = value; }
        }

        public PostPart LatestPost {
            get { return _latestPost.Value; }
            set { _latestPost.Value = value; }
        }

        public string ClosedDescription {
            get { return Record.ClosedDescription; }
            set { Record.ClosedDescription = value; }
        }

        public int ReplyCount {
            get { return PostCount >= 1 ? PostCount - 1 : 0; }
        }

        public bool IsClosed {
            get { return ClosedOnUtc != null; }
        }
    }

    public interface IThreadPart : IContent {
        DateTime? ClosedOnUtc { get; set; }
        IUser ClosedBy { get; set; }
        string ClosedDescription { get; set; }
    }

    public class ThreadPartRecord : ContentPartRecord {
        public virtual int PostCount { get; set; }
        public virtual bool IsSticky { get; set; }

        public virtual DateTime? ClosedOnUtc { get; set; }
        public virtual int ClosedById { get; set; }
        public virtual string ClosedDescription { get; set; }
    }
}