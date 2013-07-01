using System.Web.Mvc;
using NGM.Forum.Models;
using Orchard.ContentManagement;
using Orchard.UI.Navigation;
using Orchard.Utility.Extensions;

namespace NGM.Forum.Extensions {
    public static class UrlHelperExtensions {

        // Default Route
        public static string Forums(this UrlHelper urlHelper) {
            return urlHelper.Action("List", "Forum", new { area = Constants.LocalArea });
        }

        /* Forum */

        public static string ForumForAdmin(this UrlHelper urlHelper, ForumPart forumPart) {
            return urlHelper.Action("Item", "ForumAdmin", new { forumId = forumPart.Id, area = Constants.LocalArea });
        }

        public static string ForumsForAdmin(this UrlHelper urlHelper) {
            return urlHelper.Action("List", "ForumAdmin", new { area = Constants.LocalArea });
        }

        public static string ForumCreateForAdmin(this UrlHelper urlHelper) {
            return urlHelper.Action("Create", "ForumAdmin", new { area = Constants.LocalArea });
        }

        public static string ForumEdit(this UrlHelper urlHelper, ForumPart forumPart) {
            return urlHelper.Action("Edit", "ForumAdmin", new { forumId = forumPart.Id, area = Constants.LocalArea });
        }


        public static string ForumView(this UrlHelper urlHelper, ForumPart forumPart) {
            return urlHelper.Action("Item", "Forum", new { forumId = forumPart.Id, area = Constants.LocalArea });
        }

        /* Thread */

        public static string ThreadForAdmin(this UrlHelper urlHelper, ThreadPart threadPart) {
            return urlHelper.Action("Item", "ThreadAdmin", new { threadId = threadPart.Id, area = Constants.LocalArea });
        }

        public static string ThreadMoveForAdmin(this UrlHelper urlHelper, ThreadPart threadPart) {
            return urlHelper.Action("Move", "ThreadAdmin", new { threadId = threadPart.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }

        public static string ThreadCloseForAdmin(this UrlHelper urlHelper, ThreadPart threadPart) {
            return urlHelper.Action("Close", "ThreadAdmin", new { threadId = threadPart.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }

        public static string ThreadOpenForAdmin(this UrlHelper urlHelper, ThreadPart threadPart) {
            return urlHelper.Action("Open", "ThreadAdmin", new { threadId = threadPart.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }

        public static string ThreadCreate(this UrlHelper urlHelper, int forumId) {
            return urlHelper.Action("Create", "Thread", new { forumId = forumId, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }

        public static string ThreadCreate(this UrlHelper urlHelper, ForumPart forumPart) {
            return urlHelper.Action("Create", "Thread", new { forumId = forumPart.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }

        public static string ThreadView(this UrlHelper urlHelper, ThreadPart threadPart) {
            return urlHelper.Action("Item", "Thread", new { forumId = threadPart.ForumPart.Id, threadId = threadPart.Id, area = Constants.LocalArea });
        }

        public static string ThreadView(this UrlHelper urlHelper, ThreadPart threadPart, Pager pager) {
            return urlHelper.Action("Item", "Thread", new { forumId = threadPart.ForumPart.Id, threadId = threadPart.Id, page = pager.Page, area = Constants.LocalArea });
        }

        public static string ThreadDelete(this UrlHelper urlHelper, ThreadPart threadPart) {
            return urlHelper.Action("Delete", "Post", new { contentId = threadPart.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }

        /* Post */

        public static string PostReply(this UrlHelper urlHelper, PostPart postPart) {
            return PostCreateByContent(urlHelper, postPart);
        }

        public static string PostReplyWithQuote(this UrlHelper urlHelper, PostPart postPart) {
            return urlHelper.Action("CreateWithQuote", "Post", new { contentId = postPart.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString()});
        }

        public static string PostView(this UrlHelper urlHelper, PostPart postPart) {
            return string.Format("{0}#{1}", ThreadView(urlHelper, postPart.ThreadPart), postPart.Id);
        }

        public static string PostView(this UrlHelper urlHelper, PostPart postPart, Pager pager) {
            if (pager.Page >= 2)
                return string.Format("{0}#{1}", ThreadView(urlHelper, postPart.ThreadPart, pager), postPart.Id);
            else
                return PostView(urlHelper, postPart);
        }

        public static string PostDelete(this UrlHelper urlHelper, PostPart postPart) {
            return urlHelper.Action("Delete", "Post", new { contentId = postPart.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }

        private static string PostCreateByContent(this UrlHelper urlHelper, IContent content) {
            return urlHelper.Action("Create", "Post", new { contentId = content.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }
    }
}