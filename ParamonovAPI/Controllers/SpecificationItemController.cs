using Microsoft.AspNetCore.Mvc;
using ParamonovAPI.Models;
using ParamonovAPI.Services;

namespace ParamonovAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SpecificationItemController : ControllerBase
    {
        private readonly IService<SpecificationItem> _service;

        public SpecificationItemController(IService<SpecificationItem> service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<SpecificationItem>>> GetAll()
            => Ok(await _service.GetAll());

        [HttpGet("{id}")]
        public async Task<ActionResult<SpecificationItem>> GetById(int id)
        {
            var item = await _service.GetById(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SpecificationItem item)
        {
            await _service.Create(item);
            return CreatedAtAction(nameof(GetById), new { id = item.IdItem }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, SpecificationItem item)
        {
            if (id != item.IdItem) return BadRequest();
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
