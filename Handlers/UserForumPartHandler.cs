using NGM.Forum.Models;
using Orchard.ContentManagement.Handlers;
using Orchard.Data;

namespace NGM.Forum.Handlers {
    public class UserForumPartHandler : ContentHandler {
        public UserForumPartHandler(IRepository<UserForumPartRecord> repository) {
            Filters.Add(StorageFilter.For(repository));

            OnInitializing<UserForumPart>((context, part) => { part.RequiresModeration = true; });
        }
    }
}