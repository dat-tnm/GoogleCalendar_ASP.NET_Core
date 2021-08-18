﻿using Google.Apis.Auth.OAuth2;
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
using System.ComponentModel.DataAnnotations;
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




        public async Task<IActionResult> Index([FromQuery]string month, [FromQuery]string year)
        {

            int month_val, year_val;
            try
            {
                int input_m_val = int.Parse(month);
                int input_y_val = int.Parse(year);
                if (input_m_val < 1 || input_m_val > 12 || input_y_val < 1)
                {
                    throw new Exception();
                }
                month_val = input_m_val;
                year_val = input_y_val;
            }
            catch (Exception)
            {
                month_val = DateTime.Now.Month;
                year_val = DateTime.Now.Year;
            }
            var service = GetConfigCalendarService();

            // Define parameters of request.
            EventsResource.ListRequest request = service.Events.List("primary");
            request.TimeMin = Convert.ToDateTime($"01/{month_val}/{year_val} 00:00:00 AM");
            request.TimeMax = ((DateTime)request.TimeMin).AddMonths(1);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            Events events = await request.ExecuteAsync();
            var viewModel = new CalendarIndexVM()
            {
                Events = events.Items,
                SelectedMonthYear = (DateTime)request.TimeMin
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Create([Bind("Summary,Start,End,Description")]EventVM viewModel)
        {
            var listAttendees = new List<EventAttendee>();
            viewModel.Attendees = new List<Attendee>();
            bool hasError = false;
            if (HttpContext.Request.Form.ContainsKey("Emails"))
            {
                var emailCheck = new EmailAddressAttribute();
                string input_email;
                EventAttendee attendee;
                for (int i = 0; i < HttpContext.Request.Form["Emails"].Count; i++)
                {
                    input_email = HttpContext.Request.Form["Emails"][i];
                    if (emailCheck.IsValid(input_email))
                    {
                        attendee = new EventAttendee() { Email = input_email };
                        try
                        {
                            attendee.Optional = bool.Parse(HttpContext.Request.Form["Optionals"][i]);
                            attendee.Resource = bool.Parse(HttpContext.Request.Form["Resources"][i]);
                        }
                        catch (Exception) { }
                        listAttendees.Add(attendee);
                        viewModel.Attendees.Add(new Attendee()
                        {
                            Email = input_email,
                            Optional = attendee.Optional == true ? true : false,
                            Resource = attendee.Resource == true ? true : false
                        });
                    }
                    else
                    {
                        viewModel.Attendees.Add(new Attendee() { Email = input_email, Error = "Email is not valid" });
                        hasError = true;
                    }
                }
            }
            if (hasError)
            {
                return View(viewModel);
            }
            

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
                Description = viewModel.Description,

            };
            if (listAttendees.Count > 0)
            {
                body.Attendees = listAttendees;
            }

            var service = GetConfigCalendarService();
            var request = service.Events.Insert(body, "primary");
            Event result = await request.ExecuteAsync();
            if (string.IsNullOrEmpty(result.Id))
            {
                return View(viewModel);
            }

            return RedirectToAction("Index", new { month = viewModel.Start.Month, year = viewModel.Start.Year });
        }

        public async Task<IActionResult> Edit(string id)
        {
            var service = GetConfigCalendarService();
            var request = service.Events.Get("primary", id);
            Event result = await request.ExecuteAsync();
            var viewModel = new EventVM()
            {
                Summary = result.Summary,
                Start = result.Start.DateTime == null ? DateTime.Now : (DateTime)result.Start.DateTime,
                End = result.End.DateTime == null ? DateTime.Now : (DateTime)result.End.DateTime,
                Description = result.Description,
                Attendees = new List<Attendee>()
            };
            foreach (var item in result.Attendees)
            {
                viewModel.Attendees.Add(new Attendee()
                {
                    Email = item.Email,
                    Optional = item.Optional == true ? true : false
                });
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, [Bind("Summary,Start,End,Description")] EventVM viewModel)
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
                return NotFound();
            }

            var listAttendees = new List<EventAttendee>();
            viewModel.Attendees = new List<Attendee>();
            bool hasError = false;
            if (HttpContext.Request.Form.ContainsKey("Emails"))
            {
                var emailCheck = new EmailAddressAttribute();
                string input_email;
                EventAttendee attendee;
                for (int i = 0; i < HttpContext.Request.Form["Emails"].Count; i++)
                {
                    input_email = HttpContext.Request.Form["Emails"][i];
                    if (emailCheck.IsValid(input_email))
                    {
                        attendee = new EventAttendee() { Email = input_email };
                        try
                        {
                            attendee.Optional = bool.Parse(HttpContext.Request.Form["Optionals"][i]);
                            attendee.Resource = bool.Parse(HttpContext.Request.Form["Resources"][i]);
                        }
                        catch (Exception) { }
                        listAttendees.Add(attendee);
                        viewModel.Attendees.Add(new Attendee()
                        {
                            Email = input_email,
                            Optional = attendee.Optional == true ? true : false,
                            Resource = attendee.Resource == true ? true : false
                        });
                    }
                    else
                    {
                        viewModel.Attendees.Add(new Attendee() { Email = input_email, Error = "Email is not valid" });
                        hasError = true;
                    }
                }
            }
            if (hasError)
            {
                return View(viewModel);
            }
            if (listAttendees.Count > 0)
            {
                result.Attendees = listAttendees;
            }

            result.Summary = viewModel.Summary;
            result.Start.DateTime = viewModel.Start;
            result.End.DateTime = viewModel.End;
            result.Description = viewModel.Description;

            var post_request = service.Events.Update(result, "primary", result.Id);
            var post_result = await post_request.ExecuteAsync();

            return RedirectToAction("Index", new { month = viewModel.Start.Month, year = viewModel.Start.Year });
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            bool success = false;
            var service = GetConfigCalendarService();
            var request = service.Events.Get("primary", id);
            try
            {
                Event result = await request.ExecuteAsync();
                var delete_request = service.Events.Delete("primary", result.Id);
                await delete_request.ExecuteAsync();
                success = true;
            }
            catch (Exception)
            {
            }

            if (success)
            {
                return Json(new { success = true, message = "Delete Successful" });
            }
            return Json(new { success = false, message = "Delete Not Successful" });
        }


        public IActionResult GetAttendeesForm()
        {
            return PartialView("_AttendeesPartial");
        }
    }
}
