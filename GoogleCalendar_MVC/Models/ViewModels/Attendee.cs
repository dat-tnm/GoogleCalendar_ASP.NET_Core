using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCalendar_MVC.Models.ViewModels
{
    public class Attendee
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public bool? Organizer { get; set; }
        public bool? Self { get; set; }
        public bool? Resource { get; set; }
        public bool? Optional { get; set; }
        public string ResponseStatus { get; set; }
        public string Comment { get; set; }
        public int? AdditionalGuests { get; set; }
    }
}
