using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleCalendar_MVC.Repository.IRepository
{
    public interface IEventRepository
    {
        Task<Events> GetListAsync(Action<EventsResource.ListRequest> configureRequest);
        Task<Event> CreateAsync(Event objToCreate);
        Task<Event> GetAsync(string id);
        Task<bool> UpdateAsync(string id, Event objToUpdate);
        Task<bool> DeleteAsync(string id);
    }
}
