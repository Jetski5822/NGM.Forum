using System;
using System.Web.Mvc;
using Orchard;

namespace NGM.Forum.Extensions {
    public static class ContainerExtensions {
        /// <summary>
        /// This method performed by Erik Weisz.
        /// </summary>
        /// <see cref="http://en.wikipedia.org/wiki/Harry_Houdini"/>
        /// <returns>himself</returns>
        public static TService Resolve<TService>(this UrlHelper urlHelper) {
            var workContext = urlHelper.RequestContext.GetWorkContext();

            if (workContext == null)
                throw new ApplicationException(string.Format(@"The WorkContext cannot be found for the request. Unable to resolve '{0}'.", typeof(TService)));

            return workContext.Resolve<TService>();
        }
    }
}