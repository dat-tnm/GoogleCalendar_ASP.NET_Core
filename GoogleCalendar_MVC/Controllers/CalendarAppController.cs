using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using GoogleCalendar_MVC.Configs;
using GoogleCalendar_MVC.Data;
using GoogleCalendar_MVC.Models;
using GoogleCalendar_MVC.Models.ViewModels;
using GoogleCalendar_MVC.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using static Google.Apis.Calendar.v3.CalendarService;

namespace GoogleCalendar_MVC.Controllers
{
    [Authorize]
    public class CalendarAppController : Controller
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ApplicationDbContext _db;


        public CalendarAppController(IWebHostEnvironment hostingEnvironment, ApplicationDbContext db)
        {
            _hostingEnvironment = hostingEnvironment;
            _db = db;
        }

        private CalendarService GetConfigCalendarService()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
            string userid = claim.Value;

            UserCredential credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                            new ClientSecrets()
                            {
                                ClientId = StaticDetails.GoogleCalendar_clientId,
                                ClientSecret = StaticDetails.GoogleCalendar_clientSecret,
                            },
                            StaticDetails.GoogleCalendar_Scopes,
                            userid,
                            CancellationToken.None,
                            new MyDataStore(_db)).Result;

            // Create Google Calendar API service.
            return new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = StaticDetails.GoogleCalendar_AppName,
            });

        }




        public IActionResult Index()
        {
            var service = GetConfigCalendarService();

            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List("primary");
            var mindate = DateTime.Now;
            mindate.AddMonths(1 - DateTime.Now.Month);
            mindate.AddDays(1 - DateTime.Now.Day);
            request.TimeMin = mindate;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            Events events = request.Execute();

            return View(events);
        }


        [HttpPost]
        public async Task<IActionResult> Create([Bind("Summary,Start,End,EndTimeUnspecified,Description")]EventVM viewModel)
        {
            Event body = new Event()
            {
                Summary = viewModel.Summary,
                Start = new EventDateTime()
                {
                    DateTime = viewModel.Start,
                    TimeZone = "Asia/Ho_Chi_Minh"
                },
                End = new EventDateTime()
                {
                    DateTime = viewModel.End,
                    TimeZone = "Asia/Ho_Chi_Minh"
                },
                EndTimeUnspecified = viewModel.EndTimeUnspecified,
                Description = viewModel.Description,

            };
            var service = GetConfigCalendarService();
            var request = service.Events.Insert(body, "primary");
            Event result = await request.ExecuteAsync();
            if (string.IsNullOrEmpty(result.Id))
            {
                return View(viewModel);
            }

            return RedirectToAction("Index");
        }

    }
}
