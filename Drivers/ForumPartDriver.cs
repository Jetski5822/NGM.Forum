using JetBrains.Annotations;
using NGM.Forum.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class ForumPartDriver : ContentPartDriver<ForumPart> {
        private const string OpenClosedTemplateName = "Parts.OpenClosed.ForumPart";

        protected override DriverResult Display(ForumPart forumPart, string displayType, dynamic shapeHelper) {
            var postCount = forumPart.PostCount >= 1 ? forumPart.PostCount - 1 : forumPart.PostCount;

            return Combined(
                ContentShape("Parts_Forums_Forum_Status",
                             () => shapeHelper.Parts_Forums_Forum_Status(ContentPart: forumPart)),
                ContentShape("Parts_Forums_Forum_Manage",
                             () => shapeHelper.Parts_Forums_Forum_Manage(ContentPart: forumPart)),
                ContentShape("Parts_Forums_Forum_ThreadCount",
                             () => shapeHelper.Parts_Forums_Forum_ThreadCount(ContentPart: forumPart, ThreadCount: forumPart.ThreadCount)),
                ContentShape("Parts_Forums_Forum_PostCount",
                             () => shapeHelper.Parts_Forums_Forum_PostCount(ContentPart: forumPart, PostCount: postCount))
                );
        }

        protected override DriverResult Editor(ForumPart part, dynamic shapeHelper) {
            return
                Combined(
                    ContentShape("Parts_OpenClosed_Forum_Edit",
                                 () => shapeHelper.EditorTemplate(TemplateName: OpenClosedTemplateName, Model: part, Prefix: Prefix)));
        }

        protected override DriverResult Editor(ForumPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}