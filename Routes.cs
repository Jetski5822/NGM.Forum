using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using NGM.Forum.Extensions;
using Orchard.Mvc.Routes;

namespace NGM.Forum {
    public class Routes : IRouteProvider {
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
                                                                                      {"area", Constants.LocalArea},
                                                                                      {"controller", "ForumAdmin"},
                                                                                      {"action", "Create"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Forums",
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea},
                                                                                      {"controller", "ForumAdmin"},
                                                                                      {"action", "List"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Forums/{forumId}",
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea},
                                                                                      {"controller", "ThreadAdmin"},
                                                                                      {"action", "List"}
                                                                                  },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Admin/Forums/{forumId}/Threads/{threadId}",
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea},
                                                                                      {"controller", "ThreadAdmin"},
                                                                                      {"action", "Item"}
                                                                                  },
                                                         new RouteValueDictionary (),
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                             new RouteDescriptor {
                                                     Route = new Route(
                                                         "Forums",
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea},
                                                                                      {"controller", "Forum"},
                                                                                      {"action", "List"}
                                                                                  },
                                                         new RouteValueDictionary(),
                                                         new RouteValueDictionary {
                                                                                      {"area", Constants.LocalArea}
                                                                                  },
                                                         new MvcRouteHandler())
                                                 },
                         };
        }
    }
}