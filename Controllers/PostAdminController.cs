using System.Web.Mvc;
using NGM.Forum.Models;
using NGM.Forum.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Services;
using Orchard.UI.Admin;
using Orchard.UI.Notify;
using Orchard.Mvc.Extensions;

namespace NGM.Forum.Controllers {

    [ValidateInput(false), Admin]
    public class PostAdminController : Controller, IUpdateModel {
        private readonly IOrchardServices _orchardServices;
        private readonly IPostService _postService;
        private readonly IClock _clock;

        public PostAdminController(IOrchardServices orchardServices,
            IPostService postService,
            IClock clock) {
            _orchardServices = orchardServices;
            _postService = postService;
            _clock = clock;

            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        bool IUpdateModel.TryUpdateModel<TModel>(TModel model, string prefix, string[] includeProperties, string[] excludeProperties) {
            return TryUpdateModel(model, prefix, includeProperties, excludeProperties);
        }

        void IUpdateModel.AddModelError(string key, LocalizedString errorMessage) {
            ModelState.AddModelError(key, errorMessage.ToString());
        }
    }
}