using System.Linq;
using System.Web.Mvc;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using NGM.Forum.Routing;
using NGM.Forum.Services;
using Orchard;
using Orchard.ContentManagement;
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

        public ForumController(IOrchardServices orchardServices, 
            IForumService forumService,
            IForumPathConstraint forumPathConstraint,
            IThreadService threadService,
            ISiteService siteService,
            IShapeFactory shapeFactory) {
            _orchardServices = orchardServices;
            _forumService = forumService;
            _forumPathConstraint = forumPathConstraint;
            _threadService = threadService;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ActionResult List() {
            var forums = _forumService.Get().Select(b => _orchardServices.ContentManager.BuildDisplay(b, "Summary"));

            var list = Shape.List();
            list.AddRange(forums);

            dynamic viewModel = Shape.ViewModel()
                .ContentItems(list);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)viewModel);
        }

        public ActionResult Close(int forumId) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, T("Couldn't close forum")))
                return new HttpUnauthorizedResult();

            var forum = _forumService.Get(forumId, VersionOptions.Latest).As<ForumPart>();
            if (forum == null)
                return HttpNotFound();

            _forumService.CloseForum(forum);

            return Redirect(Url.ViewForums());
        }

        public ActionResult Open(int forumId) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, T("Couldn't open forum")))
                return new HttpUnauthorizedResult();

            var forum = _forumService.Get(forumId, VersionOptions.Latest).As<ForumPart>();
            if (forum == null)
                return HttpNotFound();

            _forumService.OpenForum(forum);

            return Redirect(Url.ViewForums());
        }

        public ActionResult Item(string forumPath, PagerParameters pagerParameters) {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var correctedPath = _forumPathConstraint.FindPath(forumPath);
            if (correctedPath == null)
                return HttpNotFound();

            var forumPart = _forumService.Get(correctedPath);
            if (forumPart == null)
                return HttpNotFound();

            var threads = _threadService.Get(forumPart, pager.GetStartIndex(), pager.PageSize)
                .Select(b => _orchardServices.ContentManager.BuildDisplay(b, "Summary"));
            dynamic forum = _orchardServices.ContentManager.BuildDisplay(forumPart);

            var list = Shape.List();
            list.AddRange(threads);
            forum.Content.Add(Shape.Parts_Forums_Thread_List(ContentItems: list), "5");

            return new ShapeResult(this, forum);
        }
    }
}