using System;
using System.Linq;
using System.Collections.Generic;
using JetBrains.Annotations;
using NGM.Forum.Models;
using NGM.Forum.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.ContentManagement.Handlers;
using NGM.Forum.Services;
using Orchard;
using System.Web;
using Orchard.Localization;

namespace NGM.Forum.Drivers {
    [UsedImplicitly]
    public class ForumsHomePagePartDriver : ContentPartDriver<ForumsHomePagePart> {

        private readonly IForumCategoryService _forumCategoryService;
        private readonly IOrchardServices _orchardServices;
        private readonly IUserTimeZoneService _userTimeZoneService;
        private readonly IWorkContextAccessor _workContextAccessor;
        public Localizer T { get; set; }
        protected override string Prefix {
            get { return "ForumsHomePagePart"; }
        }

        public ForumsHomePagePartDriver(
            IForumCategoryService forumCategoryService,
            IOrchardServices orchardServices,
            IUserTimeZoneService userTimeZoneService,
            IWorkContextAccessor workContextAccessor
         ){
             _forumCategoryService = forumCategoryService;
            _orchardServices = orchardServices;
            _userTimeZoneService = userTimeZoneService;
            _workContextAccessor = workContextAccessor;
            T = NullLocalizer.Instance;
        }

        protected override DriverResult Display(ForumsHomePagePart part, string displayType, dynamic shapeHelper)
        {

            int? userId = null;
            if (_orchardServices.WorkContext.CurrentUser != null)
            {
                userId = _orchardServices.WorkContext.CurrentUser.Id;
            }

            _workContextAccessor.GetContext().CurrentTimeZone = _userTimeZoneService.GetUserTimeZoneInfo(userId);

            var categories = _forumCategoryService.GetCategoriesWithForums(part, VersionOptions.Published);

            var results = new List<DriverResult> ();

            if (displayType.Equals("SummaryAdmin", StringComparison.OrdinalIgnoreCase))
            {
                results.Add(ContentShape("Parts_ForumsHomePage_Menu_SummaryAdmin", () => shapeHelper.Parts_ForumsHomePage_Menu_SummaryAdmin()));
            }
            if (displayType.Equals("Detail", StringComparison.OrdinalIgnoreCase))
            {
                if (userId != null)
                {
                    results.Add(ContentShape("Parts_ForumMenu", () => shapeHelper.Parts_ForumMenu(ForumsHomePagePart: part, ShowRecent: true, ShowMarkAll: true, ReturnUrl: HttpContext.Current.Request.Url.AbsoluteUri)));
                }
            }
            results.Add(ContentShape("Parts_ForumsHomePage",
                () => shapeHelper.Parts_ForumsHomePage(
                    ContentItem: part.ContentItem,
                    ForumsHomePage: part,
                    Categories: categories.Select(cat => _orchardServices.ContentManager.BuildDisplay(cat)).ToList()
            )));

            return Combined(results.ToArray());

        }

        protected override DriverResult Editor(ForumsHomePagePart forumsHomePagePart, dynamic shapeHelper) {

            var results = new List<DriverResult> {
                ContentShape("Parts_ForumsHomePage_Edit", () => {
                    return shapeHelper.EditorTemplate(TemplateName: "Parts.ForumsHomePage.Edit", Model: forumsHomePagePart, Prefix: Prefix);
                })
            };

            return Combined(results.ToArray());
        }

        protected override DriverResult Editor(ForumsHomePagePart forumsHomePagePart, IUpdateModel updater, dynamic shapeHelper)
        {
            var forumSettings = _orchardServices.WorkContext.CurrentSite.As<ForumsSettingsPart>();
            var title = _orchardServices.WorkContext.HttpContext.Request["Title.Title"];

            if (title.Length > forumSettings.ForumsHomeTitleMaximumLength)
            {
                updater.AddModelError("Title.Title", T("The title is too long.  The title can have a maximum of {0} characters but {1} were entered. ", forumSettings.ForumsHomeTitleMaximumLength, title.Length));
            }
            

            updater.TryUpdateModel(forumsHomePagePart, Prefix, null, null);
            return Editor(forumsHomePagePart, shapeHelper);
        }

        protected override void Importing(ForumsHomePagePart part, ImportContentContext context)
        {

        }

        protected override void Exporting(ForumsHomePagePart part, ExportContentContext context)
        {

        }
    }
}