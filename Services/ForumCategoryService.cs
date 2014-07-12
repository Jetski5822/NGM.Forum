using System.Collections.Generic;
using System.Linq;
using NGM.Forum.Models;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Data;
using NGM.Forum.ViewModels;

namespace NGM.Forum.Services {

    public interface IForumCategoryService : IDependency {
        ForumCategoryPart GetParentCategory(ForumPart forumPart);

        ForumCategoryPart Get(int id, VersionOptions versionOptions);

        IEnumerable<ForumCategoryPart> Get(VersionOptions versionOptions);
        IEnumerable<ForumCategoryPart> GetCategoriesInForumsHomePage(int forumsHomePagePartId);
        IEnumerable<ForumCategoryPart> GetCategoriesInForumsHomePage(int forumsHomePagePartId, VersionOptions versionOptions);

        void Delete(ForumCategoryPart forumCategoryPart);
        IEnumerable<ForumCategoryPart> GetCategoriesWithForums(ForumsHomePagePart forumsHomePagePart, VersionOptions versionOptions);        
        void UpdateForumCategoryForumList(ContentItem item, IEnumerable<ForumEntry> forums);
        IEnumerable<ForumPart> GetForumsForCategory(ForumCategoryPart forumCategoryPart, VersionOptions versionOptions);
        IEnumerable<ForumPart> GetForumsForCategory(ForumCategoryPart forumCategoryPart, int skip, int count, VersionOptions versionOptions);
    }

    public class ForumCategoryService : IForumCategoryService
    {
        private readonly IContentManager _contentManager;
        //private readonly IRepository<ForumCategoryToForumMappingRecord> _categoryToForumMappingRepository;
        private readonly IForumService _forumService;
        private readonly IForumsHomePageService _forumsHomePageService;

        public ForumCategoryService(
            IContentManager contentManager,
            IForumService forumService,
            IForumsHomePageService forumsHomePageService
           // IRepository<ForumCategoryToForumMappingRecord> categoryToForumMappingRepository
            
         )
        {
            //_categoryToForumMappingRepository = categoryToForumMappingRepository;
            _contentManager = contentManager;
            _forumService = forumService;
            _forumsHomePageService = forumsHomePageService;
        }

        public ForumCategoryPart GetParentCategory( ForumPart forumPart ){
            ForumCategoryPart parentCategory = null;
            if( forumPart.As<CommonPart>().Container != null ) {
                parentCategory = forumPart.As<CommonPart>().Container.As<ForumCategoryPart>();
            }
            return parentCategory;
                

            /*
            var parentCategoryId = _categoryToForumMappingRepository.Table.Where(rec => rec.ForumPartRecord.Id == forumPart.Id).Select(rec => rec.ForumCategoryPartRecord.Id).FirstOrDefault();
            var parentCategory = _contentManager.Query<ForumCategoryPart, ForumCategoryPartRecord>().Where( cat=>cat.Id == parentCategoryId ).List().FirstOrDefault();
            return parentCategory;
             */
        }

        public IEnumerable<ForumPart> GetForumsForCategory(ForumCategoryPart forumCategoryPart, VersionOptions versionOptions)
        {
            var forums = _contentManager.Query<ForumPart, ForumPartRecord>(versionOptions)
                            .OrderBy( fp=>fp.Weight)
                            .WithQueryHints(new QueryHints().ExpandRecords<CommonPartRecord, TitlePartRecord>())
                            .Join<CommonPartRecord>()
                            .Where(cpr => cpr.Container.Id == forumCategoryPart.Id).List().ToList();

            //var forumIds = _categoryToForumMappingRepository.Table.Where(rec => rec.ForumCategoryPartRecord.Id == forumCategoryPart.Id).Select(rec => rec.ForumPartRecord.Id).ToList();
            //var forums = _contentManager.Query<ForumPart, ForumPartRecord>(versionOptions).Where(forum => forumIds.Contains(forum.Id)).List().ToList();
            return forums;
        }

        public IEnumerable<ForumPart> GetForumsForCategory(ForumCategoryPart forumCategoryPart, int skip, int count, VersionOptions versionOptions)
        {
            
            var forums = _contentManager.Query<ForumPart, ForumPartRecord>(versionOptions)
                .OrderBy(fp => fp.Weight)
                .WithQueryHints(new QueryHints().ExpandRecords<CommonPartRecord, TitlePartRecord>())
                .Join<CommonPartRecord>()
                .Where(cpr => cpr.Container.Id == forumCategoryPart.Id).Slice(skip, count).ToList();

            /*
             * var forumIds = _categoryToForumMappingRepository.Table.Where(rec => rec.ForumCategoryPartRecord.Id == forumCategoryPart.Id).Select(rec => rec.ForumPartRecord.Id).ToList();
            var forums = _contentManager.Query<ForumPart, ForumPartRecord>(versionOptions).OrderByDescending(fpr => fpr.Weight).Where(forum => forumIds.Contains(forumCategoryPart.Id)).Slice(skip, count).ToList();
             */

            /*
            return GetParentQuery(forumCategoryPart, versionOptions)
                .Join<ThreadPartRecord>()
                .OrderByDescending(o => o.IsSticky)
                .Join<CommonPartRecord>()
                .OrderByDescending(o => o.ModifiedUtc)
                .ForPart<ThreadPart>()
                .Slice(skip, count)
                .ToList();
             */

            return forums;
        }

        /*
        private IContentQuery<CommonPart, CommonPartRecord> GetParentQuery(IContent parentPart, VersionOptions versionOptions)
        {
            return _contentManager.Query<CommonPart, CommonPartRecord>(versionOptions)
                                  .Where(cpr => cpr.Container == parentPart.ContentItem.Record);
        }
         * */


        public void UpdateForumCategoryForumList(ContentItem item, IEnumerable<ForumEntry> forums)
        {
            /*
            var forums = _contentManager.Query<ForumPart, ForumPartRecord>(VersionOptions.AllVersions)
           .OrderBy(fp => fp.Weight)
           .WithQueryHints(new QueryHints().ExpandRecords<CommonPartRecord, TitlePartRecord>())
           .Join<CommonPartRecord>()
           .Where(cpr => cpr.Container.Id == forumCategoryPart.Id).Slice(skip, count).ToList();
            */

            /*
        var record = item.As<ForumCategoryPart>().Record;

        if (forums != null) //if there are forums (i.e. its not a new category) 
        {

            
            var oldForumMappingList = _categoryToForumMappingRepository.Fetch(r => r.ForumCategoryPartRecord == record);
            var lookupNew = forums.Where(e => e.IsChecked).Select(e => e).ToDictionary(r => r.ForumPartRecord, r => false);

            //set the container on the forum parts so that the urls are proper
            //this is going to mess up the N to N since a forum can only be used in one category now
            var forumIds = lookupNew.Select(l => l.Key.Id).ToList();
            var forumParts = _forumService.Get(VersionOptions.Latest).Where(e => forumIds.Contains(e.Id));
            foreach (var part in forumParts)
                part.As<CommonPart>().Container = item;

            // Delete the records that are no longer there and mark the ones that should stay
            foreach (var mappingRecord in oldForumMappingList)
            {
                if (lookupNew.ContainsKey(mappingRecord.ForumPartRecord))
                {
                    lookupNew[mappingRecord.ForumPartRecord] = true;
                }
                else
                {
                    _categoryToForumMappingRepository.Delete(mappingRecord);
                }
            }
            List<ForumPart> selectedForums = new List<ForumPart>();
            // Add the new forums
            foreach (var forumRecord in lookupNew.Where(f => !f.Value).Select(f => f.Key))
            {

                _categoryToForumMappingRepository.Create(new ForumCategoryToForumMappingRecord
                {
                    ForumCategoryPartRecord = record,
                    ForumPartRecord = forumRecord
                });

            }
                 
        }
             * */
        }

        
        public IEnumerable<ForumCategoryPart> GetCategoriesWithForums( ForumsHomePagePart forumsHomePagePart, VersionOptions versionOptions)
        {
            //leave this untouched to maintain order            
           var categories = GetCategoriesInForumsHomePage(forumsHomePagePart.Id, versionOptions).OrderBy(o => o.Weight);
           var forums =  _forumService.GetForumsForCategories( categories.Select( c=>c.Id).ToList(), versionOptions );
           var forumToCategoryMapping = forums.ToLookup( forum=> forum.As<CommonPart>().Container.Id, forumPart =>forumPart.Id);
           foreach ( var category in categories ) {
                //var relatedForums = mappings.Where( m=> m.ForumCategoryPartRecord.Id == category.Id ).Select( m=>m.ForumPartRecord.Id).ToList();
                category.Forums.AddRange( forums.Where( f=> forumToCategoryMapping[category.Id].Contains( f.Id ) ).ToList() );
            }
            /*
            var forums = _forumService.Get(versionOptions).ToList();

            var mappings = _categoryToForumMappingRepository.Table.ToLookup( map=> map.ForumCategoryPartRecord.Id, m=>m.ForumPartRecord.Id);
         
            //this is not at all efficient but the number of entries should be relatively small
            foreach ( var category in categories ) {
                //var relatedForums = mappings.Where( m=> m.ForumCategoryPartRecord.Id == category.Id ).Select( m=>m.ForumPartRecord.Id).ToList();
                category.Forums.AddRange( forums.Where( f=> mappings[category.Id].Contains( f.Id ) ).ToList() );
            }
            */
             return categories;
        }

        public IEnumerable<ForumCategoryPart> Get(VersionOptions versionOptions)
        {
            return _contentManager.Query<ForumCategoryPart, ForumCategoryPartRecord>(versionOptions)                       
                       .WithQueryHints(new QueryHints().ExpandRecords<CommonPartRecord>())
                       .OrderBy(o => o.Weight)
                       .List().ToList();
        }
        public IEnumerable<ForumCategoryPart> GetCategoriesInForumsHomePage(int forumsHomePageId)
        {
            return GetCategoriesInForumsHomePage(forumsHomePageId, VersionOptions.Published);
        }

        public IEnumerable<ForumCategoryPart> GetCategoriesInForumsHomePage(int forumsHomePagePartId, VersionOptions versionOptions)
        {
           return  _contentManager.Query<ForumCategoryPart, ForumCategoryPartRecord>(versionOptions)
                               .OrderBy(o => o.Weight)
                               .Join<CommonPartRecord>()
                               .Where(cpr => cpr.Container.Id == forumsHomePagePartId)
                               .WithQueryHints(new QueryHints().ExpandRecords<CommonPartRecord>())
                               .List().ToList();
        }

        public ForumCategoryPart Get(int Id, VersionOptions versionOptions)
        {
            return _contentManager.Query<ForumCategoryPart, ForumCategoryPartRecord>(versionOptions)
                .WithQueryHints(new QueryHints().ExpandRecords<CommonPartRecord>())
                .Where(x => x.Id == Id)
                .List()
                .SingleOrDefault();
        }

        public void Delete(ForumCategoryPart forumCategoryPart)
        {
            _contentManager.Remove(forumCategoryPart.ContentItem);
        }

    }
}