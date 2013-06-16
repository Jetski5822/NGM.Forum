using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using NGM.Forum.Models;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;

namespace NGM.Forum.Services {
    public interface IForumService : IDependency {
        ForumPart Get(int id, VersionOptions versionOptions);
        IEnumerable<ForumPart> Get();
        IEnumerable<ForumPart> Get(VersionOptions versionOptions);
        ForumPart Get(string path);
        void Delete(ForumPart forum);
    }

    [UsedImplicitly]
    public class ForumService : IForumService {
        private readonly IContentManager _contentManager;

        public ForumService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public ForumPart Get(string path) {
            return _contentManager
                .Query<ForumPart, ForumPartRecord>()
                .WithQueryHints(new QueryHints().ExpandRecords<AutoroutePartRecord, TitlePartRecord, CommonPartRecord>())
                .Join<AutoroutePartRecord>()
                .Where(o => o.DisplayAlias == path)
                .List()
                .FirstOrDefault();
        }

        public IEnumerable<ForumPart> Get() {
            return Get(VersionOptions.Published);
        }

        public IEnumerable<ForumPart> Get(VersionOptions versionOptions) {
            // To avoid a join the order is done after.
            return _contentManager.Query<ForumPart, ForumPartRecord>(versionOptions)
                .WithQueryHints(new QueryHints().ExpandRecords<AutoroutePartRecord, TitlePartRecord, CommonPartRecord>())
                .List()
                .OrderBy(o => o.Title);
        }

        public ForumPart Get(int id, VersionOptions versionOptions) {
            return _contentManager
                .Query<ForumPart, ForumPartRecord>()
                .WithQueryHints(new QueryHints().ExpandRecords<AutoroutePartRecord, TitlePartRecord, CommonPartRecord>())
                .Where(x => x.Id == id).List().FirstOrDefault();
        }

        public void Delete(ForumPart forum) {
            _contentManager.Remove(forum.ContentItem);
        }
    }
}