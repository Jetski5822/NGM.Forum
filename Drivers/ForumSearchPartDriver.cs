using JetBrains.Annotations;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using Orchard.UI.Notify;
using NGM.Forum.Models;
using NGM.Forum.Services;
using Orchard.Caching;

namespace NGM.Forum.Drivers
{
    [UsedImplicitly]
	
    public class ForumSearchPartDriver : ContentPartDriver<ForumSearchPart>
    {
        private readonly INotifier _notifier;
        private readonly ICurrentContentAccessor _currentContentAccessor;
        private readonly ICacheManager _cacheManager;
        
        private const string CACHE_KEY = "ForumPartMapping";

        private const string TemplateName = "Parts/ForumSearchPart";
       
        public Localizer T { get; set; }

        public ForumSearchPartDriver(
            INotifier notifier,
            ICurrentContentAccessor currentContentAccessor,
            ICacheManager cacheManager
             
        )
        {
            _notifier = notifier;
            _cacheManager = cacheManager;
            _currentContentAccessor = currentContentAccessor;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(ForumSearchPart part, string displayType, dynamic shapeHelper)
        {
            var contentItem = _currentContentAccessor.CurrentContentItem;
            int forumsHomePageId = 0;
            if ( contentItem.Is<ForumsHomePagePart>() ) {
                forumsHomePageId = contentItem.As<ForumsHomePagePart>().Id;
            }else if (contentItem.Is<ForumPart>() ){
                forumsHomePageId = contentItem.As<ForumPart>().ForumCategoryPart.ForumsHomePagePart.Id;
            } else if ( contentItem.Is<ThreadPart>() ) {
                forumsHomePageId = contentItem.As<ThreadPart>().ForumPart.ForumCategoryPart.ForumsHomePagePart.Id;
            } 
            /* will cache this lookup later
            var data = _cacheManager.Get(CACHE_KEY, ctx => {
                ctx.Monitor(_clock.When(TimeSpan.FromMinutes(cacheDuration)));
                return _service.GetPhotos(...);
            });
             */


            return ContentShape("Parts_Forum_Search",
                () => shapeHelper.Parts_Forum_Search(ForumsHomeId: forumsHomePageId, ContentItem: part.ContentItem));
        }

        protected override DriverResult Editor(ForumSearchPart part, dynamic shapeHelper)
        {
            return ContentShape("Parts_Forum_Search",
                    () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(ForumSearchPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            if (updater.TryUpdateModel(part, Prefix, null, null))
            {
                _notifier.Information(T("ForumSearchPart edited successfully"));
            }
            else
            {
                _notifier.Error(T("Error during ForumSearchPart update!"));
            }
            return Editor(part, shapeHelper);
        }

    }
}