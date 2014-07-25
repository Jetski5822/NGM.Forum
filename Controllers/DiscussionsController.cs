using System.Linq;
using System.Web.Mvc;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using NGM.Forum.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System.Collections.Generic;
using Orchard.UI.Zones;
using Orchard.Core.Common.Models;

namespace NGM.Forum.Controllers {
    [Themed]
    [ValidateInput(false)]
    public class DiscussionsController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IForumService _forumService;
        private readonly IThreadService _threadService;
        private readonly IPostService _postService;
        private readonly ISiteService _siteService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ISubscriptionService _subscriptionService;

        public DiscussionsController(IOrchardServices orchardServices,
            IForumService forumService,
            IThreadService threadService,
            IPostService postService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IAuthorizationService authorizationService,
            IAuthenticationService authenticationService,
            ISubscriptionService subscriptionService
            )
        {
            _orchardServices = orchardServices;
            _forumService = forumService;
            _threadService = threadService;
            _postService = postService;
            _siteService = siteService;
            _subscriptionService = subscriptionService;
            _authorizationService = authorizationService;
            _authenticationService = authenticationService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ActionResult ListDiscussions(int forumsHomeId)
        {

            if (!_orchardServices.WorkContext.HttpContext.User.Identity.IsAuthenticated)
                return new HttpUnauthorizedResult(T("You must be logged in to view your discussions").ToString());

            if (!_orchardServices.Authorizer.Authorize(Permissions.CanPostToForums, T("You do not have permissions to view this area.")))
                return new HttpUnauthorizedResult();

            ForumsHomePagePart forumsHomepagePart = null;
            var siteForums = _orchardServices.ContentManager.Query<ForumsHomePagePart>().List().ToList();

            if (forumsHomeId != 0) //0 flags 'all forums'
            {
                forumsHomepagePart = siteForums.Where(forumsHomePart => forumsHomePart.Id == forumsHomeId).FirstOrDefault();
            } 
            

            var userId = _orchardServices.WorkContext.CurrentUser.Id;

            //get all threads started by this user
            var threadParts = _orchardServices.ContentManager.Query<ThreadPart>(VersionOptions.Published)
                              .Join<CommonPartRecord>().Where(cpr => cpr.OwnerId == userId).List().ToList();

            var threadPartsFromPosts = _orchardServices.ContentManager.Query<ThreadPart>(VersionOptions.Published)
                              .Join<CommonPartRecord>().Where(cpr => cpr.OwnerId == userId)
                              .Where(cpr => cpr.OwnerId == userId)
                              .Join<PostPartRecord>()
                              .List().ToList();
            /*
             *             return _contentManager.Query<CommonPart, CommonPartRecord>(versionOptions)
                                  .Where(cpr => cpr.Container == parentPart.ContentItem.Record);
             */
            threadParts.AddRange(threadPartsFromPosts);

            var threadDisplay = threadParts.Select(b => _orchardServices.ContentManager.BuildDisplay(b, "Summary")); ;

            var list = Shape.List();
            list.AddRange(threadDisplay);
            var menuShape = Shape.Parts_ForumMenu(ForumsHomePagePart: forumsHomepagePart, ShowRecent: false, ShowMarkAll: false, ReturnUrl: HttpContext.Request.Url.AbsoluteUri);
            dynamic viewModel = _orchardServices.New.ViewModel().ContentItems(list).ForumMenu(menuShape).ForumsHomepagePart(forumsHomepagePart).SiteForumsList(siteForums); ;
            //viewModel.Content.Add(Shape.Parts_Thread_Subscription_List( ContentItems: list), "5");
            return View((object)viewModel);

        }

    }

       
}