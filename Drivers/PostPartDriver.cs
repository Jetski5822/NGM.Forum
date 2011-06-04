using JetBrains.Annotations;
using NGM.Forum.Models;
using Orchard.ContentManagement.Drivers;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class PostPartDriver : ContentPartDriver<PostPart> {
        protected override DriverResult Display(PostPart postPart, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Posts_Post_Manage",
                             () => shapeHelper.Parts_Posts_Post_Manage(ContentPart: postPart))
                );
        }
    }
}