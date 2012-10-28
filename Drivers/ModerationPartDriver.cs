using JetBrains.Annotations;
using NGM.Forum.Models;
using Orchard;
using Orchard.ContentManagement.Drivers;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class ModerationPartDriver : ContentPartDriver<ModerationPart> {
        public ModerationPartDriver(IOrchardServices services) {
            Services = services;
        }

        public IOrchardServices Services { get; set; }

        protected override string Prefix {
            get { return "ModerationPart"; }
        }

        protected override DriverResult Display(ModerationPart part, string displayType, dynamic shapeHelper) {
            return Combined(
                ContentShape("Parts_Moderation_SummaryAdmin",
                    () => shapeHelper.Parts_Moderation_SummaryAdmin()),
                ContentShape("Parts_Moderation_Metadata_SummaryAdmin",
                    () => shapeHelper.Parts_Moderation_Metadata_SummaryAdmin(ContentPart: part))
                );
        }
    }
}