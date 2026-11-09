using Microsoft.AspNetCore.Mvc;
using ParamonovAPI.Models;
using ParamonovAPI.Services;

namespace ParamonovAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IService<Invoice> _service;

        public InvoiceController(IService<Invoice> service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Invoice>>> GetAll()
            => Ok(await _service.GetAll());

        [HttpGet("{id}")]
        public async Task<ActionResult<Invoice>> GetById(int id)
        {
            var item = await _service.GetById(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Invoice item)
        {
            await _service.Create(item);
            return CreatedAtAction(nameof(GetById), new { id = item.IdInvoice }, item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Invoice item)
        {
            if (id != item.IdInvoice) return BadRequest();
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
