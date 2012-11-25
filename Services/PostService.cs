using System.Collections.Generic;
using System.Linq;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;

namespace NGM.Forum.Services {
    public interface IPostService : IDependency {
        ContentItem Get(int id, VersionOptions versionOptions);
        IEnumerable<PostPart> Get(ThreadPart threadPart);
        IEnumerable<PostPart> Get(ThreadPart threadPart, VersionOptions versionOptions);
        IEnumerable<PostPart> Get(ThreadPart threadPart, bool isthreaded, int skip, int count);
        IEnumerable<PostPart> Get(ThreadPart threadPart, bool isthreaded, int skip, int count, VersionOptions versionOptions);
        PostPart GetFirstPost(ThreadPart threadPart, VersionOptions versionOptions);
        PostPart GetLatestPost(ForumPart forumPart, VersionOptions versionOptions);
        PostPart GetLatestPost(ThreadPart threadPart, VersionOptions versionOptions);
    }

    public class PostService : IPostService {
        private readonly IContentManager _contentManager;

        public PostService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IEnumerable<PostPart> Get(ThreadPart threadPart) {
            return Get(threadPart, VersionOptions.Published);
        }

        public ContentItem Get(int id, VersionOptions versionOptions) {
            return _contentManager.Get(id, versionOptions);
        }

        public PostPart GetLatestPost(ForumPart forumPart, VersionOptions versionOptions) {
            var threadParts = _contentManager
                .Query<ThreadPart, ThreadPartRecord>(versionOptions)
                .Join<CommonPartRecord>().Where(cpr => cpr.Container == forumPart.ContentItem.Record)
                .List();

            return threadParts
                .Select(o => GetLatestPost(o, versionOptions))
                .OrderBy(o => o.As<ICommonPart>().PublishedUtc)
                .LastOrDefault();
        }

        public PostPart GetFirstPost(ThreadPart threadPart, VersionOptions versionOptions) {
            return GetQuery(threadPart, versionOptions).List().FirstOrDefault(o => o.IsParentThread());
        }

        public PostPart GetLatestPost(ThreadPart threadPart, VersionOptions versionOptions) {
            return GetQuery(threadPart, versionOptions).List().LastOrDefault();
        }

        public IEnumerable<PostPart> Get(ThreadPart threadPart, VersionOptions versionOptions) {
            return (Get(threadPart, false, 0, 0, versionOptions));
        }

        public IEnumerable<PostPart> Get(ThreadPart threadPart, bool isthreaded, int skip, int count) {
            return Get(threadPart, isthreaded, skip, count, VersionOptions.Published);
        }

        public IEnumerable<PostPart> Get(ThreadPart threadPart, bool isthreaded, int skip, int count, VersionOptions versionOptions) {
            if (isthreaded) {
                var query = _contentManager.Query<PostPart, PostPartRecord>(versionOptions);
                // Order by the Replied on, then by the published date... That should be good enough
                var pp = query
                    .Join<CommonPartRecord>()//.OrderBy(c => c.PublishedUtc)
                    .Where(cpr => cpr.Container == threadPart.ContentItem.Record)
                    .Join<PostPartRecord>()
                    .OrderBy(p => p.RepliedOn)
                        .Slice(skip, count)
                        .ToList();

                return pp;
            }

            return GetQuery(threadPart, versionOptions)
                        .Slice(skip, count)
                        .ToList();
        }

        private IContentQuery<PostPart, CommonPartRecord> GetQuery(ThreadPart threadPart, VersionOptions versionOptions) {
            return _contentManager.Query<PostPart, PostPartRecord>(versionOptions)
                .Join<CommonPartRecord>()
                .Where(cpr => cpr.Container == threadPart.ContentItem.Record)
                .OrderBy(c => c.PublishedUtc);
        }

    }
}