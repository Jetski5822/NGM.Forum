using Orchard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NGM.Forum.Services
{
    /// <summary>
    /// This interface is created to allow the forums to use 
    /// any external user timezone management (i.e. allow each user to select a timezone).  
    /// In this usage, this interface is basically a proxy.
    /// 
    /// To use an external service, inject the external service
    /// and return values from the external service as appropriate.
    /// 
    /// By default without an external service, this service returns UTC dates.  
    /// These dates are converted to 'local time' via a javascript on the client side.
    /// 
    /// Ideally, the setup is as follows:
    /// ==> If the user has a personalized timezone, return it.
    /// ==> If the user has no personalized timezone (ie. not selected or not logged in), 
    ///     use the UTC timezone so that dates will be adjusted via javascript or shown in UTC  if the user has no javascript.      
    /// </summary>
    public interface IUserTimeZoneService : IDependency
    {        
        TimeZoneInfo GetUserTimeZone(string userName);
        string GetUserCulture(int userId);
        string GetUserCulture(string userName);
        TimeZoneInfo GetUserTimeZoneInfo(int? userId);
    }
}