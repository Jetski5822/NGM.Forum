using NGM.Forum.Models;
using NGM.Moderation.Events;
using NGM.Moderation.Models;
using Orchard.ContentManagement;

namespace NGM.Forum.Events {
    public class PostModerationEventHandler : IModerationEventHandler {
        public void Approving(ApprovingContext context) {

        }

        public void Approved(ModerationPart moderationPart) {
            var postPart = moderationPart.As<PostPart>();

            if (postPart == null)
                return;

            if (!postPart.IsParentThread())
                return;

            postPart.ThreadPart.Moderation.Approved = postPart.Moderation.Approved;
            postPart.ThreadPart.Moderation.ApprovalUtc = postPart.Moderation.ApprovalUtc;
        }
    }
}