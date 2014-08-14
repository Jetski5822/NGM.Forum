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
using Orchard.Core.Common.Models;
using Orchard.Mvc.Extensions;
using NGM.Forum.ViewModels;
using Orchard.Core.Title.Models;
using Orchard.Autoroute.Models;
using System;
using Orchard.ContentManagement.Aspects;
using System.Collections.Generic;


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
            IAuthenticationService authenticationService

        ) {
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

/*
        public ActionResult Create(int forumId) {
            //removed the use of dashboard editors on the front end because as of 1.8 there is no way to make alts
            //and re-style the parts
            // https://orchard.codeplex.com/discussions/434112
            // https://orchard.codeplex.com/workitem/19479

            if (!_orchardServices.Authorizer.Authorize(Permissions.CreateThreadsAndPosts, T("Not allowed to create thread")))
                return new HttpUnauthorizedResult();

            var forumPart = _forumService.Get(forumId, VersionOptions.Latest);
            if (forumPart == null)
                return HttpNotFound();            

            var viewModel = new ThreadCreateViewModel();
            viewModel.ReturnUrl = Request.Url.AbsoluteUri;

            //TODO: this needs  to take into account 'own forums as well' but need to rework permissions in general
            //so leaving it for now
            viewModel.ShowIsSticky = _orchardServices.Authorizer.Authorize(Permissions.ManageForums);

            return View((object)viewModel);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(ThreadCreateViewModel threadViewModel)
        {
            //removed the use of dashboard editors on the front end because as of 1.8 there is no way to make alts
            //and re-style the parts
            // https://orchard.codeplex.com/discussions/434112
            // https://orchard.codeplex.com/workitem/19479

            var forumPart = _forumService.Get(threadViewModel.ForumId, VersionOptions.Latest);
            if (forumPart == null)
                return HttpNotFound();

            var forumSettings = _orchardServices.WorkContext.CurrentSite.As<ForumsSettingsPart>();
            if (threadViewModel.ThreadTitle.Length > forumSettings.ThreadTitleMaximumLength)
            {
                _orchardServices.Notifier.Add(NotifyType.Error, T("The thread title is too long.  The title can have a maximum of {0} characters but {1} characters were entered.", forumSettings.ThreadTitleMaximumLength, threadViewModel.ThreadTitle.Length));

                return View((object)threadViewModel);
            }

            var thread = _orchardServices.ContentManager.Create<ThreadPart>(forumPart.ThreadType, VersionOptions.Draft, o => { o.ForumPart = forumPart; });

            //removing this as it requires the content item persmission  module 
            //if (!_orchardServices.Authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, thread, T("Not allowed to create thread")))
            if (!_orchardServices.Authorizer.Authorize( Permissions.CreateThreadsAndPosts, T("Not allowed to create thread")))
                return new HttpUnauthorizedResult();

            thread.ForumPart = forumPart;
            thread.As<TitlePart>().Title =  threadViewModel.ThreadTitle.Substring(0, Math.Min( threadViewModel.ThreadTitle.Length, forumSettings.ThreadTitleMaximumLength));            
            thread.IsSticky = threadViewModel.isSticky;
            
            var post = _orchardServices.ContentManager.Create<PostPart>(forumPart.PostType, VersionOptions.Draft, o => { o.ThreadPart = thread; });
            post.ThreadPart = thread;
            post.Text = threadViewModel.Text;

            if (!_orchardServices.Authorizer.Authorize(Permissions.CreateThreadsAndPosts, post, T("Not allowed to create post")))
                return new HttpUnauthorizedResult();

            if (!ModelState.IsValid)
            {
                _orchardServices.TransactionManager.Cancel();

                return View((object)threadViewModel);
            }
            _orchardServices.ContentManager.Publish(thread.ContentItem);
           
            //needed to publish before the autoroute has values
            var url = thread.As<AutoroutePart>().DisplayAlias;
            if (forumSettings.ThreadTitleMaximumLength > 0)
            {
                url = url.Substring(0, Math.Min(forumSettings.ThreadTitleMaximumLength, url.Length));          
            }

            thread.As<AutoroutePart>().DisplayAlias = url;

            _orchardServices.ContentManager.Publish(post.ContentItem);

            _orchardServices.Notifier.Information(T("Your {0} has been created.", thread.TypeDefinition.DisplayName));
            return Redirect(Url.ThreadView(thread));
        }
     */
        /*
         * ORIGINAL CODE USING THE ADMIN VIEW EDITORS ON THE FRONT END WHEN MAKING A NEW THREAD
         */
         public ActionResult Create(int forumId) {
            var forumPart = _forumService.Get(forumId, VersionOptions.Latest);
            if (forumPart == null)
                return HttpNotFound();

            var thread = _orchardServices.ContentManager.New<ThreadPart>(forumPart.ThreadType);
            thread.ForumPart = forumPart;

            if (!_orchardServices.Authorizer.Authorize(Permissions.CreateThreadsAndPosts, thread, T("Not allowed to create thread")))
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

            if (!_orchardServices.Authorizer.Authorize(Permissions.CreateThreadsAndPosts, thread, T("Not allowed to create thread")))
                return new HttpUnauthorizedResult();

            var threadModel = _orchardServices.ContentManager.UpdateEditor(thread, this);

            var post = _orchardServices.ContentManager.Create<PostPart>(forumPart.PostType, VersionOptions.Draft, o => { o.ThreadPart = thread; });

            if (!_orchardServices.Authorizer.Authorize(Permissions.CreateThreadsAndPosts, post, T("Not allowed to create post")))
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



        /// <summary>
        /// Gets the posts for a thread
        /// </summary>
        /// <param name="forumId"></param>
        /// <param name="threadId"></param>
        /// <param name="pagerParameters"></param>
        /// <returns></returns>
        public ActionResult Item(int forumId, int threadId, PagerParameters pagerParameters) {
            var threadPart = _threadService.Get(forumId, threadId, VersionOptions.Published);
            if (threadPart == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Orchard.Core.Contents.Permissions.ViewContent, threadPart, T("Not allowed to view thread")))
                return new HttpUnauthorizedResult();

            //end read logic
            var pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);         

            var postList = _postService.Get(threadPart, pager.GetStartIndex(), pager.PageSize, VersionOptions.Published);
                
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums)) {
                //if the user is not an admin or moderator, don't include posts marked as inappropriate
                postList = postList.Where(p => p.IsInappropriate == false);
            }
            var posts = postList.Select(b => _orchardServices.ContentManager.BuildDisplay(b, "Detail")); 

            dynamic thread = _orchardServices.ContentManager.BuildDisplay(threadPart);

            var pagerObject = Shape.Pager(pager).TotalItemCount(threadPart.PostCount);

            
            var list = Shape.List();
            list.AddRange(posts);
            thread.Content.Add(Shape.Parts_Threads_Post_List(ContentPart: threadPart, ContentItems: list, Pager: pagerObject), "5");
            thread.Header.Add(Shape.Parts_Forum_Search(ForumsHomeId: threadPart.ForumsHomepageId), "1");

            var part = _orchardServices.ContentManager.New<PostPart>(threadPart.ForumPart.PostType);

            /* Get Edit Post*/
            if (!threadPart.IsClosed && IsAllowedToCreatePost(part)) {
                dynamic model = _orchardServices.ContentManager.BuildEditor(part);
                var firstPostId =  _postService.GetPositional(threadPart, true, ThreadPostPositional.First).Id;
                thread.Content.Add(Shape.Parts_Thread_Post_Create(ContentEditor: model, ContentId: firstPostId), "10");
                
            }

            return new ShapeResult(this, thread);
        }

        /// <summary>
        /// Displaying a specific post (via content-item id) in the 'thread view' (as opposed to view *only* the post content-item by itself).
        /// </summary>
        /// <param name="postPart"></param>
        /// <returns></returns>
        public ActionResult ViewPostInThread( int postId )
        {
            PostPart postPart = _postService.Get(postId, VersionOptions.Published);

            if (postPart == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Orchard.Core.Contents.Permissions.ViewContent, postPart.ThreadPart, T("Not allowed to view thread")))
                return new HttpUnauthorizedResult();

            var site = _orchardServices.WorkContext.CurrentSite;
            //var pager = new ThreadPager(_orchardServices.WorkContext.CurrentSite, post.ThreadPart.PostCount);
            var pager = new Pager(site, (int)Math.Ceiling((decimal)postPart.ThreadPart.PostCount / (decimal)site.PageSize), site.PageSize);
            return Redirect(Url.PostView(postPart, pager));
        }

        private bool IsAllowedToCreatePost(PostPart postPart) {
            return _authorizationService.TryCheckAccess(Orchard.Core.Contents.Permissions.PublishContent, _authenticationService.GetAuthenticatedUser(), postPart);
        }

        public ActionResult Move(int threadId)
        {
            var threadPart = _threadService.Get(threadId, VersionOptions.Latest);

            if (threadPart == null)
                return HttpNotFound(T("could not find thread").Text);

            if (!_orchardServices.Authorizer.Authorize(Permissions.MoveThread, threadPart, T("Not allowed to move thread")))
                return new HttpUnauthorizedResult();

            //TODO: This will list all forums in the system. May want to filter them either by 'forums home' or at least show which group they belong too
            //since there may be more than one 'forums home' in the system.  
            //*in this case should also verify the user has permission to move the thread between forumshomes.
            var forums = _forumService.Get();
            //What if I have 1 forum?

            var viewModel = new ThreadMoveAdminViewModel
            {
                ThreadId = threadPart.Id,
                AvailableForums = forums
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Move(int threadId, string returnUrl, ThreadMoveAdminViewModel viewModel)
        {
            var threadPart = _threadService.Get(threadId, VersionOptions.Latest);

            if (threadPart == null)
                return HttpNotFound(T("Could not find thread").Text);

            if (!_orchardServices.Authorizer.Authorize(Permissions.MoveThread, threadPart, T("Not allowed to move thread")))
                return new HttpUnauthorizedResult();

            var forumPart = _forumService.Get(viewModel.ForumId, VersionOptions.Latest);

            if (forumPart == null)
                return HttpNotFound(T("Could not find forum").Text);

            var currentForumName = threadPart.ForumPart.As<ITitleAspect>().Title;
            var newForumName = forumPart.As<ITitleAspect>().Title;

            threadPart.ForumPart = forumPart;

            _orchardServices.ContentManager.Publish(threadPart.ContentItem);

            _orchardServices.Notifier.Information(T("{0} has been moved from '{1}' to '{2}'.", threadPart.TypeDefinition.DisplayName, currentForumName, newForumName));

            return this.RedirectLocal(returnUrl, "~/");
        }

        public ActionResult Close(int threadId)
        {
            var threadPart = _threadService.Get(threadId, VersionOptions.Latest);

            if (threadPart == null)
                return HttpNotFound(T("could not find thread").Text);

            if (!_orchardServices.Authorizer.Authorize(Permissions.CloseThread, threadPart, T("Not allowed to close thread")))
                return new HttpUnauthorizedResult();

            var viewModel = new ThreadCloseAdminViewModel
            {
                ThreadId = threadPart.Id,
            };

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Close(int threadId, string returnUrl, ThreadCloseAdminViewModel viewModel)
        {
            var threadPart = _threadService.Get(threadId, VersionOptions.Latest);

            if (threadPart == null)
                return HttpNotFound(T("Could not find thread").Text);

            if (!_orchardServices.Authorizer.Authorize(Permissions.CloseThread, threadPart, T("Not allowed to close thread")))
                return new HttpUnauthorizedResult();

            threadPart.ClosedBy = _orchardServices.WorkContext.CurrentUser;
            threadPart.ClosedOnUtc = DateTime.UtcNow;
            threadPart.ClosedDescription = viewModel.Description;

            _orchardServices.ContentManager.Publish(threadPart.ContentItem);

            _orchardServices.Notifier.Information(T("{0} has been closed.", threadPart.TypeDefinition.DisplayName));

            return this.RedirectLocal(returnUrl, "~/");
        }

        public ActionResult Open(int threadId, string returnUrl)
        {
            var threadPart = _threadService.Get(threadId, VersionOptions.Latest);

            if (threadPart == null)
                return HttpNotFound(T("could not find thread").Text);

            if (!_orchardServices.Authorizer.Authorize(Permissions.CloseThread, threadPart, T("Not allowed to open thread")))
                return new HttpUnauthorizedResult();

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