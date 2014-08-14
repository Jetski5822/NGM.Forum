using System;
using System.Collections.Generic;
using System.Xml;
using JetBrains.Annotations;
using NGM.Forum.Models;
using NGM.Forum.Services;
using NGM.Forum.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using Orchard.Security;
using Orchard.UI.Navigation;
using Orchard.Autoroute.Models;
using System.Web;
using Orchard.Core.Title.Models;


namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class ThreadPartDriver : ContentPartDriver<ThreadPart>
    {
        private readonly IPostService _postService;
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IContentManager _contentManager;
        private readonly IMembershipService _membershipService;
        private readonly IOrchardServices _orchardServices;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IThreadLastReadService _threadLastReadService;
        private readonly IUserTimeZoneService _userTimeZoneService;

        public ThreadPartDriver(
            IPostService postService,
            IWorkContextAccessor workContextAccessor,
            IContentManager contentManager,
            IMembershipService membershipService,
            IOrchardServices orchardServices,
            ISubscriptionService subscriptionService,
            IThreadLastReadService threadLastReadService,
            IUserTimeZoneService userTimeZoneService

        ) {
            _postService = postService;
            _workContextAccessor = workContextAccessor;
            _contentManager = contentManager;
            _membershipService = membershipService;
            _orchardServices = orchardServices;
            _subscriptionService = subscriptionService;
            _threadLastReadService = threadLastReadService;
            _userTimeZoneService = userTimeZoneService;
        }

        protected override string Prefix {
            get { return "ThreadPart"; }
        }


        protected override DriverResult Display(ThreadPart part, string displayType, dynamic shapeHelper)
        {

            var results = new List<DriverResult>();
            int? userId = null;
            if (_orchardServices.WorkContext.CurrentUser != null)
            {
                userId = _orchardServices.WorkContext.CurrentUser.Id;
            }

            _workContextAccessor.GetContext().CurrentTimeZone = _userTimeZoneService.GetUserTimeZoneInfo(userId);

            var forumsHomePage = _contentManager.Get(part.ForumsHomepageId, VersionOptions.Published, new QueryHints().ExpandParts<TitlePart, ForumsHomePagePart>());  //TOOO: is this going to be cached or looked up on each thread?

            //the read state may have already been looked up and set in a controller.. if not, set it
            //TODO: is this really needed?  is there a path that will reach this?
            if (userId != null && part.ReadState == ThreadPart.ReadStateEnum.NOT_SET)
            {
                int i = 1;
                //part.ReadState = _threadLastReadService.GetReadState(userId.Value, part);
            }
            else if (!userId.HasValue)
            {
                //if user is not logged in make a default
                part.ReadState = ThreadPart.ReadStateEnum.Unread;
            }

            if (displayType.Equals("Detail"))
            {
                //subscription logic
                bool isSubscribed = false;
                bool canSubscribe = _orchardServices.Authorizer.Authorize(Permissions.CreateThreadsAndPosts); ;
                if (_orchardServices.WorkContext.CurrentUser != null)
                {
                    isSubscribed = _subscriptionService.IsSubscribed(userId.Value, part.Id);
                }

                //end subscribe logic
                //is read logic
                if (userId != null)
                {
                    _threadLastReadService.MarkThreadAsRead(userId.Value, part.Id);
                }
                results.Add(ContentShape( "Parts_Thread_SubscribeButton", ()=>shapeHelper.Parts_Thread_SubscribeButton(IsSubscribed: isSubscribed, CanSubscribe: canSubscribe, ThreadId: part.Id)));

                //results.Add( ContentShape("Parts_Forum_Search", ()=> shapeHelper.Parts_foru(ForumsHomeId: forumsHomePagePart.Id); ;
               //TODO: is this optimal or high overhead -- only used in the detail view .. but ??
                var categoryPart = part.ForumPart.ForumCategoryPart;
                var forumPart = part.ForumPart;
               
                results.Add(ContentShape("Parts_BreadCrumb",
                    () => shapeHelper.Parts_BreadCrumb(ForumsHomePagePart: forumsHomePage.As<ForumsHomePagePart>(), ForumCategoryPart: categoryPart, ForumPart: forumPart, ThreadPart: null)
                ));

                if (userId != null)
                {
                    results.Add(ContentShape("Parts_ForumMenu", () => shapeHelper.Parts_ForumMenu(ForumsHomePagePart: forumsHomePage.As<ForumsHomePagePart>(), ShowRecent: true, ShowMarkAll: true, ReturnUrl: HttpContext.Current.Request.Url.AbsoluteUri)));
                }
            }

            if (displayType.Equals("SummaryAdmin", StringComparison.OrdinalIgnoreCase)) {
                results.Add(ContentShape("Parts_Threads_Thread_SummaryAdmin",
                    () => shapeHelper.Parts_Threads_Thread_SummaryAdmin()));
                results.Add(ContentShape("Parts_Threads_Thread_Metadata_SummaryAdmin", 
                    () => shapeHelper.Parts_Threads_Thread_Metadata_SummaryAdmin()));
            }

            if (displayType.Equals("Subscription", StringComparison.OrdinalIgnoreCase))
            {
                results.Add(ContentShape("Parts_Threads_Subscription", () => shapeHelper.Parts_Threads_Subscription(ThreadPart:part)));
            }
            if (part.IsClosed) {
                results.Add(ContentShape("Parts_Threads_Thread_Closed",
                        () => shapeHelper.Parts_Threads_Thread_Closed()));
            }

            results.AddRange(new [] { 
                ContentShape("Parts_Threads_Thread_ReadState",
                    () => shapeHelper.Parts_Threads_Thread_ReadState(ReadState: part.ReadState)),
                ContentShape("Parts_Threads_Thread_ThreadReplyCount",
                    () => shapeHelper.Parts_Threads_Thread_ThreadReplyCount(ReplyCount: part.ReplyCount)),
                ContentShape("Parts_Thread_Manage", () => {
                    var newPost = _contentManager.New<PostPart>(part.FirstPost.ContentItem.ContentType);                    
                    newPost.ThreadPart = part;
                    
                    return shapeHelper.Parts_Thread_Manage(FirstPost: part.FirstPost, NewPost: newPost, ForumsHomePagePart: forumsHomePage.As<ForumsHomePagePart>() );
                }),
                ContentShape("Forum_Metadata_First", () => shapeHelper.Forum_Metadata_First(Post: part.FirstPost)),
                ContentShape("Forum_Metadata_Latest", () => {
                        var post = part.LatestPost;
                        var site = _workContextAccessor.GetContext().CurrentSite;
                        var pager = new Pager(site, (int) Math.Ceiling((decimal) part.PostCount/(decimal) site.PageSize), site.PageSize);
                        //var pager = new Pager(_workContextAccessor.GetContext().CurrentSite, part.PostCount);
                        return shapeHelper.Forum_Metadata_Latest(Post: post, Pager: pager);
                    }),
                ContentShape("Parts_Thread_Posts_Users", () => {
                        var users = _postService.GetUsersPosted(part);
                        return shapeHelper.Parts_Thread_Posts_Users(Users: users);
                    })
            });

            return Combined(results.ToArray());
        }


        protected override DriverResult Editor(ThreadPart part, dynamic shapeHelper)
        {

                return ContentShape("Parts_Threads_Thread_Fields", () =>
                    shapeHelper.EditorTemplate(TemplateName: "Parts.Threads.Thread.Fields", Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(ThreadPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            updater.TryUpdateModel(part, Prefix, null, null);

            return Editor(part, shapeHelper);
        }

        protected override void Importing(ThreadPart part, ImportContentContext context)
        {
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

        protected override void Exporting(ThreadPart part, ExportContentContext context)
        {
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