using System.Collections.Generic;
using System.Globalization;
using NGM.Forum.Extensions;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Localization;

namespace NGM.Forum.Settings {
    public class ForumPartSettings {
        public bool DefaultThreadedPosts { get; set; }
        public string PostType { get; set; }
    }

    public class ForumPartSettingsEvents : ContentDefinitionEditorEventsBase {

        public Localizer T { get; set; }

        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "ForumPart")
                yield break;

            var settings = definition.Settings.GetModel<ForumPartSettings>();

            if (string.IsNullOrWhiteSpace(settings.PostType)) {
                settings.PostType = Constants.Parts.Post;
            }

            yield return DefinitionTemplate(settings);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "ForumPart")
                yield break;

            var settings = new ForumPartSettings();

            if (updateModel.TryUpdateModel(settings, "ForumPartSettings", null, null)) {
                builder.WithSetting("ForumPartSettings.DefaultThreadedPosts", settings.DefaultThreadedPosts.ToString(CultureInfo.InvariantCulture));
                builder.WithSetting("ForumPartSettings.PostType", settings.PostType);
            }

            yield return DefinitionTemplate(settings);
        }
    }
}
