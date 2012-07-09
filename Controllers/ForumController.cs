using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using NGM.Forum.Routing;
using NGM.Forum.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Feeds;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Navigation;

namespace NGM.Forum.Controllers {
    [Themed]
    public class ForumController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IForumService _forumService;
        private readonly IForumPathConstraint _forumPathConstraint;
        private readonly IThreadService _threadService;
        private readonly ISiteService _siteService;
        private readonly IFeedManager _feedManager;

        public ForumController(IOrchardServices orchardServices, 
            IForumService forumService,
            IForumPathConstraint forumPathConstraint,
            IThreadService threadService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IFeedManager feedManager) {
            _orchardServices = orchardServices;
            _forumService = forumService;
            _forumPathConstraint = forumPathConstraint;
            _threadService = threadService;
            _siteService = siteService;
            _feedManager = feedManager;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ActionResult List() {
            var forums = _forumService.Get().OrderBy(o => o.Position).Select(fbc => _orchardServices.ContentManager.BuildDisplay(fbc, "Summary"));

            var list = Shape.List();
            list.AddRange(forums);

            dynamic viewModel = Shape.ViewModel()
                .ContentItems(list);

            return View((object)viewModel);
        }

        public ActionResult Close(int forumId) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, T("Not allowed to close forum")))
                return new HttpUnauthorizedResult();

            var forum = _forumService.Get(forumId, VersionOptions.Latest).As<ForumPart>();
            if (forum == null)
                return HttpNotFound();

            _forumService.CloseForum(forum);

            return Redirect(Url.ViewForums());
        }

        public ActionResult Open(int forumId) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, T("Not allowed to open forum")))
                return new HttpUnauthorizedResult();

            var forum = _forumService.Get(forumId, VersionOptions.Latest).As<ForumPart>();
            if (forum == null)
                return HttpNotFound();

            _forumService.OpenForum(forum);

            return Redirect(Url.ViewForums());
        }

        public ActionResult Item(string forumPath, PagerParameters pagerParameters) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ViewForum, T("Not allowed to view forum")))
                return new HttpUnauthorizedResult();

            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var correctedPath = _forumPathConstraint.FindPath(forumPath);
            if (correctedPath == null)
                return HttpNotFound();

            var forumPart = _forumService.Get(string.Format("Forum/{0}", correctedPath));
            if (forumPart == null)
                return HttpNotFound();

            dynamic forum = _orchardServices.ContentManager.BuildDisplay(forumPart);

            //_feedManager.Register(forumPart);
            var threadParts = _threadService.Get(forumPart, pager.GetStartIndex(), pager.PageSize);
            var stickyThreads = threadParts.Where(o => o.IsSticky).Select(b => _orchardServices.ContentManager.BuildDisplay(b, "Summary"));

            var list = Shape.List();

            if (forumPart.UsePopularityAlgorithm)
                list.AddRange(stickyThreads.Union(threadParts.Where(o => !o.IsSticky).OrderByDescending(p => _threadService.CalculatePopularity(p)).Select(b => _orchardServices.ContentManager.BuildDisplay(b, "Summary"))));
            else {
                list.AddRange(stickyThreads.Union(threadParts.Where(o => !o.IsSticky).Select(b => _orchardServices.ContentManager.BuildDisplay(b, "Summary"))));
            }

            var pagerObject = Shape.Pager(pager).TotalItemCount(forumPart.ThreadCount);
            forum.Content.Add(Shape.Parts_Forums_Thread_List(ContentItems: list, Pager: pagerObject), "5");

            return new ShapeResult(this, forum);
        }
    }
}