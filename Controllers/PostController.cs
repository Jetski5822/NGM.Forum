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
            if (!_orchardServices.Authorizer.Authorize(Permissions.CreatePost, T("Not allowed to create post")))
                return new HttpUnauthorizedResult();

            var contentItem = _orchardServices.ContentManager.Get(contentId, VersionOptions.Latest);

            if (contentItem.As<PostPart>() == null) {
                if (contentItem.As<ThreadPart>() == null)
                    return HttpNotFound();

                if (IsNotAllowedToReplyToPost())
                    return new HttpUnauthorizedResult();
            }

            var forumPart = GetForum(contentItem);
            var part = _orchardServices.ContentManager.New<PostPart>(forumPart.PostType);
            var model = _orchardServices.ContentManager.BuildEditor(part);

            return View((object)model);
        }

        public ActionResult CreateWithQuote(int contentId) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.CreatePost, T("Not allowed to create post")))
                return new HttpUnauthorizedResult();

            var contentItem = _orchardServices.ContentManager.Get(contentId, VersionOptions.Latest);
            if (contentItem.As<PostPart>() == null) {
                if (contentItem.As<ThreadPart>() == null)
                    return HttpNotFound();

                if (IsNotAllowedToReplyToPost())
                    return new HttpUnauthorizedResult();
            }

            var forumPart = GetForum(contentItem);
            var part = _orchardServices.ContentManager.New<PostPart>(forumPart.PostType);

            part.Text = string.Format("<blockquote>{0}</blockquote>{1}", contentItem.As<PostPart>().Text, Environment.NewLine);

            var model = _orchardServices.ContentManager.BuildEditor(part);

            return View("Create", (object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(int contentId) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.CreatePost, T("Not allowed to create post")))
                return new HttpUnauthorizedResult();

            var contentItem = _orchardServices.ContentManager.Get(contentId, VersionOptions.Latest);
            if (contentItem.As<PostPart>() == null) {
                if (contentItem.As<ThreadPart>() == null)
                    return HttpNotFound();

                if (IsNotAllowedToReplyToPost())
                    return new HttpUnauthorizedResult();
            }

            var forumPart = GetForum(contentItem);
            var post = _orchardServices.ContentManager.Create<PostPart>(forumPart.PostType, VersionOptions.Draft);
            var model = _orchardServices.ContentManager.UpdateEditor(post, this);

            if (contentItem.As<PostPart>() == null) {
                // Perform a check
                if (_postService.GetFirstPost(contentItem.As<ThreadPart>(), VersionOptions.Published) != null) {
                    _orchardServices.Notifier.Error(T("You cannot attach two parent posts to a thread."));
                    return Redirect(Url.ThreadView(contentItem.As<ThreadPart>()));
                }

                post.ThreadPart = contentItem.As<ThreadPart>();
            }
            else {
                post.ThreadPart = contentItem.As<PostPart>().ThreadPart;
                post.RepliedOn = contentItem.As<PostPart>().Id;
            }

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
                if (!_orchardServices.Authorizer.Authorize(Permissions.DeletePost, contentItem, T("Not allowed to delete thread")))
                    return new HttpUnauthorizedResult();

                _orchardServices.ContentManager.Remove(contentItem);
                _orchardServices.Notifier.Information(T("Thread has been deleted."));
                return Redirect(Url.ForumView(thread.ForumPart));
            }

            var post = contentItem.As<PostPart>();

            if (post != null) {
                if (!_orchardServices.Authorizer.Authorize(Permissions.DeletePost, contentItem, T("Not allowed to delete post")))
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

        private bool IsNotAllowedToReplyToPost() {
            return !_orchardServices.Authorizer.Authorize(Permissions.ReplyPost, T("Not allowed to reply to a post"));
        }

        private static ForumPart GetForum(IContent content) {
            var postPart = content.As<PostPart>();
            var threadPart = content.As<ThreadPart>();

            if (postPart == null) {
                return threadPart == null ? null : threadPart.ForumPart;
            }
            return postPart.ThreadPart.ForumPart;
        }
    }
}