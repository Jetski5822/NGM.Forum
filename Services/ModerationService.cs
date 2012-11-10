using System.Collections.Generic;
using NGM.Forum.Events;
using NGM.Forum.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Services;

namespace NGM.Forum.Services {
    public interface IModerationService : IDependency {
        void Approve(ModerationPart moderationPart, bool approved);
        IEnumerable<ContentItem> Get(ModerationOptions moderationOptions);
    }

    public class ModerationService : IModerationService {
        private readonly IEnumerable<IModerationEventHandler> _moderationEventHandlers;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;

        public ModerationService(IEnumerable<IModerationEventHandler> moderationEventHandlers, 
            IClock clock,
            IContentManager contentManager) {
            _moderationEventHandlers = moderationEventHandlers;
            _clock = clock;
            _contentManager = contentManager;
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

        public IEnumerable<ContentItem> Get(ModerationOptions moderationOptions) {
            var query = _contentManager.Query(VersionOptions.AllVersions);

            if (!Equals(moderationOptions, ModerationOptions.All)) {
                query = query.Join<ModerationPartRecord>().Where(trd => trd.Approved == moderationOptions.IsApproved && trd.ApprovalUtc == null);
            }

            return query.List();
        }
    }
}