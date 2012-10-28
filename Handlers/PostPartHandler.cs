using System.Linq;
using System.Web.Routing;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using NGM.Forum.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Localization;
using Orchard.Security;
using Orchard.UI.Notify;

namespace NGM.Forum.Handlers {
    public class PostPartHandler : ContentHandler {
        private readonly IPostService _postService;
        private readonly IThreadService _threadService;
        private readonly IForumService _forumService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IOrchardServices _orchardServices;

        public PostPartHandler(IRepository<PostPartRecord> repository, 
            IPostService postService, 
            IThreadService threadService, 
            IForumService forumService,
            IAuthenticationService authenticationService,
            IOrchardServices orchardServices) {
            _postService = postService;
            _threadService = threadService;
            _forumService = forumService;
            _authenticationService = authenticationService;
            _orchardServices = orchardServices;

            T = NullLocalizer.Instance;

            Filters.Add(StorageFilter.For(repository));

            OnGetDisplayShape<PostPart>(SetModelProperties);
            OnGetEditorShape<PostPart>(SetModelProperties);
            OnUpdateEditorShape<PostPart>(SetModelProperties);

            OnCreated<PostPart>((context, part) => UpdatePostCount(part));
            OnPublished<PostPart>((context, part) => { 
                UpdatePostCount(part);
                PostNotifications(part);
                VerifyThreadUnicity(part);
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

        public Localizer T { get; set; }

        private void PostNotifications(PostPart part) {
            if (!part.Moderation.Approved)
                _orchardServices.Notifier.Information(T("Your post will be available once it has been approved by a moderator."));

        }

        private void VerifyThreadUnicity(PostPart part) {
            if (!part.IsParentThread())
                return;

            part.ThreadPart.Moderation.Approved = part.Moderation.Approved;
            part.ThreadPart.Moderation.ApprovalUtc = part.Moderation.ApprovalUtc;
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

                forumPart.ThreadCount = _threadService.Get(forumPart, VersionOptions.Published).Count();
                forumPart.PostCount = _threadService
                    .Get(forumPart, VersionOptions.Published)
                    .Sum(publishedThreadPart => _postService
                        .Get(publishedThreadPart, VersionOptions.Published)
                        .Count());
            }
        }
    }
}