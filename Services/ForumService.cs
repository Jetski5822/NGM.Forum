using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NGM.Forum.Models;
using NGM.Forum.Routing;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Title.Models;

namespace NGM.Forum.Services {
    public interface IForumService : IDependency {
        ContentItem Get(int id, VersionOptions versionOptions);
        IEnumerable<ForumPart> Get();
        IEnumerable<ForumPart> Get(VersionOptions versionOptions);
        ForumPart Get(string path);
        void Delete(ContentItem forum);
    }

    [UsedImplicitly]
    public class ForumService : IForumService {
        private readonly IContentManager _contentManager;
        private readonly IForumPathConstraint _forumPathConstraint;

        public ForumService(IContentManager contentManager, IForumPathConstraint forumPathConstraint) {
            _contentManager = contentManager;
            _forumPathConstraint = forumPathConstraint;
        }

        public ForumPart Get(string path) {
            return _contentManager.Query<ForumPart>().List().FirstOrDefault(rr => rr.As<IAliasAspect>().Path == path);
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

        public void Delete(ContentItem forum) {
            _contentManager.Remove(forum);
            _forumPathConstraint.RemovePath(forum.As<IAliasAspect>().Path);
        }
    }
}