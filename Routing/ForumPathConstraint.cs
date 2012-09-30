using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using JetBrains.Annotations;
using Orchard.Logging;

namespace NGM.Forum.Routing {
    [UsedImplicitly]
    public class ForumPathConstraint : IForumPathConstraint {
        private readonly ConcurrentDictionary<string, string> _paths = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public ForumPathConstraint() {
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void SetPaths(IEnumerable<string> paths) {
            _paths.Clear();
            foreach (var path in paths) {
                AddPath(path);
            }

            Logger.Debug("Forum paths: {0}", string.Join(", ", paths.ToArray()));
        }

        public string FindPath(string path) {
            string actual;
            // path can be null for homepage
            path = path ?? String.Empty;

            return _paths.TryGetValue(path, out actual) ? actual : path;
        }

        public void AddPath(string path) {
            // path can be null for homepage
            path = path ?? String.Empty;

            _paths[path] = path;
        }

        public void RemovePath(string path) {
            // path can be null for homepage
            path = path ?? String.Empty;

            _paths.TryRemove(path, out path);
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection) {
            if (routeDirection == RouteDirection.UrlGeneration)
                return true;

            object value;
            if (values.TryGetValue(parameterName, out value)) {
                var parameterValue = Convert.ToString(value);

                return _paths.ContainsKey(parameterValue);
            }

            return false;
        }
    }
}