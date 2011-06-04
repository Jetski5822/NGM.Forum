using JetBrains.Annotations;
using NGM.Forum.Models;
using Orchard.ContentManagement.Drivers;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class ForumPartDriver : ContentPartDriver<ForumPart> {
        protected override DriverResult Display(ForumPart forumPart, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Forums_Forum_Manage",
                             () => shapeHelper.Parts_Forums_Forum_Manage(ContentPart: forumPart)),
                ContentShape("Parts_Forums_Forum_ThreadCount",
                             () => shapeHelper.Parts_Forums_Forum_ThreadCount(ContentPart: forumPart, ThreadCount: forumPart.ThreadCount)),
                ContentShape("Parts_Forums_Forum_PostCount",
                             () => shapeHelper.Parts_Forums_Forum_PostCount(ContentPart: forumPart, PostCount: forumPart.PostCount))
                );
        }
    }
}