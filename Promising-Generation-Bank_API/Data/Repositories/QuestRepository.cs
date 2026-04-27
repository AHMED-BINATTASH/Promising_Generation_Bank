namespace Promising_Generation_Bank_API.Data.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Promising_Generation_Bank_API.Enums;
    using Promising_Generation_Bank_API.Models;

    namespace PromisingGenerationBank.Repositories
    {
        public class QuestRepository
        {
            private readonly AppDbContext _context;

            public QuestRepository(AppDbContext context)
            {
                _context = context;
            }

            public async Task<IEnumerable<Quest>> GetAllAsync() => await _context.Quests.ToListAsync();
            public async Task<Quest?> GetByIdAsync(int id) => await _context.Quests.FindAsync(id);
            public async Task AddAsync(Quest quest) => await _context.Quests.AddAsync(quest);
            public void Update(Quest quest) => _context.Quests.Update(quest);
            public void Delete(Quest quest) => _context.Quests.Remove(quest);
            public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

            // دوال مخصصة للمشروع
            public async Task<IEnumerable<Quest>> GetPendingApprovalsForParentAsync(int parentId)
            {
                // جلب المهام التي أكملها الطفل وبانتظار موافقة الأب
                return await _context.Quests
                    .Include(q => q.Child)
                    .Where(q => q.Child.ParentId == parentId && q.Status == QuestStatus.Completed)
                    .ToListAsync();
            }
        }
    }
}
