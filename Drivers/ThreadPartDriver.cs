﻿using System;
using System.Collections.Generic;
using System.Xml;
using NGM.Forum.Models;
using NGM.Forum.Services;
using NGM.Forum.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Security;

namespace NGM.Forum.Drivers {
    public class ThreadPartDriver : ContentPartDriver<ThreadPart> {
        private readonly IPostService _postService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IContentManager _contentManager;
        private readonly IMembershipService _membershipService;

        public ThreadPartDriver(
            IPostService postService,
            IWorkContextAccessor workContextAccessor,
            IContentManager contentManager,
            IMembershipService membershipService) {
            _postService = postService;
            _workContextAccessor = workContextAccessor;
            _contentManager = contentManager;
            _membershipService = membershipService;
        }

        protected override string Prefix {
            get { return "ThreadPart"; }
        }

        protected override DriverResult Display(ThreadPart part, string displayType, dynamic shapeHelper) {
            var results = new List<DriverResult>();

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
                ContentShape("Parts_Thread_Manage", () => {
                    var newPost = _contentManager.New<PostPart>(part.FirstPost.ContentItem.ContentType);
                    newPost.ThreadPart = part;
                    return shapeHelper.Parts_Thread_Manage(FirstPost: part.FirstPost, NewPost: newPost);
                }),
                ContentShape("Forum_Metadata_First", () => shapeHelper.Forum_Metadata_First(Post: part.FirstPost)),
                ContentShape("Forum_Metadata_Latest", () => {
                        var post = part.LatestPost;
                        var pager = new ThreadPager(_workContextAccessor.GetContext().CurrentSite, part.PostCount);
                        return shapeHelper.Forum_Metadata_Latest(Post: post, Pager: pager);
                    }),
                ContentShape("Parts_Thread_Posts_Users", () => {
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

        protected override void Importing(ThreadPart part, ImportContentContext context) {
            var postCount = context.Attribute(part.PartDefinition.Name, "PostCount");
            if (postCount != null) {
                part.PostCount = Convert.ToInt32(postCount);
            }

            var isSticky = context.Attribute(part.PartDefinition.Name, "IsSticky");
            if (isSticky != null) {
                part.IsSticky = Convert.ToBoolean(isSticky);
            }

            var closedOnUtc = context.Attribute(part.PartDefinition.Name, "ClosedOnUtc");
            if (closedOnUtc != null) {
                part.ClosedOnUtc = XmlConvert.ToDateTime(closedOnUtc, XmlDateTimeSerializationMode.Utc);

                var closedBy = context.Attribute(part.PartDefinition.Name, "ClosedBy");
                if (closedBy != null) {
                    var contentIdentity = new ContentIdentity(closedBy);
                    part.ClosedBy = _membershipService.GetUser(contentIdentity.Get("User.UserName"));
                }

                var closedDescription = context.Attribute(part.PartDefinition.Name, "ClosedDescription");
                if (closedDescription != null) {
                    part.ClosedDescription = closedDescription;
                }
            }
        }

        protected override void Exporting(ThreadPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("PostCount", part.PostCount);
            context.Element(part.PartDefinition.Name).SetAttributeValue("IsSticky", part.IsSticky);

            if (part.ClosedOnUtc != null) {
                context.Element(part.PartDefinition.Name)
                    .SetAttributeValue("ClosedOnUtc", XmlConvert.ToString(part.ClosedOnUtc.Value, XmlDateTimeSerializationMode.Utc));

                if (part.ClosedBy != null) {
                    var closedByIdentity = _contentManager.GetItemMetadata(part.ClosedBy).Identity;
                    context.Element(part.PartDefinition.Name).SetAttributeValue("ClosedBy", closedByIdentity.ToString());
                }

                context.Element(part.PartDefinition.Name).SetAttributeValue("ClosedDescription", part.ClosedDescription);
            }
        }
    }
}