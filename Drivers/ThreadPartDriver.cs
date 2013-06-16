using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NGM.Forum.Models;
using NGM.Forum.Services;
using NGM.Forum.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class ThreadPartDriver : ContentPartDriver<ThreadPart> {
        private readonly IPostService _postService;
        private readonly IOrchardServices _orchardServices;

        public ThreadPartDriver(
            IPostService postService,
            IOrchardServices orchardServices) {
            _postService = postService;
            _orchardServices = orchardServices;
        }

        protected override string Prefix {
            get { return "ThreadPart"; }
        }

        protected override DriverResult Display(ThreadPart part, string displayType, dynamic shapeHelper) {
            List<DriverResult> results = new List<DriverResult>();

            if (displayType.Equals("SummaryAdmin", StringComparison.OrdinalIgnoreCase)) {
                results.Add(ContentShape("Parts_Threads_Thread_SummaryAdmin",
                    () => shapeHelper.Parts_Threads_Thread_SummaryAdmin()));
                results.Add(ContentShape("Parts_Threads_Thread_Metadata_SummaryAdmin", 
                    () => shapeHelper.Parts_Threads_Thread_Metadata_SummaryAdmin()));
            }

            if (part.IsClosed) {
                results.Add(ContentShape("Parts_Threads_Thread_Closed",
                        () => shapeHelper.Parts_Threads_Thread_Closed()));
            }

            

            results.AddRange(new [] { 
                ContentShape("Parts_Threads_Thread_ThreadReplyCount",
                    () => shapeHelper.Parts_Threads_Thread_ThreadReplyCount(ReplyCount: part.ReplyCount)),
                ContentShape("Parts_Thread_Manage", () =>
                    {
                        var firstPost = _postService.GetFirstPost(part, VersionOptions.Published);
                        return shapeHelper.Parts_Thread_Manage(Post: firstPost);
                    }),
                ContentShape("Forum_Metadata_First", () => 
                    {
                        var firstPost = _postService.GetFirstPost(part, VersionOptions.Published);
                        return shapeHelper.Forum_Metadata_First(Post: firstPost);
                    }),
                ContentShape("Forum_Metadata_Latest", () =>
                    {
                        var post = _postService.GetLatestPost(part, VersionOptions.Published);

                        var pager = new ThreadPager(_orchardServices.WorkContext.CurrentSite, part.PostCount);
                        return shapeHelper.Forum_Metadata_Latest(Post: post, Pager: pager);
                    }),
                ContentShape("Parts_Thread_Posts_Users", () =>
                    {
                        var users = _postService.GetUsersPosted(part);

                        return shapeHelper.Parts_Thread_Posts_Users(Users: users);
                    })
            });

            return Combined(results.ToArray());
        }

        protected override DriverResult Editor(ThreadPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Threads_Thread_Fields", () => 
                shapeHelper.EditorTemplate(TemplateName: "Parts.Threads.Thread.Fields", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(ThreadPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);

            return Editor(part, shapeHelper);
        }
    }
}