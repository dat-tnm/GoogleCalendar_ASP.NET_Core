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
        Task<Events> GetList(Action<EventsResource.ListRequest> configureRequest);
        Task<Event> Create(Event objToCreate);
        Task<Event> Get(string id);
        Task<bool> Update(string id, Event objToUpdate);
        Task<bool> Delete(string id);
    }
}
