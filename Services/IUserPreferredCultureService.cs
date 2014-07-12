using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NGM.Forum.Services
{
    /// <summary>
    /// This interface is created to allow the forums to use 
    /// any external user preferred culture selector  
    /// (i.e. the user has a prefered site culture selected in their profile).  
    /// For this purpose, this interface is basically a proxy.
    /// 
    /// To use an external service, inject it here and make it 
    /// return the user's selected site culture from their profile (or where ever it comes from)
    /// 
    /// By default without an external service, this service simply returns the site's default culture
    /// </summary>
    public interface IUserPreferredCultureService : IDependency
    {        
        /// <summary>
        /// Given a unique list of user ids, looks up the preferred culture of each user
        /// or the site's default culture if none is provided. 
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns>Sort the user's cultures into a dictionary keyed by each unique culture and with a list of user with that selected the culture.</returns>
        Dictionary<string, IEnumerable<int>> GetUsersCultures(IEnumerable<int> userIds);
    }
}