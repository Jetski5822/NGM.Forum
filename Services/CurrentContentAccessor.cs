using System.Web.Routing;
/*
 * From http://www.skywalkersoftwaredevelopment.net/blog/getting-the-current-content-item-in-orchard
 */
using Orchard;
using Orchard.Alias;
using Orchard.ContentManagement;
using Orchard.Core.Common.Utilities;

namespace NGM.Forum.Services
{
    public interface ICurrentContentAccessor : IDependency
    {
        ContentItem CurrentContentItem { get; }
    }

    public class CurrentContentAccessor : ICurrentContentAccessor
    {
        private readonly LazyField<ContentItem> _currentContentItemField = new LazyField<ContentItem>();
        private readonly IContentManager _contentManager;
        private readonly RequestContext _requestContext;
        private readonly IAliasService _aliasService;

        public CurrentContentAccessor(IContentManager contentManager, RequestContext requestContext, IAliasService aliasService)
        {
            _contentManager = contentManager;
            _requestContext = requestContext;
            _aliasService = aliasService;
            _currentContentItemField.Loader(GetCurrentContentItem);

        }

        public ContentItem CurrentContentItem
        {
            get { return _currentContentItemField.Value; }
        }

        private ContentItem GetCurrentContentItem()
        {
            var contentId = GetCurrentContentItemId();
            return contentId == null ? null : _contentManager.Get(contentId.Value);
        }

        private int? GetCurrentContentItemId()
        {
            //get content item ID by RouteData
            object id;
            if (_requestContext.RouteData.Values.TryGetValue("id", out id))
            {
                int contentId;
                if (int.TryParse(id as string, out contentId))
                    return contentId;
            }
            if (_requestContext.RouteData.Values.TryGetValue("forumId", out id))
            {
                int contentId;
                if (int.TryParse(id as string, out contentId))
                    return contentId;
            }
            //get content item ID by path
            object path;
            if (_requestContext.RouteData.Values.TryGetValue("path", out path))
            {
                var pathStr = path.ToString().TrimEnd(new[] { '/' });

                var routes = _aliasService.Get(pathStr);

                if (routes != null && routes.ContainsKey("Id")) return int.Parse(routes["Id"].ToString());
            }

            return null;
        }
    }
}