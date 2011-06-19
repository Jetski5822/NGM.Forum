using JetBrains.Annotations;
using NGM.Forum.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class ThreadPartDriver : ContentPartDriver<ThreadPart> {
        private const string TemplateName = "Parts.Routable.ThreadPart";

        protected override string Prefix {
            get { return "ThreadPart"; }
        }

        protected override DriverResult Display(ThreadPart threadPart, string displayType, dynamic shapeHelper) {
            if (threadPart.ForumPart.IsClosed) {
                return Combined(
                    ContentShape("Parts_Threads_Thread_PostCount",
                                 () => shapeHelper.Parts_Threads_Thread_PostCount(ContentPart: threadPart, PostCount: threadPart.PostCount))
                    );
            }
            
            return Combined(
                ContentShape("Parts_Threads_Thread_Manage",
                         () => shapeHelper.Parts_Threads_Thread_Manage(ContentPart: threadPart)),
                ContentShape("Parts_Threads_Thread_PostCount",
                             () => shapeHelper.Parts_Threads_Thread_PostCount(ContentPart: threadPart, PostCount: threadPart.PostCount))
                );
        }

        protected override DriverResult Editor(ThreadPart part, dynamic shapeHelper) {
            return ContentShape("Parts_Routable_Thread_Edit",
                () => shapeHelper.EditorTemplate(TemplateName: TemplateName, Model: part, Prefix: Prefix));
        }

        protected override DriverResult Editor(ThreadPart part, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(part, Prefix, null, null);
            return Editor(part, shapeHelper);
        }
    }
}