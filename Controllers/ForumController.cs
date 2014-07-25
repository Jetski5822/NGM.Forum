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

namespace NGM.Forum.Controllers {
    [Themed]
    public class ForumController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IForumService _forumService;
        private readonly IThreadService _threadService;
        private readonly ISiteService _siteService;
        private readonly IThreadLastReadService _threadLastReadService;

        public ForumController(IOrchardServices orchardServices, 
            IForumService forumService,
            IThreadService threadService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IThreadLastReadService threadLastReadService
            
         ) {
            _orchardServices = orchardServices;
            _forumService = forumService;
            _threadService = threadService;
            _siteService = siteService;
            _threadLastReadService = threadLastReadService;

            Shape = shapeFactory;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        dynamic Shape { get; set; }
        protected ILogger Logger { get; set; }
        public Localizer T { get; set; }

        /*
        public ActionResult List() {
            var forums = _forumService.Get().Select(fbc => _orchardServices.ContentManager.BuildDisplay(fbc, "Summary"));

            var list = Shape.List();
            list.AddRange(forums);

            dynamic viewModel = Shape.ViewModel()
                .ContentItems(list);

            return View((object)viewModel);
        }
         * */


        /// <summary>
        /// Gets a list of threads found in the specified forum
        /// </summary>
        /// <param name="forumId"></param>
        /// <param name="pagerParameters"></param>
        /// <returns></returns>
        public ActionResult Item( int forumId, PagerParameters pagerParameters)
        {
            var forumPart = _forumService.Get(forumId, VersionOptions.Published);
            if (forumPart == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Orchard.Core.Contents.Permissions.ViewContent, forumPart, T("Not allowed to view forum")))
                return new HttpUnauthorizedResult();

            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            
            var threadList = _threadService.Get(forumPart, pager.GetStartIndex(), pager.PageSize, VersionOptions.Published);

            int? userId = null;
            if (_orchardServices.WorkContext.CurrentUser != null)
            {
                userId = _orchardServices.WorkContext.CurrentUser.Id;
            }
            
            //get the read state of each thread part to be displayed
            if (userId != null)
            {

                _threadLastReadService.GetThreadReadState(userId.Value, forumPart.ForumCategoryPart.ForumsHomePagePart.Id, threadList);
            }

            var threads = threadList.Select(b => _orchardServices.ContentManager.BuildDisplay(b, "Summary"));

            dynamic forum = _orchardServices.ContentManager.BuildDisplay(forumPart);

            var list = Shape.List();
            list.AddRange(threads);
            forum.Content.Add(Shape.Parts_Forums_Thread_List(ContentPart: forumPart, ContentItems: list), "5");

            var totalItemCount = forumPart.ThreadCount;
            forum.Content.Add(Shape.Pager(pager).TotalItemCount(totalItemCount), "Content:after");

            return new ShapeResult(this, forum);
        }
    }
}