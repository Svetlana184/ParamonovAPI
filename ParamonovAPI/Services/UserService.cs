using Microsoft.EntityFrameworkCore;
using ParamonovAPI.Models;
using Task = System.Threading.Tasks.Task;

namespace ParamonovAPI.Services
{
    public class UserService : IService<User>
    {
        private readonly PostgresContext _context;

        public UserService(PostgresContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAll()
            => await _context.Users.ToListAsync();

        public async Task<User> GetById(int id)
           => await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.IdUser == id);

        public async Task Create(User item)
        {
            await _context.Users.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task Update(User item)
        {
            _context.Entry(item).State = EntityState.Modified;
            _context.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var item = await _context.Users.FindAsync(id);
            if (item != null)
            {
                _context.Users.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}
