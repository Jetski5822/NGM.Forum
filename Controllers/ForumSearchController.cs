using System;
using System.Linq;
using System.Web.Mvc;
using Orchard.Collections;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Indexing;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Search.Models;
using Orchard.Search.Services;
using Orchard.Search.ViewModels;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using NGM.Forum.ViewModels;
using Orchard;
using System.Globalization;
using NGM.Forum.Services;
using NGM.Forum.Models;
using Orchard.Core.Title.Models;
using NGM.Forum.Handlers;
using Orchard.Utility.Extensions;

namespace NGM.Forum.Controllers
{

    [ValidateInput(false), Themed]
    public class ForumSearchController : Controller
    {
        private readonly IForumSearchService _forumSearchService;
        private readonly IContentManager _contentManager;
        private readonly ISiteService _siteService;

        public ForumSearchController(
            IOrchardServices services,
            IForumSearchService forumSearchService,
            IContentManager contentManager,
            ISiteService siteService,
            IShapeFactory shapeFactory
        )
        {
            _orchardServices = services;
            _forumSearchService = forumSearchService;
            _contentManager = contentManager;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Logger = NullLogger.Instance;
            Shape = shapeFactory;
        }

        private IOrchardServices _orchardServices { get; set; }
        public Localizer T { get; set; }
        public ILogger Logger { get; set; }
        dynamic Shape { get; set; }


        public ActionResult Index(PagerParameters pagerParameters, int? forumsHomeId, string q = "")
        {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var searchSettingPart = _orchardServices.WorkContext.CurrentSite.As<SearchSettingsPart>();
            ForumsHomePagePart forumsHomePagePart = null;

            if ( forumsHomeId != null ) {
                forumsHomePagePart = _contentManager.Get<ForumsHomePagePart>( forumsHomeId.Value );
            }

            string searchIndex = ForumSearchService.FORUMS_INDEX_NAME;

            if (forumsHomePagePart == null) {
                _orchardServices.Notifier.Error(T("Error: The search index was not found.  Searching failed.  Please review the logs for additional information"));
                Logger.Log(LogLevel.Error, new Exception(String.Format("The forums default search index '{0}' was not found.  Please manually create an index by this name and associate it with the Post contentitem",searchIndex)),null, null);
                return HttpNotFound();
            }

            IPageOfItems<ISearchHit> searchHits = new PageOfItems<ISearchHit>(new ISearchHit[] { });
            try
            {

                searchHits = _forumSearchService.Query(q, forumsHomeId, pager.Page, pager.PageSize,
                                                  _orchardServices.WorkContext.CurrentSite.As<SearchSettingsPart>().FilterCulture,
                                                  searchIndex,
                                                  searchSettingPart.SearchedFields,
                                                  searchHit => searchHit);
            }
            catch (Exception exception)
            {
                Logger.Error(T("Invalid search query: {0}", exception.Message).Text);
                _orchardServices.Notifier.Error(T("Invalid search query: {0}", exception.Message));
            }

            var list = Shape.List();
            var foundIds = searchHits.Select(searchHit => searchHit.ContentItemId).ToList();

            // ignore search results which content item has been removed or unpublished
            var foundItems = _contentManager.GetMany<IContent>(foundIds, VersionOptions.Published, new QueryHints()).ToList();
            foreach (var contentItem in foundItems)
            {
                if (contentItem.Is<PostPart>())
                {
                    if (contentItem.As<PostPart>().IsInappropriate == false)
                    {
                        list.Add(_contentManager.BuildDisplay(contentItem, "SearchResult"));
                    }
                }
            }

            searchHits.TotalItemCount -= foundIds.Count() - foundItems.Count();

            var pagerShape = Shape.Pager(pager).TotalItemCount(searchHits.TotalItemCount);

            dynamic menuShape = null;
            if (_orchardServices.WorkContext.CurrentUser != null)
            {
                menuShape = Shape.Parts_ForumMenu(ForumsHomePagePart: forumsHomePagePart, ShowRecent: true, ShowMarkAll: true, ReturnUrl: HttpContext.Request.Url.AbsoluteUri);    
            }

            
            //var search = _orchardServices.ContentManager.New("ForumSearch");
            var breadCrumb = Shape.Parts_BreadCrumb(ForumsHomePagePart:forumsHomePagePart);
            var searchShape = Shape.Parts_Forum_Search(ForumsHomeId: forumsHomePagePart.Id); ;

            var forumsSearchViewModel = new ForumsSearchViewModel
            {
                Query = q,
                ForumsHomeId = forumsHomeId,
                TotalItemCount = searchHits.TotalItemCount,
                StartPosition = (pager.Page - 1) * pager.PageSize + 1,
                EndPosition = pager.Page * pager.PageSize > searchHits.TotalItemCount ? searchHits.TotalItemCount : pager.Page * pager.PageSize,
                ContentItems = list,
                Pager = pagerShape,
                ForumSearch = searchShape,
                ForumMenu = menuShape,
                BreadCrumb = breadCrumb
            };



            return View((object)forumsSearchViewModel);

        }
    }
}