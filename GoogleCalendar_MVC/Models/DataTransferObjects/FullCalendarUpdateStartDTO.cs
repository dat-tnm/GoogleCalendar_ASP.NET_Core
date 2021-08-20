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
        public int Minutes  { get; set; }

        public FullCalendarUpdateStartDTO(IDictionary<string, object> jsonObj)
        {
            if (jsonObj.ContainsKey("id"))
                Id = jsonObj["id"].ToString();
            if (jsonObj.ContainsKey("minutes"))
                Minutes = int.Parse(jsonObj["minutes"].ToString());
            if (jsonObj.ContainsKey("start"))
                Start = Convert.ToDateTime(jsonObj["start"].ToString());
        }
    }
}
