using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NGM.Forum.Services
{

    //  *See comments in IUserPreferredCultureService
    public class UserPreferredCultureService : IUserPreferredCultureService
    {
        private readonly ICultureManager _cultureManager;

        public UserPreferredCultureService(
            ICultureManager cultureManager    
        )
        {
            _cultureManager = cultureManager;
        }

        public Dictionary<string, IEnumerable<int>> GetUsersCultures(IEnumerable<int> userIds)
        {
            //this is the default implementation that returns all users using the sites default culture.
            //this is meant to be customized if your site allows users to select a preferred culture.
            //*See comments in IUserPreferredCultureService 
            var userCultureDict = new Dictionary<string, IEnumerable<int>>();
            //userCultureDict.Add(_cultureManager.GetSiteCulture(), userIds);
            userCultureDict.Add("fr-FR", userIds);
            return userCultureDict;
        }

    }
}