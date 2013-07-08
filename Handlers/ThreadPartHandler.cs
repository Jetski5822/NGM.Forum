using System.Linq;
using System.Web.Routing;
using JetBrains.Annotations;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using NGM.Forum.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Security;

namespace NGM.Forum.Handlers {
    [UsedImplicitly]
    public class ThreadPartHandler : ContentHandler {
        private readonly IPostService _postService;
        private readonly IThreadService _threadService;
        private readonly IForumService _forumService;
        private readonly IContentManager _contentManager;

        public ThreadPartHandler(IRepository<ThreadPartRecord> repository, 
            IPostService postService,
            IThreadService threadService,
            IForumService forumService,
            IContentManager contentManager) {
            _postService = postService;
            _threadService = threadService;
            _forumService = forumService;
            _contentManager = contentManager;

            Filters.Add(StorageFilter.For(repository));

            OnGetDisplayShape<ThreadPart>(SetModelProperties);
            OnGetEditorShape<ThreadPart>(SetModelProperties);
            OnUpdateEditorShape<ThreadPart>(SetModelProperties);

            OnActivated<ThreadPart>(PropertyHandlers);
            OnLoading<ThreadPart>((context, part) => LazyLoadHandlers(part));
            OnCreated<ThreadPart>((context, part) => UpdateForumPartCounters(part));
            OnPublished<ThreadPart>((context, part) => UpdateForumPartCounters(part));
            OnUnpublished<ThreadPart>((context, part) => UpdateForumPartCounters(part));
            OnVersioning<ThreadPart>((context, part, newVersionPart) => LazyLoadHandlers(newVersionPart));
            OnVersioned<ThreadPart>((context, part, newVersionPart) => UpdateForumPartCounters(newVersionPart));
            OnRemoved<ThreadPart>((context, part) => UpdateForumPartCounters(part));
            
            OnRemoved<ForumPart>((context, b) =>
                _threadService.Delete(context.ContentItem.As<ForumPart>()));
        }

        private void SetModelProperties(BuildShapeContext context, ThreadPart threadPart) {
            context.Shape.Forum = threadPart.ForumPart;
            context.Shape.StickyClass = threadPart.IsSticky ? "Sticky" : string.Empty;
        }

        private void UpdateForumPartCounters(ThreadPart threadPart) {
            var commonPart = threadPart.As<CommonPart>();
            if (commonPart != null && commonPart.Record.Container != null) {

                var forumPart = threadPart.ForumPart ?? _forumService.Get(commonPart.Record.Container.Id, VersionOptions.Published);

                // TODO: Refactor this to do the count in the DB and not make 3 DB calls.
                forumPart.ThreadCount = _threadService.Count(forumPart, VersionOptions.Published);
                forumPart.PostCount = _threadService
                    .Get(forumPart, VersionOptions.Published)
                    .Sum(publishedThreadPart => _postService
                        .Count(publishedThreadPart, VersionOptions.Published));
            }
        }

        protected void LazyLoadHandlers(ThreadPart part) {
            // Add handlers that will load content for id's just-in-time
            part.ClosedByField.Loader(() => _contentManager.Get<IUser>(part.Record.ClosedById));
        }

        protected void PropertyHandlers(ActivatedContentContext context, ThreadPart part) {
            // Add handlers that will update records when part properties are set

            part.ClosedByField.Setter(user => {
                part.Record.ClosedById = user == null
                    ? 0
                    : user.ContentItem.Id;
                return user;
            });

            // Force call to setter if we had already set a value
            if (part.ClosedByField.Value != null)
                part.ClosedByField.Value = part.ClosedByField.Value;

            // Setup FirstPost & LatestPost fields
            part.FirstPostField.Loader(() => _postService.GetPositional(part, ThreadPostPositional.First));
            part.LatestPostField.Loader(() => _postService.GetPositional(part, ThreadPostPositional.Latest));
        }

        protected override void GetItemMetadata(GetContentItemMetadataContext context) {
            var thread = context.ContentItem.As<ThreadPart>();

            if (thread == null)
                return;

            context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                {"Area", Constants.LocalArea},
                {"Controller", "Thread"},
                {"Action", "Item"},
                {"forumId", thread.ForumPart.ContentItem.Id},
                {"threadId", context.ContentItem.Id}
            };
            context.Metadata.AdminRouteValues = new RouteValueDictionary {
                {"Area", Constants.LocalArea},
                {"Controller", "ThreadAdmin"},
                {"Action", "Item"},
                {"threadId", context.ContentItem.Id}
            };
        }
    }
}