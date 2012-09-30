using System.Collections.Generic;
using System.Linq;
using System.Web;
using JetBrains.Annotations;
using NGM.Forum.Models;
using NGM.Forum.Services;
using NGM.Forum.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Services;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class ThreadPartDriver : ContentPartDriver<ThreadPart> {
        private readonly IPostService _postService;
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;

        public ThreadPartDriver(
            IPostService postService,
            IEnumerable<IHtmlFilter> htmlFilters) {
            _postService = postService;
            _htmlFilters = htmlFilters;
        }

        protected override string Prefix {
            get { return "ThreadPart"; }
        }

        protected override DriverResult Display(ThreadPart part, string displayType, dynamic shapeHelper) {
            var firstPost = _postService.GetFirstPost(part, VersionOptions.Published);
            //var latestPost = _postService.GetLatestPost(part, VersionOptions.Published);

            return Combined(
                ContentShape("Parts_Threads_Thread_SummaryAdmin",
                    () => shapeHelper.Parts_Threads_Thread_SummaryAdmin()),
                ContentShape("Parts_Threads_Thread_ThreadReplyCount",
                    () => shapeHelper.Parts_Threads_Thread_ThreadReplyCount(ReplyCount: part.ReplyCount)),
                ContentShape("Parts_Thread_Manage",
                    () => shapeHelper.Parts_Thread_Manage(ContentPart: firstPost))
                //ContentShape("Parts_Threads_Thread_FirstPostSummary",
                //    () => shapeHelper.Parts_Threads_Post_Summary(ContentPart: firstPost)),
                //ContentShape("Parts_Threads_Thread_LatestPostSummary",
                //    () => shapeHelper.Parts_Threads_Post_Summary(ContentPart: latestPost))
                );
        }
    }
}