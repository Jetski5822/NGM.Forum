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
using Orchard.Localization.Services;
using System.Text.RegularExpressions;
using System.Web;

namespace NGM.Forum.Services {

    public interface ISubscriptionEmailTemplateService : IDependency
    {
        void Update(string title, string bodyPlainText, string bodyHtml);
        SubscriptionEmailTemplateRecord Get();
        SubscriptionEmailTemplateTranslation GetEmailTemplateTranslation( string culture , string threadTitle, string postText, string postType, string postUrl, string siteName );
    }

    public class SubscriptionEmailTemplateService : ISubscriptionEmailTemplateService {

        /*
         *   <li>[ThreadTitle] - the name of the thread the new post occured in</li>
        <li>[PostText] - the text of the new post</li>
        <li>[PostLink] - An href tag linking to the post. Useable only in the html email body.</li>
        <li>[PostUrl] - is replaced by the http:// url to the new post. Suitable for the plan text email.</li>
        <li>[SiteName] - The site name as found in orchard settings.</li>
         */
        private List<string> templateTokens = new List<string>{ "[ThreadTitle]", "[PostText]", "[PostUrl]", "[SiteName]" };

        private readonly IContentManager _contentManager;
        private readonly IRepository<SubscriptionEmailTemplateRecord> _subscriptionNotificationTranslationRepository;
        private readonly ILocalizedStringManager _localizedStringManager;

        private enum BodyType { PlainText, Html };

        public SubscriptionEmailTemplateService(
            IContentManager contentManager,
            IRepository<SubscriptionEmailTemplateRecord> subscriptionNotificationTranslationRepository,
            ILocalizedStringManager localizedStringManager
        )
        {
            _contentManager = contentManager;
            _subscriptionNotificationTranslationRepository = subscriptionNotificationTranslationRepository;
            _localizedStringManager = localizedStringManager;
        }


        public SubscriptionEmailTemplateRecord Get()
        {
            return _subscriptionNotificationTranslationRepository.Table.First();
        }


        public void Update(string title, string bodyPlainText, string bodyHtml)
        {
            //check there is no existing subscription before adding a new one
            var rec = _subscriptionNotificationTranslationRepository.Table.First();
            rec.Title = title;
            rec.BodyPlainText = bodyPlainText;
            rec.BodyHtml = bodyHtml;

           _subscriptionNotificationTranslationRepository.Update(rec);           
        }

        public SubscriptionEmailTemplateTranslation GetEmailTemplateTranslation( string culture , string threadTitle, string postText, string postType, string postUrl, string siteName ) {

            var template = this.Get();
            var title = template.Title;
            var bodyPlainText = template.BodyPlainText;
            var bodyHtml = template.BodyHtml;
            
            //title of email will always be plain text
            var localizedTitle = ReplaceTemplateTokens( BodyType.PlainText, culture, _localizedStringManager.GetLocalizedString("ngm.forum.subscriptionemailtemplate", title, culture), threadTitle, "text", postType, postUrl, siteName); ;

            //get the translation of the body in both html and plain text
            var localizedPlainTextBody = ReplaceTemplateTokens( BodyType.PlainText, culture, LocalizePlainTextBody( bodyPlainText, culture ), threadTitle, postText, postType, postUrl, siteName );
            var localizedHtmlBody = ReplaceTemplateTokens(BodyType.Html, culture, LocalizeHtmlBody(bodyHtml, culture), threadTitle, postText, postType, postUrl, siteName);

            var translation = new SubscriptionEmailTemplateTranslation
            {
                Culture = culture, // really not needed.. caller already knows what it is and we don't know if the translation worked or not
                Title = localizedTitle,
                BodyPlainText = localizedPlainTextBody,
                BodyHtml = localizedHtmlBody
            };

            return translation;
            
        }
        

        private string ReplaceTemplateTokens(BodyType bodyType, string culture,  string text, string threadTitle, string postText, string postType, string postUrl, string siteName)
        {
            foreach ( var token in this.templateTokens ) {
                int pos =0;
                if ( (pos = text.IndexOf( token )) > 0 ) {
                    switch( token){
                        case "[ThreadTitle]":
                            text = text.Replace("[ThreadTitle]", threadTitle );
                        break;
                        case "[PostText]":
                            //don't want to insert an html post into the text version of the email.
                            if (bodyType == BodyType.PlainText && postType.ToLowerInvariant().Equals(("html")))
                            {
                                text = text.Replace("[PostText]", _localizedStringManager.GetLocalizedString("ngm.forum.subscriptionemailtemplate", "[Sorry this post cannot be displayed in the email because the display type is incompatible with your email client.]", culture));
                            }
                            else
                            {
                                text = text.Replace("[PostText]", postText);
                            }
                            break;
                        case "[PostUrl]":
                        text = text.Replace("[PostUrl]", postUrl);
                        break;
                        case "[SiteName]":
                        text = text.Replace("[SiteName]", siteName);
                        break;
                    }
                }
            }
            return text;
        }
        //translate based on a new line...
        private string LocalizePlainTextBody(string body, string culture){
            var originalLines = body.Split(new string[] { System.Environment.NewLine }, StringSplitOptions.None);
            List<string> translatedLines = new List<string>();
            foreach( var line in originalLines ){
                translatedLines.Add(_localizedStringManager.GetLocalizedString("ngm.forum.subscriptionemailtemplate", line, culture));
            }

            String translation ="";
            foreach( var line in translatedLines ) {
                translation += line + Environment.NewLine;
            }
            return translation;
        }


        //translate based on a new line...
        private string LocalizeHtmlBody(string body, string culture){

            //Regex ItemRegex = new Regex("@T\\(\"(.*?)\"\\);", RegexOptions.Compiled);
            Regex ItemRegex = new Regex("\\{\"(.*?)\"\\};", RegexOptions.Compiled);
            foreach (Match ItemMatch in ItemRegex.Matches(body))
            {
                body = body.Replace(ItemMatch.Value, _localizedStringManager.GetLocalizedString("ngm.forum.subscriptionemailtemplate", ItemMatch.Groups[1].Value, culture));
            }

            return body;
        }

    }
}