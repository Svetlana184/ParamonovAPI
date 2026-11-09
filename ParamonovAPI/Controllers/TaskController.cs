using Microsoft.AspNetCore.Mvc;
using ParamonovAPI.Services;
using TaskModel = ParamonovAPI.Models.Task;

namespace ParamonovAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaskController : ControllerBase
    {
        private readonly IService<TaskModel> _service;

        public TaskController(IService<TaskModel> service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskModel>>> GetAll()
            => Ok(await _service.GetAll());

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskModel>> GetById(int id)
        {
            var item = await _service.GetById(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaskModel item)
        {
            await _service.Create(item);
            return CreatedAtAction(nameof(GetById), new { id = item.IdTask }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, TaskModel item)
        {
            if (id != item.IdTask) return BadRequest();
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
