using System.Web.Mvc;
using NGM.Forum.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Settings;

namespace NGM.Forum.Extensions {
    public static class UrlHelperExtensions {
        public static string AdminThreadsForForum(this UrlHelper urlHelper, ForumPart forumPart) {
            return urlHelper.Action("List", "ThreadAdmin", new { forumId = forumPart.Id, area = Constants.LocalArea });
        }

        public static string AdminForumCreate(this UrlHelper urlHelper) {
            return urlHelper.Action("Create", "ForumAdmin", new { area = Constants.LocalArea });
        }

        public static string ViewForums(this UrlHelper urlHelper) {
            return urlHelper.Action("List", "Forum", new { area = Constants.LocalArea });
        }

        public static string ViewThread(this UrlHelper urlHelper, ThreadPart threadPart) {
            return urlHelper.Action("Item", "Thread", new { forumPath = threadPart.ForumPart.As<IRoutableAspect>().Path, threadSlug = threadPart.As<IRoutableAspect>().Slug, area = Constants.LocalArea });
        }

        public static string ViewLatestPost(this UrlHelper urlHelper, PostPart postPart) {
            var siteSettings = urlHelper.Resolve<ISiteService>().GetSiteSettings();

            var result = postPart.ThreadPart.PostCount % siteSettings.PageSize;

            var numberOfPages = 0;
            if (result == 0) {
                numberOfPages = postPart.ThreadPart.PostCount / siteSettings.PageSize;
            } else {
                numberOfPages = postPart.ThreadPart.PostCount / siteSettings.PageSize + 1;
            }

            if (numberOfPages == 0)
                return ViewPost(urlHelper, postPart);

            return string.Format("{0}?page={1}#{2}", ViewThread(urlHelper, postPart.ThreadPart), numberOfPages, postPart.Id);
        }

        public static string ViewPost(this UrlHelper urlHelper, PostPart postPart) {
            return string.Format("{0}#{1}", ViewThread(urlHelper, postPart.ThreadPart), postPart.Id);
        }

        public static string CreateThread(this UrlHelper urlHelper, ForumPart forumPart) {
            return urlHelper.Action("Create", "Thread", new { forumId = forumPart.Id, area = Constants.LocalArea });
        }

        public static string ReplyPostWithQuote(this UrlHelper urlHelper, IContent content) {
            return urlHelper.Action("CreateWithQuote", "Post", new { contentId = content.Id, area = Constants.LocalArea });
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

        public static string MarkPostAsAnswer(this UrlHelper urlHelper, PostPart postPart) {
            return urlHelper.Action("MarkPostAsAnswer", "Post", new { contentId = postPart.Id, area = Constants.LocalArea });
        }

        public static string UnMarkPostAsAnswer(this UrlHelper urlHelper, PostPart postPart) {
            return urlHelper.Action("UnMarkPostAsAnswer", "Post", new { contentId = postPart.Id, area = Constants.LocalArea });
        }
 
    }
}