using System.Collections.Generic;
using System.Linq;
using NGM.Forum.Models;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Security;

namespace NGM.Forum.Services {
    public interface IPostService : IDependency {
        ContentItem Get(int id, VersionOptions versionOptions);
        IEnumerable<PostPart> Get(ThreadPart threadPart);
        IEnumerable<PostPart> Get(ThreadPart threadPart, VersionOptions versionOptions);
        IEnumerable<PostPart> Get(ThreadPart threadPart, int skip, int count);
        IEnumerable<PostPart> Get(ThreadPart threadPart, int skip, int count, VersionOptions versionOptions);
        PostPart GetFirstPost(ThreadPart threadPart, VersionOptions versionOptions);
        PostPart GetLatestPost(ThreadPart threadPart, VersionOptions versionOptions);
        IEnumerable<IUser> GetUsersPosted(ThreadPart part);
        int Count(ThreadPart threadPart, VersionOptions versionOptions);
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

        public ContentItem Get(int id, VersionOptions versionOptions) {
            return _contentManager.Get(id, versionOptions);
        }

        public PostPart GetFirstPost(ThreadPart threadPart, VersionOptions versionOptions) {
            return GetParentQuery(threadPart, versionOptions)
                .OrderBy(o => o.PublishedUtc)
                .ForPart<PostPart>()
                .Slice(1)
                .FirstOrDefault();
        }

        public PostPart GetLatestPost(ThreadPart threadPart, VersionOptions versionOptions) {
            return GetParentQuery(threadPart, versionOptions)
                .OrderByDescending(o => o.PublishedUtc)
                .ForPart<PostPart>()
                .Slice(1)
                .FirstOrDefault();
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
}