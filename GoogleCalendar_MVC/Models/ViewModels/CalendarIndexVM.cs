using Google.Apis.Calendar.v3.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCalendar_MVC.Models.ViewModels
{
    public class CalendarIndexVM
    {
        public IList<Event> Events { get; set; }
        [Range(0, 12)]
        public DateTime SelectedMonthYear { get; set; }
    }
}
