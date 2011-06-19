using System.Linq;
using System.Web.Routing;
using JetBrains.Annotations;
using NGM.Forum.Models;
using NGM.Forum.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;

namespace NGM.Forum.Handlers {
    [UsedImplicitly]
    public class PostPartHandler : ContentHandler {
        private readonly IPostService _postService;
        private readonly IThreadService _threadService;
        private readonly IForumService _forumService;

        public PostPartHandler(IRepository<PostPartRecord> repository, IPostService postService, IThreadService threadService, IForumService forumService, RequestContext requestContext) {
            _postService = postService;
            _threadService = threadService;
            _forumService = forumService;

            Filters.Add(StorageFilter.For(repository));

            OnGetDisplayShape<PostPart>(SetModelProperties);
            OnGetEditorShape<PostPart>(SetModelProperties);
            OnUpdateEditorShape<PostPart>(SetModelProperties);

            OnCreated<PostPart>((context, part) => UpdatePostCount(part));
            OnPublished<PostPart>((context, part) => UpdatePostCount(part));
            OnUnpublished<PostPart>((context, part) => UpdatePostCount(part));
            OnVersioned<PostPart>((context, part, newVersionPart) => UpdatePostCount(newVersionPart));
            OnRemoved<PostPart>((context, part) => UpdatePostCount(part));
        }

        private static void SetModelProperties(BuildShapeContext context, PostPart postPart) {
            context.Shape.Thread = postPart.ThreadPart;
        }

        private void UpdatePostCount(PostPart postPart) {
            UpdateThreadPartCounters(postPart);
        }

        private void UpdateThreadPartCounters(PostPart postPart) {
            CommonPart commonPart = postPart.As<CommonPart>();
            if (commonPart != null &&
                commonPart.Record.Container != null) {

                ThreadPart threadPart = postPart.ThreadPart ??
                                        _threadService.Get(commonPart.Record.Container.Id, VersionOptions.Published).As<ThreadPart>();

                threadPart.ContentItem.ContentManager.Flush();
                threadPart.PostCount = _postService.Get(threadPart, VersionOptions.Published).Count();

                UpdateForumPartCounters(threadPart);
            }
        }

        private void UpdateForumPartCounters(ThreadPart threadPart) {
            CommonPart commonPart = threadPart.As<CommonPart>();
            if (commonPart != null &&
                commonPart.Record.Container != null) {

                ForumPart forumPart = threadPart.ForumPart ??
                                      _forumService.Get(commonPart.Record.Container.Id, VersionOptions.Published).As<ForumPart>();

                forumPart.ContentItem.ContentManager.Flush();
                forumPart.PostCount = _threadService.Get(forumPart, VersionOptions.Published).Count();
            }
        }
    }
}