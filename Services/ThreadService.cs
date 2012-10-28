using System.Collections.Generic;
using System.Linq;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;

namespace NGM.Forum.Services {
    public interface IThreadService : IDependency {
        ThreadPart Get(ForumPart forumPart, string slug, VersionOptions versionOptions);
        ContentItem Get(int id, VersionOptions versionOptions);
        IEnumerable<ThreadPart> Get(ForumPart forumPart);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, VersionOptions versionOptions);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count, VersionOptions versionOptions, ApprovalOptions approvalOptions);
        int ThreadCount(ForumPart forumPart, VersionOptions versionOptions);
    }

    public class ThreadService : IThreadService {
        private readonly IContentManager _contentManager;

        public ThreadService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public ThreadPart Get(ForumPart forumPart, string slug, VersionOptions versionOptions) {
            return _contentManager
                .Query<ThreadPart, ThreadPartRecord>()
                .Join<AutoroutePartRecord>()
                .Where(r => r.DisplayAlias.EndsWith(slug))
                .Join<CommonPartRecord>()
                .Where(cr => cr.Container == forumPart.Record.ContentItemRecord)
                .List()
                .FirstOrDefault();
        }

        public ContentItem Get(int id, VersionOptions versionOptions) {
            return _contentManager.Get(id, versionOptions);
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart) {
            return Get(forumPart, VersionOptions.Published);
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, VersionOptions versionOptions) {
            return GetForumQuery(forumPart, versionOptions, ApprovalOptions.All).List().Select(ci => ci.As<ThreadPart>());
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count) {
            return Get(forumPart, skip, count, VersionOptions.Published, ApprovalOptions.All);
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count, VersionOptions versionOptions, ApprovalOptions approvalOptions) {
            return GetForumQuery(forumPart, versionOptions, approvalOptions)
                .Slice(skip, count)
                .ToList()
                .Select(ci => ci.As<ThreadPart>());
        }

        public int ThreadCount(ForumPart forumPart, VersionOptions versionOptions) {
            return GetForumQuery(forumPart, versionOptions, ApprovalOptions.All).Count();
        }

        private IContentQuery<ContentItem, CommonPartRecord> GetForumQuery(ContentPart<ForumPartRecord> forum, VersionOptions versionOptions, ApprovalOptions approvalOptions) {
            var query = _contentManager.Query(versionOptions, Constants.Parts.Thread);

            if (!Equals(approvalOptions, ApprovalOptions.All)) {
                query = query.Join<ThreadPartRecord>().Where(trd => trd.Approved == approvalOptions.IsApproved);
            }

            return query.Join<CommonPartRecord>().Where(
                cr => cr.Container == forum.Record.ContentItemRecord).OrderByDescending(cr => cr.CreatedUtc)
                .WithQueryHintsFor(Constants.Parts.Thread);
        }
    }
}