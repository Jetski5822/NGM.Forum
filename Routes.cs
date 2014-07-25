using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using NGM.Forum.Extensions;
using Orchard.Mvc.Routes;

namespace NGM.Forum
{
    public class Routes : IRouteProvider
    {
        public void GetRoutes(ICollection<RouteDescriptor> routes)
        {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes()
        {
            return new[] {
                
	                new RouteDescriptor {
                    Route = new Route(
                        "Admin/Forums/Create/{type}",
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea},
                                                    {"controller", "ForumAdmin"},
                                                    {"action", "Create"},
                                                    {"type", UrlParameter.Optional}
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


                /*
                new RouteDescriptor {
                    Route = new Route(
                        "Forums",
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea},
                                                    {"controller", "ForumCategory"},
                                                    {"action", "List"}
                                                },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea}
                                                },
                        new MvcRouteHandler())
                },
                */
                /*
                new RouteDescriptor {
                    Route = new Route(
                        "Forums/{controller}/{action}/{id}",
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea},
                                                    {"controller", "controller"},
                                                    {"action", "action"}
                                                },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea}
                                                },
                        new MvcRouteHandler())
                },
                 */
                new RouteDescriptor { 
                    Name="ForumSubscriptions",                     
                    Route = new Route( 
                        "Forums/Subscriptions", 
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea},
                                                    {"controller", "Subscription"},
                                                    {"action", "ViewSubscriptions"}
                                                },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea}
                                                },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Forums/Subscription/AddSubscription/{threadId}",
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea},
                                                    {"controller", "Subscription"},
                                                    {"action", "AddSubscription"}
                                                },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea}
                                                },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Forums/Subscription/DeleteSubscription/{threadId}",
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea},
                                                    {"controller", "Subscription"},
                                                    {"action", "DeleteSubscription"}
                                                },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea}
                                                },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Forums/Discussions",
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea},
                                                    {"controller", "Discussions"},
                                                    {"action", "ListDiscussions"}
                                                },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea}
                                                },
                        new MvcRouteHandler())
                },
                                 
                 new RouteDescriptor {
                    Route = new Route(
                        "Forums/ReportInappropriatePost/ResolveReport/{reportId}",
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea},
                                                    {"controller", "ReportPostAdmin"},
                                                    {"action", "ResolveReport"}
                                                },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea}
                                                },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Forums/ReportInappropriatePost/{postId}",
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea},
                                                    {"controller", "ReportPost"},
                                                    {"action", "ReportInappropriatePost"}
                                                },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea}
                                                },
                        new MvcRouteHandler())
                },
        

                new RouteDescriptor {
                    Route = new Route(
                        "Forums/ReportPostAdmin/ListPostReports/",
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea},
                                                    {"controller", "ReportPostAdmin"},
                                                    {"action", "ListPostReports"}
                                                },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea}
                                                },
                        new MvcRouteHandler())
                },
                
                new RouteDescriptor {
                    Route = new Route(
                        "Forums/ReportInappropriatePost/ResolveReport/{reportId}",
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea},
                                                    {"controller", "ReportPostAdmin"},
                                                    {"action", "ResolveReport"}
                                                },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea}
                                                },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Forums/ListNewPostsByThread/{forumsHomeId}",
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea},
                                                    {"controller", "ForumsHomePage"},
                                                    {"action", "ListNewPostsByThread"}
                                                },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea}
                                                },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Forums/ListNewPosts/{forumsHomeId}",
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea},
                                                    {"controller", "ForumsHomePage"},
                                                    {"action", "ListNewPosts"}
                                                },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea}
                                                },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Forums/MarkAllRead/{forumsHomeId}",
                        new RouteValueDictionary {
                                                    {"area", Constants.LocalArea},
                                                    {"controller", "ForumsHomePage"},
                                                    {"action", "MarkAllRead"}
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