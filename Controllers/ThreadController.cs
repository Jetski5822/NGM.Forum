using System.Linq;
using System.Web.Mvc;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using NGM.Forum.Routing;
using NGM.Forum.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace NGM.Forum.Controllers {
    [Themed]
    [ValidateInput(false)]
    public class ThreadController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly IForumService _forumService;
        private readonly IForumPathConstraint _forumPathConstraint;
        private readonly IThreadService _threadService;
        private readonly IPostService _postService;
        private readonly ISiteService _siteService;

        public ThreadController(IOrchardServices orchardServices, 
            IForumService forumService,
            IForumPathConstraint forumPathConstraint,
            IThreadService threadService,
            IPostService postService,
            ISiteService siteService,
            IShapeFactory shapeFactory) {
            _orchardServices = orchardServices;
            _forumService = forumService;
            _forumPathConstraint = forumPathConstraint;
            _threadService = threadService;
            _postService = postService;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ActionResult Create(int forumId) {
            if (IsNotAllowedToCreateThread())
                return new HttpUnauthorizedResult();

            var forum = _forumService.Get(forumId, VersionOptions.Latest).As<ForumPart>();
            if (forum == null)
                return HttpNotFound();

            var thread = _orchardServices.ContentManager.New<ThreadPart>("Thread");
            thread.ForumPart = forum;

            dynamic model = _orchardServices.ContentManager.BuildEditor(thread);

            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(int forumId) {
            if (IsNotAllowedToCreateThread())
                return new HttpUnauthorizedResult();

            var forum = _forumService.Get(forumId, VersionOptions.Latest).As<ForumPart>();
            if (forum == null)
                return HttpNotFound();

            var thread = _orchardServices.ContentManager.New<ThreadPart>("Thread");
            thread.ForumPart = forum;
            
            _orchardServices.ContentManager.Create(thread, VersionOptions.Draft);
            var model = _orchardServices.ContentManager.UpdateEditor(thread, this);

            if (!ModelState.IsValid) {
                _orchardServices.TransactionManager.Cancel();
                return View((object)model);
            }

            _orchardServices.ContentManager.Publish(thread.ContentItem);
            _forumPathConstraint.AddPath(thread.As<IRoutableAspect>().Path);

            _orchardServices.Notifier.Information(T("Your {0} has been created.", thread.TypeDefinition.DisplayName));
            return Redirect(Url.ViewThread(thread));
        }

        public ActionResult Close(int threadId) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.CloseThread, T("Couldn't close thread")))
                return new HttpUnauthorizedResult();

            var thread = _threadService.Get(threadId, VersionOptions.Latest).As<ThreadPart>();
            if (thread == null)
                return HttpNotFound();

            _threadService.CloseThread(thread);

            return Redirect(Url.ViewThread(thread));
        }

        public ActionResult Open(int threadId) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.OpenThread, T("Couldn't open thread")))
                return new HttpUnauthorizedResult();

            var thread = _threadService.Get(threadId, VersionOptions.Latest).As<ThreadPart>();
            if (thread == null)
                return HttpNotFound();

            _threadService.OpenThread(thread);

            return Redirect(Url.ViewThread(thread));
        }

        public ActionResult Item(string forumPath, string threadSlug, PagerParameters pagerParameters) {
            if (!_orchardServices.Authorizer.Authorize(StandardPermissions.AccessFrontEnd, T("Couldn't view thread")))
                return new HttpUnauthorizedResult();

            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var correctedPath = _forumPathConstraint.FindPath(forumPath);
            if (correctedPath == null)
                return HttpNotFound();

            var forumPart = _forumService.Get(correctedPath);
            if (forumPart == null)
                return HttpNotFound();

            var threadPart = _threadService.Get(forumPart, threadSlug, VersionOptions.Published);
            if (threadPart == null)
                return HttpNotFound();

            var posts = _postService.Get(threadPart, pager.GetStartIndex(), pager.PageSize)
                .Select(b => _orchardServices.ContentManager.BuildDisplay(b, "Detail"));
            dynamic thread = _orchardServices.ContentManager.BuildDisplay(threadPart);

            var list = Shape.List();
            list.AddRange(posts);
            thread.Content.Add(Shape.Parts_Threads_Post_List(ContentItems: list), "5");

            return new ShapeResult(this, thread);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        private bool IsNotAllowedToCreateThread() {
            return !_orchardServices.Authorizer.Authorize(Permissions.AddThread, T("Not allowed to create thread"));
        }
    }
}