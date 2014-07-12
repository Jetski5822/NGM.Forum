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

        }

        protected override string Prefix {
            get { return "PostPart"; }
        }

        protected override DriverResult Display(PostPart part, string displayType, dynamic shapeHelper) {
            
            //need a timezone info that is 'c
            if (displayType == "Editor")
            {
                var view = BuildEditorViewModel(part, this._requestContext);
                return Combined(ContentShape("Post_Body_Editor",
                    () => shapeHelper.Post_Body_Editor(ViewModel: view)));
            }
            else
            {
                int? userId = null;
                if (_orchardServices.WorkContext.CurrentUser != null)
                {
                    userId = _orchardServices.WorkContext.CurrentUser.Id;
                }

                _workContextAccessor.GetContext().CurrentTimeZone = _userTimeZoneService.GetUserTimeZoneInfo(userId);

                var results = new List<DriverResult>();

                if (userId != null)  //if user is authenticated all reporting posts ??TODO: review permission here
                {
                    results.Add(ContentShape("Parts_Thread_Post_ReportPost", () =>
                    {
                        return shapeHelper.Parts_Thread_Post_ReportPost(ReportedById: userId);
                    }));
                }

                results.Add(ContentShape("Parts_Threads_Post_Body", () =>
                {
                    var bodyText = _htmlFilters.Aggregate(part.Text, (text, filter) => filter.ProcessContent(text, GetFlavor(part)));
                    var body = new HtmlString(bodyText.ToString().Replace(Environment.NewLine, "<p>" + Environment.NewLine + "</p>"));
                    var saneBody = HtmlSanitizer.sanitizer(body.ToString()).html;
                    return shapeHelper.Parts_Threads_Post_Body(Html: saneBody);
                }));

                results.Add(ContentShape("Parts_Threads_Post_Body_Summary", () =>
                {
                    var site = _workContextAccessor.GetContext().CurrentSite;
                    var pager = new Pager(site, (int)Math.Ceiling((decimal)part.ThreadPart.PostCount / (decimal)site.PageSize), site.PageSize);
                    //var pager = new ThreadPager(_workContextAccessor.GetContext().CurrentSite, part.ThreadPart.PostCount);
                    var bodyText = _htmlFilters.Aggregate(part.Text, (text, filter) => filter.ProcessContent(text, GetFlavor(part)));
                    return shapeHelper.Parts_Threads_Post_Body_Summary(Html: new HtmlString(bodyText), Pager: pager);
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

                return Combined(results.ToArray());
            }

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
                part.Format = model.EditorFlavor;
                //when first created there is no last edited date
                if (model.PostPart.LastEdited != null)
                {
                    var editRecord = new PostEditHistoryRecord { PostId = model.PostPart.Id, EditDate = DateTime.UtcNow, Text = text, UserId = userId, Format = model.PostPart.Format };
                    _postEditHistoryService.SaveEdit(editRecord);
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