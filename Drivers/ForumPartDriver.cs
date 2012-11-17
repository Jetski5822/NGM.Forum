using System.Collections.Generic;
using JetBrains.Annotations;
using NGM.Forum.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class ForumPartDriver : ContentPartDriver<ForumPart> {
        protected override string Prefix {
            get { return "ForumPart"; }
        }

        protected override DriverResult Display(ForumPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Forums_Forum_Manage",
                    () => shapeHelper.Parts_Forums_Forum_Manage()),
                ContentShape("Parts_Forums_Forum_Description",
                    () => shapeHelper.Parts_Forums_Forum_Description(Description: part.Description)),
                ContentShape("Parts_Forums_Forum_SummaryAdmin",
                    () => shapeHelper.Parts_Forums_Forum_SummaryAdmin()),
                ContentShape("Parts_Forums_Forum_ForumReplyCount",
                    () => shapeHelper.Parts_Forums_Forum_ForumReplyCount(ReplyCount: part.ReplyCount)),
                ContentShape("Parts_Forums_Forum_ForumThreadCount",
                    () => shapeHelper.Parts_Forums_Forum_ForumThreadCount(ThreadCount: part.ThreadCount)),
                ContentShape("Parts_Forum_Manage",
                    () => shapeHelper.Parts_Forum_Manage())
                );
        }

        protected override DriverResult Editor(ForumPart forumPart, dynamic shapeHelper) {
            var results = new List<DriverResult> {
                ContentShape("Parts_Forums_Forum_Fields",
                             () => shapeHelper.EditorTemplate(TemplateName: "Parts.Forums.Forum.Fields", Model: forumPart, Prefix: Prefix))
            };


            if (forumPart.Id > 0)
                results.Add(ContentShape("Forum_DeleteButton",
                    deleteButton => deleteButton));

            return Combined(results.ToArray());
        }

        protected override DriverResult Editor(ForumPart forumPart, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(forumPart, Prefix, null, null);
            return Editor(forumPart, shapeHelper);
        }
    }
}