using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCalendar_MVC.Models.ViewModels
{
    public class EventVM
    {
        public string Id { get; set; }
        [Required]
        public string Summary { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public string ColorId { get; set; }
        [Required]
        public DateTime Start { get; set; }
        [Required]
        public DateTime End { get; set; }
        public string Transparency { get; set; }
        public string Visibility { get; set; }
        public int Sequence { get; set; }
        public bool AnyoneCanAddSelf { get; set; }
        public bool GuestsCanInviteOthers { get; set; }
        public bool GuestsCanSeeOtherGuests { get; set; }
        public bool GuestsCanModify { get; set; }
        public bool PrivateCopy { get; set; }
        public bool Locked { get; set; }
        public bool AttendeesOmitted { get; set; }
        public string EventType { get; set; }

        public IList<string> Recurrence { get; set; }
        public string RecurringEventId { get; set; }
        public IList<Attendee> Attendees { get; set; }
        public ExtendedProperties ExtendedProperties { get; set; }
        public Gadget Gadget { get; set; }
        public Reminders Reminders { get; set; }
        public IList<Attachment> Attachments { get; set; }
    }
}
