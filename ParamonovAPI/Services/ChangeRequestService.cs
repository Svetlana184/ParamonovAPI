using Microsoft.EntityFrameworkCore;
using ParamonovAPI.Models;
using Task = System.Threading.Tasks.Task;

namespace ParamonovAPI.Services
{
    public class ChangeRequestService : IService<ChangeRequest>
    {
        private readonly PostgresContext _context;

        public ChangeRequestService(PostgresContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ChangeRequest>> GetAll()
            => await _context.ChangeRequests.ToListAsync();

        public async Task<ChangeRequest> GetById(int id)
            => await _context.ChangeRequests.FindAsync(id);

        public async Task Create(ChangeRequest item)
        {
            await _context.ChangeRequests.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task Update(ChangeRequest item)
        {
            _context.Entry(item).State = EntityState.Modified;
            _context.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var item = await _context.ChangeRequests.FindAsync(id);
            if (item != null)
            {
                _context.ChangeRequests.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}
