using System.Linq;
using NGM.Forum.Models;
using NGM.Forum.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Services;

namespace NGM.Forum.Handlers {
    public class PostPartHandler : ContentHandler {
        private readonly IPostService _postService;
        private readonly IThreadService _threadService;
        private readonly IForumService _forumService;
        private readonly IClock _clock;

        public PostPartHandler(IRepository<PostPartRecord> repository, 
            IPostService postService, 
            IThreadService threadService, 
            IForumService forumService,
            IClock clock) {
            _postService = postService;
            _threadService = threadService;
            _forumService = forumService;
            _clock = clock;

            Filters.Add(StorageFilter.For(repository));

            OnGetDisplayShape<PostPart>(SetModelProperties);
            OnGetEditorShape<PostPart>(SetModelProperties);
            OnUpdateEditorShape<PostPart>(SetModelProperties);

            OnCreated<PostPart>((context, part) => UpdatePostCount(part));
            OnPublished<PostPart>((context, part) => { 
                UpdatePostCount(part);
                UpdateThreadVersioningDates(part);
            });
            OnUnpublished<PostPart>((context, part) => UpdatePostCount(part));
            OnVersioned<PostPart>((context, part, newVersionPart) => UpdatePostCount(newVersionPart));
            OnRemoved<PostPart>((context, part) => UpdatePostCount(part));

            OnRemoved<ThreadPart>((context, b) =>
                _postService
                    .Get(context.ContentItem.As<ThreadPart>())
                    .ToList()
                    .ForEach(post => context.ContentManager.Remove(post.ContentItem)));

            OnIndexing<PostPart>((context, postPart) => context.DocumentIndex
                                                    .Add("body", postPart.Record.Text).RemoveTags().Analyze()
                                                    .Add("format", postPart.Record.Format).Store());
        }

        private void UpdateThreadVersioningDates(PostPart postPart) {
            var utcNow = _clock.UtcNow;
            postPart.ThreadPart.As<ICommonPart>().ModifiedUtc = utcNow;
            postPart.ThreadPart.As<ICommonPart>().VersionModifiedUtc = utcNow;
        }

        private void SetModelProperties(BuildShapeContext context, PostPart postPart) {
            context.Shape.Thread = postPart.ThreadPart;
        }
        
        private void UpdatePostCount(PostPart postPart) {
            if (postPart.IsParentThread())
                return;

            UpdateThreadPartCounters(postPart);
        }

        private void UpdateThreadPartCounters(PostPart postPart) {
            var commonPart = postPart.As<CommonPart>();
            if (commonPart != null &&
                commonPart.Record.Container != null) {

                ThreadPart threadPart = postPart.ThreadPart ??
                                        _threadService.Get(commonPart.Record.Container.Id, VersionOptions.Published);

                threadPart.PostCount = _postService.Count(threadPart, VersionOptions.Published);

                UpdateForumPartCounters(threadPart);
            }
        }

        private void UpdateForumPartCounters(ThreadPart threadPart) {
            var commonPart = threadPart.As<CommonPart>();
            if (commonPart != null &&
                commonPart.Record.Container != null) {

                ForumPart forumPart = threadPart.ForumPart ??
                                      _forumService.Get(commonPart.Record.Container.Id, VersionOptions.Published);

                forumPart.ThreadCount = _threadService.Count(forumPart, VersionOptions.Published);
                forumPart.PostCount = _threadService
                    .Get(forumPart, VersionOptions.Published)
                    .Sum(publishedThreadPart => 
                        publishedThreadPart.PostCount);
            }
        }
    }
}