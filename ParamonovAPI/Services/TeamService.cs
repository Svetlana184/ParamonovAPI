using Microsoft.EntityFrameworkCore;
using ParamonovAPI.Models;
using Task = System.Threading.Tasks.Task;

namespace ParamonovAPI.Services
{
    public class TeamService : IService<Team>
    {
        private readonly PostgresContext _context;

        public TeamService(PostgresContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Team>> GetAll()
            => await _context.Teams.ToListAsync();

        public async Task<Team> GetById(int id)
            => await _context.Teams.FindAsync(id);

        public async Task Create(Team item)
        {
            await _context.Teams.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Team item)
        {
            _context.Entry(item).State = EntityState.Modified;
            _context.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var item = await _context.Teams.FindAsync(id);
            if (item != null)
            {
                _context.Teams.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}
