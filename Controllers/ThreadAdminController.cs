using System.Linq;
using System.Web.Mvc;
using NGM.Forum.Models;
using NGM.Forum.Services;
using NGM.Forum.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Services;
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
        private readonly ISiteService _siteService;
        private readonly IPostService _postService;
        private readonly IClock _clock;

        public ThreadAdminController(IOrchardServices orchardServices,
            IForumService forumService,
            IThreadService threadService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IPostService postService,
            IClock clock) {
            _orchardServices = orchardServices;
            _forumService = forumService;
            _threadService = threadService;
            _siteService = siteService;
            _postService = postService;
            _clock = clock;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ActionResult List(int forumId) {
            var forumPart = _forumService.Get(forumId, VersionOptions.Latest);

            if (forumPart == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Orchard.Core.Contents.Permissions.ViewContent, forumPart, T("Not allowed to view forum")))
                return new HttpUnauthorizedResult();

            var list = _orchardServices.New.List();

            list.AddRange(_threadService.Get(forumPart)
                              .Select(b => _orchardServices.ContentManager.BuildDisplay(b, "SummaryAdmin")));

            dynamic viewModel = _orchardServices.New.ViewModel()
                .ContentItems(list);
            
            return View((object)viewModel);
        }

        public ActionResult Item(int threadId, PagerParameters pagerParameters) {
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var threadPart = _threadService.Get(threadId, VersionOptions.Latest);

            if (threadPart == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Orchard.Core.Contents.Permissions.ViewContent, threadPart, T("Not allowed to view thread")))
                return new HttpUnauthorizedResult();

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

            var threadPart = _threadService.Get(threadId, VersionOptions.Latest);

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

            var threadPart = _threadService.Get(threadId, VersionOptions.Latest);

            if (threadPart == null)
                return HttpNotFound(T("Could not find thread").ToString());

            var forumPart = _forumService.Get(viewModel.ForumId, VersionOptions.Latest);

            if (forumPart == null)
                return HttpNotFound(T("Could not find forum").ToString());

            var currentForumName = threadPart.ForumPart.As<ITitleAspect>().Title;
            var newForumName = forumPart.As<ITitleAspect>().Title;

            threadPart.ForumPart = forumPart;
            
            _orchardServices.ContentManager.Publish(threadPart.ContentItem);

            _orchardServices.Notifier.Information(T("{0} has been moved from {1} to {2}.", threadPart.TypeDefinition.DisplayName, currentForumName, newForumName));

            return this.RedirectLocal(returnUrl, "~/");
        }

        public ActionResult Close(int threadId) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.CloseThread, T("Not allowed to close thread")))
                return new HttpUnauthorizedResult();

            var threadPart = _threadService.Get(threadId, VersionOptions.Latest);

            if (threadPart == null)
                return HttpNotFound(T("could not find thread").ToString());

            var viewModel = new ThreadCloseAdminViewModel {
                ThreadId = threadPart.Id,
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Close(int threadId, string returnUrl, ThreadCloseAdminViewModel viewModel) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.MoveThread, T("Not allowed to close thread")))
                return new HttpUnauthorizedResult();

            var threadPart = _threadService.Get(threadId, VersionOptions.Latest);

            if (threadPart == null)
                return HttpNotFound(T("Could not find thread").ToString());

            threadPart.ClosedBy = _orchardServices.WorkContext.CurrentUser;
            threadPart.ClosedOnUtc = _clock.UtcNow;
            threadPart.ClosedDescription = viewModel.Description;

            _orchardServices.ContentManager.Publish(threadPart.ContentItem);

            _orchardServices.Notifier.Information(T("{0} has been closed.", threadPart.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, "~/");
        }

        public ActionResult Open(int threadId, string returnUrl) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.CloseThread, T("Not allowed to open thread")))
                return new HttpUnauthorizedResult();

            var threadPart = _threadService.Get(threadId, VersionOptions.Latest);

            if (threadPart == null)
                return HttpNotFound(T("could not find thread").ToString());

            threadPart.ClosedBy = null;
            threadPart.ClosedDescription = null;
            threadPart.ClosedOnUtc = null;

            _orchardServices.Notifier.Information(T("{0} has been opened.", threadPart.TypeDefinition.DisplayName));

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