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

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class PostPartDriver : ContentPartDriver<PostPart> {
        private readonly IOrchardServices _orchardServices;
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;
        private readonly RequestContext _requestContext;

        private const string TemplateName = "Parts.Threads.Post.Body";

        public PostPartDriver(IOrchardServices orchardServices, 
            IEnumerable<IHtmlFilter> htmlFilters, 
            RequestContext requestContext) {
            _orchardServices = orchardServices;
            _htmlFilters = htmlFilters;
            _requestContext = requestContext;
        }

        protected override string Prefix {
            get { return "PostPart"; }
        }

        protected override DriverResult Display(PostPart part, string displayType, dynamic shapeHelper) {
            var pager = new ThreadPager(_orchardServices.WorkContext.CurrentSite, part.ThreadPart.PostCount);

            return Combined(
                ContentShape("Parts_Threads_Post_Body",
                             () => {
                                 var bodyText = _htmlFilters.Where(x => x.GetType().Name.Equals(GetFlavor(part) + "filter", StringComparison.OrdinalIgnoreCase)).Aggregate(part.Text, (text, filter) => filter.ProcessContent(text));
                                 return shapeHelper.Parts_Threads_Post_Body(Html: new HtmlString(bodyText));
                             }),
                ContentShape("Parts_Threads_Post_Body_Summary",
                             () => {
                                 var bodyText = _htmlFilters.Where(x => x.GetType().Name.Equals(GetFlavor(part) + "filter", StringComparison.OrdinalIgnoreCase)).Aggregate(part.Text, (text, filter) => filter.ProcessContent(text));
                                 return shapeHelper.Parts_Threads_Post_Body_Summary(Html: new HtmlString(bodyText), Pager: pager);
                             }),
                ContentShape("Parts_Post_Manage", () => 
                    shapeHelper.Parts_Post_Manage(ContentPart: part)),
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
                part.RepliedOn = Convert.ToInt32(repliedOn);
            }

            var text = context.Attribute(part.PartDefinition.Name, "Text");
            if (text != null) {
                part.Text = text;
            }
        }

        protected override void Exporting(PostPart part, ExportContentContext context) {
            context.Element(part.PartDefinition.Name).SetAttributeValue("Format", part.Format);

            if (part.RepliedOn != null) {
                context.Element(part.PartDefinition.Name).SetAttributeValue("RepliedOn", part.RepliedOn);
            }

            context.Element(part.PartDefinition.Name).SetAttributeValue("Text", part.Text);
        }
    }
}