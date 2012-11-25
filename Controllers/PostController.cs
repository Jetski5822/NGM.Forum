using System.Web.Mvc;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using NGM.Forum.Services;
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

        public PostController(IOrchardServices orchardServices, IPostService postService) {
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

            var part = _orchardServices.ContentManager.New<PostPart>(Constants.Parts.Post);

            dynamic model = _orchardServices.ContentManager.BuildEditor(part);

            return View((object)model);
        }

        //public ActionResult CreateWithQuote(int contentId) {
        //    if (!_orchardServices.Authorizer.Authorize(Permissions.CreatePost, T("Not allowed to create post")))
        //        return new HttpUnauthorizedResult();

        //    var contentItem = _orchardServices.ContentManager.Get(contentId, VersionOptions.Latest);
        //    if (contentItem.As<PostPart>() == null) {
        //        if (contentItem.As<ThreadPart>() == null)
        //            return HttpNotFound();

        //        if (IsNotAllowedToReplyToPost())
        //            return new HttpUnauthorizedResult();
        //    }

        //    var part = _orchardServices.ContentManager.New<PostPart>(Constants.Parts.Post);

        //    part.Text =
        //        string.Format("<blockquote><div class='quote-header'>Quote: {0}<div class='quote-logo'></div></div><div class='quote-content'>{1}</div></blockquote><br />",
        //        contentItem.As<ICommonPart>().Owner.UserName,
        //        contentItem.As<PostPart>().Text);

        //    dynamic model = _orchardServices.ContentManager.BuildEditor(part);

        //    return View("Create", (object)model);
        //}

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

            var post = _orchardServices.ContentManager.Create<PostPart>(Constants.Parts.Post, VersionOptions.Draft);
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

            return Redirect(Url.ThreadView(post.ThreadPart));
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }

        //private bool IsNotAllowedToCreatePost() {
        //    return !_orchardServices.Authorizer.Authorize(Permissions.CreatePost, T("Not allowed to create post"));
        //}

        private bool IsNotAllowedToReplyToPost() {
            return !_orchardServices.Authorizer.Authorize(Permissions.ReplyPost, T("Not allowed to reply to a post"));
        }
    }
}