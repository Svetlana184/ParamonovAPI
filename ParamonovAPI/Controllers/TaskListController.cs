using Microsoft.AspNetCore.Mvc;
using ParamonovAPI.Models;
using ParamonovAPI.Services;

namespace ParamonovAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskListController : ControllerBase
    {
        private readonly IService<TaskList> _service;

        public TaskListController(IService<TaskList> service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskList>>> GetAll()
            => Ok(await _service.GetAll());

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskList>> GetById(int id)
        {
            var item = await _service.GetById(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaskList item)
        {
            await _service.Create(item);
            return CreatedAtAction(nameof(GetById), new { id = item.IdTaskList }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TaskList item)
        {
            if (id != item.IdTaskList) return BadRequest();
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
