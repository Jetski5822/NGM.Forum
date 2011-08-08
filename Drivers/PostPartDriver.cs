using System.Collections.Generic;
using JetBrains.Annotations;
using NGM.Forum.Models;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Routable.Models;
using Orchard.ContentManagement;
using Orchard.Security;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class PostPartDriver : ContentPartDriver<PostPart> {
        private readonly IAuthenticationService _authenticationService;
        private readonly IAuthorizationService _authorizationService;

        public PostPartDriver(IAuthenticationService authenticationService,
            IAuthorizationService authorizationService) {
            _authenticationService = authenticationService;
            _authorizationService = authorizationService;
        }

        protected override string Prefix {
            get { return "PostPart"; }
        }

        protected override DriverResult Display(PostPart postPart, string displayType, dynamic shapeHelper) {
            bool canMarkAsAnswer = false;
            if (postPart.ThreadPart.Type == ThreadType.Question) {
                if (_authorizationService.TryCheckAccess(Permissions.MarkPostAsAnswer, _authenticationService.GetAuthenticatedUser(), postPart))
                    canMarkAsAnswer = true;
            }
            
            var contentShapeResults = new List<ContentShapeResult>(new[] {
                ContentShape("Parts_Posts_Post_Manage",
                    () => shapeHelper.Parts_Posts_Post_Manage(ContentPart: postPart, IsClosed: postPart.ThreadPart.IsClosed, CanMarkAsAnswer: canMarkAsAnswer)),
                ContentShape("Parts_Posts_Post_Metadata",
                    () => shapeHelper.Parts_Posts_Post_Metadata(ContentPart: postPart, CommonPart: postPart.As<ICommonPart>()))
            });


            if (postPart.IsParentThread())
                return Combined(contentShapeResults.ToArray());

            contentShapeResults.Add(ContentShape("Parts_Posts_Post_Title",
                             () => shapeHelper.Parts_Posts_Post_Title(ContentPart: postPart, CommonPart: postPart.As<ICommonPart>(), RoutePart: postPart.ThreadPart.As<RoutePart>())));

            return Combined(contentShapeResults.ToArray());
        }
    }
}