using System.Web.Routing;
using JetBrains.Annotations;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard;
using Orchard.Autoroute.Models;
using System;
using Orchard.Indexing.Services;
using Orchard.Indexing;
using Orchard.Logging;
using Orchard.Utility.Extensions;
using Orchard.Localization;

namespace NGM.Forum.Handlers {
    [UsedImplicitly]
    public class ForumsHomePagePartHandler : ContentHandler {

        private readonly IOrchardServices _orchardServices;
        private readonly IIndexingService _indexingService;
        private readonly IIndexManager _indexManager;
        public ILogger Logger { get; set; }
        public Localizer T { get; set; }

        public ForumsHomePagePartHandler(
            IRepository<ForumsHomePagePartRecord> repository,
            IOrchardServices orchardServices,
            IIndexingService indexingService,
            IIndexManager indexManager
            
            ) {
            _orchardServices = orchardServices;
            Filters.Add(StorageFilter.For(repository));
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;

            OnPublished<ForumsHomePagePart>((context, part) =>
            {
               VerifyUrlLength( part );            
            });
        }

        public void VerifyUrlLength (ForumsHomePagePart part){

             var forumSettings = _orchardServices.WorkContext.CurrentSite.As<ForumsSettingsPart>();
            var url = part.As<AutoroutePart>().DisplayAlias;
            if (forumSettings.ForumsHomeUrlMaximumLength > 0)
            {
                part.As<AutoroutePart>().DisplayAlias = url.Substring(0, Math.Min(forumSettings.ForumsHomeUrlMaximumLength, url.Length));
            }
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var forum = context.ContentItem.As<ForumsHomePagePart>();

            if (forum == null)
                return;

            /*
                context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                     {"Area", Constants.LocalArea},
                     {"Controller", "ForumsHomePage"},
                     {"Action", "Item"},
                     {"forumsHomePageId", context.ContentItem.Id}
                 };

     
                 context.Metadata.CreateRouteValues = new RouteValueDictionary {
                     {"Area", Constants.LocalArea},
                     {"Controller", "ForumAdmin"},
                     {"Action", "Create"}
                 };
                 context.Metadata.EditorRouteValues = new RouteValueDictionary {
                     {"Area", Constants.LocalArea},
                     {"Controller", "ForumAdmin"},
                     {"Action", "Edit"},
                     {"forumId", context.ContentItem.Id}
                 };
                 context.Metadata.RemoveRouteValues = new RouteValueDictionary {
                     {"Area", Constants.LocalArea},
                     {"Controller", "ForumAdmin"},
                     {"Action", "Remove"},
                     {"forumId", context.ContentItem.Id}
                 };
                 context.Metadata.AdminRouteValues = new RouteValueDictionary {
                     {"Area", Constants.LocalArea},
                     {"Controller", "ForumAdmin"},
                     {"Action", "Item"},
                     {"forumId", context.ContentItem.Id}
                 };
                  */
        }
    }
}