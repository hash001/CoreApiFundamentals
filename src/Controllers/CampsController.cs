using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace CoreCodeCamp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampsController : ControllerBase
    {
        private readonly ICampRepository _campRepository;
        private readonly IMapper _mapper;
        private readonly LinkGenerator _linkGenerator;

        public CampsController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
        {
            _campRepository = campRepository;
            _mapper = mapper;
            _linkGenerator = linkGenerator;
        }

        [HttpGet]
        public async Task<ActionResult<CampModel[]>> GetCamps(bool includeTalks = false)
        {
            try
            {
                var results = await _campRepository.GetAllCampsAsync(includeTalks);
                return _mapper.Map<CampModel[]>(results);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("{moniker}")]
        public async Task<ActionResult<CampModel>> GetCamp(string moniker)
        {
            try
            {
                var result = await _campRepository.GetCampAsync(moniker);
                if (result == null) return NotFound();
                return _mapper.Map<CampModel>(result);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<CampModel[]>> SearchByDate(DateTime date, bool includeTalks = false)
        {
            try
            {
                var result = await _campRepository.GetAllCampsByEventDate(date, includeTalks);
                if (!result.Any()) return NotFound();
                return _mapper.Map<CampModel[]>(result);
            }
            catch (Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpPost]
        public async Task<ActionResult<CampModel>> CreateCamp(CampModel model)
        {
            try
            {
                var existingCamp = await _campRepository.GetCampAsync(model.Moniker);
                if (existingCamp != null) return BadRequest("Moniker in use");
                var location = _linkGenerator.GetPathByAction("GetCamp", "Camps", new {moniker = model.Moniker});
                if (string.IsNullOrWhiteSpace(location)) return BadRequest("Could not use current moniker.");
                var camp = _mapper.Map<Camp>(model);
                _campRepository.Add(camp);
                var res = await _campRepository.SaveChangesAsync();
                if (!res) return BadRequest();
                return Created(location, _mapper.Map<CampModel>(camp));
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpPut("{moniker}")]
        public async Task<ActionResult<CampModel>> UpdateCamp(string moniker, CampModel model)
        {
            try
            {
                var existingCamp = await _campRepository.GetCampAsync(moniker);
                if (existingCamp == null) return NotFound($"Camp with {moniker} does not exist.");
                _mapper.Map(model, existingCamp);
                var res = await _campRepository.SaveChangesAsync();
                if (!res) return BadRequest();
                return _mapper.Map<CampModel>(existingCamp);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }

        [HttpDelete("{moniker}")]
        public async Task<IActionResult> DeleteCamp(string moniker)
        {
            try
            {
                var existingCamp = await _campRepository.GetCampAsync(moniker);
                if (existingCamp == null) return NotFound($"No camps exist with {moniker} moniker.");
                _campRepository.Delete(existingCamp);
                var res = await _campRepository.SaveChangesAsync();
                if (!res) return BadRequest();
                return Ok();
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
            }
        }
    }
}