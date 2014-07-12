using System.Linq;
using NGM.Forum.Models;
using NGM.Forum.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Services;
using Orchard.Core.Title.Models;
using NGM.Forum.Settings;
using System.Web;

namespace NGM.Forum.Handlers {
    public class PostPartHandler : ContentHandler {
        private readonly IPostService _postService;
        private readonly IClock _clock;
        private readonly IReportPostService _reportPostService;
        private readonly ICountersService _countersService;
        private readonly ISubscriptionService _subscriptionService;

        public PostPartHandler(IRepository<PostPartRecord> repository, 
            IPostService postService, 
            IClock clock,
            IReportPostService reportPostService,
            ICountersService countersService,
            ISubscriptionService subscriptionService

            ) {
            _postService = postService;
            _clock = clock;
            _reportPostService = reportPostService;
            _countersService = countersService;
            _subscriptionService = subscriptionService;

            Filters.Add(StorageFilter.For(repository));

            OnGetDisplayShape<PostPart>(SetModelProperties);
            OnGetEditorShape<PostPart>(SetModelProperties);
            OnUpdateEditorShape<PostPart>(SetModelProperties);

            OnCreated<PostPart>((context, part) => _countersService.UpdateCounters(part));
            OnPublished<PostPart>((context, part) => {
                _countersService.UpdateCounters(part);
                UpdateThreadVersioningDates(part);
                SendNewPostNotification(part);
            });
            OnUnpublished<PostPart>((context, part) => _countersService.UpdateCounters(part));
            OnVersioned<PostPart>((context, part, newVersionPart) => _countersService.UpdateCounters(newVersionPart));
            OnRemoved<PostPart>((context, part) =>
            {
                _countersService.UpdateCounters(part); 

                                                    //going to leave the history record for historic purposes
                                                    // RemoveReports(part); 
                                                    });

            OnRemoved<ThreadPart>((context, b) =>
                _postService.Delete(context.ContentItem.As<ThreadPart>()));

            OnIndexing<PostPart>((context, postPart) => context.DocumentIndex        
                                                    .Add("body", postPart.Record.Text).RemoveTags().Analyze()
                                                    .Add("format", postPart.Record.Format).Store()
                                                    .Add("forumsHomeId", postPart.ThreadPart.ForumPart.ForumCategoryPart.ForumsHomePagePart.Id)
                                                    );

            OnIndexing<ThreadPart>((context, threadPart) => context.DocumentIndex.Add("Title", threadPart.As<TitlePart>().Title ));
          
        }

        private void SendNewPostNotification(PostPart postPart)
        {
            _subscriptionService.SendEmailNotificationToSubscribers(postPart);
        }

        private void UpdateThreadVersioningDates(PostPart postPart) {
            var utcNow = _clock.UtcNow;
            postPart.ThreadPart.As<ICommonPart>().ModifiedUtc = utcNow;
            postPart.ThreadPart.As<ICommonPart>().VersionModifiedUtc = utcNow;
        }

        private void SetModelProperties(BuildShapeContext context, PostPart postPart) {
            context.Shape.Thread = postPart.ThreadPart;
            if (context.Shape.Metadata.DisplayType != null)
            {
                if (context.Shape.Metadata.DisplayType == "Editor")
                {
                    context.Shape.EditorFlavor = GetFlavor(postPart);
                    context.Shape.ReturnUrl = HttpContext.Current.Request.UrlReferrer.AbsoluteUri;
                }
            }
        }
        private static string GetFlavor(PostPart part)
        {
            var typePartSettings = part.Settings.GetModel<PostTypePartSettings>();
            return (typePartSettings != null && !string.IsNullOrWhiteSpace(typePartSettings.Flavor))
                       ? typePartSettings.Flavor
                       : part.PartDefinition.Settings.GetModel<PostPartSettings>().FlavorDefault;
        }
        private void RemoveReports(PostPart postPart)
        {
            //If a post is deleted from the system remove its related 'inappropriate reports'
            //Once the post is gone from the system, the report can no longer be substantiated, so keeping it for historic purposes serves no purpose.
            var reportIds = _reportPostService.Get().Where( report=>report.PostId == postPart.Id ).Select( r=>r.Id).ToList();
            _reportPostService.DeleteReports(reportIds);

        }


    }
}