using System.Collections.Generic;
using System.Linq;
using NGM.Forum.Models;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Data;
using NGM.Forum.ViewModels;

namespace NGM.Forum.Services {

    public interface IForumsHomePageService : IDependency {
        IEnumerable<ForumsHomePagePart> Get(VersionOptions versionOptions);
        ForumsHomePagePart Get(int Id, VersionOptions versionOptions);
   
    }

    public class ForumsHomePageService : IForumsHomePageService
    {
        private readonly IContentManager _contentManager;
        private readonly IForumService _forumService;

        public ForumsHomePageService(
            IContentManager contentManager,
            IForumService forumService   
         )
        {
            _contentManager = contentManager;
            _forumService = forumService;
        }

        public IEnumerable<ForumsHomePagePart> Get(VersionOptions versionOptions)
        {
           return  _contentManager.Query<ForumsHomePagePart>(versionOptions).List().ToList();
        }

        public ForumsHomePagePart Get(int Id, VersionOptions versionOptions)
        {
            return _contentManager.Query<ForumsHomePagePart,ForumsHomePagePartRecord>(versionOptions).Where( part=>part.Id == Id).List().FirstOrDefault();
        }
    }
}