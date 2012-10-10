using JetBrains.Annotations;
using NGM.Forum.Models;
using Orchard;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class UserForumPartDriver : ContentPartDriver<UserForumPart> {
        private const string TemplateName = "Parts.User.Forums.Edit";

        public UserForumPartDriver(IOrchardServices services) {
            Services = services;
        }

        public IOrchardServices Services { get; set; }

        protected override string Prefix {
            get { return "UserForumPart"; }
        }

        //protected override DriverResult Display(UserForumPart part, string displayType, dynamic shapeHelper) {
        //    /*
        //     User => UserForum (Settings are attached.)
             
        //     * 
        //     Forum => UserForum? how does this work
        //     */
        //    return ContentShape("Parts_RequiresModeration", () =>
        //                                             shapeHelper.Parts_RequiresModeration(ContentPart: part));

        //}

        protected override DriverResult Editor(UserForumPart part, dynamic shapeHelper) {
            return ContentShape("Parts_User_Forums_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(UserForumPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);

            return ContentShape("Parts_User_Forums_Edit",
                                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }
    }
}