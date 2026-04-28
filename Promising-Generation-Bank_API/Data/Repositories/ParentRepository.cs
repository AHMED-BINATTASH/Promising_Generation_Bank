namespace Promising_Generation_Bank_API.Data.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Promising_Generation_Bank_API.Models;

    namespace PromisingGenerationBank.Repositories
    {
        public class ParentRepository
        {
            private readonly AppDbContext _context;
            public ParentRepository(AppDbContext context) => _context = context;

            public async Task<Parent?> GetByIdAsync(int id)
            {
                return await _context.Parents
                    .Include(p => p.Children)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }

            public async Task<Parent> AddAsync(Parent parent)
            {
                _context.Parents.Add(parent);
                await _context.SaveChangesAsync();
                return parent;
            }

            public async Task<Parent?> UpdateAsync(int id, Parent parentUpdate)
            {
                var existing = await _context.Parents.FindAsync(id);
                if (existing == null) return null;

                existing.Name = parentUpdate.Name;
                // أضف أي حقول أخرى تود تحديثها هنا

                await _context.SaveChangesAsync();
                return existing;
            }
        }
    }
}
