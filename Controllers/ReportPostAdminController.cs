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
using System;
using Html.Helpers;

namespace NGM.Forum.Controllers {
    [Themed]
    public class ReportPostAdminController : Controller
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
        private readonly ICountersService _countersService;

        public ReportPostAdminController(
            IOrchardServices orchardServices,
            IForumService forumService,
            IThreadService threadService,
            IPostService postService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IAuthorizationService authorizationService,
            IAuthenticationService authenticationService,
            ISubscriptionService subscriptionService,
            IReportPostService reportPostService,
            ICountersService countersService
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
            _countersService = countersService;
            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }
        const int RecordsPerPage = 25;


        [HttpGet]
        public ActionResult ListPostReports(ReportPostSearchViewModel model, PagerParameters pagerParameters) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ModerateInappropriatePosts, T("You do not have the proper permissions to administer inappropriate posts")))
                return new HttpUnauthorizedResult();

            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            if (!_orchardServices.WorkContext.HttpContext.User.Identity.IsAuthenticated)
                return new HttpUnauthorizedResult(T("You must be logged in").ToString());

            if (Request.QueryString["ShowUnresolvedOnly"] != null)
            {
                model.ShowUnresolvedOnly = Request.QueryString["ShowUnresolvedOnly"].ToLower().Contains("true"); ;
            }

            IEnumerable<ReportedPostEntry> searchResults = new List<ReportedPostEntry>();
            int totalItemCount = 0;
            if (!string.IsNullOrEmpty(model.SearchButton) )
            {

                var results = _reportPostService.GetPostReports(
                                                            model.PostId,
                                                            model.StartDate,
                                                            model.EndDate,
                                                            model.ReportedByUserName,
                                                            model.ReviewedByUserName,
                                                            model.ReportedUserName,                                                            
                                                            model.ShowUnresolvedOnly,
                                                            pager.GetStartIndex(),
                                                            pager.PageSize
                                                            );
                totalItemCount = results.Item1;
                searchResults = results.Item2.Select(x => new ReportedPostEntry { IsChecked = false,  ReportedPostRecordViewModel = x });

                //sanitize the user input html -- not required since its not html input
                /*
                foreach (var row in searchResults)
                {
                    row.ReportedPostRecordViewModel.Note = HtmlSanitizer.sanitizer(row.ReportedPostRecordViewModel.Note).html;
                    row.ReportedPostRecordViewModel.ReasonReported = HtmlSanitizer.sanitizer(row.ReportedPostRecordViewModel.ReasonReported).html;
                }
                 * */

                model.SearchResults = searchResults;
              
            }

            model.Pager = Shape.Pager(pager).TotalItemCount(totalItemCount);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);

        }

        /// <summary>
        /// Used to remove an inappropriate flag on a post
        /// </summary>
        /// <param name="reportId"></param>
        /// <param name="note"></param>
        /// <param name="isResolved"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [ActionName("RemoveInappropriate"), HttpGet]
        public ActionResult RemoveInappropriate_GET(int contentId)
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ModerateInappropriatePosts, T("You do not have the proper permissions to administer inappropriate posts")))
                return new HttpUnauthorizedResult();
     
            var report = _reportPostService.GetReportsForPost(contentId).FirstOrDefault();
            var model = new RemoveInappropriateFlagViewModel();
            if (report != null)
            {
                model.Report = report;
                model.ReturnUrl = Request.UrlReferrer.AbsoluteUri;
            }
            model.ReturnUrl = Request.UrlReferrer.AbsoluteUri;
            return View(model); ;
        }

        [ActionName("RemoveInappropriate"), HttpPost]
        public ActionResult RemoveInappropriate_POST(RemoveInappropriateFlagViewModel model)
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ModerateInappropriatePosts, T("You do not have the proper permissions to administer inappropriate posts")))
                return new HttpUnauthorizedResult();

            //update the report with any new notes and new status
            var report = _reportPostService.GetReportsForPost( model.Report.PostId).FirstOrDefault();
            if (report == null)
            {
                _orchardServices.Notifier.Add(NotifyType.Error, T("The report associated with this post could not be found. "));
            }
            else
            {
                report.Note = model.Report.Note;
                report.IsResolved = model.Report.IsResolved;
            }

            //update the post as being appropriate
            var post = _postService.Get(model.Report.PostId, VersionOptions.Published);
            if (post != null)
            {
                if (post.IsParentThread())
                {
                    post.ThreadPart.IsInappropriate = false;
                }
                post.IsInappropriate = false;
                _countersService.UpdateThreadPartAndForumPartCounters(post);
            }
            else
            {
                _orchardServices.Notifier.Add(NotifyType.Error, T("The post could not be found."));
            }

            return RedirectPermanent(model.ReturnUrl);
        }


        //BASIC USECASE: the moderator is viewing a post while reading the forums and wants to mark it as inappropriate.. 
        //this will mark the post as inappriopriate so its not viewed by general users 
        // the moderator enters a note about what is inappropriate, any actions taken, etc. 
        // leaving a 'report' as well so that there is viewable history of bad posts for the given poster
        //THIS IS NOT *REPORTING* an inapprorpriate reports ... it is marking it as inappropriate.
        [ActionName("MarkInappropriate"), HttpGet]
        public ActionResult MarkInappropriate_GET(int contentId)
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ModerateInappropriatePosts, T("You do not have the proper permissions to administer inappropriate posts")))
                return new HttpUnauthorizedResult();

            var contentItem = _orchardServices.ContentManager.Get(contentId);

            ReportedPostRecord report = _reportPostService.GetReportsForPost(contentId).FirstOrDefault();
            var model = new RemoveInappropriateFlagViewModel();

            //if no pre-existing report exists make one
            if (report == null)
            {
                //don't know who the poster is at this point so set it after below
                report = new ReportedPostRecord
                {
                    IsResolved = true,
                    PostId = contentId,               
                };
            }

            model.Report = report;
            model.ReturnUrl = Request.UrlReferrer.AbsoluteUri;

            return View(model); ;
        }

        /// <summary>
        /// Used by mod/admin to make a post inappropriate
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [ActionName("MarkInappropriate"), HttpPost]
        public ActionResult MarkInappropriate_POST(MarkInappropriateViewModel model)
        {
            var contentItem = _orchardServices.ContentManager.Get(model.Report.PostId);

            /*  This is what should be done so people can only manage reports on their own forums
             *  however for now, allowing anyone with ManageForum permissions to administer reports regardless of if its
             *  their forum because there is no filtering in place to show reports only for a particular user (owner)
             *  nor to give permissions to moderators to mod only particular forums. For now.. if you can mod reports or are an owner
             *  you can manage all innapropriate posts
             *
            ContentItem forumsHomePage = null;
            if (contentItem.Is<ThreadPart>())
            {
                forumsHomePage = _orchardServices.ContentManager.Get(contentItem.As<ThreadPart>().ForumsHomepageId);
            }
            else
            {
                forumsHomePage = _orchardServices.ContentManager.Get(contentItem.As<PostPart>().ThreadPart.ForumsHomepageId);
            }
            if (!_orchardServices.Authorizer.Authorize(Permissions.ModerateInappropriatePosts, forumsHomePage, T("You do not have the proper permissions to administer inappropriate posts")))
                return new HttpUnauthorizedResult();
            
            */
            if (!_orchardServices.Authorizer.Authorize(Permissions.ModerateInappropriatePosts,  T("You do not have the proper permissions to administer inappropriate posts")))
                return new HttpUnauthorizedResult();
            //see if there is an existing report
            ReportedPostRecord report = _reportPostService.GetReportsForPost(model.Report.PostId).FirstOrDefault();
            
            //if not pre-existing report exists make one
            if (report == null)
            {
                //don't know who the poster is at this point so set it after below
                report = new ReportedPostRecord
                {
                    PostId = model.Report.PostId,      
                    ReportedDate = DateTime.UtcNow,
                    ReportedByUserId = _orchardServices.WorkContext.CurrentUser.Id,
                    Note = model.Report.Note,
                    PostedByUserId = contentItem.As<CommonPart>().Owner.Id,                   

                };

                _reportPostService.CreateReport(report);
            }

            report.IsResolved = model.Report.IsResolved;
            if (report.IsResolved)
            {
                report.ResolvedByUserId = _orchardServices.WorkContext.CurrentUser.Id;
                report.ResolvedDate = DateTime.UtcNow;
            }
            report.Note = model.Report.Note;
            report.PostId = report.PostId;
            
            _reportPostService.UpdateReport(report);

            if (report == null)
            {
                _orchardServices.Notifier.Add(NotifyType.Error, T("Something went wrong. No report exists for this post."));
            }

            var thread = contentItem.As<ThreadPart>();            
            if (thread != null)
            {
                /* i think this is dead code.. the threadpart is never directly marked as inappropriate
                 * instead its corresponding post part is marked
                 */
                thread.IsInappropriate = true;
                _countersService.UpdateForumPartCounters(thread);
                _orchardServices.Notifier.Information(T("Thread has been marked as inappropriate."));                
            }

            var post = contentItem.As<PostPart>();
            if (post != null)
            {

                if (post.IsParentThread())
                {
                    post.ThreadPart.IsInappropriate = true;
                    post.IsInappropriate = true;
                }
                else
                {
                    post.IsInappropriate = true;
                }
                _countersService.UpdateThreadPartAndForumPartCounters(post);
            }

            return Redirect(model.ReturnUrl);

        }

        [ActionName("ResolveReport"), HttpPost]
        public ActionResult ResolveReport_Post(ResolvePostReportViewModel model)
        {

            if (!_orchardServices.Authorizer.Authorize(Permissions.ModerateInappropriatePosts, T("You do not have the proper permissions to administer inappropriate posts")))
                return new HttpUnauthorizedResult();

            var report = _reportPostService.GetPostReport(model.Report.Id);

            report.Note = model.Report.Note;
            report.IsResolved = model.Report.IsResolved;
            if (report.IsResolved)
            {
                report.ResolvedByUserId = _orchardServices.WorkContext.CurrentUser.Id;
                report.ReportedDate = DateTime.UtcNow;                
            }

            _reportPostService.UpdateReport(report);                
            _orchardServices.Notifier.Add(NotifyType.Information, T("The report was successfully updated."));                

            return RedirectPermanent(model.ReturnUrl);
        }

        [ActionName("ResolveReport"), HttpGet]
        public ActionResult ResolveReport_GET(int reportId)
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ModerateInappropriatePosts, T("You do not have the proper permissions to administer inappropriate posts")))
                return new HttpUnauthorizedResult();

            var report = _reportPostService.GetPostReport(reportId);
            var model = new ResolvePostReportViewModel();
            if (report == null)
            {
                model.Report = new ReportedPostRecord();
            }

            else
            {
                model.Report = report;                
            }

            model.ReturnUrl = Request.UrlReferrer.AbsoluteUri;
            return View(model);
        }
    }
}