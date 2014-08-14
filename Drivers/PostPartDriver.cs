using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using JetBrains.Annotations;
using NGM.Forum.Models;
using NGM.Forum.Settings;
using NGM.Forum.ViewModels;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Services;
using Html.Helpers;
using Orchard.UI.Navigation;
using NGM.Forum.Services;
using Orchard.Core.Common.Models;
using System.Threading;
using Orchard.Localization;

namespace NGM.Forum.Drivers {

    [UsedImplicitly]
    public class PostPartDriver : ContentPartDriver<PostPart> {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;
        private readonly RequestContext _requestContext;
        private readonly IContentManager _contentManager;
        private readonly IOrchardServices _orchardServices;
        private readonly IPostEditHistoryService _postEditHistoryService;
        private readonly IUserTimeZoneService _userTimeZoneService;


        private const string TemplateName = "Parts.Threads.Post.Body";

        public PostPartDriver(IWorkContextAccessor workContextAccessor, 
            IEnumerable<IHtmlFilter> htmlFilters, 
            RequestContext requestContext,
            IContentManager contentManager,
            IOrchardServices orchardServices,
            IPostEditHistoryService postEditHistoryService,
            IUserTimeZoneService userTimeZoneService
            ) {
            _workContextAccessor = workContextAccessor;
            _htmlFilters = htmlFilters;
            _requestContext = requestContext;
            _contentManager = contentManager;
            _orchardServices = orchardServices;
            _postEditHistoryService = postEditHistoryService;
            _userTimeZoneService = userTimeZoneService;
            T = NullLocalizer.Instance;

        }
        public Localizer T { get; set; }
        protected override string Prefix {
            get { return "PostPart"; }
        }

        protected override DriverResult Display(PostPart part, string displayType, dynamic shapeHelper) {
            
            int? userId = null;
            if (_orchardServices.WorkContext.CurrentUser != null)
            {
                userId = _orchardServices.WorkContext.CurrentUser.Id;
            }

            _workContextAccessor.GetContext().CurrentTimeZone = _userTimeZoneService.GetUserTimeZoneInfo(userId);

            var results = new List<DriverResult>();

            if ( displayType.ToLowerInvariant().Equals("newpostpreview")){
                results.Add(ContentShape("Parts_Post_ViewInThread", () => shapeHelper.Parts_Post_ViewInThread(ContentItem: part)));
            }


        //    if (displayType.ToLowerInvariant().Equals("searchresult") )
      //      {
                results.Add(ContentShape("Parts_Threads_Post_Body_Summary", () =>
                {
                    var site = _workContextAccessor.GetContext().CurrentSite;
                    var pager = new Pager(site, (int)Math.Ceiling((decimal)part.ThreadPart.PostCount / (decimal)site.PageSize), site.PageSize);
                    //var pager = new ThreadPager(_workContextAccessor.GetContext().CurrentSite, part.ThreadPart.PostCount);
                    var bodyText = _htmlFilters.Aggregate(part.Text, (text, filter) => filter.ProcessContent(text, GetFlavor(part)));
                    return shapeHelper.Parts_Threads_Post_Body_Summary(Html: new HtmlString(bodyText), Pager: pager);
                }));

                results.Add(ContentShape("Parts_Thread_Post_Metadata_SummaryAdmin", () =>
                        shapeHelper.Parts_Thread_Post_Metadata_SummaryAdmin(ContentPart: part))
                );
        //    }
         //   else
        //    {

                if (userId != null)
                {
                    results.Add(ContentShape("Parts_Thread_Post_ReportPost", () =>
                    {
                        //TODO: is this going to be a cached look up for all posts, or gets called constantly?                        
                        var forumsHomePage = _orchardServices.ContentManager.Get(part.ThreadPart.ForumsHomepageId, VersionOptions.Published);
                        var canManageForums = _orchardServices.Authorizer.Authorize(Permissions.ManageForums, forumsHomePage);
                        return shapeHelper.Parts_Thread_Post_ReportPost(ReportedById: userId, CanManageForums: canManageForums);
                    }));
                }

                results.Add(ContentShape("Parts_Threads_Post_Body", () =>
                {
                    var bodyText = _htmlFilters.Aggregate(part.Text, (text, filter) => filter.ProcessContent(text, GetFlavor(part)));
                    var body = new HtmlString(bodyText.ToString().Replace(Environment.NewLine, "<p>" + Environment.NewLine + "</p>"));
                    var saneBody = HtmlSanitizer.sanitizer(body.ToString()).html;
                    return shapeHelper.Parts_Threads_Post_Body(Html: saneBody);
                }));

                results.Add(ContentShape("Parts_Post_Manage", () =>
                {
                    var newPost = _contentManager.New<PostPart>(part.ContentItem.ContentType);
                    newPost.ThreadPart = part.ThreadPart;
                    return shapeHelper.Parts_Post_Manage(ContentPart: part, NewPost: newPost, UserId: userId);
                }));
                results.Add(ContentShape("Parts_Thread_Post_LastEdited", () =>
                        shapeHelper.Parts_Thread_Post_LastEdited(LastEdited: part.LastEdited)
                ));

                if (part.LastEdited != null && _orchardServices.Authorizer.Authorize(Permissions.ManageForums))
                {
                    results.Add(ContentShape("Parts_Thread_Post_ViewEditHistory", () =>
                            shapeHelper.Parts_Thread_Post_ViewEditHistory(ContentPart: part)
                    ));
                }

                results.Add(ContentShape("Parts_Thread_Post_Metadata_SummaryAdmin", () =>
                        shapeHelper.Parts_Thread_Post_Metadata_SummaryAdmin(ContentPart: part))
                );
        //    }

            return Combined(results.ToArray());

        }

        protected override DriverResult Editor(PostPart part, dynamic shapeHelper) {
            var model = BuildEditorViewModel(part, _requestContext);
            return Combined(ContentShape("Parts_Threads_Post_Body_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix)));
        }

        protected override DriverResult Editor(PostPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = BuildEditorViewModel(part, _requestContext);
            var text = part.Text; //save original text before updating it
            var userId = part.As<CommonPart>().Owner.Id; //TODO: is this going to be from the last person editing? or has it already changed


            if (updater.TryUpdateModel(model, Prefix, null, null)) {
                if (String.IsNullOrWhiteSpace(model.Text))
                {
                    updater.AddModelError("Text", T("The post cannot be empty."));
                }
                else
                {
                    part.Format = model.EditorFlavor;
                    //when first created there is no last edited date
                    if (model.PostPart.LastEdited != null)
                    {
                        var editRecord = new PostEditHistoryRecord { PostId = model.PostPart.Id, EditDate = DateTime.UtcNow, Text = text, UserId = userId, Format = model.PostPart.Format };
                        _postEditHistoryService.SaveEdit(editRecord);
                    }
                }
            }

            //this is where the input text from the user's post is saved
            return Combined(ContentShape("Parts_Threads_Post_Body_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix)));
        }

        private static PostBodyEditorViewModel BuildEditorViewModel(PostPart part, RequestContext requestContext) {
            return new PostBodyEditorViewModel {
                PostPart = part,
                EditorFlavor = GetFlavor(part),             
            };
        }

        private static string GetFlavor(PostPart part) {
            var typePartSettings = part.Settings.GetModel<PostTypePartSettings>();
            return (typePartSettings != null && !string.IsNullOrWhiteSpace(typePartSettings.Flavor))
                       ? typePartSettings.Flavor
                       : part.PartDefinition.Settings.GetModel<PostPartSettings>().FlavorDefault;
        }

        protected override void Importing(PostPart part, ImportContentContext context) {
            var format = context.Attribute(part.PartDefinition.Name, "Format");
            if (format != null) {
                part.Format = format;
            }

            var repliedOn = context.Attribute(part.PartDefinition.Name, "RepliedOn");
            if (repliedOn != null) {
                part.RepliedOn = context.GetItemFromSession(repliedOn).Id;
            }

            var text = context.Attribute(part.PartDefinition.Name, "Text");
            if (text != null) {
                part.Text = text;
            }
        }

        protected override void Exporting(PostPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Format", part.Format);

            if (part.RepliedOn != null) {
                var repliedOnIdentity = _contentManager.GetItemMetadata(_contentManager.Get(part.RepliedOn.Value)).Identity;
                context.Element(part.PartDefinition.Name).SetAttributeValue("RepliedOn", repliedOnIdentity.ToString());
            }

            context.Element(part.PartDefinition.Name).SetAttributeValue("Text", part.Text);
        }


    }
}