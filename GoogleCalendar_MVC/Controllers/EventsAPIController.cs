using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using GoogleCalendar_MVC.Models.ViewModels;
using GoogleCalendar_MVC.Repository.IRepository;
using GoogleCalendar_MVC.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Dynamic;
using GoogleCalendar_MVC.Models.DataTransferObjects;

namespace GoogleCalendar_MVC.Controllers
{
    public class EventsAPIController : Controller
    {
        private readonly IEventRepository _eventRepo;

        public EventsAPIController(IEventRepository eventRepository)
        {
            _eventRepo = eventRepository;
        }


        [HttpGet]
        public async Task<IActionResult> GetListEvent([FromQuery] string start, [FromQuery] string end)
        {
            DateTime startDate;
            DateTime endDate;
            try
            {
                startDate = Convert.ToDateTime(start);
                endDate = Convert.ToDateTime(end);
            }
            catch (Exception)
            {
                return BadRequest();
            }

            var events = await _eventRepo.GetListAsync(request =>
            {
                request.TimeMin = startDate;
                request.TimeMax = endDate;
                request.ShowDeleted = false;
                request.SingleEvents = true;
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;
            });

            var fullcalendarVMs = new List<FullCalendarEventVM>();
            var eventMapper = new EventModelMapper();
            FullCalendarEventVM itemVM;
            foreach (var item in events.Items)
            {
                itemVM = new FullCalendarEventVM();
                eventMapper.MappingToViewModel(item, itemVM);
                fullcalendarVMs.Add(itemVM);
            }

            return Ok(fullcalendarVMs);
        }


        [HttpGet]
        public async Task<IActionResult> GetEvent(string id)
        {
            Event result = await _eventRepo.GetAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            var fullCalendarVM = new FullCalendarEventVM();
            new EventModelMapper().MappingToViewModel(result, fullCalendarVM);

            return Ok(fullCalendarVM);
        }


        [HttpPost]
        public async Task<IActionResult> CreateEvent([FromBody] ExpandoObject jsonObj) 
        {
            FullCalendarEventVM viewModel = new FullCalendarEventVM(jsonObj);
            
            var eventToCreate = new Event();
            new EventModelMapper().MappingToModel(eventToCreate, viewModel);
            var result = await _eventRepo.CreateAsync(eventToCreate);

            if (result == null)
            {
                return BadRequest();
            }

            return StatusCode(StatusCodes.Status201Created);
        }


        [HttpPatch]
        [HttpPut]
        public async Task<IActionResult> UpdateEvent([FromBody] ExpandoObject jsonObj)
        {
            FullCalendarUpdateStartDTO viewModel = new FullCalendarUpdateStartDTO(jsonObj);

            var result = await _eventRepo.GetAsync(viewModel.Id);
            if (result == null)
            {
                return NotFound();
            }
            result.Start.DateTime = viewModel.Start;
            result.End.DateTime = ((DateTime)result.End.DateTime).AddMinutes(viewModel.Minutes);

            bool success = await _eventRepo.UpdateAsync(viewModel.Id, result);
            if (!success)
            {
                return BadRequest();
            }

            return NoContent();
        }


        [HttpDelete]
        public async Task<IActionResult> DeleteEvent(string id)
        {
            var result = await _eventRepo.GetAsync(id);
            if (result == null)
            {
                return NotFound();
            }

            bool success = await _eventRepo.DeleteAsync(id);
            if (!success)
            {
                return BadRequest();
            }

            return NoContent();
        }

    }
}
