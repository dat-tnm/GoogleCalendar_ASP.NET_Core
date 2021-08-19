using System.ComponentModel.DataAnnotations;

namespace GoogleCalendar_MVC.Models.ViewModels
{
    public class Attendee
    {
        [EmailAddress]
        public string Email { get; set; }
        public bool Resource { get; set; }
        public bool Optional { get; set; }
        public string Error { get; set; }
    }
}
