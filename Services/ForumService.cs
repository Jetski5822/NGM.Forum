using System.Collections.Generic;
using System.Linq;
using NGM.Forum.Models;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;

namespace NGM.Forum.Services {
    public interface IForumService : IDependency {
        ForumPart Get(int id, VersionOptions versionOptions);
        IEnumerable<ForumPart> Get();
        IEnumerable<ForumPart> Get(VersionOptions versionOptions);
        void Delete(ForumPart forum);
        IEnumerable<ContentTypeDefinition> GetForumTypes();
    }

    public class ForumService : IForumService {
        private readonly IContentManager _contentManager;

        public ForumService(IContentManager contentManager) {
            _contentManager = contentManager;
        }

        public IEnumerable<ForumPart> Get() {
            return Get(VersionOptions.Published);
        }

        public IEnumerable<ForumPart> Get(VersionOptions versionOptions) {
            return _contentManager.Query<ForumPart, ForumPartRecord>(versionOptions)
                .WithQueryHints(new QueryHints().ExpandRecords<AutoroutePartRecord, TitlePartRecord, CommonPartRecord>())
                .OrderBy(o => o.Weight)
                .List()
                .ToList();
        }

        public ForumPart Get(int id, VersionOptions versionOptions) {
            return _contentManager.Query<ForumPart, ForumPartRecord>(versionOptions)
                .WithQueryHints(new QueryHints().ExpandRecords<AutoroutePartRecord, TitlePartRecord, CommonPartRecord>())
                .Where(x => x.Id == id)
                .List()
                .SingleOrDefault();
        }

        public void Delete(ForumPart forum) {
            _contentManager.Remove(forum.ContentItem);
        }

        public IEnumerable<ContentTypeDefinition> GetForumTypes() {
            return _contentManager.GetContentTypeDefinitions().Where(x => x.Parts.Any(p => p.PartDefinition.Name == typeof(ForumPart).Name)).Select(x => x);
        }
    }
}