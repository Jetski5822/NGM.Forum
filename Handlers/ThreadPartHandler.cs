using System.Linq;
using System.Web.Routing;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using NGM.Forum.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;

namespace NGM.Forum.Handlers {
    public class ThreadPartHandler : ContentHandler {
        private readonly IPostService _postService;
        private readonly IThreadService _threadService;
        private readonly IForumService _forumService;

        public ThreadPartHandler(IRepository<ThreadPartRecord> repository, 
            IPostService postService,
            IThreadService threadService, 
            IForumService forumService) {
            _postService = postService;
            _threadService = threadService;
            _forumService = forumService;

            Filters.Add(StorageFilter.For(repository));

            OnGetDisplayShape<ThreadPart>(SetModelProperties);

            OnGetEditorShape<ThreadPart>(SetModelProperties);
            OnUpdateEditorShape<ThreadPart>(SetModelProperties);

            OnCreated<ThreadPart>((context, part) => UpdateForumPartCounters(part));
            OnPublished<ThreadPart>((context, part) => UpdateForumPartCounters(part));
            OnUnpublished<ThreadPart>((context, part) => UpdateForumPartCounters(part));
            OnVersioned<ThreadPart>((context, part, newVersionPart) => UpdateForumPartCounters(newVersionPart));
            OnRemoved<ThreadPart>((context, part) => UpdateForumPartCounters(part));

            OnRemoved<ForumPart>((context, b) => 
                _threadService
                    .Get(context.ContentItem.As<ForumPart>())
                    .ToList()
                    .ForEach(thread => context.ContentManager.Remove(thread.ContentItem)));
        }

        private void SetModelProperties(BuildShapeContext context, ThreadPart threadPart) {
            context.Shape.Forum = threadPart.ForumPart;
        }

        private void UpdateForumPartCounters(ThreadPart threadPart) {
            CommonPart commonPart = threadPart.As<CommonPart>();
            if (commonPart != null &&
                commonPart.Record.Container != null) {

                ForumPart forumPart = threadPart.ForumPart ??
                                      _forumService.Get(commonPart.Record.Container.Id, VersionOptions.Published).As<ForumPart>();

                forumPart.ContentItem.ContentManager.Flush();

                forumPart.ThreadCount = _threadService.Get(forumPart, VersionOptions.Published).Count();
                forumPart.PostCount = _threadService
                    .Get(forumPart, VersionOptions.Published)
                    .Sum(publishedThreadPart => _postService
                        .Get(publishedThreadPart, VersionOptions.Published)
                        .Count());
            }
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var thread = context.ContentItem.As<ThreadPart>();

            if (thread == null)
                return;

            context.Metadata.AdminRouteValues = new RouteValueDictionary {
                {"Area", Constants.LocalArea},
                {"Controller", "ThreadAdmin"},
                {"Action", "Item"},
                {"threadId", context.ContentItem.Id}
            };
        }
    }
}