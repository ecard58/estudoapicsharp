using AutoMapper;
using AwesomeDevEvents.API.Entities;
using AwesomeDevEvents.API.Models;
using AwesomeDevEvents.API.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AwesomeDevEvents.API.Controllers
{
    [Route("api/dev-events")]
    [ApiController]
    public class DevEventsController : ControllerBase
    {
        private readonly DevEventsDbContext _context;
        private readonly IMapper _mapper;

        public DevEventsController(DevEventsDbContext context,
            IMapper mapper)
        {
            _context = context; //o underline faz ser um campo privado
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var devEvents = _context.DevEvents.Where(d => !d.IsDeleted).ToList();
            var viewModel = _mapper.Map<List<DevEventViewModel>>(devEvents);
            return Ok(viewModel);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(Guid id)
        {
            var devEvent = _context.DevEvents
                .Include(de => de.Speakers)
                .SingleOrDefault(d => d.Id == id);
            if (devEvent == null) 
            {
                return NotFound();
            }
            var viewModel = _mapper.Map<DevEventViewModel>(devEvent);
            return Ok(viewModel);
        }

        [HttpPost]
        public IActionResult Post(DevEventInputModel input)
        {
            var devEvent = _mapper.Map<DevEvent> (input);
            _context.DevEvents.Add(devEvent);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new {id = devEvent.Id}, devEvent);
        }

        [HttpPut("{id}")]
        public IActionResult Update(Guid id, DevEventInputModel input)
        {
            var devEvent = _context.DevEvents.SingleOrDefault(d => d.Id == id);
            if (devEvent == null)
            {
                return NotFound();
            }
            
            devEvent.Update(input.Title, input.Description, input.StartDate, input.EndDate);

            _context.DevEvents.Update(devEvent);
            _context.SaveChanges();
            return NoContent();

        }

        [HttpDelete("{id}")]
        public IActionResult Delete(Guid id)
        {
            var devEvent = _context.DevEvents.SingleOrDefault(d => d.Id == id);

            if(devEvent == null)
            {
                return NotFound();
            }

            devEvent.Delete();
            _context.SaveChanges();
            return NoContent();
        }

        [HttpPost("{id}/speakers")]

        public ActionResult PostSpeaker(Guid id, DevEventSpeakerInputModel input)
        {
            var speaker = _mapper.Map<DevEventSpeaker>(input);
            speaker.DevEventId = id;

            var devEvent = _context.DevEvents.Any(d => d.Id == id);
            if (!devEvent)
            {
                return NotFound();
            }
            //devEvent.Speakers.Add(speaker);
            _context.DevEventSpeakers.Add(speaker);
            _context.SaveChanges();

            return NoContent();
        }
    }
}
