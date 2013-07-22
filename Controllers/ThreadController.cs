using System.Linq;
using System.Web.Mvc;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using NGM.Forum.Services;
using Orchard;
using Orchard.ContentManagement;
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
        private readonly IThreadService _threadService;
        private readonly IPostService _postService;
        private readonly ISiteService _siteService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthenticationService _authenticationService;

        public ThreadController(IOrchardServices orchardServices,
            IForumService forumService,
            IThreadService threadService,
            IPostService postService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IAuthorizationService authorizationService,
            IAuthenticationService authenticationService) {
            _orchardServices = orchardServices;
            _forumService = forumService;
            _threadService = threadService;
            _postService = postService;
            _siteService = siteService;
            _authorizationService = authorizationService;
            _authenticationService = authenticationService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ActionResult Create(int forumId) {
            var forumPart = _forumService.Get(forumId, VersionOptions.Latest);
            if (forumPart == null)
                return HttpNotFound();

            var thread = _orchardServices.ContentManager.New<ThreadPart>(forumPart.ThreadType);
            thread.ForumPart = forumPart;

            if (!_orchardServices.Authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, thread, T("Not allowed to create thread")))
                return new HttpUnauthorizedResult();

            var post = _orchardServices.ContentManager.New<PostPart>(forumPart.PostType);
            post.ThreadPart = thread;
            
            var threadModel = _orchardServices.ContentManager.BuildEditor(thread);
            var postModel = _orchardServices.ContentManager.BuildEditor(post);

            DynamicZoneExtensions.RemoveItemFrom(threadModel.Sidebar, "Content_SaveButton");

            var viewModel = Shape.ViewModel()
                .Thread(threadModel)
                .Post(postModel);

            return View((object)viewModel);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(int forumId) {
            var forumPart = _forumService.Get(forumId, VersionOptions.Latest);
            if (forumPart == null)
                return HttpNotFound();

            var thread = _orchardServices.ContentManager.Create<ThreadPart>(forumPart.ThreadType, VersionOptions.Draft, o => { o.ForumPart = forumPart; });

            if (!_orchardServices.Authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, thread, T("Not allowed to create thread")))
                return new HttpUnauthorizedResult();

            var threadModel = _orchardServices.ContentManager.UpdateEditor(thread, this);

            var post = _orchardServices.ContentManager.Create<PostPart>(forumPart.PostType, VersionOptions.Draft, o => { o.ThreadPart = thread; });
            
            if (!_orchardServices.Authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, post, T("Not allowed to create post")))
                return new HttpUnauthorizedResult();

            var postModel = _orchardServices.ContentManager.UpdateEditor(post, this);
            post.ThreadPart = thread;

            if (!ModelState.IsValid) {
                _orchardServices.TransactionManager.Cancel();

                DynamicZoneExtensions.RemoveItemFrom(threadModel.Sidebar, "Content_SaveButton");

                var viewModel = Shape.ViewModel()
                .Thread(threadModel)
                .Post(postModel);

                return View((object)viewModel);
            }

            _orchardServices.ContentManager.Publish(thread.ContentItem);
            _orchardServices.ContentManager.Publish(post.ContentItem);

            _orchardServices.Notifier.Information(T("Your {0} has been created.", thread.TypeDefinition.DisplayName));            
            return Redirect(Url.ThreadView(thread));
        }


        public ActionResult Item(int forumId, int threadId, PagerParameters pagerParameters) {
            var threadPart = _threadService.Get(forumId, threadId, VersionOptions.Published);
            if (threadPart == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Orchard.Core.Contents.Permissions.ViewContent, threadPart, T("Not allowed to view thread")))
                return new HttpUnauthorizedResult();

            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            var posts = _postService.Get(threadPart, pager.GetStartIndex(), pager.PageSize, VersionOptions.Published)
                .Select(b => _orchardServices.ContentManager.BuildDisplay(b, "Detail"));

            dynamic thread = _orchardServices.ContentManager.BuildDisplay(threadPart);

            var pagerObject = Shape.Pager(pager).TotalItemCount(threadPart.PostCount);

            var list = Shape.List();
            list.AddRange(posts);
            thread.Content.Add(Shape.Parts_Threads_Post_List(ContentPart: threadPart, ContentItems: list, Pager: pagerObject), "5");

            var part = _orchardServices.ContentManager.New<PostPart>(threadPart.ForumPart.PostType);

            /* Get Edit Post*/
            if (!threadPart.IsClosed && IsAllowedToCreatePost(part)) {
                dynamic model = _orchardServices.ContentManager.BuildEditor(part);

                var firstPostId =  _postService.GetPositional(threadPart, ThreadPostPositional.First).Id;

                thread.Content.Add(Shape.Parts_Thread_Post_Create(ContentEditor: model, ContentId: firstPostId), "10");
            }

            return new ShapeResult(this, thread);
        }

        private bool IsAllowedToCreatePost(PostPart postPart) {
            return _authorizationService.TryCheckAccess(Orchard.Core.Contents.Permissions.PublishContent, _authenticationService.GetAuthenticatedUser(), postPart);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}