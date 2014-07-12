using System;

using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Localization;
using NGM.Forum.Models;


namespace NGM.Forum.Drivers {

    public class ForumsSettingsPartDriver : ContentPartDriver<ForumsSettingsPart> {
        public ForumsSettingsPartDriver() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        protected override string Prefix { get { return "ForumsSettings"; } }

        protected override DriverResult Editor(ForumsSettingsPart part, dynamic shapeHelper) {

            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(ForumsSettingsPart part, IUpdateModel updater, dynamic shapeHelper) {

            return ContentShape("Parts_Forums_SiteSettings", () => {
                    if (updater != null) {
                        updater.TryUpdateModel(part.Record, Prefix, null, null);
                    }
                    return shapeHelper.EditorTemplate(TemplateName: "Parts.Forums.SiteSettings", Model: part.Record, Prefix: Prefix); 
                })
                .OnGroup("Forums");
        }
    }
}