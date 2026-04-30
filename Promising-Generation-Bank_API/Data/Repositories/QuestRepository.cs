using Microsoft.EntityFrameworkCore;
using Promising_Generation_Bank_API.Enums;
using Promising_Generation_Bank_API.Models;

namespace Promising_Generation_Bank_API.Data.Repositories
{
    public class QuestRepository
    {
        private readonly AppDbContext _context;

        public QuestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Quest>> GetQuestsByParentAsync(int parentId)
        {
            return await _context.Quests
                .Where(q => q.ParentId == parentId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }
        public async Task<IEnumerable<Quest>> GetActiveQuestsForChildAsync(int childId)
        {
            // نعرض للطفل المهام التي حالتها ليست "Approved" (أي Pending أو Completed)
            return await _context.Quests
                .Where(q => q.ChildId == childId && q.Status != QuestStatus.Approved)
                .ToListAsync();
        }
        public async Task<IEnumerable<Quest>> GetChildQuestsById(int childId)
        {
            // نعرض للطفل المهام التي حالتها ليست "Approved" (أي Pending أو Completed)
            return await _context.Quests
                .Where(q => q.ChildId == childId)
                .ToListAsync();
        }
        public async Task<Quest> AddAsync(Quest quest)
        {
            _context.Quests.Add(quest);
            await _context.SaveChangesAsync();
            return quest;
        }
        public async Task<bool> MakeQuestApprovedAsync(int id)
        {
            // 1. جلب المهمة مع بيانات الطفل المرتبطة بها لتحديث رصيده لاحقاً
            var quest = await _context.Quests
                                      .Include(q => q.Child)
                                      .FirstOrDefaultAsync(q => q.Id == id);

            if (quest == null) return false;

            // إجراء حماية: التأكد أن المهمة لم تتم الموافقة عليها مسبقاً لمنع مضاعفة الرصيد
            if (quest.Status == QuestStatus.Approved)
            {
                return false;
            }

            // 2. تحديث حالة المهمة إلى "موافق عليها"
            quest.Status = QuestStatus.Approved;

            // 3. إضافة مبلغ المهمة إلى رصيد مدخرات الطفل
            if (quest.Child != null)
            {
                quest.Child.SavingsBalance += quest.Amount;
            }

            // 4. حفظ التغييرات في قاعدة البيانات (سيتم حفظ حالة المهمة ورصيد الطفل معاً)
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> MakeQuestCompletedAsync(int questId)
        {
            // 1. جلب المهمة مع بيانات الطفل المرتبطة بها (للحصول على اسمه)
            var quest = await _context.Quests
                                      .Include(q => q.Child)
                                      .FirstOrDefaultAsync(q => q.Id == questId);

            if (quest == null) return false;

            // إجراء حماية: التأكد أن المهمة لم تكتمل مسبقاً
            if (quest.Status == QuestStatus.Completed)
            {
                return false; // أو يمكنك رمي Exception حسب قواعد العمل لديك
            }

            // 2. تحديث حالة المهمة إلى مكتملة
            quest.Status = QuestStatus.Completed;

            // 3. إنشاء سجل الحركة (Transaction) بالمعلومات المطلوبة
            var transaction = new Transaction
            {
                ChildId = quest.ChildId,
                QuestId = quest.Id,
                ActivityName = quest.Title,
                CashAmount = quest.Amount,
                Date = DateTime.UtcNow
            };

            // إضافة السجل الجديد إلى سياق قاعدة البيانات
            await _context.Transactions.AddAsync(transaction);

            // 4. حفظ التغييرات (EF Core سيقوم بتحديث الـ Quest وإضافة الـ Transaction في عملية واحدة)
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var quest = await _context.Quests.FindAsync(id);
            if (quest == null) return false;

            _context.Quests.Remove(quest);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<Quest?> GetByIdAsync(int id)
        {
            return await _context.Quests.FindAsync(id);
        }
    }
}

