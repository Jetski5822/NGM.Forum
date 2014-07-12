using System.Linq;
using System.Web.Mvc;
using NGM.Forum.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Logging;
using Orchard.Mvc;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Navigation;
using NGM.Forum.Models;
using Orchard.Core.Common.Models;

namespace NGM.Forum.Controllers {
    [Themed]
    public class ForumsHomePageController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IForumService _forumService;
        private readonly IThreadService _threadService;
        private readonly ISiteService _siteService;
        private readonly IThreadLastReadService _threadLastReadService;
        private readonly IForumCategoryService _forumCategoryService;


        public ForumsHomePageController(
            IOrchardServices orchardServices, 
            IForumCategoryService forumCategoryService,
            IForumService forumService,
            IThreadService threadService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IThreadLastReadService threadLastReadService            
         ) {
            _orchardServices = orchardServices;
            _forumService = forumService;
            _threadService = threadService;
            _siteService = siteService;
            _threadLastReadService = threadLastReadService;
            _forumCategoryService = forumCategoryService;

            Shape = shapeFactory;
            Logger = NullLogger.Instance;
            T = NullLocalizer.Instance;
        }

        dynamic Shape { get; set; }
        protected ILogger Logger { get; set; }
        public Localizer T { get; set; }

       
    
    }
}