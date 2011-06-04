using System.Linq;
using System.Web.Mvc;
using NGM.Forum.Models;
using NGM.Forum.Extensions;
using NGM.Forum.Routing;
using NGM.Forum.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;

namespace NGM.Forum.Controllers {

    [ValidateInput(false), Admin]
    public class ForumAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly IForumService _forumService;
        private readonly IThreadService _threadService;
        private readonly IForumPathConstraint _forumPathConstraint;
        private readonly ISiteService _siteService;

        public ForumAdminController(IOrchardServices orchardServices, 
            IForumService forumService, 
            IThreadService threadService,
            IForumPathConstraint forumPathConstraint,
            ISiteService siteService,
            IShapeFactory shapeFactory) {
            _orchardServices = orchardServices;
            _forumService = forumService;
            _threadService = threadService;
            _forumPathConstraint = forumPathConstraint;
            _siteService = siteService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ActionResult Create() {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, T("Not allowed to create forums")))
                return new HttpUnauthorizedResult();

            var forum = _orchardServices.ContentManager.New<ForumPart>("Forum");
            if (forum == null)
                return HttpNotFound();

            dynamic model = _orchardServices.ContentManager.BuildEditor(forum);
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST() {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, T("Not allowed to create forums")))
                return new HttpUnauthorizedResult();

            var forum = _orchardServices.ContentManager.New<ForumPart>("Forum");

            _orchardServices.ContentManager.Create(forum, VersionOptions.Draft);
            dynamic model = _orchardServices.ContentManager.UpdateEditor(forum, this);

            if (!ModelState.IsValid) {
                _orchardServices.TransactionManager.Cancel();
                return View((object)model);
            }

            _orchardServices.ContentManager.Publish(forum.ContentItem);
            _forumPathConstraint.AddPath(forum.As<IRoutableAspect>().Path);

            return Redirect(Url.ForumForAdmin(forum));
        }

        public ActionResult Item(int forumId, PagerParameters pagerParameters) {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);
            ForumPart forumPart = _forumService.Get(forumId, VersionOptions.Latest).As<ForumPart>();

            if (forumPart == null)
                return HttpNotFound();

            var threads = _threadService.Get(forumPart, pager.GetStartIndex(), pager.PageSize, VersionOptions.Latest)
                .Select(bp => _orchardServices.ContentManager.BuildDisplay(bp, "SummaryAdmin"));

            dynamic forum = _orchardServices.ContentManager.BuildDisplay(forumPart, "DetailAdmin");

            var list = Shape.List();
            list.AddRange(threads);
            forum.Content.Add(Shape.Parts_Forums_Thread_ListAdmin(ContentItems: list), "5");

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)forum);
        }

        public ActionResult List() {
            var list = _orchardServices.New.List();
            list.AddRange(_forumService.Get(VersionOptions.Latest)
                              .Select(b => {
                                  var forum = _orchardServices.ContentManager.BuildDisplay(b, "SummaryAdmin");
                                  forum.TotalPostCount = _threadService.Get(b, VersionOptions.Latest).Count();
                                  return forum;
                              }));

            dynamic viewModel = _orchardServices.New.ViewModel()
                .ContentItems(list);
            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)viewModel);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}