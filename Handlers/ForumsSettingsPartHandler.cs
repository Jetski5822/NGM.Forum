using System.Web.Routing;
using JetBrains.Annotations;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;
using Orchard.Localization;

namespace NGM.Forum.Handlers {
    [UsedImplicitly]
    public class ForumsSettingsPartHandler : ContentHandler {

        protected override void Activated(ActivatedContentContext context)
        {
            base.Activated(context);
        }
        public ForumsSettingsPartHandler(IRepository<ForumsSettingsPartRecord> repository)
        {
            T = NullLocalizer.Instance;
            Filters.Add(new ActivatingFilter<ForumsSettingsPart>("Site"));
            Filters.Add(StorageFilter.For(repository));
        }

        public Localizer T { get; set; }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            if (context.ContentItem.ContentType != "Site")
                return;
            base.GetItemMetadata(context);
            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Forums")));
        }

    }
}