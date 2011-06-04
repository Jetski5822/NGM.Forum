using JetBrains.Annotations;
using NGM.Forum.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace NGM.Forum.Handlers {
    [UsedImplicitly]
    public class ForumPartHandler : ContentHandler {
        public ForumPartHandler(IRepository<ForumPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));
        }
    }
}