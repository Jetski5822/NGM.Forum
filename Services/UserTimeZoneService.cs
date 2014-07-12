using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NGM.Forum.Services
{

    //see comments in IUserTimeZoneService
    public class UserTimeZoneService : IUserTimeZoneService
    {
        public UserTimeZoneService()
        {

        }

        public string GetUserCulture(int userId)
        {
            throw new NotImplementedException();
        }

        public string GetUserCulture(string userName)
        {
            throw new NotImplementedException();
        }

        public TimeZoneInfo GetUserTimeZoneInfo(int? userId)
        {
            return TimeZoneInfo.Utc;
            //return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"); ;
        }
        public TimeZoneInfo GetUserTimeZone(string userName)
        {
            return TimeZoneInfo.Utc;
            //return TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"); ;
        }
    }
}