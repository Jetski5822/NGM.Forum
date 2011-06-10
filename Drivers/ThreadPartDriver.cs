using JetBrains.Annotations;
using NGM.Forum.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class ThreadPartDriver : ContentPartDriver<ThreadPart> {
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

        protected override DriverResult Editor(ThreadPart threadPart, dynamic shapeHelper) {
            return null;
        }

        protected override DriverResult Editor(ThreadPart threadPart, IUpdateModel updater, dynamic shapeHelper) {
            updater.TryUpdateModel(threadPart.Record, Prefix, null, null);

            return Editor(threadPart, shapeHelper);
        }
    }
}