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
        private readonly IThreadLastReadService _threadLastReadService;

        public DiscussionsController(IOrchardServices orchardServices,
            IForumService forumService,
            IThreadService threadService,
            IPostService postService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IAuthorizationService authorizationService,
            IAuthenticationService authenticationService,
            ISubscriptionService subscriptionService,
            IThreadLastReadService threadLastReadService
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
            _threadLastReadService = threadLastReadService;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ActionResult ListDiscussions(int forumsHomeId, PagerParameters pagerParameters)
        {
            

            if (!_orchardServices.WorkContext.HttpContext.User.Identity.IsAuthenticated)
                return new HttpUnauthorizedResult(T("You must be logged in to view your discussions").ToString());

            if (!_orchardServices.Authorizer.Authorize(Permissions.CreateThreadsAndPosts, T("You do not have permissions to view this area.")))
                return new HttpUnauthorizedResult();

            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            ForumsHomePagePart forumsHomepagePart = null;
            var siteForums = _orchardServices.ContentManager.Query<ForumsHomePagePart>().List().ToList();

            if (forumsHomeId != 0) //0 flags 'all forums'
            {
                forumsHomepagePart = siteForums.Where(forumsHomePart => forumsHomePart.Id == forumsHomeId).FirstOrDefault();
            } 
            

            var userId = _orchardServices.WorkContext.CurrentUser.Id;

            //get all posts that this user has made and get the unique thread ids from them
            var posts = _orchardServices.ContentManager.Query<CommonPart, CommonPartRecord>(VersionOptions.Published)                                                    
                        .Where( common => common.OwnerId == userId )         
                        .Join<PostPartRecord>()
                        .List().Select( common=>common.Container.Id ).ToList().Distinct().ToList();


            //get all threads for this user
            var threadParts = _orchardServices.ContentManager.Query<ThreadPart,ThreadPartRecord>(VersionOptions.Published)
                                .Where( thread=>thread.IsDeleted==false && thread.IsInappropriate == false && posts.Contains( thread.Id));

            //filter for current forum if necessary -- TODO: need to do this UI still
            if (forumsHomeId != 0) {
                 threadParts = threadParts.Where<ThreadPartRecord>( thread => thread.ForumsHomepageId == forumsHomeId );
            }

            var totalItemCount = threadParts.Count();
            var discussions = threadParts.Slice(pager.GetStartIndex(), pager.PageSize).ToList();
            _threadLastReadService.SetThreadsReadState(userId, forumsHomeId, discussions);

            var threadDisplay = discussions.Select(b => _orchardServices.ContentManager.BuildDisplay(b, "Summary")); ;

            var list = Shape.List();
            list.AddRange(threadDisplay);


            bool showMenuOptions = forumsHomeId != 0;
            var menuShape = Shape.Parts_ForumMenu(ForumsHomePagePart: forumsHomepagePart, ShowRecent: showMenuOptions, ShowMarkAll: showMenuOptions, ReturnUrl: HttpContext.Request.Url.AbsoluteUri);

            var breadCrumb = Shape.Parts_BreadCrumb(ForumsHomePagePart: forumsHomepagePart);
            var searchShape = Shape.Parts_Forum_Search(ForumsHomeId: forumsHomepagePart.Id); ;

            dynamic viewModel = _orchardServices.New.ViewModel()
                                .ForumMenu(menuShape)
                                .BreadCrumb(breadCrumb)
                                .ForumSearch(searchShape)
                                .ContentItems(list)
                                .ForumsHomepagePart(forumsHomepagePart)
                                .SiteForumsList(siteForums)
                                .Pager(Shape.Pager(pager).TotalItemCount(totalItemCount)); 
            
            return View((object)viewModel);

        }

    }

       
}