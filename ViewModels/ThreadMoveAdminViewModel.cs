using System.Collections.Generic;
using NGM.Forum.Models;

namespace NGM.Forum.ViewModels {
    public class ThreadMoveAdminViewModel {
        public int ThreadId { get; set; }
        public int ForumId { get; set; }

        public IEnumerable<ForumPart> AvailableForums { get; set; }

        public bool AllowRedirect { get; set; }
    }
}