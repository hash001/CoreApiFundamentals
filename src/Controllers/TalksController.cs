using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    [Route("api/camps/{moniker}/talks")]
    public class TalksController : ControllerBase
    {
        private readonly ICampRepository _campRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public TalksController(ICampRepository campRepository,
                               IMapper mapper,
                               LinkGenerator linkGenerator)
        {
            _campRepository = campRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<TalkModel[]>> GetTalks(string moniker)
        {
            try
            {
                var talks = await _campRepository.GetTalksByMonikerAsync(moniker, true);
                if (!talks.Any()) return NotFound("No talks by that moniker.");

                return _mapper.Map<TalkModel[]>(talks);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to get talks.");
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TalkModel>> GetTalk(string moniker, int id)
        {
            try
            {
                var talk = await _campRepository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null) return NotFound("Talk not found.");

                return _mapper.Map<TalkModel>(talk);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong.");
            }
        }

        [HttpPost]
        public async Task<ActionResult<TalkModel>> CreateTalk(string moniker, TalkModel model)
        {
            try
            {
                var camp = await _campRepository.GetCampAsync(moniker);
                if (camp == null) return BadRequest("Camp not found.");
                var talk = _mapper.Map<Talk>(model);

                if (model.Speaker == null) return BadRequest("SpeakerId is required");
                var speaker = await _campRepository.GetSpeakerAsync(model.Speaker.SpeakerId);
                if (speaker == null) return BadRequest("Speaker not found.");

                talk.Camp = camp;
                talk.Speaker = speaker;
                _campRepository.Add(talk);

                var res = await _campRepository.SaveChangesAsync();
                if (!res) return BadRequest();

                var location = _linkGenerator.GetPathByAction(HttpContext, "GetTalk", "Talks", new { moniker, id = talk.TalkId });
                return Created(location, _mapper.Map<TalkModel>(talk));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong.");
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<TalkModel>> UpdateTalk(string moniker, int id, TalkModel model)
        {
            try
            {
                var talk = await _campRepository.GetTalkByMonikerAsync(moniker, id, true);
                if (talk == null) return NotFound("Talk not found");
                var modTalk = _mapper.Map(model, talk);
                if (model.Speaker != null)
                {
                    var speaker = await _campRepository.GetSpeakerAsync(model.Speaker.SpeakerId);
                    if (speaker != null)
                    {
                        talk.Speaker = speaker;
                    }
                }

                var res = await _campRepository.SaveChangesAsync();
                if (!res) return BadRequest();

                return _mapper.Map(modTalk, model);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong.");
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTalk(string moniker, int id)
        {
            try
            {
                var talk = await _campRepository.GetTalkByMonikerAsync(moniker, id);
                if (talk == null) return NotFound($"No talks exist for moniker: {moniker} and id: {id}");

                _campRepository.Delete(talk);
                var res = await _campRepository.SaveChangesAsync();
                if (!res) return BadRequest();

                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Something went wrong.");
            }
        }
    }
}
