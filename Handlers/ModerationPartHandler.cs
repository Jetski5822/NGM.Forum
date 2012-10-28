using System;
using System.Linq;
using System.Web.Routing;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using NGM.Forum.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Core.Common.Models;
using Orchard.Data;
using Orchard.Security;
using Orchard.Services;

namespace NGM.Forum.Handlers {
    public class ModerationPartHandler : ContentHandler {
        private readonly IAuthenticationService _authenticationService;
        private readonly IClock _clock;

        public ModerationPartHandler(IRepository<ModerationPartRecord> repository,
            IAuthenticationService authenticationService,
            IClock clock) {
            _authenticationService = authenticationService;
            _clock = clock;

            OnCreating<ModerationPart>(SetInitialProperties);

            Filters.Add(StorageFilter.For(repository));

        }

        private void SetInitialProperties(CreateContentContext context, ModerationPart part) {
            var requiresModeration = _authenticationService.GetAuthenticatedUser().As<UserForumPart>().RequiresModeration;

            if (!requiresModeration) {
                part.Approved = true;
                part.ApprovalUtc = _clock.UtcNow;
            }
        }
    }
}