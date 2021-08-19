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
        public Event Model { get; set; }
        public EventVM ViewModel { get; set; }

        public EventModelMapper(Event model, EventVM viewModel)
        {
            Model = model;
            ViewModel = viewModel;
        }

        public void MappingToModel()
        {
            Model.Summary = ViewModel.Summary;
            Model.Start = new EventDateTime()
            {
                DateTime = ViewModel.Start,
                TimeZone = "Asia/Ho_Chi_Minh"
            };
            Model.End = new EventDateTime()
            {
                DateTime = ViewModel.End,
                TimeZone = "Asia/Ho_Chi_Minh"
            };
            Model.Description = ViewModel.Description;
            Model.AttendeesOmitted = ViewModel.AttendeesOmitted;
            Model.Transparency = ViewModel.Transparency;
            Model.Visibility = ViewModel.Visibility;
            Model.GuestsCanInviteOthers = ViewModel.GuestsCanInviteOthers;
            Model.GuestsCanModify = ViewModel.GuestsCanModify;
            Model.GuestsCanSeeOtherGuests = ViewModel.GuestsCanSeeOtherGuests;
            Model.AnyoneCanAddSelf = ViewModel.AnyoneCanAddSelf;
            Model.PrivateCopy = ViewModel.PrivateCopy;
            Model.Locked = ViewModel.Locked;
            Model.Location = ViewModel.Location;
        }

        public void MappingToViewModel()
        {
            ViewModel.Summary = Model.Summary;
            ViewModel.Start = Model.Start.DateTime == null ? DateTime.Now : (DateTime)Model.Start.DateTime;
            ViewModel.End = Model.Start.DateTime == null ? DateTime.Now : (DateTime)Model.Start.DateTime;
            ViewModel.Description = Model.Description;
            ViewModel.AttendeesOmitted = Model.AttendeesOmitted == null ? false : (bool)Model.AttendeesOmitted;
            ViewModel.Transparency = Model.Transparency;
            ViewModel.Visibility = Model.Visibility;
            ViewModel.GuestsCanInviteOthers = Model.GuestsCanInviteOthers == null ? true : (bool)Model.GuestsCanInviteOthers;
            ViewModel.GuestsCanModify = Model.GuestsCanModify == null ? false : (bool)Model.GuestsCanModify;
            ViewModel.GuestsCanSeeOtherGuests = Model.GuestsCanSeeOtherGuests == null ? true : (bool)Model.GuestsCanSeeOtherGuests;
            ViewModel.AnyoneCanAddSelf = Model.AnyoneCanAddSelf == null ? false : (bool)Model.AnyoneCanAddSelf;
            ViewModel.PrivateCopy = Model.PrivateCopy == null ? false : (bool)Model.PrivateCopy;
            ViewModel.Locked = Model.Locked == null ? false : (bool)Model.Locked;
            ViewModel.Location = Model.Location;

            ViewModel.InitializeProperty();
            if (Model.Attendees != null)
                foreach (var item in Model.Attendees)
                {
                    ViewModel.Attendees.Add(new Attendee()
                    {
                        Email = item.Email,
                        Optional = item.Optional == true ? true : false
                    });
                }
            if (Model.Reminders.UseDefault == false)
                foreach (var item in Model.Reminders.Overrides)
                {
                    ViewModel.Reminders.Overrides.Add(new Reminder()
                    {
                        Method = item.Method,
                        Minutes = item.Minutes == null ? 0 : (int)item.Minutes
                    });
                }
        }
    }
}
