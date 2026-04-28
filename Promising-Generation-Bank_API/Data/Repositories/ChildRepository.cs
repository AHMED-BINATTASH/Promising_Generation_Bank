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
            public ChildRepository(AppDbContext context) => _context = context;

            public async Task<Child> AddAsync(Child child)
            {
                _context.Children.Add(child);
                await _context.SaveChangesAsync();
                return child;
            }

            public async Task<IEnumerable<Child>> GetChildrenByParentIdAsync(int parentId)
            {
                return await _context.Children
                    .Where(c => c.ParentId == parentId)
                    .ToListAsync();
            }

            public async Task<Child?> GetChildDashboardDataAsync(int id)
            {
                return await _context.Children
                    .Include(c => c.Quests)
                    .Include(c => c.SavingsGoals)
                    .FirstOrDefaultAsync(c => c.Id == id);
            }

            public async Task<Child?> UpdateAsync(int id, Child childUpdate)
            {
                var existing = await _context.Children.FindAsync(id);
                if (existing == null) return null;

                existing.Name = childUpdate.Name;
                existing.Age = childUpdate.Age;
                existing.AvatarUrl = childUpdate.AvatarUrl;

                await _context.SaveChangesAsync();
                return existing;
            }

            public async Task<bool> DeleteAsync(int id)
            {
                var child = await _context.Children.FindAsync(id);
                if (child == null) return false;

                _context.Children.Remove(child);
                await _context.SaveChangesAsync();
                return true;
            }

            //public async Task<Child?> AddXPAsync(int childId, int xpToAdd)
            //{
            //    var child = await _context.Children.FindAsync(childId);
            //    if (child == null) return null;

            //    child.XP += xpToAdd;
            //    // منطق بسيط لزيادة الـ Level (كل 100 نقطة مستوى)
            //    child.Level = (child.XP / 100) + 1;

            //    await _context.SaveChangesAsync();
            //    return child;
            //}
        }
    }
}
