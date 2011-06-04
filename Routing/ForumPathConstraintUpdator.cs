using System.Linq;
using JetBrains.Annotations;
using NGM.Forum.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Environment;
using Orchard.Tasks;

namespace NGM.Forum.Routing {
    [UsedImplicitly]
    public class ForumPathConstraintUpdator : IOrchardShellEvents, IBackgroundTask {
        private readonly IForumPathConstraint _forumPathConstraint;
        private readonly IForumService _forumService;

        public ForumPathConstraintUpdator(IForumPathConstraint forumPathConstraint, IForumService forumService) {
            _forumPathConstraint = forumPathConstraint;
            _forumService = forumService;
        }
        
        void IOrchardShellEvents.Activated() {
            Refresh();
        }

        void IOrchardShellEvents.Terminating() {
        }

        void IBackgroundTask.Sweep() {
            Refresh();
        }

        private void Refresh() {
            _forumPathConstraint.SetPaths(_forumService.Get().Select(b => b.As<IRoutableAspect>().Slug));
        }
    }
}