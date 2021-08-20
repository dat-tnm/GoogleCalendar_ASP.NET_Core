using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace GoogleCalendar_MVC.Models.ViewModels
{
    public class FullCalendarEventVM
    {
        public FullCalendarEventVM()
        {

        }

        public FullCalendarEventVM(IDictionary<string, object> jsonObj)
        {
            if (jsonObj.ContainsKey("id"))
                Id = jsonObj["id"].ToString();
            if (jsonObj.ContainsKey("title"))
                Title = jsonObj["title"].ToString();
            if (jsonObj.ContainsKey("start"))
                Start = Convert.ToDateTime(jsonObj["start"].ToString());
            if (jsonObj.ContainsKey("end"))
                End = Convert.ToDateTime(jsonObj["end"].ToString());
        }

        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("title")]
        public string Title { get; set; }
        [JsonProperty("start")]
        public DateTime Start { get; set; }
        [JsonProperty("end")]
        public DateTime End { get; set; }
    }
}
