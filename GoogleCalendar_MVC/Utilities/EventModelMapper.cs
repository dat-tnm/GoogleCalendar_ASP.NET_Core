using Google.Apis.Calendar.v3.Data;
using GoogleCalendar_MVC.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCalendar_MVC.Utilities
{
    public class EventModelMapper
    {
        private void MappingEventVM_To_Event(Event model, EventVM viewModel)
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

        private void MappingFullCalendarVM_To_Event(Event model, FullCalendarEventVM viewModel)
        {
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
            model.Summary = viewModel.Title;
        }

        private void MappingEvent_To_EventVM(Event model, EventVM viewModel)
        {
            viewModel.Summary = model.Summary;
            viewModel.Start = model.Start.DateTime == null ? DateTime.Now : (DateTime)model.Start.DateTime;
            viewModel.End = model.End.DateTime == null ? DateTime.Now : (DateTime)model.End.DateTime;
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
            if (model.Attendees != null)
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

        private void MappingEvent_To_FullCalendarVM(Event model, FullCalendarEventVM viewModel)
        {
            viewModel.Id = model.Id;
            viewModel.Title = model.Summary;
            viewModel.Start = (DateTime)model.Start.DateTime;
            viewModel.End = (DateTime)model.End.DateTime;
        }





        public void MappingToModel(Event model, object viewModel)
        {
            if (viewModel is EventVM)
            {
                MappingEventVM_To_Event(model, viewModel as EventVM);
            }
            else if (viewModel is FullCalendarEventVM)
            {
                MappingFullCalendarVM_To_Event(model, viewModel as FullCalendarEventVM);
            }
        }

        public void MappingToViewModel(Event model, object viewModel)
        {
            if (viewModel is EventVM)
            {
                MappingEvent_To_EventVM(model, viewModel as EventVM);
            }
            else if (viewModel is FullCalendarEventVM)
            {
                MappingEvent_To_FullCalendarVM(model, viewModel as FullCalendarEventVM);
            }
        }


    }

}
