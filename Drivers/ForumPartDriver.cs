using JetBrains.Annotations;
using NGM.Forum.Models;
using NGM.Forum.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Security;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class ForumPartDriver : ContentPartDriver<ForumPart> {
        private const string StatusCloseTemplateName = "Parts.Status.Close.ForumPart";

        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IPostService _postService;

        public ForumPartDriver(
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService,
            IPostService postService) {
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _postService = postService;
        }

        protected override string Prefix {
            get { return "ForumPart"; }
        }

        protected override DriverResult Display(ForumPart forumPart, string displayType, dynamic shapeHelper) {
            var latestPostPart = _postService.GetLatestPost(forumPart, VersionOptions.Published);
            
            return Combined(
                ContentShape("Parts_Forums_Forum_Status",
                             () => shapeHelper.Parts_Forums_Forum_Status(ContentPart: forumPart)),
                ContentShape("Parts_Forums_Forum_Manage",
                             () => shapeHelper.Parts_Forums_Forum_Manage(ContentPart: forumPart)),
                ContentShape("Parts_Forums_Forum_LatestPost",
                             () => shapeHelper.Parts_Forums_Forum_LatestPost(ContentPart: latestPostPart)),
                ContentShape("Parts_Forums_Forum_ThreadCount",
                             () => shapeHelper.Parts_Forums_Forum_ThreadCount(ContentPart: forumPart, ThreadCount: forumPart.ThreadCount)),
                ContentShape("Parts_Forums_Forum_PostCount",
                             () => shapeHelper.Parts_Forums_Forum_PostCount(ContentPart: forumPart, PostCount: forumPart.ReplyCount))
                );
        }

        protected override DriverResult Editor(ForumPart part, dynamic shapeHelper) {
            if (!_authorizationService.TryCheckAccess(Permissions.ManageForums, _authenticationService.GetAuthenticatedUser(), part))
                return null;

            return
                Combined(ContentShape("Parts_Status_Close_Forum_Edit",
                                 () => shapeHelper.EditorTemplate(TemplateName: StatusCloseTemplateName, Model: part, Prefix: Prefix)));
        }

        protected override DriverResult Editor(ForumPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}