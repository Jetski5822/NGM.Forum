using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NGM.Forum.Models;
using NGM.Forum.Services;
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
            List<DriverResult> results = new List<DriverResult>();

            if (displayType.Equals("SummaryAdmin", StringComparison.OrdinalIgnoreCase)) {
                results.Add(ContentShape("Parts_Threads_Thread_SummaryAdmin",
                    () => shapeHelper.Parts_Threads_Thread_SummaryAdmin()));
                results.Add(ContentShape("Parts_Threads_Thread_Metadata_SummaryAdmin", 
                    () => shapeHelper.Parts_Threads_Thread_Metadata_SummaryAdmin(ContentPart: part)));
            }

            if (part.IsClosed) {
                results.Add(ContentShape("Parts_Threads_Thread_Closed",
                        () => shapeHelper.Parts_Threads_Thread_Closed(ContentPart: part)));
            }

            var firstPost = _postService.GetFirstPost(part, VersionOptions.Published);

            results.AddRange(new [] { 
                ContentShape("Parts_Threads_Thread_ThreadReplyCount",
                    () => shapeHelper.Parts_Threads_Thread_ThreadReplyCount(ReplyCount: part.ReplyCount)),
                ContentShape("Parts_Thread_Manage", 
                    () => shapeHelper.Parts_Thread_Manage(ContentPart: firstPost)),
                ContentShape("Forum_Metadata_First", 
                    () => shapeHelper.Forum_Metadata_First(ContentPart: firstPost)),
                ContentShape("Forum_Metadata_Latest", 
                    () => {
                        var post = _postService.GetLatestPost(part, VersionOptions.Published);
                        return shapeHelper.Forum_Metadata_Latest(ContentPart: post);
                    })});

            return Combined(results.ToArray());
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