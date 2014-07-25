using System;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Records;
using Orchard.Core.Common.Utilities;
using Orchard.Security;
using Orchard.Core.Title.Models;

namespace NGM.Forum.Models {
    public class ThreadPart : ContentPart<ThreadPartRecord>, IThreadPart {

        private readonly LazyField<IUser> _closedBy = new LazyField<IUser>();
        private readonly LazyField<PostPart> _firstPost = new LazyField<PostPart>();
        private readonly LazyField<PostPart> _latestPost = new LazyField<PostPart>();

        public LazyField<IUser> ClosedByField { get { return _closedBy; } }
        public LazyField<PostPart> FirstPostField { get { return _firstPost; } }
        public LazyField<PostPart> LatestPostField { get { return _latestPost; } }

        public  enum ReadStateEnum { NOT_SET, Unread, NewPosts, ReadNoNewPosts };

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

        //this is to optimize the lookup of 'new posts', and must be manually syncronized on every post 
        //it should also change such as when the lastest post is inappropriate.
        public DateTime? LastestValidPostDate
        {
            get { return Record.LatestValidPostDate; }
            set { Record.LatestValidPostDate = value; }
        }

        public string ClosedDescription {
            get { return Record.ClosedDescription; }
            set { Record.ClosedDescription = value; }
        }

        public int ReplyCount {
            get
            {
                int count = 0;
                //if the thread is marked as inappropriate, then it's first thread is already removed from the count
                if (this.IsInappropriate == true)
                {
                    count = this.PostCount;
                }
                else
                {
                    count = PostCount >= 1 ? PostCount - 1 : 0;
                }
                return count;
            }
        }

        public bool IsClosed {
            get { return ClosedOnUtc != null; }        
        }

        public bool IsDeleted
        {
            get { return Record.IsDeleted; }
            set { Record.IsDeleted = value; }
        }

        public bool IsInappropriate
        {
            get { return Record.IsInappropriate; }
            set { Record.IsInappropriate = value; }
        }

        public int ForumsHomepageId {
            get { return Record.ForumsHomepageId; }
            set { Record.ForumsHomepageId = value; }
        }

        public ReadStateEnum ReadState { get; set; }

        /*not db stored.  Used by the subscription system*/
        public bool UserIsSubscribedByEmail { get;set;}

        
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

        public virtual bool IsDeleted { get; set; }
        public virtual bool IsInappropriate { get; set; }

        public virtual DateTime? LatestValidPostDate { get; set; }

        //redundant information required for speed purposes
        public virtual int ForumsHomepageId { get; set; }
    }
}