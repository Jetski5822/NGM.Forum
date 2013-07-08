using System.Collections.Generic;
using System.Linq;
using NGM.Forum.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Security;

namespace NGM.Forum.Services {
    public interface IPostService : IDependency {
        PostPart Get(int id, VersionOptions versionOptions);
        IEnumerable<PostPart> Get(ThreadPart threadPart);
        IEnumerable<PostPart> Get(ThreadPart threadPart, VersionOptions versionOptions);
        IEnumerable<PostPart> Get(ThreadPart threadPart, int skip, int count);
        IEnumerable<PostPart> Get(ThreadPart threadPart, int skip, int count, VersionOptions versionOptions);
        PostPart GetPositional(ThreadPart threadPart, VersionOptions versionOptions,
                               ThreadPostPositional positional);
        IEnumerable<IUser> GetUsersPosted(ThreadPart part);
        int Count(ThreadPart threadPart, VersionOptions versionOptions);
        void Delete(ThreadPart threadPart);
    }

    public class PostService : IPostService {
        private readonly IContentManager _contentManager;
        private readonly IRepository<CommonPartRecord> _commonRepository;

        public PostService(IContentManager contentManager, IRepository<CommonPartRecord> commonRepository) {
            _contentManager = contentManager;
            _commonRepository = commonRepository;
        }

        public IEnumerable<PostPart> Get(ThreadPart threadPart) {
            return Get(threadPart, VersionOptions.Published);
        }

        public IEnumerable<PostPart> Get(ThreadPart threadPart, VersionOptions versionOptions) {
            return GetParentQuery(threadPart, versionOptions)
                .ForPart<PostPart>()
                .List();
        }

        public PostPart Get(int id, VersionOptions versionOptions) {
            return _contentManager.Query<PostPart, PostPartRecord>(versionOptions)
                .WithQueryHints(new QueryHints().ExpandRecords<CommonPartRecord>())
                .Where(x => x.Id == id)
                .List()
                .SingleOrDefault();
        }

        public PostPart GetPositional(ThreadPart threadPart, VersionOptions versionOptions,
                                      ThreadPostPositional positional) {
            var query = GetParentQuery(threadPart, versionOptions);

            if (positional == ThreadPostPositional.First)
                query = query.OrderBy(o => o.PublishedUtc);

            if (positional == ThreadPostPositional.Latest)
                query = query.OrderByDescending(o => o.PublishedUtc);

            return query
                .ForPart<PostPart>()
                .Slice(1)
                .SingleOrDefault();
        }

        public IEnumerable<IUser> GetUsersPosted(ThreadPart part) {
            var users = _commonRepository.Table.Where(o => o.Container.Id == part.Id)
                             .Select(o => o.OwnerId)
                             .Distinct();

            return _contentManager
                .GetMany<IUser>(users, VersionOptions.Published, new QueryHints())
                .ToList();
        }

        public int Count(ThreadPart threadPart, VersionOptions versionOptions) {
            return GetParentQuery(threadPart, versionOptions).Count();
        }

        public void Delete(ThreadPart threadPart) {
            Get(threadPart, VersionOptions.AllVersions)
                .ToList()
                .ForEach(post => _contentManager.Remove(post.ContentItem));
        }

        public IEnumerable<PostPart> Get(ThreadPart threadPart, int skip, int count) {
            return Get(threadPart, skip, count, VersionOptions.Published);
        }

        public IEnumerable<PostPart> Get(ThreadPart threadPart, int skip, int count, VersionOptions versionOptions) {
            return GetParentQuery(threadPart, versionOptions)
                .OrderBy(o => o.CreatedUtc)
                .ForPart<PostPart>()
                .Slice(skip, count)
                .ToList();
        }

        private IContentQuery<CommonPart, CommonPartRecord> GetParentQuery(IContent parentPart, VersionOptions versionOptions) {
            return _contentManager.Query<CommonPart, CommonPartRecord>(versionOptions)
                                  .Where(cpr => cpr.Container == parentPart.ContentItem.Record);
        }
    }

    public enum ThreadPostPositional {
        First,
        Latest
    }
}