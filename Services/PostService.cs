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
        IEnumerable<PostPart> Get(ThreadPart threadPart, int skip, int count);
        IEnumerable<PostPart> Get(ThreadPart threadPart, int skip, int count, VersionOptions versionOptions, ModerationOptions moderationOptions);
        PostPart GetFirstPost(ThreadPart threadPart, VersionOptions versionOptions, ModerationOptions moderationOptions);
        PostPart GetLatestPost(ForumPart forumPart, VersionOptions versionOptions, ModerationOptions moderationOptions);
        PostPart GetLatestPost(ThreadPart threadPart, VersionOptions versionOptions, ModerationOptions moderationOptions);
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

        public PostPart GetLatestPost(ForumPart forumPart, VersionOptions versionOptions, ModerationOptions moderationOptions) {
            var threadParts = _contentManager
                .Query<ThreadPart, ThreadPartRecord>(versionOptions)
                .Join<CommonPartRecord>().Where(cpr => cpr.Container == forumPart.ContentItem.Record)
                .List();

            return threadParts
                .Select(o => GetLatestPost(o, versionOptions, moderationOptions))
                .OrderBy(o => o.As<ICommonPart>().PublishedUtc)
                .LastOrDefault();
        }

        public PostPart GetFirstPost(ThreadPart threadPart, VersionOptions versionOptions, ModerationOptions moderationOptions) {
            return GetQuery(threadPart, versionOptions, moderationOptions).FirstOrDefault(o => o.IsParentThread());
        }

        public PostPart GetLatestPost(ThreadPart threadPart, VersionOptions versionOptions, ModerationOptions moderationOptions) {
            return GetQuery(threadPart, versionOptions, moderationOptions).LastOrDefault();
        }

        private IEnumerable<PostPart> GetQuery(ThreadPart threadPart, VersionOptions versionOptions, ModerationOptions moderationOptions) {
            var query = _contentManager.Query<PostPart, PostPartRecord>(versionOptions);

            if (!Equals(moderationOptions, ModerationOptions.All)) {
                return query.Join<ModerationPartRecord>().Where(trd => trd.Approved == moderationOptions.IsApproved)
                            .Join<CommonPartRecord>().Where(cpr => cpr.Container == threadPart.ContentItem.Record)
                            .List();
            }

            return query.Join<CommonPartRecord>().Where(cpr => cpr.Container == threadPart.ContentItem.Record)
                .List();
        }

        public IEnumerable<PostPart> Get(ThreadPart threadPart, VersionOptions versionOptions) {
            return _contentManager
                .Query(versionOptions, Constants.Parts.Post)
                .Join<CommonPartRecord>().Where(cpr => cpr.Container == threadPart.ContentItem.Record)
                .List()
                .ToList()
                .Select(ci => ci.As<PostPart>());            
        }

        public IEnumerable<PostPart> Get(ThreadPart threadPart, int skip, int count) {
            return Get(threadPart, skip, count, VersionOptions.Published, ModerationOptions.Approved);
        }

        public IEnumerable<PostPart> Get(ThreadPart threadPart, int skip, int count, VersionOptions versionOptions, ModerationOptions moderationOptions) {
            var query = _contentManager.Query(versionOptions, Constants.Parts.Post);

            if (!Equals(moderationOptions, ModerationOptions.All)) {
                query = query.Join<ModerationPartRecord>().Where(trd => trd.Approved == moderationOptions.IsApproved);
            }

            return query.Join<CommonPartRecord>().Where(cpr => cpr.Container == threadPart.ContentItem.Record)
            .Slice(skip, count)
            .ToList()
            .Select(ci => ci.As<PostPart>());
        }

    }
}