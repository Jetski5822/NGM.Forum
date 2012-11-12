using JetBrains.Annotations;
using NGM.Forum.Models;
using NGM.Forum.Services;
using NGM.Moderation.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class ThreadPartDriver : ContentPartDriver<ThreadPart> {
        private readonly IPostService _postService;

        public ThreadPartDriver(
            IPostService postService) {
            _postService = postService;
        }

        protected override string Prefix {
            get { return "ThreadPart"; }
        }

        protected override DriverResult Display(ThreadPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Threads_Thread_SummaryAdmin",
                    () => shapeHelper.Parts_Threads_Thread_SummaryAdmin()),
                ContentShape("Parts_Threads_Thread_ThreadReplyCount",
                    () => shapeHelper.Parts_Threads_Thread_ThreadReplyCount(ReplyCount: part.ReplyCount)),
                ContentShape("Parts_Thread_Manage", () => {
                        var post = _postService.GetFirstPost(part, VersionOptions.Published, ModerationOptions.Approved);
                        return shapeHelper.Parts_Thread_Manage(ContentPart: post);
                    }),
                //ContentShape("Parts_Threads_Thread_FirstPostSummary",
                //    () => shapeHelper.Parts_Threads_Post_Summary(ContentPart: firstPost)),
                ContentShape("Forum_Metadata_Latest", () => {
                        var post = _postService.GetLatestPost(part, VersionOptions.Published, ModerationOptions.Approved);
                        return shapeHelper.Forum_Metadata_Latest(ContentPart: post);
                    }),
                ContentShape("Parts_Threads_Thread_Metadata_SummaryAdmin", () => 
                    shapeHelper.Parts_Threads_Thread_Metadata_SummaryAdmin(ContentPart: part))
                );
        }

        protected override DriverResult Editor(ThreadPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Threads_Thread_Fields", () => 
                shapeHelper.EditorTemplate(TemplateName: "Parts.Threads.Thread.Fields", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(ThreadPart part, IUpdateModel updater, dynamic shapeHelper) {
            if (updater.TryUpdateModel(part, Prefix, null, null)) {
            }

            return Editor(part, shapeHelper);
        }
    }
}