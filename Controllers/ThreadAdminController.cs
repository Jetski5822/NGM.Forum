using System.Linq;
using System.Web.Mvc;
using NGM.Forum.Models;
using NGM.Forum.Extensions;
using NGM.Forum.Routing;
using NGM.Forum.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;

namespace NGM.Forum.Controllers {

    [ValidateInput(false), Admin]
    public class ThreadAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly IForumService _forumService;
        private readonly IThreadService _threadService;
        private readonly IForumPathConstraint _forumPathConstraint;
        private readonly ISiteService _siteService;
        private readonly IPostService _postService;

        public ThreadAdminController(IOrchardServices orchardServices,
            IForumService forumService,
            IThreadService threadService,
            IForumPathConstraint forumPathConstraint,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IPostService postService) {
            _orchardServices = orchardServices;
            _forumService = forumService;
            _threadService = threadService;
            _forumPathConstraint = forumPathConstraint;
            _siteService = siteService;
            _postService = postService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ActionResult List(int forumId) {
            var list = _orchardServices.New.List();

            var forum = _forumService.Get(forumId, VersionOptions.Latest).As<ForumPart>();

            list.AddRange(_threadService.Get(forum)
                              .Select(b => {
                                  return _orchardServices.ContentManager.BuildDisplay(b, "SummaryAdmin");
                              }));

            dynamic viewModel = _orchardServices.New.ViewModel()
                .ContentItems(list);
            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)viewModel);
        }

        public ActionResult Item(int threadId, PagerParameters pagerParameters) {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            ThreadPart threadPart = _threadService.Get(threadId, VersionOptions.Latest).As<ThreadPart>();

            if (threadPart == null)
                return HttpNotFound();

            var posts = _postService.Get(threadPart, pager.GetStartIndex(), pager.PageSize, VersionOptions.Latest)
                .Select(bp => _orchardServices.ContentManager.BuildDisplay(bp, "SummaryAdmin"));

            dynamic thread = _orchardServices.ContentManager.BuildDisplay(threadPart, "DetailAdmin");

            var list = Shape.List();
            list.AddRange(posts);
            thread.Content.Add(Shape.Parts_Threads_Post_ListAdmin(ContentItems: list), "5");

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)thread);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}