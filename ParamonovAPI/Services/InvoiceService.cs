using Microsoft.EntityFrameworkCore;
using ParamonovAPI.Models;
using Task = System.Threading.Tasks.Task;

namespace ParamonovAPI.Services
{
    public class InvoiceService : IService<Invoice>
    {
        private readonly PostgresContext _context;

        public InvoiceService(PostgresContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Invoice>> GetAll()
            => await _context.Invoices.ToListAsync();

        public async Task<Invoice> GetById(int id)
            => await _context.Invoices.FindAsync(id);

        public async Task Create(Invoice item)
        {
            await _context.Invoices.AddAsync(item);
            await _context.SaveChangesAsync();
        }

        public async Task Update(Invoice item)
        {
            _context.Entry(item).State = EntityState.Modified;
            _context.Update(item);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var item = await _context.Invoices.FindAsync(id);
            if (item != null)
            {
                _context.Invoices.Remove(item);
                await _context.SaveChangesAsync();
            }
        }
    }
}
