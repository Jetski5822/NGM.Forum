using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using NGM.Forum.Routing;
using Orchard.Mvc.Routes;

namespace NGM.Forum {
    public class Routes : IRouteProvider {
        private readonly IForumPathConstraint _forumPathConstraint;

        public Routes(IForumPathConstraint forumPathConstraint) {
            _forumPathConstraint = forumPathConstraint;
        }

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Forums/Create",
                                                         new RouteValueDictionary {
                                                                                      {"area", "NGM.Forum"},
                                                                                      {"controller", "ForumAdmin"},
                                                                                      {"action", "Create"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "NGM.Forum"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Forums",
                                                         new RouteValueDictionary {
                                                                                      {"area", "NGM.Forum"},
                                                                                      {"controller", "ForumAdmin"},
                                                                                      {"action", "List"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "NGM.Forum"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Forums",
                                                         new RouteValueDictionary {
                                                                                      {"area", "NGM.Forum"},
                                                                                      {"controller", "Forum"},
                                                                                      {"action", "List"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "NGM.Forum"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Forum/Post/Create",
                                                         new RouteValueDictionary {
                                                                                      {"area", "NGM.Forum"},
                                                                                      {"controller", "Post"},
                                                                                      {"action", "Create"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", "NGM.Forum"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Priority = 11,
                                                     Route = new Route(
                                                         "{forumPath}/{threadSlug}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "NGM.Forum"},
                                                                                      {"controller", "Thread"},
                                                                                      {"action", "Item"}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"forumPath", _forumPathConstraint}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "NGM.Forum"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                    Priority = 11,
                                                    Route = new Route(
                                                         "{forumPath}",
                                                         new RouteValueDictionary {
                                                                                      {"area", "NGM.Forum"},
                                                                                      {"controller", "Forum"},
                                                                                      {"action", "Item"},
                                                                                      {"forumPath", ""}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"forumPath", _forumPathConstraint}
                                                                                  },
                                                         new RouteValueDictionary {
                                                                                      {"area", "NGM.Forum"}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 }
                         };
        }
    }
}