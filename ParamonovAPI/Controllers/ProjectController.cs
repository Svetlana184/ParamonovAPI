using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ParamonovAPI.Models;
using ParamonovAPI.Services;
using System.Security.Claims;

namespace ParamonovAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : ControllerBase
    {
        private readonly IService<Project> _service;
        private readonly PostgresContext _context;

        public ProjectController(IService<Project> service, PostgresContext context)
        {
            _service = service;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetAll()
            => Ok(await _service.GetAll());

        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<Project>>> GetMyProjects()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            IQueryable<Project> query = _context.Projects;

            if (user.Role == "customer")
            {
                query = query.Where(p => p.IdCustomer == userId);
            }
            else
            {
                if (user.IdTeam == null) return Ok(new List<Project>());
                int teamId = user.IdTeam.Value;
                query = query.Where(p => p.IdTeam == teamId);
            }

            return Ok(await query.AsNoTracking().ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetById(int id)
        {
            var project = await _context.Projects
                .AsNoTracking()
                .Include(p => p.IdInvoiceNavigation)
                .Include(p => p.TechnicalSpecifications)
                .FirstOrDefaultAsync(p => p.IdProject == id);

            if (project == null) return NotFound();
            return Ok(project);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Project item)
        {
            await _service.Create(item);

            // 1. Создаем пустое шаблонное ТЗ
            var spec = new TechnicalSpecification
            {
                IdProject = item.IdProject,
                Name = "Техническое задание",
                Description = "Шаблонное ТЗ для проекта"
            };
            _context.TechnicalSpecifications.Add(spec);

            // 2. Создаем пустой счёт
            var invoice = new Invoice
            {
                Name = $"Счёт для проекта {item.Name}",
                Cost = 0,
                StartDate = DateOnly.FromDateTime(DateTime.Now),
                EndDate = null
            };
            _context.Invoices.Add(invoice);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = item.IdProject }, item);
        }
        [HttpPost("draft")]
        public async Task<IActionResult> CreateDraft([FromBody] CreateProjectDraftDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized();

            var project = new Project
            {
                Name = dto.Name ?? "Новый проект",
                Description = dto.Description ?? "",
                Status = "draft",
                IdCustomer = userId,
                TotalCost = 0
            };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var spec = new TechnicalSpecification
            {
                IdProject = project.IdProject,
                Name = "ТЗ для проекта " + project.Name,
                Description = "",
                Status = "draft"
            };
            _context.TechnicalSpecifications.Add(spec);

            var invoice = new Invoice
            {
                Name = "Счёт для проекта " + project.Name,
                Cost = 0,
                TotalCost = 0,
                IdProject = project.IdProject
            };
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            project.IdInvoice = invoice.IdInvoice;
            await _context.SaveChangesAsync();

            return Ok(project);
        }
        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitProject(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            var spec = await _context.TechnicalSpecifications
                .FirstOrDefaultAsync(s => s.IdProject == id);
            if (spec == null) return BadRequest("ТЗ не найдено");

            if (project.IdTeam == null) return BadRequest("Команда не назначена");

            spec.Status = "pending_approval";
            project.Status = "in_progress";

            await _context.SaveChangesAsync();
            return Ok(project);
        }
        [HttpPost("{id}/send-invoice")]
        public async Task<IActionResult> SendInvoice(int id)
        {
            // 1. Аутентификация
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized();

            // 2. Авторизация: только менеджер
            if (!string.Equals(user.Role, "manager", StringComparison.OrdinalIgnoreCase))
                return Forbid();

            // 3. Проект существует
            var project = await _context.Projects
                .Include(p => p.TechnicalSpecifications)
                .FirstOrDefaultAsync(p => p.IdProject == id);

            if (project == null)
                return NotFound(new { error = "Проект не найден" });

            // 4. Статус проекта — только in_progress
            if (project.Status != "in_progress")
                return BadRequest(new { error = $"Нельзя отправить счёт в статусе '{project.Status}'" });

            // 5. У проекта должен быть счёт
            if (project.IdInvoice == null)
                return BadRequest(new { error = "У проекта нет счёта" });

            // 6. Должно быть ТЗ (для IdSpecItem)
            var spec = project.TechnicalSpecifications.FirstOrDefault();
            if (spec == null)
                return BadRequest(new { error = "У проекта нет технического задания" });

            // 7. IdSpecItem — первый пункт ТЗ, если он есть
            var firstSpecItem = await _context.SpecificationItems
                .Where(si => si.IdSpec == spec.IdSpec)
                .OrderBy(si => si.IdItem)
                .FirstOrDefaultAsync();

            // 8. Защита от дублей: если уже есть активный запрос по этому счёту — не создаём новый
            var existing = await _context.ChangeRequests
                .FirstOrDefaultAsync(cr =>
                    cr.IdInvoice == project.IdInvoice &&
                    cr.Status == "created");

            if (existing == null)
            {
                var change = new ChangeRequest
                {
                    IdSpecItem = firstSpecItem?.IdItem ?? 0,
                    IdInvoice = project.IdInvoice,
                    IdTask = null,
                    Name = "Проверка счёта проекта " + project.Name,
                    Description = "Менеджер отправил счёт на проверку",
                    Status = "created",
                    IdCreator = userId
                };
                _context.ChangeRequests.Add(change);
            }

            // 9. Статус проекта → pending_invoice_approval
            project.Status = "pending_invoice_approval";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                idProject = project.IdProject,
                status = project.Status.ToString(),
                changeId = existing != null ? existing.IdChange : (int?)null
            });
        }


        [HttpPost("{id}/finish-approval")]
        public async Task<IActionResult> FinishApproval(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null) return NotFound();

            project.Status = "in_progress";
            await _context.SaveChangesAsync();
            return Ok(project);
        }

        [HttpPost("{id}/close")]
        public async Task<IActionResult> Close(int id, [FromQuery] bool permanent = true)
        {
            // 1. Аутентификация
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized();

            // 2. Авторизация: только customer или manager
            var role = user.Role?.ToLowerInvariant();
            if (role != "customer" && role != "manager")
                return Forbid();

            // 3. Проект существует
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound(new { error = "Проект не найден" });

            // 4. Только владелец (заказчик) или менеджер команды проекта
            if (role == "customer" && project.IdCustomer != userId)
                return Forbid();

            if (role == "manager" && project.IdTeam != user.IdTeam)
                return Forbid();

            // 5. Нельзя закрыть уже закрытый/законченный
            if (project.Status == "closed" || project.Status == "completed" )
                return BadRequest(new { error = "Проект уже закрыт или закончен" });

            // 6. Переводим статус
            project.Status = permanent
                ? "closed"
                : "completed";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                idProject = project.IdProject,
                status = project.Status.ToString()
            });
        }

        // ============================================================
        // POST /api/project/{id}/freeze
        // Замораживает проект (можно снять через /unfreeze)
        // ============================================================
        [HttpPost("{id}/freeze")]
        public async Task<IActionResult> Freeze(int id)
        {
            // 1. Аутентификация
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized();

            // 2. Авторизация: только customer или manager
            var role = user.Role?.ToLowerInvariant();
            if (role != "customer" && role != "manager")
                return Forbid();

            // 3. Проект существует
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound(new { error = "Проект не найден" });

            // 4. Владелец / менеджер команды
            if (role == "customer" && project.IdCustomer != userId)
                return Forbid();

            if (role == "manager" && project.IdTeam != user.IdTeam)
                return Forbid();

            // 5. Нельзя заморозить закрытый/законченный
            if (project.Status == "closed" || project.Status == "completed")
                return BadRequest(new { error = "Проект закрыт или закончен, заморозка невозможна" });

            // 6. Уже заморожен?
            if (project.Status == "frozen")
                return BadRequest(new { error = "Проект уже заморожен" });

            // 7. Замораживаем
            project.Status = "frozen";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                idProject = project.IdProject,
                status = project.Status.ToString()
            });
        }

        // ============================================================
        // POST /api/project/{id}/unfreeze
        // Разморозить → in_progress
        // ============================================================
        [HttpPost("{id}/unfreeze")]
        public async Task<IActionResult> Unfreeze(int id)
        {
            // 1. Аутентификация
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return Unauthorized();

            // 2. Авторизация: только customer или manager
            var role = user.Role?.ToLowerInvariant();
            if (role != "customer" && role != "manager")
                return Forbid();

            // 3. Проект существует
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return NotFound(new { error = "Проект не найден" });

            // 4. Владелец / менеджер команды
            if (role == "customer" && project.IdCustomer != userId)
                return Forbid();

            if (role == "manager" && project.IdTeam != user.IdTeam)
                return Forbid();

            // 5. Разморозить можно только замороженный
            if (project.Status != "frozen")
                return BadRequest(new { error = "Разморозить можно только замороженный проект" });

            // 6. Возвращаем в in_progress (по ТЗ — обратно в работу)
            project.Status = "in_progress";
            await _context.SaveChangesAsync();

            return Ok(new
            {
                idProject = project.IdProject,
                status = project.Status.ToString()
            });
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Project item)
        {
            if (id != item.IdProject) return BadRequest();
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
