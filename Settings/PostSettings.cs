using System.Collections.Generic;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.Core.Common.Settings;

namespace NGM.Forum.Settings {
    public class PostPartSettings {
        public const string FlavorDefaultDefault = "html";
        private string _flavorDefault;
        public string FlavorDefault {
            get { return !string.IsNullOrWhiteSpace(_flavorDefault)
                           ? _flavorDefault
                           : FlavorDefaultDefault; }
            set { _flavorDefault = value; }
        }
    }

    public class PostTypePartSettings {
        public string Flavor { get; set; }
    }

    public class PostSettingsHooks : ContentDefinitionEditorEventsBase {
        public override IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition) {
            if (definition.PartDefinition.Name != "PostPart")
                yield break;

            var model = definition.Settings.GetModel<PostTypePartSettings>();

            if (string.IsNullOrWhiteSpace(model.Flavor)) {
                var partModel = definition.PartDefinition.Settings.GetModel<PostPartSettings>();
                model.Flavor = partModel.FlavorDefault;
            }

            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> PartEditor(ContentPartDefinition definition) {
            if (definition.Name != "PostPart")
                yield break;

            var model = definition.Settings.GetModel<PostPartSettings>();
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "PostPart")
                yield break;

            var model = new BodyTypePartSettings();
            updateModel.TryUpdateModel(model, "PostTypePartSettings", null, null);
            builder.WithSetting("PostTypePartSettings.Flavor", !string.IsNullOrWhiteSpace(model.Flavor) ? model.Flavor : null);
            yield return DefinitionTemplate(model);
        }

        public override IEnumerable<TemplateViewModel> PartEditorUpdate(ContentPartDefinitionBuilder builder, IUpdateModel updateModel) {
            if (builder.Name != "PostPart")
                yield break;

            var model = new BodyPartSettings();
            updateModel.TryUpdateModel(model, "PostPartSettings", null, null);
            builder.WithSetting("PostPartSettings.FlavorDefault", !string.IsNullOrWhiteSpace(model.FlavorDefault) ? model.FlavorDefault : null);
            yield return DefinitionTemplate(model);
        }
    }
}
