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
        private readonly ApplicationDbContext _db;


        public CalendarAppController(ApplicationDbContext db)
        {
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

        private void MappingViewModelToModel(EventVM viewModel, Event model)
        {
            model.Summary = viewModel.Summary;
            model.Start = new EventDateTime()
            {
                DateTime = viewModel.Start,
                TimeZone = "Asia/Ho_Chi_Minh"
            };
            model.End = new EventDateTime()
            {
                DateTime = viewModel.End,
                TimeZone = "Asia/Ho_Chi_Minh"
            };
            model.Description = viewModel.Description;
            model.AttendeesOmitted = viewModel.AttendeesOmitted;
            model.Transparency = viewModel.Transparency;
            model.Visibility = viewModel.Visibility;
            model.GuestsCanInviteOthers = viewModel.GuestsCanInviteOthers;
            model.GuestsCanModify = viewModel.GuestsCanModify;
            model.GuestsCanSeeOtherGuests = viewModel.GuestsCanSeeOtherGuests;
            model.AnyoneCanAddSelf = viewModel.AnyoneCanAddSelf;
            model.PrivateCopy = viewModel.PrivateCopy;
            model.Locked = viewModel.Locked;
            model.Location = viewModel.Location;
        }

        private void MappingModelToViewModel(Event model, EventVM viewModel)
        {
            viewModel.Summary = model.Summary;
            viewModel.Start = model.Start.DateTime == null ? DateTime.Now : (DateTime)model.Start.DateTime;
            viewModel.End = model.Start.DateTime == null ? DateTime.Now : (DateTime)model.Start.DateTime;
            viewModel.Description = model.Description;
            viewModel.AttendeesOmitted = model.AttendeesOmitted == null ? false : (bool)model.AttendeesOmitted;
            viewModel.Transparency = model.Transparency;
            viewModel.Visibility = model.Visibility;
            viewModel.GuestsCanInviteOthers = model.GuestsCanInviteOthers == null ? true : (bool)model.GuestsCanInviteOthers;
            viewModel.GuestsCanModify = model.GuestsCanModify == null ? false : (bool)model.GuestsCanModify;
            viewModel.GuestsCanSeeOtherGuests = model.GuestsCanSeeOtherGuests == null ? true : (bool)model.GuestsCanSeeOtherGuests;
            viewModel.AnyoneCanAddSelf = model.AnyoneCanAddSelf == null ? false : (bool)model.AnyoneCanAddSelf;
            viewModel.PrivateCopy = model.PrivateCopy == null ? false : (bool)model.PrivateCopy;
            viewModel.Locked = model.Locked == null ? false : (bool)model.Locked;
            viewModel.Location = model.Location;

            viewModel.InitializeProperty();
            if(model.Attendees != null)
                foreach (var item in model.Attendees)
                {
                    viewModel.Attendees.Add(new Attendee()
                    {
                        Email = item.Email,
                        Optional = item.Optional == true ? true : false
                    });
                }
            if (model.Reminders.UseDefault == false)
                foreach (var item in model.Reminders.Overrides)
                {
                    viewModel.Reminders.Overrides.Add(new Reminder()
                    {
                        Method = item.Method,
                        Minutes = item.Minutes == null ? 0 : (int)item.Minutes
                    });
                }
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
            var viewModel = new EventVM() 
            {
                Start = DateTime.Now,
                End = DateTime.Now
            };
            viewModel.InitializeProperty();
            return View(viewModel);
        }


        [HttpPost]
        public async Task<IActionResult> Create(EventVM viewModel)
        {
            viewModel.InitializeProperty();
            var listAttendees = new List<EventAttendee>();
            var listReminder = new List<EventReminder>();
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

            if (HttpContext.Request.Form.ContainsKey("Reminder-Method"))
            {
                string input_method;
                EventReminder reminder;
                for (int i = 0; i < HttpContext.Request.Form["Reminder-Method"].Count; i++)
                {
                    input_method = HttpContext.Request.Form["Reminder-Method"][i];
                    if (input_method == "email" || input_method == "popup")
                    {
                        reminder = new EventReminder() { Method = input_method };
                        try
                        {
                            reminder.Minutes = int.Parse(HttpContext.Request.Form["Reminder-Minutes"][i]);
                            if (reminder.Minutes < 1 || reminder.Minutes > 40200)
                            {
                                throw new Exception();
                            }
                            listReminder.Add(reminder);
                            viewModel.Reminders.Overrides.Add(new Reminder()
                            {
                                Method = input_method,
                                Minutes = reminder.Minutes == null ? 0 : (int)reminder.Minutes
                            });
                        }
                        catch (Exception)
                        {
                            viewModel.Reminders.Overrides.Add(new Reminder() { Method = input_method, Error = "Minutes is not valid. Minutes must greater than 0 and lower than 40200" });
                            hasError = true;
                        }
                    }
                    else
                    {
                        viewModel.Reminders.Overrides.Add(new Reminder() { Method = input_method, Error = "Method is not valid" });
                        hasError = true;
                    }
                }
            }

            if (hasError)
            {
                return View(viewModel);
            }


            Event body = new Event();
            MappingViewModelToModel(viewModel, body);

            if (listAttendees.Count > 0)
            {
                body.Attendees = listAttendees;
            }
            if (listReminder.Count > 0)
            {
                body.Reminders = new Event.RemindersData();
                body.Reminders.UseDefault = false;
                body.Reminders.Overrides = listReminder;
            }

            var service = GetConfigCalendarService();
            var request = service.Events.Insert(body, "primary");
            try
            {
                Event result = await request.ExecuteAsync();
            }
            catch (Exception)
            {
                ModelState.AddModelError("error", "Some values are not valid!");
                return View(viewModel);
            }

            return RedirectToAction("Index", new { month = viewModel.Start.Month, year = viewModel.Start.Year });
        }

        public async Task<IActionResult> Edit(string id)
        {
            var service = GetConfigCalendarService();
            var request = service.Events.Get("primary", id);
            Event result;
            try
            {
                result = await request.ExecuteAsync();
            }
            catch (Exception)
            {
                return NotFound();
            }
            var viewModel = new EventVM();
            MappingModelToViewModel(result, viewModel);
            
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, EventVM viewModel)
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

            viewModel.InitializeProperty();
            var listAttendees = new List<EventAttendee>();
            var listReminder = new List<EventReminder>();
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

            if (HttpContext.Request.Form.ContainsKey("Reminder-Method"))
            {
                string input_method;
                EventReminder reminder;
                for (int i = 0; i < HttpContext.Request.Form["Reminder-Method"].Count; i++)
                {
                    input_method = HttpContext.Request.Form["Reminder-Method"][i];
                    if (input_method == "email" || input_method == "popup")
                    {
                        reminder = new EventReminder() { Method = input_method };
                        try
                        {
                            reminder.Minutes = int.Parse(HttpContext.Request.Form["Reminder-Minutes"][i]);
                            if(reminder.Minutes < 1 || reminder.Minutes > 40200)
                            {
                                throw new Exception();
                            }
                            listReminder.Add(reminder);
                            viewModel.Reminders.Overrides.Add(new Reminder()
                            {
                                Method = input_method,
                                Minutes = reminder.Minutes == null ? 0 : (int)reminder.Minutes
                            });
                        }
                        catch (Exception) 
                        {
                            viewModel.Reminders.Overrides.Add(new Reminder() { Method = input_method, Error = "Minutes is not valid. Minutes must greater than 0 and lower than 40200" });
                            hasError = true;
                        }
                    }
                    else
                    {
                        viewModel.Reminders.Overrides.Add(new Reminder() { Method = input_method, Error = "Method is not valid" });
                        hasError = true;
                    }
                }
            }

            if (hasError)
            {
                return View(viewModel);
            }

            if (listReminder.Count > 0)
            {
                result.Reminders.UseDefault = false;
                result.Reminders.Overrides = listReminder;
            }
            result.Attendees = listAttendees;
            MappingViewModelToModel(viewModel, result);

            var post_request = service.Events.Update(result, "primary", result.Id);
            try
            {
                await post_request.ExecuteAsync();
            }
            catch (Exception)
            {
                ModelState.AddModelError("error", "Some values are not valid!");
                return View(viewModel);
            }

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
            return PartialView("_AttendeesPartial", new Attendee());
        }

        public IActionResult GetRemindersForm()
        {
            return PartialView("_RemindersPartial", new Reminder());
        }
    }
}
