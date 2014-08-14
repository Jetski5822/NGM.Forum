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
using NGM.Forum.ViewModels;

namespace NGM.Forum.Controllers {
    [Themed]
    public class ReportPostController : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IForumService _forumService;
        private readonly IThreadService _threadService;
        private readonly IPostService _postService;
        private readonly ISiteService _siteService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IReportPostService _reportPostService;

        public ReportPostController(
            IOrchardServices orchardServices,
            IForumService forumService,
            IThreadService threadService,
            IPostService postService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IAuthorizationService authorizationService,
            IAuthenticationService authenticationService,
            ISubscriptionService subscriptionService,
            IReportPostService reportPostService
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
            _reportPostService = reportPostService;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        [HttpGet, ActionName("ReportInappropriatePost")]
        public ActionResult ReportInappropriatePost_GET(int postId, string returnUrl )
        {

            if (!_orchardServices.WorkContext.HttpContext.User.Identity.IsAuthenticated)
                return new HttpUnauthorizedResult(T("You must be logged in to report an inappropriate post.").ToString());

            if (!_orchardServices.Authorizer.Authorize(Permissions.CreateThreadsAndPosts, T("You require permission to post to the forums in order to report posts as inappropriate.")))
                return new HttpUnauthorizedResult();

            var userId = _orchardServices.WorkContext.CurrentUser.Id;

            ReportPostService.CreatePostResultEnum reportedAccepted = _reportPostService.PostReportStatus(postId, userId);

            //setup a model just in case there is more info to be added later
            ReportInappropriatePostConfirmationViewModel model = new ReportInappropriatePostConfirmationViewModel{
                ReturnUrl = returnUrl,
                PostId = postId,               
                ReportSubmittedResult = reportedAccepted
            };

            return View(model);
        }

        [HttpPost, ActionName("ReportInappropriatePost")]
        public ActionResult ReportInappropriatePost_POST(ReportInappropriatePostConfirmationViewModel model)
        {

            if (!_orchardServices.WorkContext.HttpContext.User.Identity.IsAuthenticated)
                return new HttpUnauthorizedResult(T("You must be logged in to report an inappropriate post.").ToString());

            if (!_orchardServices.Authorizer.Authorize(Permissions.CreateThreadsAndPosts, T("You do not have permissions to post on the forums and therefore cannot report posts.")))
                return new HttpUnauthorizedResult();

            var userId = _orchardServices.WorkContext.CurrentUser.Id;
            var post = _postService.Get( model.PostId, VersionOptions.Published);

            if (model.ReasonReported.Length > 2048)
            {
                this.ModelState.AddModelError("ReasonReported", T("The reason cannot be longer than 2048 characters.  You entered {0} characters.", model.ReasonReported.Length).ToString());
                return View(model);
            }

            if (post != null)
            {
                var reportedPostRecord = new ReportedPostRecord
                {
                    PostId = model.PostId,
                    IsResolved = false,
                    PostedByUserId = post.As<CommonPart>().Owner.Id,
                    ReasonReported = model.ReasonReported,
                    ReportedByUserId = userId,
                    ResolvedByUserId = 0,
                };
                _reportPostService.CreateReport(reportedPostRecord);
            }
            return RedirectToActionPermanent("InappropriatePostReportedSuccessfully", new { returnUrl = model.ReturnUrl });
        }

        [HttpGet]
        public ActionResult InappropriatePostReportedSuccessfully(string returnUrl)
        {
            var viewModel = _orchardServices.New.ViewModel();
            viewModel.ReturnUrl(returnUrl);
            return View(viewModel);

        }
    }

       
}