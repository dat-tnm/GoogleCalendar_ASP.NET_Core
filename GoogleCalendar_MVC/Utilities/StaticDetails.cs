using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Google.Apis.Calendar.v3.CalendarService;

namespace GoogleCalendar_MVC.Utilities
{
    public static class StaticDetails
    {
        public const string Secret = "lay-chua-tren-cao-turn-down-for-what-bleble";
        public const string GoogleCalendar_clientId = "177266376242-t8148d3dpahe3uqr4ub7osg4tj1hd06f.apps.googleusercontent.com";
        public const string GoogleCalendar_clientSecret = "pDB-3uCVvR9aTSolEyC1Viax";
        public static string[] GoogleCalendar_Scopes = { "openid", Scope.Calendar, Scope.CalendarEvents };
        public const string GoogleCalendar_AppName = "Datnm - Google calendar";


        public const string cookieGoogleCredentials = "ggcr-";
    }
}
