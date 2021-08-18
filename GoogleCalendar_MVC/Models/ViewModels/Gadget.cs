using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCalendar_MVC.Models.ViewModels
{
    public class Gadget
    {
        public string Display { get; set; }
        public int? Height { get; set; }
        public string IconLink { get; set; }
        public string Link { get; set; }
        public IDictionary<string, string> Preferences { get; set; }
        public string Title { get; set; }
        public string Type { get; set; }
        public int? Width { get; set; }
    }
}
