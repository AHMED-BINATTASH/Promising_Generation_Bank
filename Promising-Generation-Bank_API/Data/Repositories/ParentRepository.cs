namespace Promising_Generation_Bank_API.Data.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Promising_Generation_Bank_API.Models;

    namespace PromisingGenerationBank.Repositories
    {
        public class ParentRepository
        {
            private readonly AppDbContext _context;

            public ParentRepository(AppDbContext context)
            {
                _context = context;
            }

            // CRUD الأساسية
            public async Task<IEnumerable<Parent>> GetAllAsync() => await _context.Parents.ToListAsync();
            public async Task<Parent?> GetByIdAsync(int id) => await _context.Parents.FindAsync(id);
            public async Task AddAsync(Parent parent) => await _context.Parents.AddAsync(parent);
            public void Update(Parent parent) => _context.Parents.Update(parent);
            public void Delete(Parent parent) => _context.Parents.Remove(parent);
            public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

            // دوال مخصصة للمشروع
            public async Task<Parent?> GetParentWithChildrenAsync(int parentId)
            {
                return await _context.Parents
                    .Include(p => p.Children)
                    .FirstOrDefaultAsync(p => p.Id == parentId);
            }
        }
    }
}
