using JetBrains.Annotations;
using NGM.Forum.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Security;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class ThreadPartDriver : ContentPartDriver<ThreadPart> {
        private const string RoutableTemplateName = "Parts.Routable.ThreadPart";
        private const string StatusTemplateName = "Parts.Status.ThreadPart";

        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;

        public ThreadPartDriver(
            IAuthenticationService authenticationService,
            IAuthorizationService authorizationService) {
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
        }

        protected override string Prefix {
            get { return "ThreadPart"; }
        }

        protected override DriverResult Display(ThreadPart threadPart, string displayType, dynamic shapeHelper) {
            var postCount = threadPart.PostCount >= 1 ? threadPart.PostCount - 1 : threadPart.PostCount;

            if (threadPart.ForumPart.IsClosed) {
                return Combined(
                    ContentShape("Parts_Threads_Thread_Status",
                                 () => shapeHelper.Parts_Threads_Thread_Status(ContentPart: threadPart)),
                    ContentShape("Parts_Threads_Thread_PostCount",
                                 () => shapeHelper.Parts_Threads_Thread_PostCount(ContentPart: threadPart, PostCount: postCount))
                    );
            }
            
            return Combined(
                ContentShape("Parts_Threads_Thread_Status",
                    () => shapeHelper.Parts_Threads_Thread_Status(ContentPart: threadPart)),
                ContentShape("Parts_Threads_Thread_Manage",
                    () => shapeHelper.Parts_Threads_Thread_Manage(ContentPart: threadPart)),
                ContentShape("Parts_Threads_Thread_PostCount",
                    () => shapeHelper.Parts_Threads_Thread_PostCount(ContentPart: threadPart, PostCount: postCount))
                );
        }

        protected override DriverResult Editor(ThreadPart threadPart, dynamic shapeHelper) {
            if (!_authorizationService.TryCheckAccess(Permissions.ManageStickyThread, _authenticationService.GetAuthenticatedUser(), threadPart))
                return ContentShape("Parts_Routable_Thread_Edit",
                    () => shapeHelper.EditorTemplate(TemplateName: RoutableTemplateName, Model: threadPart, Prefix: Prefix));

            return
                Combined(
                    ContentShape("Parts_Status_Thread_Edit",
                                 () => shapeHelper.EditorTemplate(TemplateName: StatusTemplateName, Model: threadPart, Prefix: Prefix)),
                    ContentShape("Parts_Routable_Thread_Edit",
                                 () => shapeHelper.EditorTemplate(TemplateName: RoutableTemplateName, Model: threadPart, Prefix: Prefix)));
        }

        protected override DriverResult Editor(ThreadPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}