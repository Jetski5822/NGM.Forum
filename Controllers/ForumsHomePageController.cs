using System.Linq;
using System.Web.Mvc;
using NGM.Forum.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Navigation;
using NGM.Forum.Models;
using Orchard.Core.Common.Models;
using System.Collections.Generic;
using Orchard.Search.Services;
using Orchard.Indexing;

namespace NGM.Forum.Controllers {
    [Themed]
    public class ForumsHomePageController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IForumService _forumService;
        private readonly IThreadService _threadService;
        private readonly ISiteService _siteService;
        private readonly IThreadLastReadService _threadLastReadService;
        private readonly IForumCategoryService _forumCategoryService;

        //private readonly ISearchBuilder _searchBuilder;
       // private readonly ISearchService _searchService;
       // private readonly IIndexManager _indexManager;
       // private readonly IIndexProvider _indexProvider;

        public ForumsHomePageController(
            IOrchardServices orchardServices, 
            IForumCategoryService forumCategoryService,
            IForumService forumService,
            IThreadService threadService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IThreadLastReadService threadLastReadService
         //   IIndexManager indexManager
         ) {
            _orchardServices = orchardServices;
            _forumService = forumService;
            _threadService = threadService;
            _siteService = siteService;
            _threadLastReadService = threadLastReadService;
            _forumCategoryService = forumCategoryService;
            //string searchIndex = ForumSearchService.FORUMS_INDEX_NAME;
           // _searchBuilder = _indexManager.HasIndexProvider() ? _indexManager.GetSearchIndexProvider().CreateSearchBuilder(searchIndex) : new NullSearchBuilder();

            Shape = shapeFactory;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        dynamic Shape { get; set; }
        protected ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult ListNewPosts(int forumsHomeId, string returnUrl, PagerParameters pagerParameters)
        {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var forumsHomePagePart = _orchardServices.ContentManager.Get(forumsHomeId);
            if (forumsHomePagePart == null)
                return HttpNotFound();

            if (_orchardServices.WorkContext.CurrentUser == null)
                return new HttpUnauthorizedResult();

            var userId = _orchardServices.WorkContext.CurrentUser.Id;                        

            //get the threads that the user has read already... then compare. All other threads are unread.
            var posts = _threadLastReadService.GetNewPosts(forumsHomeId, userId, 30, pager.GetStartIndex(), pager.PageSize);

            var menu = Shape.Parts_ForumMenu(ShowMarkAll: true, ShowRecent: true, ForumsHomePagePart: forumsHomePagePart, ReturnUrl:returnUrl);
            var search = Shape.Parts_Forum_Search(ForumsHomeId: forumsHomeId);

            var list = Shape.List();
            list.AddRange(posts.Select(post => _orchardServices.ContentManager.BuildDisplay(post, "NewPostPreview")));

            var breadCrumb = Shape.Parts_BreadCrumb(ForumsHomePagePart: forumsHomePagePart.As<ForumsHomePagePart>());

            dynamic viewModel = _orchardServices.New.ViewModel()
                                    .ForumMenu(menu)
                                    .ForumSearch(search)
                                    .BreadCrumb(breadCrumb)
                                    .Posts(list)
                                    .Pager( Shape.Pager(pager).TotalItemCount(posts.Count))
                                    .ReturnUrl(returnUrl)
                                    ;

            return View((object)viewModel);
        }

        public ActionResult ListNewPostsByThread(int forumsHomeId, string returnUrl, PagerParameters pagerParameters)
        {

            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var forumsHomePagePart = _orchardServices.ContentManager.Get<ForumsHomePagePart>(forumsHomeId);
            if (forumsHomePagePart == null)
                return HttpNotFound();

            if (_orchardServices.WorkContext.CurrentUser == null)
                return new HttpUnauthorizedResult();

            var userId = _orchardServices.WorkContext.CurrentUser.Id;
            var postsByThread = _threadLastReadService.GetNewPostsByThread(forumsHomeId, userId, 30, pager.GetStartIndex(), pager.PageSize );

            var threadDic = postsByThread.Item1;
            var postsDic = postsByThread.Item2;

            var menu = Shape.Parts_ForumMenu(ShowMarkAll: true, ShowRecent: true, ForumsHomePagePart: forumsHomePagePart, ReturnUrl: returnUrl);
            var search = Shape.Parts_Forum_Search(ForumsHomeId: forumsHomeId);

            Dictionary<int, dynamic> postDisplays = new Dictionary<int, dynamic>();

            foreach (var key in postsDic.Keys)
            {
                var posts = postsDic[key].Select(post => _orchardServices.ContentManager.BuildDisplay(post));

                var list = Shape.List();
                list.AddRange(posts);
                postDisplays.Add(key, list);
            }

            var breadCrumb = Shape.Parts_BreadCrumb(ForumsHomePagePart: forumsHomePagePart.As<ForumsHomePagePart>());

            dynamic viewModel = _orchardServices.New.ViewModel()
                                    .PostByThreadDic(postDisplays)
                                    .ThreadDic(threadDic)
                                    .Pager(Shape.Pager(pager).TotalItemCount(threadDic.Count))
                                    .ForumMenu(menu)
                                    .ForumSearch(search)
                                    .BreadCrumb(breadCrumb)
                                    .ReturnUrl(returnUrl);

            return View((object)viewModel);
        }

        public ActionResult MarkAllRead(int forumsHomeId)
        {
            var forumsHomePagePart = _orchardServices.ContentManager.Get<ForumsHomePagePart>( forumsHomeId, VersionOptions.Published);
            if ( forumsHomePagePart == null ) 
                return HttpNotFound();

            if ( _orchardServices.WorkContext.CurrentUser == null ) 
                return new HttpUnauthorizedResult();

            var userId = _orchardServices.WorkContext.CurrentUser.Id;

            _threadLastReadService.MarkAllRead(forumsHomeId, userId);
            _orchardServices.Notifier.Add(Orchard.UI.Notify.NotifyType.Information, (T("All threads in the current forum were marked as read")));

            return Redirect(HttpContext.Request.UrlReferrer.AbsoluteUri);

        }
    
    }
}