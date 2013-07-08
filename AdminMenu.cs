using System.Linq;
using NGM.Forum.Extensions;
using NGM.Forum.Services;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace NGM.Forum {
    public class AdminMenu : INavigationProvider {
        private readonly IForumService _forumService;

        public AdminMenu(IForumService forumService) {
            _forumService = forumService;
        }

        public Localizer T { get; set; }

        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.AddImageSet("forum")
                .Add(T("Forum"), "1.5", BuildMenu);
        }

        private void BuildMenu(NavigationItemBuilder menu) {
            var forums = _forumService.Get(VersionOptions.AllVersions);
            var forumCount = forums.Count();
            var singleForum = forumCount == 1 ? forums.ElementAt(0) : null;

            if (forumCount > 0 && singleForum == null) {
                menu.Add(T("Manage Forums"), "3",
                         item => item.Action("List", "ForumAdmin", new { area = Constants.LocalArea }).Permission(Permissions.ManageForums));
            }
            else if (singleForum != null)
                menu.Add(T("Manage Forum"), "1.0",
                        item => item.Action("Item", "ForumAdmin", new { area = Constants.LocalArea, forumId = singleForum.Id }).Permission(Permissions.ManageOwnForums));
            
            menu.Add(T("New Forum"), "1.1",
                    item =>
                    item.Action("Create", "ForumAdmin", new { area = Constants.LocalArea }).Permission(Permissions.ManageForums));
        }
    }
}