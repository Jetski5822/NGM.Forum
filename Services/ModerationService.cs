using System.Collections.Generic;
using NGM.Forum.Events;
using NGM.Forum.Models;
using Orchard;
using Orchard.Services;

namespace NGM.Forum.Services {
    public interface IModerationService : IDependency {
        void Approve(ModerationPart moderationPart, bool approved);
    }

    public class ModerationService : IModerationService {
        private readonly IEnumerable<IModerationEventHandler> _moderationEventHandlers;
        private readonly IClock _clock;

        public ModerationService(IEnumerable<IModerationEventHandler> moderationEventHandlers, IClock clock) {
            _moderationEventHandlers = moderationEventHandlers;
            _clock = clock;
        }

        public void Approve(ModerationPart moderationPart, bool approved) {
            var context = new ApprovingContext{Approved = approved, ModerationPart = moderationPart};
            foreach (var moderationEventHandler in _moderationEventHandlers) {
                moderationEventHandler.Approving(context);
            }

            moderationPart.Approved = approved;
            moderationPart.ApprovalUtc = _clock.UtcNow;

            foreach (var moderationEventHandler in _moderationEventHandlers) {
                moderationEventHandler.Approved(moderationPart);
            }
        }
    }
}