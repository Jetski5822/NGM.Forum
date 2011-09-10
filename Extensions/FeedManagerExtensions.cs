using System.Web.Routing;
using NGM.Forum.Models;
using Orchard.Core.Feeds;

namespace NGM.Forum.Extensions {
    public static class FeedManagerExtensions {
        public static void Register(this IFeedManager feedManager, ForumPart forumPart) {
            feedManager.Register(forumPart.Title, "rss", new RouteValueDictionary { { "containerid", forumPart.Id } });
        }

        public static void Register(this IFeedManager feedManager, ThreadPart threadPart) {
            feedManager.Register(threadPart.Title, "rss", new RouteValueDictionary { { "containerid", threadPart.Id } });
        }
    }
}
