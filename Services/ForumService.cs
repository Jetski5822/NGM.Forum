using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NGM.Forum.Models;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;

namespace NGM.Forum.Services {
    public interface IForumService : IDependency {
        ContentItem Get(int id, VersionOptions versionOptions);
        IEnumerable<ForumPart> Get();
        IEnumerable<ForumPart> Get(VersionOptions versionOptions);
        ForumPart Get(string path);

        void CloseForum(ForumPart forumPart);
        void OpenForum(ForumPart forumPart);
    }

    [UsedImplicitly]
    public class ForumService : IForumService {
        private readonly IContentManager _contentManager;

        public ForumService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public ForumPart Get(string path) {
            return _contentManager.Query<ForumPart, ForumPartRecord>()
                .Join<AutoroutePartRecord>().Where(rr => rr.DisplayAlias == path)
                .List().FirstOrDefault();
        }

        public void CloseForum(ForumPart forumPart) {
            forumPart.IsClosed = true;
        }

        public void OpenForum(ForumPart forumPart) {
            forumPart.IsClosed = false;
        }

        public IEnumerable<ForumPart> Get() {
            return Get(VersionOptions.Published);
        }

        public IEnumerable<ForumPart> Get(VersionOptions versionOptions) {
            return _contentManager.Query<ForumPart, ForumPartRecord>(versionOptions)
                .Join<TitlePartRecord>()
                .OrderBy(br => br.Title)
                .List();
        }

        public ContentItem Get(int id, VersionOptions versionOptions) {
            return _contentManager.Get(id, versionOptions);
        }
    }
}