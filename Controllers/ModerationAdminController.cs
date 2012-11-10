using System.Web.Mvc;
using System.Linq;
using NGM.Forum.Models;
using NGM.Forum.Services;
using NGM.Forum.ViewModels;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Services;
using Orchard.UI.Admin;
using Orchard.UI.Notify;

namespace NGM.Forum.Controllers {

    [ValidateInput(false), Admin]
    public class ModerationAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly IModerationService _moderationService;

        public ModerationAdminController(IOrchardServices orchardServices, 
            IContentManager contentManager,
            IModerationService moderationService) {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _moderationService = moderationService;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public ActionResult Index() {
            if (!_orchardServices.Authorizer.Authorize(Permissions.ApproveUnapprove, T("Not allowed to approve/unapprove")))
                return new HttpUnauthorizedResult();

            var parts = _moderationService.Get(ModerationOptions.NotApproved);

            ModerationIndexViewModel viewModel = new ModerationIndexViewModel {Items = parts.};

            return View();
        }

        //public ActionResult Approve(int moderationId, bool isApproved, string returnUrl) {
        //    if (!_orchardServices.Authorizer.Authorize(Permissions.ApproveUnapprove, T("Not allowed to approve/unapprove")))
        //        return new HttpUnauthorizedResult();

        //    var moderationPart = _contentManager.Get(moderationId, VersionOptions.Published).As<ModerationPart>();

        //    if (moderationPart == null)
        //        return HttpNotFound(T("Could not find thread").ToString());

        //    _moderationService.Approve(moderationPart, isApproved);
            
        //    _orchardServices.Notifier.Information(isApproved ? T("Content has been Approved.") : T("Content has been Unapproved."));

        //    return this.RedirectLocal(returnUrl, "~/");
        //}

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}