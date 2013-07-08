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
        ThreadPart Get(int id, VersionOptions versionOptions);
        IEnumerable<ThreadPart> Get(ForumPart forumPart);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, VersionOptions versionOptions);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count, VersionOptions versionOptions);
        int Count(ForumPart forumPart, VersionOptions versionOptions);
        void Delete(ForumPart forumPart);
    }

    public class ThreadService : IThreadService {
        private readonly IContentManager _contentManager;

        public ThreadService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public ThreadPart Get(int id, VersionOptions versionOptions) {
            return _contentManager
                .Query<ThreadPart, ThreadPartRecord>(versionOptions)
                .WithQueryHints(new QueryHints().ExpandRecords<AutoroutePartRecord, TitlePartRecord, CommonPartRecord>())
                .Where(x => x.Id == id).List().SingleOrDefault();
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart) {
            return Get(forumPart, VersionOptions.Published);
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, VersionOptions versionOptions) {
            return Get(forumPart, 0, 0, versionOptions);
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count) {
            return Get(forumPart, skip, count, VersionOptions.Published);
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count, VersionOptions versionOptions) {
            return GetParentQuery(forumPart, versionOptions)
                .Join<ThreadPartRecord>()
                .OrderByDescending(o => o.IsSticky)
                .Join<CommonPartRecord>()
                .OrderByDescending(o => o.ModifiedUtc)
                .ForPart<ThreadPart>()
                .Slice(skip, count)
                .ToList();
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, IUser user) {
            return GetParentQuery(forumPart, VersionOptions.Published)
                .Where(o => o.OwnerId == user.Id)
                .Join<ThreadPartRecord>()
                .OrderByDescending(o => o.IsSticky)
                .Join<CommonPartRecord>()
                .OrderByDescending(o => o.ModifiedUtc)
                .ForPart<ThreadPart>()
                .List()
                .ToList();
        }

        public int Count(ForumPart forumPart, VersionOptions versionOptions) {
            return GetParentQuery(forumPart, versionOptions).Count();
        }

        public void Delete(ForumPart forumPart) {
            Get(forumPart)
                .ToList()
                .ForEach(thread => _contentManager.Remove(thread.ContentItem));
        }

        private IContentQuery<CommonPart, CommonPartRecord> GetParentQuery(IContent parentPart, VersionOptions versionOptions) {
            return _contentManager.Query<CommonPart, CommonPartRecord>(versionOptions)
                                  .Where(cpr => cpr.Container == parentPart.ContentItem.Record);
        }
    }
}