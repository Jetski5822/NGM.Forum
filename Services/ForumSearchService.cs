using System;
using System.Globalization;
using System.Linq;
using Orchard.Collections;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Localization.Services;
using Orchard;

namespace NGM.Forum.Services {

   public interface IForumSearchService : IDependency {
        IPageOfItems<T> Query<T>(string query, int? forumsHomeId, int skip, int? take, bool filterCulture, string index, string[] searchFields, Func<ISearchHit, T> shapeResult);
   }

    public class ForumSearchService : IForumSearchService {

        public const string FORUMS_INDEX_NAME = "ForumsSearchIndex";

        private readonly IIndexManager _indexManager;
        private readonly ICultureManager _cultureManager;

        public ForumSearchService(
            IOrchardServices services, 
            IIndexManager indexManager,
            ICultureManager cultureManager) 
        {
            Services = services;
            _indexManager = indexManager;
            _cultureManager = cultureManager;
            T = NullLocalizer.Instance;
        }

        public IOrchardServices Services { get; set; }
        public Localizer T { get; set; }

        ISearchBuilder Search(string index) {
            return _indexManager.HasIndexProvider()
                ? _indexManager.GetSearchIndexProvider().CreateSearchBuilder(index)
                : new NullSearchBuilder();
        }

        IPageOfItems<T> IForumSearchService.Query<T>(string query, int? forumsHomeId, int page, int? pageSize, bool filterCulture, string index, string[] searchFields, Func<ISearchHit, T> shapeResult) {

            if (string.IsNullOrWhiteSpace(query))
                return new PageOfItems<T>(Enumerable.Empty<T>());

            var searchBuilder = Search(index).Parse(searchFields, query);

            if (filterCulture) {
                var culture = _cultureManager.GetCurrentCulture(Services.WorkContext.HttpContext);

                // use LCID as the text representation gets analyzed by the query parser
                searchBuilder
                    .WithField("culture", CultureInfo.GetCultureInfo(culture).LCID)
                    .AsFilter();
            }

            if (forumsHomeId != null)
            {
                searchBuilder
               .WithField("forumsHomeId", forumsHomeId.Value)
               .AsFilter();
            }

            var totalCount = searchBuilder.Count();
            if (pageSize != null)
                searchBuilder = searchBuilder
                    .Slice((page > 0 ? page - 1 : 0) * (int)pageSize, (int)pageSize);

            var searchResults = searchBuilder.Search();

            var pageOfItems = new PageOfItems<T>(searchResults.Select(shapeResult)) {
                PageNumber = page,
                PageSize = pageSize != null ? (int)pageSize : totalCount,
                TotalItemCount = totalCount
            };

            return pageOfItems;
        }
    }
}