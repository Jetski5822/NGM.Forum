using System.Linq;
using System.Web.Mvc;
using NGM.Forum.Models;
//using NGM.Forum.Routing;
using NGM.Forum.Services;
using NGM.Forum.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Settings;
using Orchard.UI.Admin;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using Orchard.Mvc.Extensions;

namespace NGM.Forum.Controllers {

    [ValidateInput(false), Admin]
    public class PostAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly IForumService _forumService;
        private readonly IThreadService _threadService;
        private readonly ISiteService _siteService;
        private readonly IPostService _postService;
        //private readonly IForumPathConstraint _forumPathConstraint;

        public PostAdminController(IOrchardServices orchardServices,
            IForumService forumService,
            IThreadService threadService,
            ISiteService siteService,
            IShapeFactory shapeFactory,
            IPostService postService
            //IForumPathConstraint forumPathConstraint
            ) {
            _orchardServices = orchardServices;
            _forumService = forumService;
            _threadService = threadService;
            _siteService = siteService;
            _postService = postService;
            //_forumPathConstraint = forumPathConstraint;

            T = NullLocalizer.Instance;
            Shape = shapeFactory;
        }

        dynamic Shape { get; set; }
        public Localizer T { get; set; }

        public ActionResult Approving(int postId, bool isApproved, string returnUrl) {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ApprovingPost, T("Not allowed to approve/unapprove Post")))
                return new HttpUnauthorizedResult();

            var postPart = _postService.Get(postId, VersionOptions.Published).As<PostPart>();

            if (postPart == null)
                return HttpNotFound(T("could not find post").ToString());

            postPart.Approved = isApproved;

            _orchardServices.Notifier.Information(isApproved ? T("Post has been Approved.") : T("Post has been Unapproved."));

            return this.RedirectLocal(returnUrl, "~/");
        }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}