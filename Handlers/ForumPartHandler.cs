using System.Web.Routing;
using JetBrains.Annotations;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace NGM.Forum.Handlers {
    [UsedImplicitly]
    public class ForumPartHandler : ContentHandler {
        public ForumPartHandler(IRepository<ForumPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var forum = context.ContentItem.As<ForumPart>();

            if (forum == null)
                return;

            context.Metadata.AdminRouteValues = new RouteValueDictionary {
                {"Area", Constants.LocalArea},
                {"Controller", "ThreadAdmin"},
                {"Action", "List"},
                {"forumId", context.ContentItem.Id}
            };
        }
    }
}