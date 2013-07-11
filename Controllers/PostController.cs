using System;
using System.Web.Mvc;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using NGM.Forum.Services;
using NGM.Forum.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Themes;
using Orchard.UI.Notify;

namespace NGM.Forum.Controllers {
    [Themed]
    [ValidateInput(false)]
    public class PostController : Controller, IUpdateModel {
        private readonly IPostService _postService;
        private readonly IOrchardServices _orchardServices;

        public PostController(IOrchardServices orchardServices, 
            IPostService postService) {
            _postService = postService;
            _orchardServices = orchardServices;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult Create(int contentId) {
            var contentItem = _orchardServices.ContentManager.Get(contentId, VersionOptions.Latest);

            if (contentItem.As<PostPart>() == null) {
                if (contentItem.As<ThreadPart>() == null)
                    return HttpNotFound();
            }

            var forumPart = GetForum(contentItem);
            var part = _orchardServices.ContentManager.New<PostPart>(forumPart.PostType);
            part.ThreadPart = GetThreadPart(contentItem);

            if (!_orchardServices.Authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, part, T("Not allowed to create post")))
                return new HttpUnauthorizedResult();

            var model = _orchardServices.ContentManager.BuildEditor(part);

            return View((object)model);
        }

        public ActionResult CreateWithQuote(int contentId) {
            var contentItem = _orchardServices.ContentManager.Get(contentId, VersionOptions.Latest);
            
            if (contentItem.As<PostPart>() == null) {
                if (contentItem.As<ThreadPart>() == null)
                    return HttpNotFound();
            }

            var forumPart = GetForum(contentItem);
            var part = _orchardServices.ContentManager.New<PostPart>(forumPart.PostType);
            part.ThreadPart = GetThreadPart(contentItem);

            if (!_orchardServices.Authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, part, T("Not allowed to create post")))
                return new HttpUnauthorizedResult();

            part.Text = string.Format("<blockquote>{0}</blockquote>{1}", contentItem.As<PostPart>().Text, Environment.NewLine);

            var model = _orchardServices.ContentManager.BuildEditor(part);

            return View("Create", (object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(int contentId) {
            var contentItem = _orchardServices.ContentManager.Get(contentId, VersionOptions.Latest);
            
            if (contentItem.As<PostPart>() == null) {
                if (contentItem.As<ThreadPart>() == null)
                    return HttpNotFound();
            }

            var forumPart = GetForum(contentItem);
            var post = _orchardServices.ContentManager.New<PostPart>(forumPart.PostType);
            var threadPart = GetThreadPart(contentItem);
            post.ThreadPart = threadPart;

            if (contentItem.As<PostPart>() == null) {
                // Perform a check
                if (_postService.GetPositional(threadPart, ThreadPostPositional.First) != null) {
                    _orchardServices.Notifier.Error(T("You cannot attach two parent posts to a thread."));
                    return Redirect(Url.ThreadView(threadPart));
                }
            }
            else {
                post.RepliedOn = contentItem.As<PostPart>().Id;
            }

            if (!_orchardServices.Authorizer.Authorize(Orchard.Core.Contents.Permissions.PublishContent, post, T("Not allowed to create post")))
                return new HttpUnauthorizedResult();

            _orchardServices.ContentManager.Create(post.ContentItem);
            var model = _orchardServices.ContentManager.UpdateEditor(post, this);

            if (!ModelState.IsValid) {
                _orchardServices.TransactionManager.Cancel();
                return View((object)model);
            }

            _orchardServices.ContentManager.Publish(post.ContentItem);

            _orchardServices.Notifier.Information(T("Your {0} has been created.", post.TypeDefinition.DisplayName));

            var pager = new ThreadPager(_orchardServices.WorkContext.CurrentSite, post.ThreadPart.PostCount);
            return Redirect(Url.PostView(post, pager));
        }

        public ActionResult Delete(int contentId) {
            var contentItem = _orchardServices.ContentManager.Get(contentId);

            var thread = contentItem.As<ThreadPart>();

            if (thread != null) {
                if (!_orchardServices.Authorizer.Authorize(Orchard.Core.Contents.Permissions.DeleteContent, contentItem, T("Not allowed to delete thread")))
                    return new HttpUnauthorizedResult();

                _orchardServices.ContentManager.Remove(contentItem);
                _orchardServices.Notifier.Information(T("Thread has been deleted."));
                return Redirect(Url.ForumView(thread.ForumPart));
            }

            var post = contentItem.As<PostPart>();

            if (post != null) {
                if (!_orchardServices.Authorizer.Authorize(Orchard.Core.Contents.Permissions.DeleteContent, contentItem, T("Not allowed to delete post")))
                    return new HttpUnauthorizedResult();

                if (post.IsParentThread()) {
                    _orchardServices.ContentManager.Remove(post.ThreadPart.ContentItem);
                    _orchardServices.Notifier.Information(T("Thread has been deleted."));
                    return Redirect(Url.ForumView(post.ThreadPart.ForumPart));
                }
                else {
                    _orchardServices.ContentManager.Remove(contentItem);
                    _orchardServices.Notifier.Information(T("Post has been deleted."));

                    var pager = new ThreadPager(_orchardServices.WorkContext.CurrentSite, post.ThreadPart.PostCount);
                    return Redirect(Url.ThreadView(post.ThreadPart, pager));
                }
            }

            return Redirect(Url.Forums());
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        private static ForumPart GetForum(IContent content) {
            var postPart = content.As<PostPart>();
            var threadPart = content.As<ThreadPart>();

            if (postPart == null) {
                return threadPart == null ? null : threadPart.ForumPart;
            }
            return postPart.ThreadPart.ForumPart;
        }

        private static ThreadPart GetThreadPart(IContent content) {
            return content.Has<ThreadPart>() ? content.As<ThreadPart>() : content.As<PostPart>().ThreadPart;
        }
    }
}