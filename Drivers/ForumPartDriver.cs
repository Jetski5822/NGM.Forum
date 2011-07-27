using JetBrains.Annotations;
using NGM.Forum.Models;
using NGM.Forum.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class ForumPartDriver : ContentPartDriver<ForumPart> {
        private readonly IPostService _postService;

        private const string StatusTemplateName = "Parts.Status.ForumPart";

        public ForumPartDriver(IPostService postService) {
            _postService = postService;
        }

        protected override string Prefix {
            get { return "ForumPart"; }
        }

        protected override DriverResult Display(ForumPart forumPart, string displayType, dynamic shapeHelper) {
            var postCount = forumPart.PostCount >= 1 ? forumPart.PostCount - 1 : forumPart.PostCount;

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
                             () => shapeHelper.Parts_Forums_Forum_PostCount(ContentPart: forumPart, PostCount: postCount))
                );
        }

        protected override DriverResult Editor(ForumPart part, dynamic shapeHelper) {
            return
                Combined(
                    ContentShape("Parts_Status_Forum_Edit",
                                 () => shapeHelper.EditorTemplate(TemplateName: StatusTemplateName, Model: part, Prefix: Prefix)));
        }

        protected override DriverResult Editor(ForumPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}