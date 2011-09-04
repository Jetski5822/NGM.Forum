using System.Linq;
using System.Web.Mvc;
using NGM.Forum.Models;
using NGM.Forum.Routing;
using NGM.Forum.Services;
using NGM.Forum.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Routable.Services;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Mvc.Extensions;

namespace NGM.Forum.Controllers {

    [ValidateInput(false), Admin]
    public class ThreadAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly IForumService _forumService;
        private readonly IThreadService _threadService;
        private readonly IForumPathConstraint _forumPathConstraint;
        private readonly ISiteService _siteService;
        private readonly IPostService _postService;
        private readonly IRoutableService _routableService;

        public ThreadAdminController(IOrchardServices orchardServices,
            IForumService forumService,
            IThreadService threadService,
            IForumPathConstraint forumPathConstraint,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IPostService postService,
            IRoutableService routableService) {
            _orchardServices = orchardServices;
            _forumService = forumService;
            _threadService = threadService;
            _forumPathConstraint = forumPathConstraint;
            _siteService = siteService;
            _postService = postService;
            _routableService = routableService;

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

            return View((object)thread);
        }

        public ActionResult Move(int threadId) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.MoveThread, T("Not allowed to move thread")))
                return new HttpUnauthorizedResult();

            var threadPart = _threadService.Get(threadId, VersionOptions.Latest).As<ThreadPart>();

            if (threadPart == null)
                return HttpNotFound(T("could not find thread").ToString());

            var forums = _forumService.Get();
            //What if I have 1 forum?

            var viewModel = new ThreadMoveAdminViewModel {
                ThreadId = threadPart.Id,
                AvailableForums = forums
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Move(int threadId, string returnUrl, ThreadMoveAdminViewModel viewModel) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.MoveThread, T("Not allowed to move thread")))
                return new HttpUnauthorizedResult();

            var threadPart = _threadService.Get(threadId, VersionOptions.Latest).As<ThreadPart>();

            if (threadPart == null)
                return HttpNotFound(T("could not find thread").ToString());

            var forumPart = _forumService.Get(viewModel.ForumId, VersionOptions.Latest).As<ForumPart>();

            if (forumPart == null)
                return HttpNotFound(T("could not find forum").ToString());

            threadPart.ForumPart = forumPart;
            var routableAspect = threadPart.As<IRoutableAspect>();
            _routableService.ProcessSlug(routableAspect);

            _orchardServices.ContentManager.Publish(threadPart.ContentItem);

            _orchardServices.Notifier.Information(T("{0} has been moved.", threadPart.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, "~/");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}