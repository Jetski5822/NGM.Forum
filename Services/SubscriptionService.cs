using System.Collections.Generic;
using System.Linq;
using NGM.Forum.Models;
using Orchard;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.Core.Common.Models;
using Orchard.Core.Title.Models;
using Orchard.Data;
using System;
using Orchard.Security;
using Orchard.Messaging.Services;
using Orchard.Messaging.Models;
using System.Net.Mail;
using System.Net;
using Orchard.Users.Models;
using Orchard.Email.Services;
using Orchard.Mvc.Html;
using System.Web;
using System.Web.Mvc;
using Orchard.Settings;
using Orchard.UI.Navigation;

namespace NGM.Forum.Services {
    public interface ISubscriptionService : IDependency {
        IEnumerable<ThreadPart> GetSubscribedThreads(int userId);
        Dictionary< int, ThreadSubscriptionRecord> GetSubscriptionSettings(int userId);
        void DeleteSubscription(int userId, int threadId);
        void AddSubscription(int userId, int threadId);
        bool IsSubscribed(int userId, int threadId);
        void SendNewPostNotificationByEmail(int userId, int threadId, bool enableNotifyByEmail);
        bool SendEmailNotificationToSubscribers(PostPart postPart);
    }

    public class SubscriptionService : ISubscriptionService {

        private readonly IOrchardServices _orchardServices;
        private readonly IContentManager _contentManager;
        private readonly IRepository<ThreadSubscriptionRecord> _threadSubscriptionRepository;
        private readonly IRepository<UserPartRecord> _userRepository;
        private readonly IUserPreferredCultureService _userPreferredCultureService;
        private readonly ISubscriptionEmailTemplateService _subscriptionEmailTemplateService;
        private readonly IMessagingChannel _emailMessageingChannel;
        private readonly ISiteService _siteService;
        private readonly IPostService _postService;

        public SubscriptionService(
            IOrchardServices orchardServices,
            IContentManager contentManager,
            IRepository<ThreadSubscriptionRecord> threadSubscriptionRepository,
            IRepository<UserPartRecord> userRepository,
            IUserPreferredCultureService userPreferredCultureService,
            ISubscriptionEmailTemplateService subscriptionEmailTemplateService,
            IMessagingChannel emailMessagingChannel,
            ISiteService siteService,
            IPostService postService
       )
        {
            _orchardServices = orchardServices;
            _contentManager = contentManager;
            _threadSubscriptionRepository = threadSubscriptionRepository;
            _userRepository = userRepository;
            _userPreferredCultureService = userPreferredCultureService;
            _subscriptionEmailTemplateService = subscriptionEmailTemplateService;
            _emailMessageingChannel = emailMessagingChannel;
            _siteService = siteService;
            _postService = postService;
        }

        public bool SendEmailNotificationToSubscribers(PostPart postPart)
        {
            var threadPart = postPart.ThreadPart;
            var subscriptionsToThread = _threadSubscriptionRepository.Table.Where(e => e.ThreadId == threadPart.Id && e.EmailUpdates == true).ToList();
            //if there are subscribed users
            if (subscriptionsToThread.Count > 0)
            {
                //it is possible that the subscribed user no longer exists (i.e. has been deleted from the system)
                var subscribedUserIds = subscriptionsToThread.Select( t=>t.UserId ).ToList();
                var userParts = _userRepository.Table.Where(user => subscribedUserIds.Contains(user.Id)).ToList();
                if (userParts.Count > 0)
                {
                    var usersCultures = _userPreferredCultureService.GetUsersCultures(userParts.Select( user=>user.Id).ToList());
                    foreach (var culture in usersCultures.Keys)
                    {
                        var userList = userParts.Where( user=> usersCultures[culture].Contains( user.Id)).ToList();

                        //it would be possible to use the BCC to send to all subscribed users provided that the list is 'reasonable' in length 
                        //i.e. 100 or less otherwise the mail server may truncate or refuse to send.
                        //that approach however won't work with the orchard email channel because it requires a MailMessage.To recipient and misses the 
                        //case that an email is sent BCC only.
                        foreach (var user in userList)
                        {
                            if (!String.IsNullOrWhiteSpace(user.Email))
                            {
                                var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
                                string baseUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);


                                var postUrl = baseUrl + urlHelper.Action("ViewPostInThread", "Thread", new { postId=postPart.Id});
                               
                                
                                var translationEmail = _subscriptionEmailTemplateService.GetEmailTemplateTranslation(culture, postPart.ThreadPart.Title, postPart.Text, postPart.Format, postUrl, _orchardServices.WorkContext.CurrentSite.BaseUrl);
                                MessageContext messageContext = new MessageContext();
                                messageContext.MailMessage.Subject = translationEmail.Title;
                                messageContext.MailMessage.Body = translationEmail.BodyPlainText;
                                messageContext.MailMessage.To.Add(user.Email);
                                AlternateView altView = AlternateView.CreateAlternateViewFromString(translationEmail.BodyHtml, System.Text.Encoding.UTF8, "text/html");
                                messageContext.MailMessage.AlternateViews.Add(altView);
                                messageContext.MailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                                messageContext.Service = EmailMessagingChannel.EmailService;
                                _emailMessageingChannel.SendMessage(messageContext);
                            }
                        }

                    }
                }
            }

            return true;
        }


        public bool IsSubscribed(int userId, int threadId)
        {
            var subscriptionsFound = _threadSubscriptionRepository.Table.Where(e => e.UserId == userId && e.ThreadId == threadId).ToList();
            return (subscriptionsFound.Count() > 0);
        }

        public Dictionary<int, ThreadSubscriptionRecord> GetSubscriptionSettings(int userId)
        {
            return _threadSubscriptionRepository.Table.Where(e => e.UserId == userId).ToDictionary(i => i.ThreadId, i => i);
        }

        public IEnumerable<ThreadPart> GetSubscribedThreads(int userId)
        {

            var subscribedThreads = _threadSubscriptionRepository.Table.Where( e=>e.UserId == userId ).ToList();
            var subscribedThreadIDs = subscribedThreads.Select(t => t.ThreadId).ToList();
            
            //var x = _contentManager.GetMany(subscribedThreadIDs, VersionOptions.Published), new QueryHints().ExpandRecords<AutoroutePartRecord, TitlePartRecord, CommonPartRecord>();
            var threadPartList = _contentManager
            .Query<ThreadPart, ThreadPartRecord>(VersionOptions.Published)
            .WithQueryHints(new QueryHints().ExpandRecords<AutoroutePartRecord, TitlePartRecord, CommonPartRecord>())
            .Where(x => subscribedThreadIDs.Contains(x.Id)).List().ToList();

/* from the thread service.. used to get the threads for a forum
            return _contentManager.Query()
            .Join<ThreadPartRecord>()
            .OrderByDescending(o => o.IsSticky)
            .Join<CommonPartRecord>()
            .OrderByDescending(o => o.ModifiedUtc)
            .ForPart<ThreadPart>().List().ToList();
            //.Slice(skip, count)
            //.ToList();
            */

            //order them based on the subscription date and return
            /*
            var sorted = from threadPart in threadPartList
                                 join subscription in subscribedThreads
                                 on threadPart.Id equals subscription.ThreadId
                                 orderby subscription.SubscribedDate
                                 select threadPart;
             */

            return threadPartList;
        }

        public void DeleteSubscription(int userId, int threadId)
        {
            var threadsToDelete = _threadSubscriptionRepository.Table.Where(e => e.UserId == userId && e.ThreadId == threadId).ToList();
            foreach (var t in threadsToDelete)
            {
                _threadSubscriptionRepository.Delete(t);
            }
        }

        public void AddSubscription(int userId, int threadId)
        {
            //check there is no existing subscription before adding a new one
            var subscriptionExisting = _threadSubscriptionRepository.Table.Where(e => e.UserId == userId && e.ThreadId == threadId).ToList();

            //if the subscription already exists do nothing
            if (subscriptionExisting == null || subscriptionExisting.Count == 0)
            {
                ThreadSubscriptionRecord subscription = new ThreadSubscriptionRecord{
                     ThreadId = threadId,
                      UserId = userId,
                       SubscribedDate = DateTime.UtcNow,
                       EmailUpdates = false
                };
                _threadSubscriptionRepository.Create( subscription );
            }            
        }

        public void SendNewPostNotificationByEmail(int userId, int threadId, bool enableNotifyByEmail)
        {
            var subscription = _threadSubscriptionRepository.Table.Where(e => e.UserId == userId && e.ThreadId == threadId).FirstOrDefault();                
            if (subscription != null)
            {
                subscription.EmailUpdates = enableNotifyByEmail;
                _threadSubscriptionRepository.Update(subscription);
            }
        }
    }
}