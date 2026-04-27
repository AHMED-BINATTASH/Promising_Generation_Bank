namespace Promising_Generation_Bank_API.Data.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Promising_Generation_Bank_API.Enums;
    using Promising_Generation_Bank_API.Models;

    namespace PromisingGenerationBank.Repositories
    {
        public class ChildRepository
        {
            private readonly AppDbContext _context;

            public ChildRepository(AppDbContext context)
            {
                _context = context;
            }

            public async Task<IEnumerable<Child>> GetAllAsync() => await _context.Children.ToListAsync();
            public async Task<Child?> GetByIdAsync(int id) => await _context.Children.FindAsync(id);
            public async Task AddAsync(Child child) => await _context.Children.AddAsync(child);
            public void Update(Child child) => _context.Children.Update(child);
            public void Delete(Child child) => _context.Children.Remove(child);
            public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

            // دوال مخصصة للمشروع
            public async Task<Child?> GetChildDashboardDataAsync(int childId)
            {
                return await _context.Children
                    .Include(c => c.Quests.Where(q => q.Status != QuestStatus.Approved))
                    .Include(c => c.SavingsGoals)
                    .FirstOrDefaultAsync(c => c.Id == childId);
            }
        }
    }
}
