using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using NGM.Forum.Models;
using NGM.Forum.Extensions;
using NGM.Forum.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Core.Contents.Controllers;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System.Collections.Generic;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;


namespace NGM.Forum.Controllers {

    [ValidateInput(false), Admin]
    public class ForumAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly IForumService _forumService;
        private readonly IThreadService _threadService;
        private readonly ISiteService _siteService;
        private readonly IContentManager _contentManager;
        private readonly IForumCategoryService _forumCategoryService;
        private readonly IForumsHomePageService _forumsHomePageService;

        public ForumAdminController(IOrchardServices orchardServices, 
            IForumService forumService, 
            IThreadService threadService,
            ISiteService siteService,
            IContentManager contentManager,
            IShapeFactory shapeFactory,
            IForumCategoryService forumCategoryService,
            IForumsHomePageService forumsHomePageService
        ) {
            _orchardServices = orchardServices;
            _forumService = forumService;
            _threadService = threadService;
            _siteService = siteService;
            _contentManager = contentManager;
            _forumCategoryService = forumCategoryService;
            _forumsHomePageService = forumsHomePageService;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        #region FORUM ROOOT CONTROLLER ACTIONS
        public ActionResult ListForumsHomePages()
        {
            //this could be filtered to show only the forums the user has permission to see (i.e. is the general admin or owner) but its a nice to have for now
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, T("Not allowed to manage forum roots")))
                return new HttpUnauthorizedResult();

            var forumsHomePages = _forumsHomePageService.Get(VersionOptions.Latest).ToList();

            var listOfForumsHomePages = _orchardServices.New.List();
            listOfForumsHomePages.AddRange(forumsHomePages.Select(forumsHomePage => _contentManager.BuildDisplay(forumsHomePage, "SummaryAdmin")).ToList());

            dynamic viewModel = _orchardServices.New.ViewModel().ContentItems(listOfForumsHomePages);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.

            return View((object)viewModel);
        }
        //permissions good
        public ActionResult CreateForumsHomePage()
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, T("Not allowed to create forum categories"))
                && !_orchardServices.Authorizer.Authorize(Permissions.ManageOwnForums, T("Not allowed to create forum categories")))
                return new HttpUnauthorizedResult();

            var category = _orchardServices.ContentManager.New<ForumsHomePagePart>("ForumsHomePage");


            var model = _orchardServices.ContentManager.BuildEditor(category);
            return View((object)model);

        }
        //permissions good
        [HttpPost, ActionName("CreateForumsHomePage")]
        public ActionResult CreateForumsHomePagePOST()
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, T("Not allowed to manage the forums' home pages")))
                return new HttpUnauthorizedResult();

            var forumsHomePage = _orchardServices.ContentManager.New<ForumsHomePagePart>("ForumsHomePage");

            _orchardServices.ContentManager.Create(forumsHomePage, VersionOptions.Draft);

            var model = _orchardServices.ContentManager.UpdateEditor(forumsHomePage, this);

            if (!ModelState.IsValid)
            {
                _orchardServices.TransactionManager.Cancel();
                return View((object)model);
            }

            _orchardServices.ContentManager.Publish(forumsHomePage.ContentItem);
            return Redirect(Url.ForumsHomePagesListForAdmin());

        }

        //permissions good
        public ActionResult EditForumsHomePage(int forumsHomePagePartId)
        {
            var root = _forumsHomePageService.Get(forumsHomePagePartId, VersionOptions.Published);
            if (root == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, root, T("Not allowed to create forums")))
                return new HttpUnauthorizedResult();
            
            var model = _orchardServices.ContentManager.UpdateEditor(root, this);
            return View((object)model);

        }

        //permissions good
        [HttpPost, ActionName("EditForumsHomePage")]
        public ActionResult EditForumsHomePagePOST(int forumsHomePagePartId)
        {
            var root = _forumsHomePageService.Get(forumsHomePagePartId, VersionOptions.Published);
            if (root == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, root, T("Not allowed to edit forum")))
                return new HttpUnauthorizedResult();


            dynamic model = _orchardServices.ContentManager.UpdateEditor(root, this);
            if (!ModelState.IsValid)
            {
                _orchardServices.TransactionManager.Cancel();
                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            _contentManager.Publish(root.ContentItem);
            _orchardServices.Notifier.Information(T("Forum Root information updated"));

            return View((object)model);

        }
        
        #endregion

        #region FORUM CATEGORY CONTROLLER ACTIONS
        //permissions good
        public ActionResult CreateForumCategory(int forumsHomePagePartId)
        {
            var forumsHomePagePart = _forumsHomePageService.Get(forumsHomePagePartId, VersionOptions.Latest);
            if ( forumsHomePagePart == null )
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, forumsHomePagePart.ContentItem, T("Not allowed to create forum categories")))
                return new HttpUnauthorizedResult();

            var category = _orchardServices.ContentManager.New<ForumCategoryPart>("ForumCategory");

            dynamic viewModel = _orchardServices.New.ViewModel();
            
            var model = _orchardServices.ContentManager.BuildEditor(category);

            viewModel.Editor(model).ForumsHomePagePart( forumsHomePagePart);

            return View((object)viewModel);
        }

        //permissions good
        [HttpPost, ActionName("CreateForumCategory")]
        public ActionResult CreateForumCategoryPOST(int forumsHomePagePartId)
        {
            var forumsHomePage = _forumsHomePageService.Get(forumsHomePagePartId, VersionOptions.Published);
            if (forumsHomePage == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, forumsHomePage.ContentItem, T("Not allowed to manage the forum categories")))
                return new HttpUnauthorizedResult();
           
            var category = _orchardServices.ContentManager.New<ForumCategoryPart>("ForumCategory");
            _orchardServices.ContentManager.Create(category, VersionOptions.Draft);

            
            category.As<CommonPart>().Container = forumsHomePage.ContentItem;

            var model = _orchardServices.ContentManager.UpdateEditor(category, this);

            if (!ModelState.IsValid)
            {
                _orchardServices.TransactionManager.Cancel();
                return View((object)model);
            }

            _orchardServices.ContentManager.Publish(category.ContentItem);
            //return View((object)model);
            return Redirect(Url.CategoriesForAdmin(forumsHomePage));
        }

        //good permissions
        public ActionResult EditForumCategory(int forumCategoryPartId)
        {
            var category = _forumCategoryService.Get(forumCategoryPartId, VersionOptions.Latest);
            if (category == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, category.ContentItem, T("Not allowed to edit forum categories")))
                return new HttpUnauthorizedResult();

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            var viewModel = _orchardServices.New.ViewModel();            
            var model = _orchardServices.ContentManager.BuildEditor(category);

            // viewModel.Editor(model).ForumsHomePagePart(category.ForumsHomePagePart);
            viewModel.Editor(model);
         
            return View((object)viewModel);

        }
        //good permissions
        [HttpPost, ActionName("EditForumCategory")]
        [InternalFormValueRequired("submit.Save")]
        public ActionResult EditForumCategoryPOST(int forumCategoryPartId)
        {
            var category = _forumCategoryService.Get(forumCategoryPartId, VersionOptions.DraftRequired);
            if (category == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, category, T("Not allowed to edit forum categories.")))
                return new HttpUnauthorizedResult();

            dynamic model = _orchardServices.ContentManager.UpdateEditor(category, this);
            if (!ModelState.IsValid)
            {
                _orchardServices.TransactionManager.Cancel();
                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }

            _contentManager.Publish(category.ContentItem);
            _orchardServices.Notifier.Information(T("Forum Category information updated"));

            return Redirect(Url.CategoriesForAdmin( category.ForumsHomePagePart));
        }

        //good permissions
        public ActionResult ListForumCategories(int forumsHomePagePartId)
        {
            var forumsHomePage = _forumsHomePageService.Get(forumsHomePagePartId, VersionOptions.Published);
            if (forumsHomePage == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, forumsHomePage, T("Not allowed to manage this forum's categories. You are either not the owner or do not have Manage Forums permissions.")))
                return new HttpUnauthorizedResult();

            var categoriesWithForums = _forumCategoryService.GetCategoriesInForumsHomePage(forumsHomePagePartId, VersionOptions.Latest).ToList();

            var listOfCategories = _orchardServices.New.List();
            listOfCategories.AddRange(categoriesWithForums.Select(category => _contentManager.BuildDisplay(category, "SummaryAdmin")).ToList());

            var forumsHomePagePart = _forumsHomePageService.Get(forumsHomePagePartId, VersionOptions.Latest);
            dynamic viewModel = _orchardServices.New.ViewModel()
                                    .ContentItems(listOfCategories)
                                    .ForumsHomePagePart(forumsHomePagePart);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)viewModel);

        }

        //good permissions
        public ActionResult ForumCategoryItem(int forumCategoryPartId)
        {
            ForumCategoryPart forumCategory = _forumCategoryService.Get(forumCategoryPartId, VersionOptions.Latest);

            if (forumCategory == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, forumCategory, T("Not allowed to view forum category")))
                return new HttpUnauthorizedResult();

            var forumCategories = _forumCategoryService.GetForumsForCategory(forumCategory, VersionOptions.AllVersions).ToArray();
            var forumCategoryShapes = forumCategories.Select(bp => _contentManager.BuildDisplay(bp, "SummaryAdmin")).ToArray();

            dynamic forumCategoryShape = _orchardServices.ContentManager.BuildDisplay(forumCategory, "DetailAdmin");

            var list = Shape.List();
            list.AddRange(forumCategoryShapes);
            forumCategoryShape.Content.Add(Shape.Parts_Forums_Thread_ListAdmin(ContentItems: list), "5");


            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)forumCategoryShape);
        }
        #endregion

        //permissions good
        public ActionResult CreateForum(string type, int forumCategoryPartId )
        {

            if (string.IsNullOrWhiteSpace(type)) {
                var forumTypes = _forumService.GetForumTypes();
                if (forumTypes.Count > 1)
                    return Redirect(Url.ForumSelectTypeForAdmin());

                if (forumTypes.Count == 0) {
                    _orchardServices.Notifier.Warning(T("You have no forum types available. Add one to create a forum."));
                    return Redirect(Url.DashboardForAdmin());
                }

                type = forumTypes.Single().Name;
            }

            var forum = _orchardServices.ContentManager.New<ForumPart>(type);
            var forumCategoryPart = _forumCategoryService.Get(forumCategoryPartId, VersionOptions.Latest);

            if (forum == null)
                return HttpNotFound();

            if (forumCategoryPart == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, forumCategoryPart.ContentItem, T("Not allowed to create forums")))
                return new HttpUnauthorizedResult();

            var model = _orchardServices.ContentManager.BuildEditor(forum);
            dynamic viewModel = _orchardServices.New.ViewModel()
                             .Editor(model)
                             .ForumCategoryPart(forumCategoryPart);

            return View((object)viewModel);
        }

        //good permissions
        [HttpPost, ActionName("CreateForum")]
        public ActionResult CreateForumPOST(string type, int forumCategoryPartId)
        {

            var forumCategoryPart = _forumCategoryService.Get(forumCategoryPartId, VersionOptions.Latest);
            if (forumCategoryPart == null )
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums,forumCategoryPart.ContentItem, T("Not allowed to create forums")))
                return new HttpUnauthorizedResult();

            if (string.IsNullOrWhiteSpace(type)) {
                var forumTypes = _forumService.GetForumTypes();
                if (forumTypes.Count > 1)
                    return Redirect(Url.ForumSelectTypeForAdmin());

                if (forumTypes.Count == 0) {
                    _orchardServices.Notifier.Warning(T("You have no forum types available. Add one to create a forum."));
                    return Redirect(Url.DashboardForAdmin());
                }

                type = forumTypes.Single().Name;
            }

            var forum = _orchardServices.ContentManager.New<ForumPart>(type);

            _orchardServices.ContentManager.Create(forum, VersionOptions.Draft);


            forum.As<CommonPart>().Container = forumCategoryPart.ContentItem;

            var model = _orchardServices.ContentManager.UpdateEditor(forum, this);

            if (!ModelState.IsValid) {
                _orchardServices.TransactionManager.Cancel();
                return View((object)model);
            }

            _orchardServices.ContentManager.Publish(forum.ContentItem);

            return Redirect(Url.ForumForAdmin( forum));  //TODO IS THIS RIGHT? just changed it
        }

        public ActionResult SelectType()
        {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, T("Not allowed to create forums")))
                return new HttpUnauthorizedResult();

            var forumTypes = _forumService.GetForumTypes();
            var model = Shape.ViewModel(ForumTypes: forumTypes);
            return View(model);
        }

        //good permissions
        public ActionResult EditForum(int forumId) {
            var forum = _forumService.Get(forumId, VersionOptions.Latest);

            if (forum == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, forum, T("Not allowed to edit forum")))
                return new HttpUnauthorizedResult();

            var forumCategoryPart = forum.As<CommonPart>().Container.As<ForumCategoryPart>();
            var forumsHomePagePart = forumCategoryPart.As<CommonPart>().Container.As<ForumsHomePagePart>();

            dynamic model = _orchardServices.ContentManager.BuildEditor(forum);
            dynamic viewModel = _orchardServices.New.ViewModel()
                                    .Editor(model)
                                    .ForumCategoryPart( forumCategoryPart)
                                    .ForumsHomePagePart(forumsHomePagePart);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)viewModel);
        }
        //good permissions
        [HttpPost, ActionName("EditForum")]
        [InternalFormValueRequired("submit.Save")]
        public ActionResult EditForumPOST(int forumId, int forumCategoryId ) {
            var forum = _forumService.Get(forumId, VersionOptions.DraftRequired);

            if (forum == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, forum, T("Not allowed to edit forum")))
                return new HttpUnauthorizedResult();

            dynamic model = _orchardServices.ContentManager.UpdateEditor(forum, this);
            if (!ModelState.IsValid) {
                _orchardServices.TransactionManager.Cancel();
                // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
                return View((object)model);
            }
            var forumCategoryPart = _forumCategoryService.Get(forumCategoryId, VersionOptions.Latest);
            _contentManager.Publish(forum.ContentItem);

            //set the parent
            forum.As<CommonPart>().Container = forumCategoryPart.ContentItem;
            _orchardServices.Notifier.Information(T("Forum information updated"));

            return Redirect(Url.ForumsForAdmin( forum.ForumCategoryPart));
        }

        //good permissions
        [HttpPost, ActionName("Edit")]
        [InternalFormValueRequired("submit.Delete")]
        public ActionResult EditDeletePOST(int forumId) {
            var forum = _forumService.Get(forumId, VersionOptions.Latest);

            if (forum == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, forum, T("Not allowed to delete forums")))
                return new HttpUnauthorizedResult();

            return RemoveForum(forumId);
        }

        [HttpPost]
        public ActionResult RemoveForum(int forumId) {
            var forum = _forumService.Get(forumId, VersionOptions.Latest);
            
            if (forum == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, forum, T("Not allowed to delete forums")))
                return new HttpUnauthorizedResult();

            var forumCategoryPart = forum.As<CommonPart>().Container.As<ForumCategoryPart>();

            _forumService.Delete(forum);

            _orchardServices.Notifier.Information(T("Forum was successfully deleted"));
            return Redirect(Url.ForumsForAdmin(forumCategoryPart));
        }

        public ActionResult ListForums( int forumCategoryPartId ) {

            var forumCategory = _forumCategoryService.Get(forumCategoryPartId, VersionOptions.Latest);
            if (forumCategory == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, forumCategory.ContentItem, T("Not allowed to delete forums")))
                return new HttpUnauthorizedResult();

            var list = _orchardServices.New.List();
            list.AddRange(_forumService.Get(VersionOptions.Latest)
                              .Select(b => {
                                  var forum = _orchardServices.ContentManager.BuildDisplay(b, "SummaryAdmin");
                                  forum.TotalPostCount = _threadService.Get(b, true, VersionOptions.Latest).Count();
                                  return forum;
                              }));
            
            dynamic viewModel = _orchardServices.New.ViewModel()
                .ContentItems(list).ParentForumCategory(forumCategory);

            // Casting to avoid invalid (under medium trust) reflection over the protected View method and force a static invocation.
            return View((object)viewModel);
        }

        public ActionResult ForumItem(int forumId, PagerParameters pagerParameters) {
            
            ForumPart forum = _forumService.Get(forumId, VersionOptions.Latest);

            if (forum == null)
                return HttpNotFound();

            if (!_orchardServices.Authorizer.Authorize(Permissions.ManageForums, forum, T("Not allowed to view forum")))
                return new HttpUnauthorizedResult();

            Pager pager = new Pager(_siteService.GetSiteSettings(), pagerParameters);

            var threads = _threadService.Get(forum, pager.GetStartIndex(), pager.PageSize, true, VersionOptions.Latest).ToArray();
            var threadsShapes = threads.Select(bp => _contentManager.BuildDisplay(bp, "SummaryAdmin")).ToArray();

            dynamic forumShape = _orchardServices.ContentManager.BuildDisplay(forum, "DetailAdmin");

            var list = Shape.List();
            list.AddRange(threadsShapes);
            forumShape.Content.Add(Shape.Parts_Forums_Thread_ListAdmin(ContentItems: list), "5");

            var totalItemCount = _threadService.Count(forum, VersionOptions.Latest);
            forumShape.Content.Add(Shape.Pager(pager).TotalItemCount(totalItemCount), "Content:after");

            dynamic viewModel = _orchardServices.New.ViewModel()
              .ForumPart(forum).ThreadList(forumShape);
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

    public class InternalFormValueRequiredAttribute : ActionMethodSelectorAttribute
    {
        private readonly string _submitButtonName;

        public InternalFormValueRequiredAttribute(string submitButtonName)
        {
            _submitButtonName = submitButtonName;
        }

        public override bool IsValidForRequest(ControllerContext controllerContext, MethodInfo methodInfo)
        {
            var value = controllerContext.HttpContext.Request.Form[_submitButtonName];
            return !string.IsNullOrEmpty(value);
        }
    }
}