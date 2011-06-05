using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NGM.Forum.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Routable.Models;

namespace NGM.Forum.Services {
    public interface IForumService : IDependency {
        ContentItem Get(int id, VersionOptions versionOptions);
        IEnumerable<ForumPart> Get();
        IEnumerable<ForumPart> Get(VersionOptions versionOptions);
        ForumPart Get(string path);

        void CloseThread(ForumPart forumPart);
        void OpenThread(ForumPart forumPart);
    }

    [UsedImplicitly]
    public class ForumService : IForumService {
        private readonly IContentManager _contentManager;

        public ForumService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public ForumPart Get(string path) {
            return _contentManager.Query<ForumPart, ForumPartRecord>()
                .Join<RoutePartRecord>().Where(rr => rr.Path == path)
                .List().FirstOrDefault();
        }

        public void CloseThread(ForumPart forumPart) {
            forumPart.IsClosed = true;
        }

        public void OpenThread(ForumPart forumPart) {
            forumPart.IsClosed = false;
        }

        public IEnumerable<ForumPart> Get() {
            return Get(VersionOptions.Published);
        }

        public IEnumerable<ForumPart> Get(VersionOptions versionOptions) {
            return _contentManager.Query<ForumPart, ForumPartRecord>(versionOptions)
                .Join<RoutePartRecord>()
                .OrderBy(br => br.Title)
                .List();
        }

        public ContentItem Get(int id, VersionOptions versionOptions) {
            return _contentManager.Get(id, versionOptions);
        }
    }
}