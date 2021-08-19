using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using GoogleCalendar_MVC.Configs;
using GoogleCalendar_MVC.Data;
using GoogleCalendar_MVC.Repository.IRepository;
using GoogleCalendar_MVC.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleCalendar_MVC.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _db;


        private string GetCurrentUserId()
        {
            var claimsIdentity = (ClaimsIdentity)_httpContextAccessor.HttpContext.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            return claim.Value;
        }

        private CalendarService GetConfigCalendarService()
        {
            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                            new ClientSecrets()
                            {
                                ClientId = StaticDetails.GoogleCalendar_clientId,
                                ClientSecret = StaticDetails.GoogleCalendar_clientSecret,
                            },
                            StaticDetails.GoogleCalendar_Scopes,
                            GetCurrentUserId(),
                            CancellationToken.None,
                            new MyDataStore(_db)).Result;

            // Create Google Calendar API service.
            return new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = StaticDetails.GoogleCalendar_AppName,
            });
        }

        public EventRepository(IHttpContextAccessor httpContextAccessor, ApplicationDbContext db)
        {
            _httpContextAccessor = httpContextAccessor;
            _db = db;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var service = GetConfigCalendarService();
            try
            {
                var delete_request = service.Events.Delete("primary", id);
                await delete_request.ExecuteAsync();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<Event> GetAsync(string id)
        {
            var service = GetConfigCalendarService();
            var get_request = service.Events.Get("primary", id);
            Event result;
            try
            {
                result = await get_request.ExecuteAsync();
            }
            catch (Exception)
            {
                return null;
            }

            return result;
        }

        public async Task<Events> GetListAsync(Action<EventsResource.ListRequest> configureRequest)
        {
            var service = GetConfigCalendarService();

            EventsResource.ListRequest request = service.Events.List("primary");
            if (configureRequest != null)
            {
                configureRequest(request);
            }
            return await request.ExecuteAsync();
        }

        public async Task<bool> UpdateAsync(string id, Event objToUpdate)
        {
            var service = GetConfigCalendarService();
            var post_request = service.Events.Update(objToUpdate, "primary", id);
            try
            {
                await post_request.ExecuteAsync();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task<Event> CreateAsync(Event objToCreate)
        {
            var service = GetConfigCalendarService();
            var request = service.Events.Insert(objToCreate, "primary");
            Event result;
            try
            {
                result = await request.ExecuteAsync();
            }
            catch (Exception)
            {
                return null;
            }

            return result;
        }
    }
}
