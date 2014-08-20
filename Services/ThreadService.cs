using System.Collections.Generic;
using System.Linq;
using NGM.Forum.Models;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Security;

namespace NGM.Forum.Services {
    public interface IThreadService : IDependency {
        ThreadPart Get(int forumId, int threadId, bool includeInappropriate, VersionOptions versionOptions);
        ThreadPart Get(int id, bool includeInappropriate, VersionOptions versionOptions);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, bool includeInappropriate);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, bool includeInappropriate , VersionOptions versionOptions);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count, bool includeInappropriate);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count, bool includeInappropriate, VersionOptions versionOptions);
        int Count(ForumPart forumPart, VersionOptions versionOptions);
        void Delete(ForumPart forumPart, bool includeInappropriate);        
    }

    public class ThreadService : IThreadService {
        private readonly IContentManager _contentManager;

        public ThreadService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public ThreadPart Get(int forumId, int threadId, bool includeInappropriate, VersionOptions versionOptions)
        {
            var threadPart = _contentManager.Query<CommonPart, CommonPartRecord>(versionOptions)
                      .Where(cpr => cpr.Container.Id == forumId)
                      .Join<ThreadPartRecord>()
                      .Where( o=>o.Id == threadId );

            if ( includeInappropriate == false ) {
                threadPart = threadPart.Where(o => o.IsInappropriate == false);
            }
             
             return threadPart.WithQueryHints(new QueryHints().ExpandRecords<AutoroutePartRecord, TitlePartRecord, CommonPartRecord>())
                      .ForPart<ThreadPart>()
                      .Slice(1)
                      .SingleOrDefault();
        }

        public ThreadPart Get(int id, bool includeInappropriate, VersionOptions versionOptions)
        {
            var threadPart = _contentManager.Query<ThreadPart, ThreadPartRecord>(versionOptions);
            if ( includeInappropriate == false ) {
                threadPart = threadPart.Where(o => o.IsInappropriate == false);
            }
            return threadPart.WithQueryHints(new QueryHints().ExpandRecords<AutoroutePartRecord, TitlePartRecord, CommonPartRecord>()).Where(x => x.Id == id).Slice(1).SingleOrDefault();
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, bool includeInappropriate)
        {
            return Get(forumPart, includeInappropriate, VersionOptions.Published);
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, bool includeInappropriate, VersionOptions versionOptions) {
            return Get(forumPart, 0, 0,includeInappropriate, versionOptions);
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count, bool includeInappropriate) {
            return Get(forumPart, skip, count, includeInappropriate, VersionOptions.Published);
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count, bool includeInappropriate, VersionOptions versionOptions) {
            var threads= GetParentQuery(forumPart, versionOptions)
                .Join<ThreadPartRecord>();

            if ( includeInappropriate == false ) {
                threads.Where(threadRec => threadRec.IsInappropriate == false);
            }
            return threads.OrderByDescending(o => o.IsSticky)
                    .Join<CommonPartRecord>()
                    .OrderByDescending(o => o.ModifiedUtc)
                    .ForPart<ThreadPart>()
                    .Slice(skip, count)
                    .ToList();
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, bool includeInappropriate, IUser user) {
            var threads = GetParentQuery(forumPart, VersionOptions.Published)
                .Where(o => o.OwnerId == user.Id)
                .Join<ThreadPartRecord>();
            if ( includeInappropriate == false ) {
                threads.Where(threadRec => threadRec.IsInappropriate == false);
            }
            return threads.OrderByDescending(o => o.IsSticky)
                .Join<CommonPartRecord>()
                .OrderByDescending(o => o.ModifiedUtc)
                .ForPart<ThreadPart>()
                .List()
                .ToList();
        }

        public int Count(ForumPart forumPart, VersionOptions versionOptions) {
            return GetParentQuery(forumPart, versionOptions).Count();
        }

        public void Delete(ForumPart forumPart, bool includeInappropriate) {
            Get(forumPart, true)
                .ToList()
                .ForEach(thread => _contentManager.Remove(thread.ContentItem));
        }

        private IContentQuery<CommonPart, CommonPartRecord> GetParentQuery(IContent parentPart, VersionOptions versionOptions) {
            return _contentManager.Query<CommonPart, CommonPartRecord>(versionOptions)
                                  .Where(cpr => cpr.Container == parentPart.ContentItem.Record);
        }
    }
}