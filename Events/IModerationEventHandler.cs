using NGM.Forum.Models;
using Orchard.Events;

namespace NGM.Forum.Events {
    public interface IModerationEventHandler : IEventHandler {
        void Approving(ApprovingContext context);
        void Approved(ModerationPart moderationPart);
    }
}