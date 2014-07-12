using System.Web.Mvc;
using NGM.Forum.Models;
using Orchard.ContentManagement;
using Orchard.UI.Navigation;
using Orchard.Utility.Extensions;
using Orchard.Mvc.Html;
namespace NGM.Forum.Extensions {
    public static class UrlHelperExtensions {

        // Default Route
        public static string Forums(this UrlHelper urlHelper, ForumPart forumPart) {
            return ContentItemExtensions.ItemDisplayUrl(urlHelper, forumPart.ContentItem);
            //return urlHelper.Action("List", "Forum", new { area = Constants.LocalArea });
        }

        /*Forums' Home Page */
        public static string ForumsHomePagesListForAdmin(this UrlHelper urlHelper)
        {
            return urlHelper.Action("ListForumsHomePages", "ForumAdmin", new { area = Constants.LocalArea });
        }
        public static string ForumCreateForumsHomePageForAdmin(this UrlHelper urlHelper)
        {
            return urlHelper.Action("CreateForumsHomePage", "ForumAdmin", new { area = Constants.LocalArea });
        }

        /* Forum Categories */
        public static string CategoriesForAdmin(this UrlHelper urlHelper, ForumsHomePagePart forumsHomePagePart)
        {
            return urlHelper.Action("ListForumCategories", "ForumAdmin", new { forumsHomePagePartId = forumsHomePagePart.Id, area = Constants.LocalArea });
        }

        public static string CategoryForAdmin(this UrlHelper urlHelper, ForumCategoryPart forumCategoryPart)
        {
            return urlHelper.Action("ForumCategoryItem", "ForumAdmin", new { forumCategoryPartId = forumCategoryPart.Id, area = Constants.LocalArea });
        }

        //would be better for typing to use the parameter ForumsHomePagePart instead of the id, but there are some cases where the part is not available
        public static string CategoryCreateForAdmin(this UrlHelper urlHelper, ForumsHomePagePart forumsHomePagePart)
        {
            return urlHelper.Action("CreateForumCategory", "ForumAdmin", new { forumsHomePagePartId = forumsHomePagePart.Id, area = Constants.LocalArea });
        }

        public static string CategoryEditForAdmin(this UrlHelper urlHelper, ForumCategoryPart forumCategoryPart)
        {
            return urlHelper.Action("EditForumCategory", "ForumAdmin", new { forumCategoryPartId = forumCategoryPart.Id, area = Constants.LocalArea });
        }
        /* Forum */

        public static string ForumForAdmin(this UrlHelper urlHelper, ForumPart forumPart) {
            return urlHelper.Action("ForumItem", "ForumAdmin", new { forumId = forumPart.Id, area = Constants.LocalArea });
        }

        public static string ForumsForAdmin(this UrlHelper urlHelper, ForumCategoryPart forumCategoryPart) {
            return urlHelper.Action("ListForums", "ForumAdmin", new { forumCategoryPartId = forumCategoryPart.Id, area = Constants.LocalArea });
        }

        public static string ForumCreateForAdmin(this UrlHelper urlHelper, ForumCategoryPart forumCategoryPart) {
            return urlHelper.Action("CreateForum", "ForumAdmin", new { forumCategoryPartId = forumCategoryPart.Id, area = Constants.LocalArea });
        }

        public static string ForumSelectTypeForAdmin(this UrlHelper urlHelper) {
            return urlHelper.Action("SelectType", "ForumAdmin", new { area = Constants.LocalArea });
        }

        public static string ForumEdit(this UrlHelper urlHelper, ForumPart forumPart) {
            return urlHelper.Action("EditForum", "ForumAdmin", new { forumId = forumPart.Id, area = Constants.LocalArea });
        }

        public static string ForumView(this UrlHelper urlHelper, ForumPart forumPart)
        {
            return urlHelper.Action("Item", "Forum", new { forumId = forumPart.Id, area = Constants.LocalArea });
        }

        /* Thread */

        public static string ThreadForAdmin(this UrlHelper urlHelper, ThreadPart threadPart) {
            return urlHelper.Action("Item", "Thread", new { forumId = threadPart.ForumPart.Id, threadId = threadPart.Id,  area = Constants.LocalArea });
        }

        public static string ThreadMove(this UrlHelper urlHelper, ThreadPart threadPart) {
            //return to the parent forum since the thread itself will be moved
            return urlHelper.Action("Move", "Thread", new { threadId = threadPart.Id, area = Constants.LocalArea, returnUrl = ForumView( urlHelper, threadPart.ForumPart) });
        }

        public static string ThreadClose(this UrlHelper urlHelper, ThreadPart threadPart) {
            return urlHelper.Action("Close", "Thread", new { threadId = threadPart.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }

        public static string ThreadOpen(this UrlHelper urlHelper, ThreadPart threadPart) {
            return urlHelper.Action("Open", "Thread", new { threadId = threadPart.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }

        public static string ThreadCreate(this UrlHelper urlHelper, int forumId) {
            return urlHelper.Action("Create", "Thread", new { forumId = forumId, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }

        public static string ThreadCreate(this UrlHelper urlHelper, ForumPart forumPart) {
            return urlHelper.Action("Create", "Thread", new { forumId = forumPart.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }

        public static string ThreadView(this UrlHelper urlHelper, ThreadPart threadPart) {
            return urlHelper.Action("Item", "Thread", new { forumId = threadPart.ForumPart.Id, threadId = threadPart.Id, area = Constants.LocalArea });
        }

        public static string ThreadView(this UrlHelper urlHelper, ThreadPart threadPart, Pager pager) {
            return urlHelper.Action("Item", "Thread", new { forumId = threadPart.ForumPart.Id, threadId = threadPart.Id, page = pager.Page, area = Constants.LocalArea });
        }

        public static string ThreadDelete(this UrlHelper urlHelper, ThreadPart threadPart) {
            return urlHelper.Action("Delete", "Post", new { contentId = threadPart.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }

        /* Post */
        public static string PostEdit(this UrlHelper urlHelper, PostPart postPart)
        {
            return urlHelper.Action("Edit", "Post", new { contentId = postPart.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }

        public static string PostReply(this UrlHelper urlHelper, PostPart postPart) {
            return PostCreateByContent(urlHelper, postPart);
        }

        public static string PostReplyWithQuote(this UrlHelper urlHelper, PostPart postPart) {
            return urlHelper.Action("CreateWithQuote", "Post", new { contentId = postPart.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString()});
        }

        public static string PostView(this UrlHelper urlHelper, PostPart postPart) {
            return string.Format("{0}#{1}", ThreadView(urlHelper, postPart.ThreadPart), postPart.Id);
        }

        public static string PostView(this UrlHelper urlHelper, PostPart postPart, Pager pager) {
            if (pager.Page >= 2)
                return string.Format("{0}#{1}", ThreadView(urlHelper, postPart.ThreadPart, pager), postPart.Id);
            else
                return PostView(urlHelper, postPart);
        }

        public static string PostDelete(this UrlHelper urlHelper, PostPart postPart) {
            return urlHelper.Action("Delete", "Post", new { contentId = postPart.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }

        public static string PostMarkInappropriate(this UrlHelper urlHelper, PostPart postPart)
        {

            return urlHelper.Action("MarkInappropriate", "ReportPostAdmin", new { contentId = postPart.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }

        public static string PostRemoveInappropriate(this UrlHelper urlHelper, PostPart postPart)
        {
            return urlHelper.Action("RemoveInappropriate", "ReportPostAdmin", new { contentId = postPart.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }


        private static string PostCreateByContent(this UrlHelper urlHelper, IContent content) {
            return urlHelper.Action("Create", "Post", new { contentId = content.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }

        public static string PostViewEditHistory(this UrlHelper urlHelper, PostPart postPart)
        {
            return urlHelper.Action("ViewPostEditHistory", "Post", new { postId = postPart.Id, area = Constants.LocalArea, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }
        /* External */
        public static string DashboardForAdmin(this UrlHelper urlHelper) {
            return urlHelper.Action("index", "admin", new {area = "Dashboard"});
        }

        public static string ForumReportPost(this UrlHelper urlHelper, int postId)
        {
            return urlHelper.Action("ReportInappropriatePost", "ReportPost", new { area = Constants.LocalArea, postId = postId, returnUrl = urlHelper.RequestContext.HttpContext.Request.ToUrlString() });
        }

        public static string ResolveReport(this UrlHelper urlHelper, int reportId)
        {
            return urlHelper.Action("ResolveReport", "ReportPostAdmin", new { area = Constants.LocalArea, reportId = reportId});
        }        
    }
}