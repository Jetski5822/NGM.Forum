using System;
using System.Web.Mvc;
using System.Linq;
using NGM.Forum.Extensions;
using NGM.Forum.Helpers;
using NGM.Forum.Models;
using NGM.Forum.Services;
using NGM.Forum.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Themes;
using Orchard.UI.Notify;
using Orchard.UI.Navigation;
using Orchard.Core.Common.Models;
using Html.Helpers;
using System.Web;

namespace NGM.Forum.Controllers {
    [Themed]
    [ValidateInput(false)]
    public class PostController : Controller, IUpdateModel {

        private readonly IOrchardServices _orchardServices;
        private readonly IReportPostService _reportPostService;
        private readonly IPostEditHistoryService _postEditHistoryService;

        public PostController(
            IOrchardServices orchardServices,
            IReportPostService reportPostService,
            IPostEditHistoryService postEditHistoryService
            
        ) {
            _orchardServices = orchardServices;
            _reportPostService = reportPostService;
            _postEditHistoryService = postEditHistoryService;

            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
            
        }

        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ActionResult Create(int contentId) {
            var contentItem = _orchardServices.ContentManager.Get(contentId, VersionOptions.Latest);

            bool isPost = contentItem.Has<PostPart>();
            bool isThread = contentItem.Has<ThreadPart>();

            if (!isPost && !isThread)
                return HttpNotFound();

            var forumPart = HierarchyHelpers.GetForum(contentItem);
            var part = _orchardServices.ContentManager.New<PostPart>(forumPart.PostType);
            part.ThreadPart = HierarchyHelpers.GetThreadPart(contentItem);

            var forumsHomePage = _orchardServices.ContentManager.Get(part.ThreadPart.ForumsHomepageId);
            if (!_orchardServices.Authorizer.Authorize(Permissions.CreateThreadsAndPosts, forumsHomePage, T("Not allowed to create posts.")))
                return new HttpUnauthorizedResult();

            ///var model = _orchardServices.ContentManager.BuildDisplay(part, "Editor");
            var model = _orchardServices.ContentManager.BuildEditor(part);

            return View((object)model);
        }

        public ActionResult CreateWithQuote(int contentId) {
            var contentItem = _orchardServices.ContentManager.Get(contentId, VersionOptions.Latest);

            bool isPost = contentItem.Has<PostPart>();
            bool isThread = contentItem.Has<ThreadPart>();

            if (!isPost && !isThread)
                return HttpNotFound();

            var forumPart = HierarchyHelpers.GetForum(contentItem);
            var part = _orchardServices.ContentManager.New<PostPart>(forumPart.PostType);
            part.ThreadPart = HierarchyHelpers.GetThreadPart(contentItem);

            var forumsHomePage = _orchardServices.ContentManager.Get(part.ThreadPart.ForumsHomepageId);

            if (!_orchardServices.Authorizer.Authorize(Permissions.CreateThreadsAndPosts, forumsHomePage, T("Not allowed to create post")))
                return new HttpUnauthorizedResult();

            part.Text = string.Format("<blockquote>{0}</blockquote>{1}", contentItem.As<PostPart>().Text, "<p></p>");

            var model = _orchardServices.ContentManager.BuildEditor(part);

            return View("Create", (object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST(int contentId ) //, string Text)
        {
            var contentItem = _orchardServices.ContentManager.Get(contentId, VersionOptions.Latest);

            bool isPost = contentItem.Has<PostPart>();
            bool isThread = contentItem.Has<ThreadPart>();

            if (!isPost && !isThread)
                return HttpNotFound();

            var forumPart = HierarchyHelpers.GetForum(contentItem);
            var post = _orchardServices.ContentManager.New<PostPart>(forumPart.PostType);

            var threadPart = HierarchyHelpers.GetThreadPart(contentItem);
            post.ThreadPart = threadPart;

            if (isThread) {
                // Attach to parent post and NOT to the thread
                post.RepliedOn = contentItem.As<ThreadPart>().FirstPost.Id;
            }
            else {
                post.RepliedOn = contentItem.As<PostPart>().Id;
            }

            var forumsHomePage = _orchardServices.ContentManager.Get(threadPart.ForumsHomepageId);

            if (!_orchardServices.Authorizer.Authorize(Permissions.CreateThreadsAndPosts, forumsHomePage, T("Not allowed to create post")))
                return new HttpUnauthorizedResult();

            //this has to have 'draft' specified so it triggers the antispam correctly (i.e. after the values are set in the updateEditor)
            _orchardServices.ContentManager.Create(post.ContentItem, VersionOptions.Draft);
           var model = _orchardServices.ContentManager.UpdateEditor(post, this);

            post.IP = GetClientIpAddress(this.Request);

            if (!ModelState.IsValid )// || String.IsNullOrWhiteSpace( Text ))
            {
                _orchardServices.TransactionManager.Cancel();
                return View((object)model);
            }
            else
            {
                _orchardServices.ContentManager.Publish(post.ContentItem);
                _orchardServices.Notifier.Information(T("Your {0} has been created.", post.TypeDefinition.DisplayName));
            }
            var site = _orchardServices.WorkContext.CurrentSite;
            var pager = new Pager(site, (int)Math.Ceiling((decimal)post.ThreadPart.PostCount / (decimal)site.PageSize), site.PageSize);
            return Redirect(Url.PostView(post, pager));
        }


        public ActionResult Delete(int contentId) {
            var contentItem = _orchardServices.ContentManager.Get(contentId);

            var thread = contentItem.As<ThreadPart>();

            var forumsHomePage = _orchardServices.ContentManager.Get(thread.ForumsHomepageId);
            if (thread != null) {
                if (!_orchardServices.Authorizer.Authorize(Permissions.DeleteThreadsAndPosts, forumsHomePage, T("Not allowed to delete thread")))
                    return new HttpUnauthorizedResult();

                _orchardServices.ContentManager.Remove(contentItem);
                _orchardServices.Notifier.Information(T("Thread has been deleted."));
                return Redirect(Url.ForumView(thread.ForumPart));
            }

            var post = contentItem.As<PostPart>();

            if (post != null) {
                if (!_orchardServices.Authorizer.Authorize(Permissions.DeleteThreadsAndPosts, forumsHomePage , T("Not allowed to delete post")))
                    return new HttpUnauthorizedResult();

                if (post.IsParentThread()) {
                    _orchardServices.ContentManager.Remove(post.ThreadPart.ContentItem);
                    _orchardServices.Notifier.Information(T("Thread has been deleted."));
                    return Redirect(Url.ForumView(post.ThreadPart.ForumPart));
                }
                else {
                    _orchardServices.ContentManager.Remove(contentItem);
                    _orchardServices.Notifier.Information(T("Post has been deleted."));

                    //var pager = new ThreadPager(_orchardServices.WorkContext.CurrentSite, post.ThreadPart.PostCount);
                    var site = _orchardServices.WorkContext.CurrentSite;
                    var pager = new Pager(site, (int)Math.Ceiling((decimal)post.ThreadPart.PostCount / (decimal)site.PageSize), site.PageSize);
                    return Redirect(Url.ThreadView(post.ThreadPart, pager));
                }
            }

            return Redirect(Url.Forums(thread.ForumPart));
        }

         [HttpGet, ActionName("Edit")]
        public ActionResult EditPost(int contentId)
        {
            var contentItem = _orchardServices.ContentManager.Get(contentId);
            var post = contentItem.As<PostPart>();

            if (post != null)
            {
                var forumsHomePage = _orchardServices.ContentManager.Get(post.ThreadPart.ForumsHomepageId);
                if (!_orchardServices.Authorizer.Authorize(Permissions.EditPosts, forumsHomePage, T("Not allowed to edit post")))
                    return new HttpUnauthorizedResult();

                //pass the existing post to the editor as-is
                var model = _orchardServices.ContentManager.BuildEditor(post);

                return View("Edit", (object)model);
            }

            return Redirect(Url.Forums(post.ThreadPart.ForumPart));
        }

         [HttpPost, ActionName("Edit")]
         public ActionResult EditPostPOST(int contentId)
         {
             var contentItem = _orchardServices.ContentManager.Get(contentId);
             var post = contentItem.As<PostPart>();

             if (post != null)
             {
                 var forumsHomePage = _orchardServices.ContentManager.Get(post.ThreadPart.ForumsHomepageId);
                 if (!_orchardServices.Authorizer.Authorize(Permissions.EditPosts, forumsHomePage, T("Not allowed to edit post")))
                     return new HttpUnauthorizedResult();
                 string edited  = T("Last Edited on :").ToString() + DateTime.UtcNow.ToShortDateString();

                 post.LastEdited = DateTime.UtcNow;

                 var model = _orchardServices.ContentManager.UpdateEditor(post, this);

                 if (!ModelState.IsValid)
                 {
                     _orchardServices.TransactionManager.Cancel();
                     return View((object)model);
                 }

                 _orchardServices.ContentManager.Publish(post.ContentItem);

                 _orchardServices.Notifier.Information(T("Your {0} has been edited.", post.TypeDefinition.DisplayName));
                 var site = _orchardServices.WorkContext.CurrentSite;
                 //var pager = new ThreadPager(_orchardServices.WorkContext.CurrentSite, post.ThreadPart.PostCount);
                 var pager = new Pager(site, (int)Math.Ceiling((decimal)post.ThreadPart.PostCount / (decimal)site.PageSize), site.PageSize);
                 return Redirect(Url.PostView(post, pager));

             }

             return Redirect(Url.Forums(post.ThreadPart.ForumPart));
         }

        [Themed, HttpGet]
         public ActionResult ViewPostEditHistory(int postId, string returnUrl)
         {
             var post = _orchardServices.ContentManager.Get<PostPart>(postId);

            var lastPost = _orchardServices.ContentManager.BuildDisplay(post);
            //sanitize the html
            var edits = _postEditHistoryService.GetEdits(postId).Select(e => { e.Text = HtmlSanitizer.sanitizer(e.Text.ToString()).html; return e; });

            ViewPostEditHistoryViewModel viewModel = new ViewPostEditHistoryViewModel();
            viewModel.LastPost = lastPost;
            viewModel.EditHistory = edits;
            viewModel.ReturnUrl = returnUrl;

            return View((object)viewModel);
        }


        //http://stackoverflow.com/questions/2577496/how-can-i-get-the-clients-ip-address-in-asp-net-mvc
        private static string GetClientIpAddress(HttpRequestBase request)
        {
            string szRemoteAddr = request.UserHostAddress;
            string szXForwardedFor = request.ServerVariables["X_FORWARDED_FOR"];
            string szIP = "";

            if (szXForwardedFor == null)
            {
                szIP = szRemoteAddr;
            }
            else
            {
                szIP = szXForwardedFor;
                if (szIP.IndexOf(",") > 0)
                {
                    string[] arIPs = szIP.Split(',');

                    foreach (string item in arIPs)
                    {
                        if (!IsPrivateIpAddress(item))
                        {
                            return item;
                        }
                    }
                }
            }
            return szIP;
        }

        private static bool IsPrivateIpAddress(string ipAddress)
        {
            // http://en.wikipedia.org/wiki/Private_network
            // Private IP Addresses are: 
            //  24-bit block: 10.0.0.0 through 10.255.255.255
            //  20-bit block: 172.16.0.0 through 172.31.255.255
            //  16-bit block: 192.168.0.0 through 192.168.255.255
            //  Link-local addresses: 169.254.0.0 through 169.254.255.255 (http://en.wikipedia.org/wiki/Link-local_address)

            var ip = System.Net.IPAddress.Parse(ipAddress);
            var octets = ip.GetAddressBytes();

            var is24BitBlock = octets[0] == 10;
            if (is24BitBlock) return true; // Return to prevent further processing

            var is20BitBlock = octets[0] == 172 && octets[1] >= 16 && octets[1] <= 31;
            if (is20BitBlock) return true; // Return to prevent further processing

            var is16BitBlock = octets[0] == 192 && octets[1] == 168;
            if (is16BitBlock) return true; // Return to prevent further processing

            var isLinkLocalAddress = octets[0] == 169 && octets[1] == 254;
            return isLinkLocalAddress;
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}