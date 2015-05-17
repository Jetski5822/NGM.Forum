using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using NGM.Forum.Models;
using NGM.Forum.Settings;
using NGM.Forum.ViewModels;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Services;

namespace NGM.Forum.Drivers {
    public class PostPartDriver : ContentPartDriver<PostPart> {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;
        private readonly RequestContext _requestContext;
        private readonly IContentManager _contentManager;

        private const string TemplateName = "Parts.Threads.Post.Body";

        public PostPartDriver(IWorkContextAccessor workContextAccessor, 
            IEnumerable<IHtmlFilter> htmlFilters, 
            RequestContext requestContext,
            IContentManager contentManager) {
            _workContextAccessor = workContextAccessor;
            _htmlFilters = htmlFilters;
            _requestContext = requestContext;
            _contentManager = contentManager;
        }

        protected override string Prefix {
            get { return "PostPart"; }
        }

        protected override DriverResult Display(PostPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Threads_Post_Body",
                             () => {
                                 var bodyText = _htmlFilters.Aggregate(part.Text, (text, filter) => filter.ProcessContent(text, GetFlavor(part)));
                                 return shapeHelper.Parts_Threads_Post_Body(Html: new HtmlString(bodyText));
                             }),
                ContentShape("Parts_Threads_Post_Body_Summary",
                             () => {
                                 var pager = new ThreadPager(_workContextAccessor.GetContext().CurrentSite, part.ThreadPart.PostCount);
                                 var bodyText = _htmlFilters.Aggregate(part.Text, (text, filter) => filter.ProcessContent(text, GetFlavor(part)));
                                 return shapeHelper.Parts_Threads_Post_Body_Summary(Html: new HtmlString(bodyText), Pager: pager);
                             }),
                ContentShape("Parts_Post_Manage", () => {
                    var newPost = _contentManager.New<PostPart>(part.ContentItem.ContentType);
                    newPost.ThreadPart = part.ThreadPart;
                    return shapeHelper.Parts_Post_Manage(ContentPart: part, NewPost: newPost);
                }),
                ContentShape("Parts_Thread_Post_Metadata_SummaryAdmin", () =>
                    shapeHelper.Parts_Thread_Post_Metadata_SummaryAdmin(ContentPart: part))
                );
        }

        protected override DriverResult Editor(PostPart part, dynamic shapeHelper) {
            var model = BuildEditorViewModel(part, _requestContext);
            return Combined(ContentShape("Parts_Threads_Post_Body_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix)));
        }

        protected override DriverResult Editor(PostPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = BuildEditorViewModel(part, _requestContext);
            updater.TryUpdateModel(model, Prefix, null, null);

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