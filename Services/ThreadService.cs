using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Routable.Models;
using Orchard.Core.Routable.Services;

namespace NGM.Forum.Services {
    public interface IThreadService : IDependency {
        ThreadPart Get(ForumPart forumPart, string slug, VersionOptions versionOptions);
        ContentItem Get(int id, VersionOptions versionOptions);
        IEnumerable<ThreadPart> Get(ForumPart forumPart);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, VersionOptions versionOptions);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count);
        IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count, VersionOptions versionOptions);

        void CloseThread(ThreadPart threadPart);
        void OpenThread(ThreadPart threadPart);
    }

    [UsedImplicitly]
    public class ThreadService : IThreadService {
        private readonly IContentManager _contentManager;

        public ThreadService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public ThreadPart Get(ForumPart forumPart, string slug, VersionOptions versionOptions) {
            var threadPath = forumPart.As<IRoutableAspect>().GetChildPath(slug);

            return _contentManager.Query(versionOptions, ContentTypeConstants.Thread).Join<RoutePartRecord>().Where(rr => rr.Path == threadPath).
                    Join<CommonPartRecord>().Where(cr => cr.Container == forumPart.Record.ContentItemRecord).List().
                    SingleOrDefault().As<ThreadPart>();
        }

        public ContentItem Get(int id, VersionOptions versionOptions) {
            return _contentManager.Get(id, versionOptions);
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart) {
            return Get(forumPart, VersionOptions.Published);
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, VersionOptions versionOptions) {
            return GetForumQuery(forumPart, versionOptions).List().Select(ci => ci.As<ThreadPart>());
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count) {
            return Get(forumPart, skip, count, VersionOptions.Published);
        }

        public IEnumerable<ThreadPart> Get(ForumPart forumPart, int skip, int count, VersionOptions versionOptions) {
            return GetForumQuery(forumPart, versionOptions).Slice(skip, count).ToList().Select(ci => ContentExtensions.As<ThreadPart>(ci));
        }

        public void CloseThread(ThreadPart threadPart) {
            threadPart.IsClosed = true;
        }

        public void OpenThread(ThreadPart threadPart) {
            threadPart.IsClosed = false;
        }

        private IContentQuery<ContentItem, CommonPartRecord> GetForumQuery(ContentPart<ForumPartRecord> forum, VersionOptions versionOptions) {
            return
                _contentManager.Query(versionOptions, ContentTypeConstants.Thread).Join<CommonPartRecord>().Where(
                    cr => cr.Container == forum.Record.ContentItemRecord).OrderByDescending(cr => cr.CreatedUtc);
        }
    }
}