using System.Web.Mvc;
using NGM.Forum.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Routable.Services;

namespace NGM.Forum.Extensions {
    public static class UrlHelperExtensions {
        public static string ForumForAdmin(this UrlHelper urlHelper, ForumPart forumPart) {
            return urlHelper.Action("Item", "ForumAdmin", new { forumId = forumPart.Id, area = "NGM.Forum" });
        }

        public static string ForumCreate(this UrlHelper urlHelper) {
            return urlHelper.Action("Create", "ForumAdmin", new { area = "NGM.Forum" });
        }

        public static string ViewThread(this UrlHelper urlHelper, ThreadPart threadPart) {
            return urlHelper.Action("Item", "Thread", new { forumPath = threadPart.ForumPart.As<IRoutableAspect>().Path, threadSlug = threadPart.As<IRoutableAspect>().GetEffectiveSlug(), area = "NGM.Forum" });
        }

        public static string CreateThread(this UrlHelper urlHelper, ForumPart forumPart) {
            return urlHelper.Action("Create", "Thread", new { forumId = forumPart.Id, area = "NGM.Forum" });
        }

        public static string ReplyPost(this UrlHelper urlHelper, IContent content) {
            return urlHelper.Action("Create", "Post", new { contentId  = content.Id, area = "NGM.Forum" });
        }
    }
}