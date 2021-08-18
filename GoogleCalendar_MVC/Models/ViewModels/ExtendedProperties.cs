using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCalendar_MVC.Models.ViewModels
{
    public class ExtendedProperties
    {
        //
        // Summary:
        //     Properties that are private to the copy of the event that appears on this calendar.
        public virtual IDictionary<string, string> Private__ { get; set; }
        //
        // Summary:
        //     Properties that are shared between copies of the event on other attendees' calendars.
        public virtual IDictionary<string, string> Shared { get; set; }
    }
}
