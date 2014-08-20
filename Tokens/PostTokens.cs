using System;
using System.Web.Mvc;
using Orchard.ContentManagement;
using Orchard.Localization;
using Orchard.Mvc.Extensions;
using Orchard.Tokens;
using Orchard;
using NGM.Forum.Services;
using NGM.Forum.Models;
using Orchard.Core.Common.Models;
using Orchard.Autoroute.Models;
using Orchard.Mvc.Html;
using System.Web;

namespace NGM.Forum.Tokens
{

    public class PostTokens : ITokenProvider {

        public PostTokens() {
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Content", T("Post"), T("A post made to a forum."))
                .Token("PostMessage", T("Post Message"), T("The text of the post itself"))
                .Token("PostAuthor", T("Post Author"), T("The user that created the post."))
                .Token("PostAuthorEmail", T("Post Author Email"), T("The email address of user that created the post."))
                .Token("PostUserIP", T("Post UserIP"), T("The IP address of the user that created the post."))
                .Token("PostPermalink", T("Post Permalink"), T("The URL of the post."))
                .Token("PostFrontPage", T("Post FrontPage"), T("The root URL of the forums home page that the post belongs to."))
                ;
        }

        public void Evaluate(EvaluateContext context) {
            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

            context.For<IContent>("Content")
                .Token("PostMessage", content => content.As<PostPart>().Text)
                .Chain("PostMessage", "Text", content => content.As<PostPart>().Text)

                //viagra-test-123 is used by https://akismet.com/ to flag a subsmission as spam for testing purposes 
                .Token("PostAuthor", content => content.As<CommonPart>().Owner.UserName) //content => { return "viagra-test-123"; }) 
                .Chain("PostAuthor", "Text", content => content.As<CommonPart>().Owner.UserName) //content => { return "viagra-test-123"; }) 

                .Token("PostAuthorEmail", content => content.As<CommonPart>().Owner.Email)
                .Chain("PostAuthorEmail", "Text", content => content.As<CommonPart>().Owner.Email)

                .Token("PostUserIp", content => content.As<PostPart>().IP)

                .Token("PostPermalink", content => urlHelper.MakeAbsolute(urlHelper.ItemDisplayUrl(content.As<PostPart>().ContentItem)))

                .Token("PostFrontPage", content => urlHelper.MakeAbsolute(urlHelper.ItemDisplayUrl(content.As<PostPart>().ThreadPart.ForumPart.ForumCategoryPart.ForumsHomePagePart)))

                ;
        }

      
        /*
        public class CommentCheckContext
        {
            /// <summary>
            /// The front page or home URL of the instance making the request. For a blog 
            /// or wiki this would be the front page. Note: Must be a full URI, including http://.
            /// </summary>
            public string Url { get; set; }

            /// <summary>
            /// IP address of the comment submitter.
            /// </summary>
            public string UserIp { get; set; }

            /// <summary>
            /// User agent string of the web browser submitting the comment - typically 
            /// the HTTP_USER_AGENT cgi variable. Not to be confused with the user agent 
            /// of your Akismet library.
            /// </summary>
            public string UserAgent { get; set; }

            /// <summary>
            /// The content of the HTTP_REFERER header should be sent here.
            /// </summary>
            public string Referrer { get; set; }

            /// <summary>
            /// The permanent location of the entry the comment was submitted to.
            /// </summary>
            public string Permalink { get; set; }

            /// <summary>
            /// May be blank, comment, trackback, pingback, or a made up value like "registration".
            /// </summary>
            public string CommentType { get; set; }
            public string CommentAuthor { get; set; }
            public string CommentAuthorEmail { get; set; }
            public string CommentAuthorUrl { get; set; }
            public string CommentContent { get; set; }
        }
         */
        public string GetText(IContent content)
        {
            var postText = content.As<PostPart>().Text;
            return postText;
        }

    }
}