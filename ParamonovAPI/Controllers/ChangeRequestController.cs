using Microsoft.AspNetCore.Mvc;
using ParamonovAPI.Models;
using ParamonovAPI.Services;

namespace ParamonovAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChangeRequestController : ControllerBase
    {
        private readonly IService<ChangeRequest> _service;

        public ChangeRequestController(IService<ChangeRequest> service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChangeRequest>>> GetAll()
            => Ok(await _service.GetAll());

        [HttpGet("{id}")]
        public async Task<ActionResult<ChangeRequest>> GetById(int id)
        {
            var item = await _service.GetById(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ChangeRequest item)
        {
            await _service.Create(item);
            return CreatedAtAction(nameof(GetById), new { id = item.IdChange }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ChangeRequest item)
        {
            if (id != item.IdChange) return BadRequest();
            var existing = await _service.GetById(id);
            if (existing == null) return NotFound();
            await _service.Update(item);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _service.GetById(id);
            if (existing == null) return NotFound();
            await _service.Delete(id);
            return NoContent();
        }
    }
}
