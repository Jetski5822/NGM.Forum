using System.Collections.Generic;
using JetBrains.Annotations;
using NGM.Forum.Models;
using NGM.Forum.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Security;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class ThreadPartDriver : ContentPartDriver<ThreadPart> {
        private const string RoutableTemplateName = "Parts.Routable.ThreadPart";
        private const string StatusCloseTemplateName = "Parts.Status.Close.ThreadPart";
        private const string StatusStickyTemplateName = "Parts.Status.Sticky.ThreadPart";
        private const string TypeTemplateName = "Parts.Type.ThreadPart";

        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IPostService _postService;

        public ThreadPartDriver(
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService,
            IPostService postService) {
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
            _postService = postService;
        }

        protected override string Prefix {
            get { return "ThreadPart"; }
        }

        protected override DriverResult Display(ThreadPart threadPart, string displayType, dynamic shapeHelper) {
            var postCount = threadPart.PostCount >= 1 ? threadPart.PostCount - 1 : threadPart.PostCount;

            var latestPostPart = _postService.GetLatestPost(threadPart, VersionOptions.Published);

            List<ContentShapeResult> contentShapeResults = new List<ContentShapeResult>(new[] {
                ContentShape("Parts_Threads_Thread_Status",
                             () => shapeHelper.Parts_Threads_Thread_Status(ContentPart: threadPart)),
                ContentShape("Parts_Threads_Thread_LatestPost",
                             () => shapeHelper.Parts_Threads_Thread_LatestPost(ContentPart: latestPostPart)),
                ContentShape("Parts_Threads_Thread_PostCount",
                             () => shapeHelper.Parts_Threads_Thread_PostCount(ContentPart: threadPart, PostCount: postCount))
            });

            return Combined(contentShapeResults.ToArray());
        }

        protected override DriverResult Editor(ThreadPart threadPart, dynamic shapeHelper) {
            List<ContentShapeResult> contentShapeResults = new List<ContentShapeResult>(new[] {
                ContentShape("Parts_Type_Thread_Edit",
                             () => shapeHelper.EditorTemplate(TemplateName: TypeTemplateName, Model: threadPart, Prefix: Prefix)),
                ContentShape("Parts_Routable_Thread_Edit",
                             () => shapeHelper.EditorTemplate(TemplateName: RoutableTemplateName, Model: threadPart, Prefix: Prefix))
            });

            if (_authorizationService.TryCheckAccess(Permissions.ManageOpenCloseThread, _authenticationService.GetAuthenticatedUser(), threadPart)) {
                contentShapeResults.Add(ContentShape("Parts_Status_Close_Thread_Edit",
                                                     () => shapeHelper.EditorTemplate(TemplateName: StatusCloseTemplateName, Model: threadPart, Prefix: Prefix)));
            }

            if (_authorizationService.TryCheckAccess(Permissions.ManageStickyThread, _authenticationService.GetAuthenticatedUser(), threadPart)) {
                contentShapeResults.Add(ContentShape("Parts_Status_Sticky_Thread_Edit",
                                                     () => shapeHelper.EditorTemplate(TemplateName: StatusStickyTemplateName, Model: threadPart, Prefix: Prefix)));                
            }

            return Combined(contentShapeResults.ToArray());
        }

        protected override DriverResult Editor(ThreadPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}