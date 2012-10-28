using NGM.Forum.Models;

namespace NGM.Forum.Events {
    public class ApprovingContext {
        public ModerationPart ModerationPart { get; set; }
        public bool Approved { get; set; }
    }
}