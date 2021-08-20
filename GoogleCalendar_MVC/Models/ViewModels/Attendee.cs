using System.ComponentModel.DataAnnotations;

namespace GoogleCalendar_MVC.Models.ViewModels
{
    public class Attendee
    {
        [EmailAddress]
        public string Email { get; set; }
        public string Error { get; set; }
    }
}
