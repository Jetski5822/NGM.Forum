using NGM.Forum.Models;
using NGM.Forum.Services;
using NGM.Moderation.Events;
using NGM.Moderation.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;

namespace NGM.Forum.Events {
    public class ThreadModerationEventHandler : IModerationEventHandler {
        private readonly IPostService _postService;

        public ThreadModerationEventHandler(IPostService postService) {
            _postService = postService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Approving(ApprovingContext context) {
            
        }

        public void Approved(ModerationPart moderationPart) {
            var threadPart = moderationPart.As<ThreadPart>();

            if (threadPart == null)
                return;

            var post = _postService.GetFirstPost(threadPart, VersionOptions.AllVersions, ModerationOptions.All);

            if (!post.IsParentThread())
                throw new OrchardException(T("There was an error getting the Parent Post attached to the Thread."));

            post.Moderation.Approved = threadPart.Moderation.Approved;
            post.Moderation.ApprovalUtc = threadPart.Moderation.ApprovalUtc;
        }
    }
}