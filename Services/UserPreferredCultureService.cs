using Orchard.Localization.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NGM.Forum.Services
{

    //  *See comments in the interface IUserPreferredCultureService
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
            //since there is no built in management of user culture, this is a stub to tie the forums into your own implementation.
            //i.e. inject your service and get your user's prefered culture
            var userCultureDict = new Dictionary<string, IEnumerable<int>>();
            userCultureDict.Add(_cultureManager.GetSiteCulture(), userIds);

            //used to test translation
            //userCultureDict.Add("fr-FR", userIds);
            return userCultureDict;
        }

    }
}