using System.Linq;
using System.Web.Mvc;
using NGM.Forum.Extensions;
using NGM.Forum.Models;
using NGM.Forum.ViewModels;
using NGM.Forum.Services;
using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.Security;
using Orchard.Settings;
using Orchard.Themes;
using Orchard.UI.Navigation;
using Orchard.UI.Notify;
using System.Collections.Generic;
using Orchard.UI.Zones;

namespace NGM.Forum.Controllers {
    [Themed]
    [ValidateInput(false)]
    public class SubscriptionEmailTemplateAdmin : Controller
    {
        private readonly IOrchardServices _orchardServices;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthenticationService _authenticationService;
        private readonly ISubscriptionEmailTemplateService _subscriptionNotificationTranslationService;

        public SubscriptionEmailTemplateAdmin(
            IOrchardServices orchardServices,
            IAuthorizationService authorizationService,
            IAuthenticationService authenticationService,
            ISubscriptionEmailTemplateService subscriptionNotificationTranslationService
            )
        {
            _orchardServices = orchardServices;
            _authorizationService = authorizationService;
            _authenticationService = authenticationService;
            _subscriptionNotificationTranslationService = subscriptionNotificationTranslationService;
            T = NullLocalizer.Instance;
        }


        public Localizer T { get; set; }

        [HttpGet]
        public ActionResult EditSubscriptionEmailTemplate()
        {
            var rec = _subscriptionNotificationTranslationService.Get();
            return View(rec);
        }

        [HttpPost]
        public ActionResult EditSubscriptionEmailTemplate(SubscriptionEmailTemplateRecord rec)
        {
            if (TryUpdateModel(rec))
            {
                _subscriptionNotificationTranslationService.Update( rec.Title, rec.BodyPlainText, rec.BodyHtml);
                _orchardServices.Notifier.Add(NotifyType.Information, T("The email template was successfully updated."));                
            };

            return View(rec);
            

        }
    }


       
}