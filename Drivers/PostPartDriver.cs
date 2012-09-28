using System.Collections.Generic;
using JetBrains.Annotations;
using NGM.Forum.Models;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;
using Orchard.Core.Title.Models;
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
            var contentShapeResults = new List<ContentShapeResult>(new[] {
                ContentShape("Parts_Posts_Post_Manage",
                    () => shapeHelper.Parts_Posts_Post_Manage(ContentPart: postPart, IsClosed: postPart.ThreadPart.IsClosed)),
                ContentShape("Parts_Posts_Post_Metadata",
                    () => shapeHelper.Parts_Posts_Post_Metadata(ContentPart: postPart, CommonPart: postPart.As<ICommonPart>()))
            });


            if (postPart.IsParentThread())
                return Combined(contentShapeResults.ToArray());

            contentShapeResults.Add(ContentShape("Parts_Posts_Post_Title",
                             () => shapeHelper.Parts_Posts_Post_Title(ContentPart: postPart, CommonPart: postPart.As<ICommonPart>(), RoutePart: postPart.ThreadPart.As<TitlePart>())));

            return Combined(contentShapeResults.ToArray());
        }
    }
}