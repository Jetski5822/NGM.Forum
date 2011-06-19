using JetBrains.Annotations;
using NGM.Forum.Models;
using Orchard.ContentManagement.Drivers;
using Orchard.Core.Routable.Models;
using Orchard.ContentManagement;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class PostPartDriver : ContentPartDriver<PostPart> {
        protected override string Prefix {
            get { return "PostPart"; }
        }

        protected override DriverResult Display(PostPart postPart, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Posts_Post_Title",
                             () => shapeHelper.Parts_Posts_Post_Title(RoutePart: postPart.ThreadPart.As<RoutePart>())),
                ContentShape("Parts_Posts_Post_Manage",
                             () => shapeHelper.Parts_Posts_Post_Manage(ContentPart: postPart))
                );
        }
    }
}