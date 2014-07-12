using JetBrains.Annotations;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using NGM.Forum.Models;
using NGM.Forum.Services;
using Orchard.ContentManagement;
using System.Collections.Generic;
using System.Linq;
using System.Web.Routing;
using NGM.Forum.Extensions;
using Orchard;
using Orchard.Autoroute.Models;
using System;

namespace NGM.Forum.Handlers
{
    [UsedImplicitly]
	
    public class ForumCategoryPartHandler : ContentHandler
    {

        private readonly IForumCategoryService _forumCategoryService;
        private readonly IForumService _forumService;
        private readonly IOrchardServices _orchardServices;

        public ForumCategoryPartHandler(
            IRepository<ForumCategoryPartRecord> repository,
            IForumCategoryService forumCategoryService,
            IForumService forumService,
            IOrchardServices orchardServices
        )
        {
            _forumCategoryService = forumCategoryService;
            _forumService = forumService;
            _orchardServices = orchardServices;
            Filters.Add(StorageFilter.For(repository));

            OnPublished<ForumCategoryPart>((context, part) =>
            {
                VerifyUrlLength(part);
            });

            OnRemoving<ForumCategoryPart>((context, part) =>
            {
                //   foreach(var response in contentManager.Query<CommentPart, CommentPartRecord>().Where(x => x.RepliedOn == comment.Id).List()) {
                //       contentManager.Remove(response.ContentItem);
                //  }
            });

            OnUpdating<ForumCategoryPart>((context, part) =>
            {
                int i = 0;
                i = i + 1;
            });


            OnUpdated<ForumCategoryPart>((context, part) =>
            {
                int i = 0;
                i = i + 1;
                //part.Forums = _forumCategoryService.GetForumsForCategory(part, VersionOptions.AllVersions).ToList();
            });

        }

        public void VerifyUrlLength(ForumCategoryPart part)
        {

            var forumSettings = _orchardServices.WorkContext.CurrentSite.As<ForumsSettingsPart>();
            var url = part.As<AutoroutePart>().DisplayAlias;

            if (forumSettings.CategoryTitleMaximumLength > 0)
            {
                part.As<AutoroutePart>().DisplayAlias = url.Substring(0, Math.Min(forumSettings.CategoryUrlMaximumLength, url.Length));
            }
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            var forumCategoryPart = context.ContentItem.As<ForumCategoryPart>();

            if (forumCategoryPart == null)
                return;

            /*
            context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                {"Area", Constants.LocalArea},
                {"Controller", "ForumCategory"},
                {"Action", "Item"},
                {"forumCategoryId", context.ContentItem.Id}
            };
            */

            context.Metadata.EditorRouteValues = new RouteValueDictionary {
                {"Area", Constants.LocalArea},
                {"Controller", "ForumAdmin"},
                {"Action", "EditForumCategory"},
                {"forumCategoryPartId", context.ContentItem.Id}
            };

        }
        
    }
}
