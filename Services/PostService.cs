using System.Collections.Generic;
using System.Linq;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;

namespace NGM.Forum.Services {
    public interface IPostService : IDependency {
        ContentItem Get(int id, VersionOptions versionOptions);
        IEnumerable<PostPart> Get(ThreadPart threadPart, VersionOptions versionOptions);
        IEnumerable<PostPart> Get(ThreadPart threadPart, int skip, int count);
        IEnumerable<PostPart> Get(ThreadPart threadPart, int skip, int count, VersionOptions versionOptions);
        PostPart GetLatestPost(ForumPart forumPart, VersionOptions versionOptions);
        PostPart GetLatestPost(ThreadPart threadPart, VersionOptions versionOptions);
    }

    public class PostService : IPostService {
        private readonly IContentManager _contentManager;

        public PostService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public ContentItem Get(int id, VersionOptions versionOptions) {
            return _contentManager.Get(id, versionOptions);
        }

        public PostPart GetLatestPost(ForumPart forumPart, VersionOptions versionOptions) {
            return _contentManager
                .Query<PostPart, PostPartRecord>(versionOptions)
                .Join<ThreadPartRecord>()
                .Join<ForumPartRecord>().Where(f => f.Id == forumPart.Id)
                .List()
                .LastOrDefault();
        }

        public PostPart GetLatestPost(ThreadPart threadPart, VersionOptions versionOptions) {
            return _contentManager
                .Query<PostPart, PostPartRecord>(versionOptions)
                .Join<ThreadPartRecord>()
                .Join<ForumPartRecord>().Where(f => f.Id == threadPart.Id)
                .List()
                .LastOrDefault();
        }

        public IEnumerable<PostPart> Get(ThreadPart threadPart, VersionOptions versionOptions) {
            return _contentManager
                .Query(versionOptions, ContentPartConstants.Post)
                .Join<CommonPartRecord>().Where(cpr => cpr.Container == threadPart.ContentItem.Record)
                .List()
                .ToList()
                .Select(ci => ci.As<PostPart>());            
        }

        public IEnumerable<PostPart> Get(ThreadPart threadPart, int skip, int count) {
            return Get(threadPart, skip, count, VersionOptions.Published);
        }

        public IEnumerable<PostPart> Get(ThreadPart threadPart, int skip, int count, VersionOptions versionOptions) {
            return _contentManager
                .Query(versionOptions, ContentPartConstants.Post)
                .Join<CommonPartRecord>().Where(cpr => cpr.Container == threadPart.ContentItem.Record)
                .Slice(skip, count)
                .ToList()
                .Select(ci => ci.As<PostPart>());
        }

    }
}