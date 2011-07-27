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
            var blog = context.ContentItem.As<ForumPart>();

            if (blog == null)
                return;

            //context.Metadata.EditorRouteValues = new RouteValueDictionary {
            //    {"Area", Constants.LocalArea},
            //    {"Controller", "ForumAdmin"},
            //    {"Action", "Edit"},
            //    {"forumId", context.ContentItem.Id}
            //};
            context.Metadata.AdminRouteValues = new RouteValueDictionary {
                {"Area", Constants.LocalArea},
                {"Controller", "ForumAdmin"},
                {"Action", "Item"},
                {"forumId", context.ContentItem.Id}
            };
        }
    }
}