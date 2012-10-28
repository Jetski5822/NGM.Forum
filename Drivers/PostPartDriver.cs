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
using Orchard.Services;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class PostPartDriver : ContentPartDriver<PostPart> {
        private readonly IEnumerable<IHtmlFilter> _htmlFilters;
        private readonly RequestContext _requestContext;

        private const string TemplateName = "Parts.Threads.Post.Body";

        public PostPartDriver(IOrchardServices services, 
            IEnumerable<IHtmlFilter> htmlFilters, 
            RequestContext requestContext) {
            _htmlFilters = htmlFilters;
            Services = services;
            _requestContext = requestContext;
        }

        public IOrchardServices Services { get; set; }

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
                                 var bodyText = _htmlFilters.Aggregate(part.Text, (text, filter) => filter.ProcessContent(text, GetFlavor(part)));
                                 return shapeHelper.Parts_Threads_Post_Body_Summary(Html: new HtmlString(bodyText));
                             }),
                ContentShape("Parts_Post_Manage", () => 
                    shapeHelper.Parts_Post_Manage(ContentPart: part)),
                ContentShape("Parts_Thread_Post_Metadata_SummaryAdmin", () =>
                    shapeHelper.Parts_Thread_Post_Metadata_SummaryAdmin(ContentPart: part))
                );
        }

        protected override DriverResult Editor(PostPart part, dynamic shapeHelper) {
            var model = BuildEditorViewModel(part, _requestContext);
            return ContentShape("Parts_Threads_Post_Body_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
        }

        protected override DriverResult Editor(PostPart part, IUpdateModel updater, dynamic shapeHelper) {
            var model = BuildEditorViewModel(part, _requestContext);
            updater.TryUpdateModel(model, Prefix, null, null);

            return ContentShape("Parts_Threads_Post_Body_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: model, Prefix: Prefix));
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
    }
}