using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCalendar_MVC.Models.DataTransferObjects
{
    public class FullCalendarUpdateStartDTO
    {
        public string Id { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public int Days  { get; set; }

        public FullCalendarUpdateStartDTO(IDictionary<string, object> jsonObj)
        {
            if (jsonObj.ContainsKey("id"))
                Id = jsonObj["id"].ToString();
            if (jsonObj.ContainsKey("days"))
                Days = int.Parse(jsonObj["days"].ToString());
            if (jsonObj.ContainsKey("end"))
                End = Convert.ToDateTime(jsonObj["end"].ToString());
            if (jsonObj.ContainsKey("start"))
                Start = Convert.ToDateTime(jsonObj["start"].ToString());
        }
    }
}
