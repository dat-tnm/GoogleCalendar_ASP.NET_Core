using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCalendar_MVC.Models.ViewModels
{
    public class Reminders
    {
        public IList<Reminder> Overrides { get; set; }
        public bool? UseDefault { get; set; }
    }

    public class Reminder
    {
        public string Method { get; set; }
        public int? Minutes { get; set; }
    }
}
