using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using GoogleCalendar_MVC.Configs;
using GoogleCalendar_MVC.Data;
using GoogleCalendar_MVC.Models;
using GoogleCalendar_MVC.Models.ViewModels;
using GoogleCalendar_MVC.Repository.IRepository;
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
    public class CalendarAppController : Controller
    {
        private readonly IEventRepository _eventRepo;

        public CalendarAppController(IEventRepository eventRepository)
        {
            _eventRepo = eventRepository;
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
                            Email = input_email
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


            Event eventToCreate = new Event();
            new EventModelMapper().MappingToModel(eventToCreate, viewModel);

            if (listAttendees.Count > 0)
            {
                eventToCreate.Attendees = listAttendees;
            }
            if (listReminder.Count > 0)
            {
                eventToCreate.Reminders = new Event.RemindersData();
                eventToCreate.Reminders.UseDefault = false;
                eventToCreate.Reminders.Overrides = listReminder;
            }

            var result = await _eventRepo.CreateAsync(eventToCreate);
            if(result == null)
            {
                ModelState.AddModelError("error", "Some values are not valid!");
                return View(viewModel);
            }

            return RedirectToAction("Index", new { month = viewModel.Start.Month, year = viewModel.Start.Year });
        }

        public async Task<IActionResult> GetEditForm(string id)
        {
            Event result = await _eventRepo.GetAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            var viewModel = new EventVM();
            new EventModelMapper().MappingToViewModel(result, viewModel);

            return PartialView("_EditEventPartial",viewModel);
        }

        public async Task<IActionResult> Edit(string id)
        {
            Event result = await _eventRepo.GetAsync(id);
            if(result == null)
            {
                return NotFound();
            }

            var viewModel = new EventVM();
            new EventModelMapper().MappingToViewModel(result, viewModel);
            
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, EventVM viewModel)
        {
            Event result = await _eventRepo.GetAsync(id);
            if(result == null)
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
                            Email = input_email
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
            new EventModelMapper().MappingToModel(result, viewModel);

            bool success = await _eventRepo.UpdateAsync(id, result);
            if (!success)
            {
                ModelState.AddModelError("error", "Some values are not valid!");
                return View(viewModel);
            }

            //return RedirectToAction("Index", new { month = viewModel.Start.Month, year = viewModel.Start.Year });
            return Ok();
        }


        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            if (await _eventRepo.DeleteAsync(id))
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
