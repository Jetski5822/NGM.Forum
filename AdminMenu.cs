using System.Linq;
using NGM.Forum.Extensions;
using NGM.Forum.Services;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace NGM.Forum {
    public class AdminMenu : INavigationProvider {
        private readonly IForumService _forumService;
        private readonly IForumCategoryService _forumCategoryService;
        private readonly IForumsHomePageService _forumForumsHomePageService;

        public AdminMenu(
            IForumsHomePageService forumForumsHomePageService,
            IForumCategoryService forumCategoryService,
            IForumService forumService
        ) {
            _forumForumsHomePageService = forumForumsHomePageService;
            _forumCategoryService = forumCategoryService;
            _forumService = forumService;                        
        }

        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("forum")
                .Add(T("Forum"), "1", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu) {
            //want to sequence the showing of the menus from start up
            //first require a forum root to be made via the create forum root menu 
            //then the manage forum root menu can be shown
            //once a root exists, then allow a category to be made via the create category menu
            //once a category is made, show the manage 
            //once a categor exists, then allow a forum to be made  before showing the categ

            var forums = _forumService.Get(VersionOptions.Latest); //TODO: this with VersionOptions.AllVersions ... should it be? Latest would be more appropriate?
            var forumCount = forums.Count();

            var singleForum = forumCount == 1 ? forums.ElementAt(0) : null;

            var forumsHomePages = _forumForumsHomePageService.Get(VersionOptions.Latest);
            var forumsHomePagesCount = forumsHomePages.Count();

            var forumCategories = _forumCategoryService.Get(VersionOptions.Latest);
           // var forumCategoriesCount = forumCategories.Count();

            menu.Add(T("blah"), "1.0",
                item => item.Action("ListForumsHomePages", "ForumAdmin", new { area = Constants.LocalArea }).Permission(Permissions.ManageForums).Permission(Permissions.ManageOwnForums));
            //by default this menu will also be shown.  It is the starting point for creating forums
            menu.Add(T("Manage Forums"), "1.1",
                 item => item.Action("ListForumsHomePages", "ForumAdmin", new { area = Constants.LocalArea }).Permission(Permissions.ManageForums).Permission(Permissions.ManageOwnForums));


            /*
            if (forumsHomePagesCount > 0)
            {
                menu.Add(T("Manage Forum Categories"), "1.2",
                     item => item.Action("ListForumCategories", "ForumAdmin", new { area = Constants.LocalArea }).Permission(Permissions.ManageForumCategories));
            }

            if (forumCategoriesCount > 0)
            {
                menu.Add(T("Manage Forum Topics"), "1.3",
                    item => item.Action("ListForums", "ForumAdmin", new { area = Constants.LocalArea }).Permission(Permissions.ManageForums));
            }
            */
            menu.Add(T("Reported Posts"), "1.4",
                    item => item.Action("ListPostReports", "ReportPostAdmin", new { area = Constants.LocalArea }).Permission(Permissions.ModerateInappropriatePosts).Permission(Permissions.ModerateOwnInappropriatePosts));
            menu.Add(T("Subscription Translations"), "1.5",
                    item => item.Action("EditSubscriptionEmailTemplate", "SubscriptionEmailTemplateAdmin", new { area = Constants.LocalArea }).Permission(Permissions.ManageForums));
        }
    }
}