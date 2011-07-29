using System.Web.Mvc;
using NGM.Forum.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Routable.Services;

namespace NGM.Forum.Extensions {
    public static class UrlHelperExtensions {
        public static string ForumForAdmin(this UrlHelper urlHelper, ForumPart forumPart) {
            return urlHelper.Action("Item", "ForumAdmin", new { forumId = forumPart.Id, area = Constants.LocalArea });
        }

        public static string ForumCreate(this UrlHelper urlHelper) {
            return urlHelper.Action("Create", "ForumAdmin", new { area = Constants.LocalArea });
        }

        public static string ViewForums(this UrlHelper urlHelper) {
            return urlHelper.Action("List", "Forum", new { area = Constants.LocalArea });
        }

        public static string ViewThread(this UrlHelper urlHelper, ThreadPart threadPart) {
            return urlHelper.Action("Item", "Thread", new { forumPath = threadPart.ForumPart.As<IRoutableAspect>().Path, threadSlug = threadPart.As<IRoutableAspect>().GetEffectiveSlug(), area = Constants.LocalArea });
        }

        public static string CreateThread(this UrlHelper urlHelper, ForumPart forumPart) {
            return urlHelper.Action("Create", "Thread", new { forumId = forumPart.Id, area = Constants.LocalArea });
        }

        public static string ReplyPost(this UrlHelper urlHelper, PostPart postPart) {
            return ReplyContent(urlHelper, postPart);
        }

        public static string ReplyContent(this UrlHelper urlHelper, IContent content) {
            return urlHelper.Action("Create", "Post", new { contentId = content.Id, area = Constants.LocalArea });
        }

        public static string ReplyThread(this UrlHelper urlHelper, ThreadPart threadPart) {
            return ReplyContent(urlHelper, threadPart);
        }
        
        public static string OpenThread(this UrlHelper urlHelper, ThreadPart threadPart) {
            return urlHelper.Action("Open", "Thread", new { threadId = threadPart.Id, area = Constants.LocalArea });
        }

        public static string CloseThread(this UrlHelper urlHelper, ThreadPart threadPart) {
            return urlHelper.Action("Close", "Thread", new { threadId = threadPart.Id, area = Constants.LocalArea });
        }

        public static string OpenForum(this UrlHelper urlHelper, ForumPart forumPart) {
            return urlHelper.Action("Open", "Forum", new { forumId = forumPart.Id, area = Constants.LocalArea });
        }

        public static string CloseForum(this UrlHelper urlHelper, ForumPart forumPart) {
            return urlHelper.Action("Close", "Forum", new { forumId = forumPart.Id, area = Constants.LocalArea });
        }
    }
}