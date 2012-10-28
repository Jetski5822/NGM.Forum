using NGM.Forum.Models;
using NGM.Forum.Services;
using Orchard.ContentManagement;

namespace NGM.Forum.Events {
    public class ThreadModerationEventHandler : IModerationEventHandler {
        private readonly IPostService _postService;

        public ThreadModerationEventHandler(IPostService postService) {
            _postService = postService;
        }

        public void Approving(ApprovingContext context) {
            
        }

        public void Approved(ModerationPart moderationPart) {
            var threadPart = moderationPart.As<ThreadPart>();

            if (threadPart == null)
                return;

            var post = _postService.GetFirstPost(threadPart, VersionOptions.AllVersions, ModerationOptions.All);
            post.Moderation.Approved = threadPart.Moderation.Approved;
            post.Moderation.ApprovalUtc = threadPart.Moderation.ApprovalUtc;
        }
    }
}