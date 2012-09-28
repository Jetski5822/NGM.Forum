using System.Linq;
using System.Web.Mvc;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using NGM.Forum.Routing;
using NGM.Forum.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Feeds;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
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
        private readonly IFeedManager _feedManager;

        public ThreadController(IOrchardServices orchardServices, 
            IForumService forumService,
            IForumPathConstraint forumPathConstraint,
            IThreadService threadService,
            IPostService postService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IFeedManager feedManager) {
            _orchardServices = orchardServices;
            _forumService = forumService;
            _forumPathConstraint = forumPathConstraint;
            _threadService = threadService;
            _postService = postService;
            _siteService = siteService;
            _feedManager = feedManager;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ActionResult Create(int forumId) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.CreatePost, T("Not allowed to create thread")))
                return new HttpUnauthorizedResult();

            var forum = _forumService.Get(forumId, VersionOptions.Latest).As<ForumPart>();
            if (forum == null)
                return HttpNotFound();

            var thread = _orchardServices.ContentManager.New<ThreadPart>(Constants.Parts.Thread);
            thread.ForumPart = forum;
            var post = _orchardServices.ContentManager.New<PostPart>(Constants.Parts.Post);
            post.ThreadPart = thread;

            dynamic postModel = _orchardServices.ContentManager.BuildEditor(post);

            var viewModel = Shape.ViewModel()
                .Post(postModel);

            return View((object)viewModel);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(int forumId) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.CreatePost, T("Not allowed to create thread")))
                return new HttpUnauthorizedResult();

            var forum = _forumService.Get(forumId, VersionOptions.Latest).As<ForumPart>();
            if (forum == null)
                return HttpNotFound();

            var thread = _orchardServices.ContentManager.Create<ThreadPart>(Constants.Parts.Thread, VersionOptions.Draft, (o) => { o.ForumPart = forum; });

            var post = _orchardServices.ContentManager.Create<PostPart>(Constants.Parts.Post, VersionOptions.Draft, (o) => { o.ThreadPart = thread; });
            var postModel = _orchardServices.ContentManager.UpdateEditor(post, this);
            post.ThreadPart = thread;
            
            if (!ModelState.IsValid) {
                _orchardServices.TransactionManager.Cancel();

                var viewModel = Shape.ViewModel()
                .Post(postModel);

                return View((object)viewModel);
            }

            _orchardServices.ContentManager.Publish(thread.ContentItem);
            _orchardServices.ContentManager.Publish(post.ContentItem);

            _orchardServices.Notifier.Information(T("Your {0} has been created.", thread.TypeDefinition.DisplayName));
            return Redirect(Url.ViewThread(thread));
        }

        public ActionResult Item(string forumPath, string threadSlug, PagerParameters pagerParameters) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ViewPost, T("Not allowed to view thread")))
                return new HttpUnauthorizedResult();

            var correctedPath = _forumPathConstraint.FindPath(forumPath);
            if (correctedPath == null)
                return HttpNotFound();

            var forumPart = _forumService.Get(string.Format("Forum/{0}", correctedPath));
            if (forumPart == null)
                return HttpNotFound();

            var threadPart = _threadService.Get(forumPart, threadSlug, VersionOptions.Published);
            if (threadPart == null)
                return HttpNotFound();

            //_feedManager.Register(threadPart);
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var posts = _postService.Get(threadPart, pager.GetStartIndex(), pager.PageSize)
                .Select(b => _orchardServices.ContentManager.BuildDisplay(b, "Detail"));
            dynamic thread = _orchardServices.ContentManager.BuildDisplay(threadPart);

            var pagerObject = Shape.Pager(pager).TotalItemCount(threadPart.PostCount);

            var list = Shape.List();
            list.AddRange(posts);
            thread.Content.Add(Shape.Parts_Threads_Post_List(ContentItems: list, Pager: pagerObject), "5");

            return new ShapeResult(this, thread);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}