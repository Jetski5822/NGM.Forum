using Orchard.ContentManagement.Records;

namespace NGM.Forum.Models {
    public class ThreadPartRecord : ContentPartRecord {
        public virtual int PostCount { get; set; }
    }
}