using Microsoft.EntityFrameworkCore;
using ParamonovAPI.Models;
using Task = System.Threading.Tasks.Task;

namespace ParamonovAPI.Services
{
    public class ProjectService : IService<Project>
    {
        private readonly PostgresContext _context;

        public ProjectService(PostgresContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Project>> GetAll()
            => await _context.Projects.ToListAsync();

        public async Task<Project> GetById(int id)
            => await _context.Projects.FindAsync(id);

        public async Task Create(Project item)
        {
            await _context.Projects.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Project item)
        {
            _context.Entry(item).State = EntityState.Modified;
            _context.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var item = await _context.Projects.FindAsync(id);
            if (item != null)
            {
                _context.Projects.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}
