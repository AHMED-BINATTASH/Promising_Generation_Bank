using Microsoft.EntityFrameworkCore;
using Promising_Generation_Bank_API.Models;

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

            // 1. جلب كل مهام العائلة (لواجهة الأب)
            public async Task<IEnumerable<Quest>> GetQuestsByParentAsync(int parentId)
            {
                return await _context.Quests
                    .Where(q => q.ParentId == parentId)
                    .OrderByDescending(q => q.CreatedAt)
                    .ToListAsync();
            }

            // 2. جلب المهام المتاحة للطفل (التي لم يتم الموافقة عليها بعد)
            public async Task<IEnumerable<Quest>> GetActiveQuestsForChildAsync(int childId)
            {
                // نعرض للطفل المهام التي حالتها ليست "Approved" (أي Pending أو Completed)
                return await _context.Quests
                    .Where(q => q.ChildId == childId && q.Status != QuestStatus.Approved)
                    .ToListAsync();
            }

            // 3. إضافة مهمة جديدة
            public async Task<Quest> AddAsync(Quest quest)
            {
                _context.Quests.Add(quest);
                await _context.SaveChangesAsync();
                return quest;
            }

            // 4. تحديث حالة المهمة (مثل من Pending إلى Completed)
            public async Task<Quest?> UpdateStatusAsync(int id, string status)
            {
                var quest = await _context.Quests.FindAsync(id);
                if (quest == null) return null;

                // 1. Validate the Enum input properly
                if (!Enum.TryParse<QuestStatus>(status, true, out var result))
                {
                    // Option A: Throw an exception (if your controller handles them)
                    // Option B: Return a specific indicator that the input was bad
                    throw new ArgumentException($"Invalid status value: {status}");
                }


                // 2. Update the property
                quest.Status = result;

                if (quest.Status == QuestStatus.Approved)
                {
                   var child = _context.Children.Where(c => c.Id == quest.ChildId).First();
                    child.SavingsBalance += quest.Amount;
                }

                // 3. Save changes only if the update was valid
                await _context.SaveChangesAsync();

                return quest;
            }
            // 5. حذف مهمة
            public async Task<bool> DeleteAsync(int id)
            {
                var quest = await _context.Quests.FindAsync(id);
                if (quest == null) return false;

                _context.Quests.Remove(quest);
                await _context.SaveChangesAsync();
                return true;
            }

            // دالة إضافية مفيدة لـ "الموافقة النهائية"
            public async Task<Quest?> GetByIdAsync(int id)
            {
                return await _context.Quests.FindAsync(id);
            }
        }
    }
}

