using System.Linq;
using NGM.Forum.Services;
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
            var forums = _forumService.Get();
            var forumCount = forums.Count();

            if (forumCount > 0) {
                menu.Add(T("Manage Forums"), "3",
                         item => item.Action("List", "ForumAdmin", new { area = "NGM.Forum" }).Permission(Permissions.ManageForums));
            }

            menu.Add(T("New Forum"), "1.0",
                     item =>
                     item.Action("Create", "ForumAdmin", new {area = "NGM.Forum"}).Permission(Permissions.ManageForums));
        }
    }
}