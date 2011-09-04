//using System.Web.Mvc;
//using Orchard.Mvc.Filters;
//using Orchard.UI.Resources;

//namespace NGM.Forum.Filters {
//    public class ContentMediaFilter : FilterProvider, IResultFilter {
//        private readonly IResourceManager _resourceManager;

//        public ContentMediaFilter(IResourceManager resourceManager) {
//            _resourceManager = resourceManager;
//        }

//        public void OnResultExecuting(ResultExecutingContext filterContext) {
//            // should only run on a full view rendering result
//            if (!(filterContext.Result is ViewResult))
//                return;
//            _resourceManager.Require("script", "jQuery");
//            _resourceManager.Include("script", "~/Modules/Orchard.MediaPicker/Scripts/MediaPicker.js", null);
//        }

//        public void OnResultExecuted(ResultExecutedContext filterContext) {
//        }
//    }
//}