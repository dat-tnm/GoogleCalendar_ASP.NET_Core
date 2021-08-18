using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCalendar_MVC.Models.ViewModels
{
    public class Attendee
    {
        public string Email { get; set; }
        public bool Resource { get; set; }
        public bool Optional { get; set; }
        public string Error { get; set; }
    }
}
