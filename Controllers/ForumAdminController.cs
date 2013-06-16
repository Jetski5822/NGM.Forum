using System.Linq;
using System.Web.Mvc;
using NGM.Forum.Models;
using NGM.Forum.Extensions;
using NGM.Forum.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Controllers;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;

namespace NGM.Forum.Controllers {

    [ValidateInput(false), Admin]
    public class ForumAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly IForumService _forumService;
        private readonly IThreadService _threadService;
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;

        public ForumAdminController(IOrchardServices orchardServices, 
            IForumService forumService, 
            IThreadService threadService,
            ISiteService siteService,
            IContentManager contentManager,
            IShapeFactory shapeFactory) {
            _orchardServices = orchardServices;
            _forumService = forumService;
            _threadService = threadService;
            _siteService = siteService;
            _contentManager = contentManager;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ActionResult Create() {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, T("Not allowed to create forums")))
                return new HttpUnauthorizedResult();

            var forum = _orchardServices.ContentManager.New<ForumPart>(Constants.Parts.Forum);
            if (forum == null)
                return HttpNotFound();

            dynamic model = _orchardServices.ContentManager.BuildEditor(forum);
            return View((object)model);
        }

        [HttpPost, ActionName("Create")]
        public ActionResult CreatePOST() {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, T("Not allowed to create forums")))
                return new HttpUnauthorizedResult();

            var forum = _orchardServices.ContentManager.New<ForumPart>(Constants.Parts.Forum);

            _orchardServices.ContentManager.Create(forum, VersionOptions.Draft);
            dynamic model = _orchardServices.ContentManager.UpdateEditor(forum, this);

            if (!ModelState.IsValid) {
                _orchardServices.TransactionManager.Cancel();
                return View((object)model);
            }

            _orchardServices.ContentManager.Publish(forum.ContentItem);

            return Redirect(Url.ForumForAdmin(forum));
        }

        public ActionResult Edit(int forumId) {
            var blog = _forumService.Get(forumId, VersionOptions.Latest);

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, blog, T("Not allowed to edit forum")))
                return new HttpUnauthorizedResult();

            if (blog == null)
                return HttpNotFound();

            dynamic model = _orchardServices.ContentManager.BuildEditor(blog);
            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Delete")]
        public ActionResult EditDeletePOST(int forumId) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, T("Couldn't delete forum")))
                return new HttpUnauthorizedResult();

            var forum = _forumService.Get(forumId, VersionOptions.DraftRequired);
            if (forum == null)
                return HttpNotFound();
            _forumService.Delete(forum);

            _orchardServices.Notifier.Information(T("Forum deleted"));

            return Redirect(Url.ForumsForAdmin());
        }


        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public ActionResult EditPOST(int forumId) {
            var forum = _forumService.Get(forumId, VersionOptions.DraftRequired);

            if (forum == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, forum, T("Couldn't edit forum")))
                return new HttpUnauthorizedResult();

            dynamic model = _orchardServices.ContentManager.UpdateEditor(forum, this);
            if (!ModelState.IsValid) {
                _orchardServices.TransactionManager.Cancel();
                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            _contentManager.Publish(forum.ContentItem);
            _orchardServices.Notifier.Information(T("Forum information updated"));

            return Redirect(Url.ForumsForAdmin());
        }

        [HttpPost]
        public ActionResult Remove(int forumId) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, T("Couldn't delete forum")))
                return new HttpUnauthorizedResult();

            var forum = _forumService.Get(forumId, VersionOptions.Latest);

            if (forum == null)
                return HttpNotFound();

            _forumService.Delete(forum);

            _orchardServices.Notifier.Information(T("Forum was successfully deleted"));
            return Redirect(Url.ForumsForAdmin());
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

        public ActionResult Item(int forumId, PagerParameters pagerParameters) {
            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            ForumPart forumPart = _forumService.Get(forumId, VersionOptions.Latest).As<ForumPart>();

            if (forumPart == null)
                return HttpNotFound();

            var threads = _threadService.Get(forumPart, pager.GetStartIndex(), pager.PageSize, VersionOptions.Latest).ToArray();
            var threadsShapes = threads.Select(bp => _contentManager.BuildDisplay(bp, "SummaryAdmin")).ToArray();

            dynamic forum = _orchardServices.ContentManager.BuildDisplay(forumPart, "DetailAdmin");

            var list = Shape.List();
            list.AddRange(threadsShapes);
            forum.Content.Add(Shape.Parts_Forums_Thread_ListAdmin(ContentItems: list), "5");

            var totalItemCount = _threadService.ThreadCount(forumPart, VersionOptions.Latest);
            forum.Content.Add(Shape.Pager(pager).TotalItemCount(totalItemCount), "Content:after");

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)forum);
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}