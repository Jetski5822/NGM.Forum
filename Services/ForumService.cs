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
        IList<ContentTypeDefinition> GetForumTypes();
        IEnumerable<ForumPart> GetForumsForCategory(ForumCategoryPart forumCategoryPart, VersionOptions versionOptions);
        IEnumerable<ForumPart> GetForumsForCategories(List<int> ids, VersionOptions versionOptions);
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

        public IEnumerable<ForumPart> GetForumsForCategories(List<int> ids, VersionOptions versionOptions)
        {
            return _contentManager.Query<ForumPart, ForumPartRecord>(versionOptions)
                .WithQueryHints(new QueryHints().ExpandRecords<AutoroutePartRecord, TitlePartRecord, CommonPartRecord>())
                .OrderBy(fp => fp.Weight)
                .Join<CommonPartRecord>()
                .Where(cpr => ids.Contains( cpr.Container.Id ))
                .List();
        }

        public  IEnumerable<ForumPart> GetForumsForCategory(ForumCategoryPart forumCategoryPart, VersionOptions versionOptions)
        {
            return _contentManager.Query<ForumPart, ForumPartRecord>(versionOptions)
                .WithQueryHints(new QueryHints().ExpandRecords<AutoroutePartRecord, TitlePartRecord, CommonPartRecord>())
                .OrderBy( fp=>fp.Weight)
                .Join<CommonPartRecord>()
                .Where( cpr=>cpr.Container.Id == forumCategoryPart.Id)
                .List();
        }
        public void Delete(ForumPart forum) {
            _contentManager.Remove(forum.ContentItem);
        }

        public IList<ContentTypeDefinition> GetForumTypes() {
            var name = typeof (ForumPart).Name;

            return _contentManager
                .GetContentTypeDefinitions()
                .Where(x => 
                    x.Parts.Any(p => p.PartDefinition.Name == name))
                .Select(x => x)
                .ToList();
        }
    }
}