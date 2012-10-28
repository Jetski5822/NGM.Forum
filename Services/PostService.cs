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
        IEnumerable<PostPart> Get(ThreadPart threadPart, int skip, int count, VersionOptions versionOptions, ApprovalOptions approvalOptions);
        PostPart GetFirstPost(ThreadPart threadPart, VersionOptions versionOptions, ApprovalOptions approvalOptions);
        PostPart GetLatestPost(ForumPart forumPart, VersionOptions versionOptions, ApprovalOptions approvalOptions);
        PostPart GetLatestPost(ThreadPart threadPart, VersionOptions versionOptions, ApprovalOptions approvalOptions);
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

        public PostPart GetLatestPost(ForumPart forumPart, VersionOptions versionOptions, ApprovalOptions approvalOptions) {
            var threadParts = _contentManager
                .Query<ThreadPart, ThreadPartRecord>(versionOptions)
                .Join<CommonPartRecord>().Where(cpr => cpr.Container == forumPart.ContentItem.Record)
                .List();

            return threadParts
                .Select(o => GetLatestPost(o, versionOptions, approvalOptions))
                .OrderBy(o => o.As<ICommonPart>().PublishedUtc)
                .LastOrDefault();
        }

        public PostPart GetFirstPost(ThreadPart threadPart, VersionOptions versionOptions, ApprovalOptions approvalOptions) {
            return GetQuery(threadPart, versionOptions, approvalOptions).FirstOrDefault(o => o.IsParentThread());
        }

        public PostPart GetLatestPost(ThreadPart threadPart, VersionOptions versionOptions, ApprovalOptions approvalOptions) {
            return GetQuery(threadPart, versionOptions, approvalOptions).LastOrDefault();
        }

        private IEnumerable<PostPart> GetQuery(ThreadPart threadPart, VersionOptions versionOptions, ApprovalOptions approvalOptions) {
            var query = _contentManager.Query<PostPart, PostPartRecord>(versionOptions);

            if (approvalOptions == ApprovalOptions.All) {
                query = query.Join<PostPartRecord>().Where(trd => trd.Approved == approvalOptions.IsApproved);
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
            return Get(threadPart, skip, count, VersionOptions.Published, ApprovalOptions.Approved);
        }

        public IEnumerable<PostPart> Get(ThreadPart threadPart, int skip, int count, VersionOptions versionOptions, ApprovalOptions approvalOptions) {
            var query = _contentManager.Query(versionOptions, Constants.Parts.Post);

            if (!Equals(approvalOptions, ApprovalOptions.All)) {
                query = query.Join<ThreadPartRecord>().Where(trd => trd.Approved == approvalOptions.IsApproved);
            }

            return query.Join<CommonPartRecord>().Where(cpr => cpr.Container == threadPart.ContentItem.Record)
            .Slice(skip, count)
            .ToList()
            .Select(ci => ci.As<PostPart>());
        }

    }
}