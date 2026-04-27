using Microsoft.EntityFrameworkCore;
using Promising_Generation_Bank_API.Models;

namespace Promising_Generation_Bank_API.Data.Repositories
{
        public class TransactionRepository
        {
            private readonly AppDbContext _context;

            public TransactionRepository(AppDbContext context)
            {
                _context = context;
            }

            public async Task<IEnumerable<Transaction>> GetAllAsync() => await _context.Transactions.ToListAsync();
            public async Task<Transaction?> GetByIdAsync(int id) => await _context.Transactions.FindAsync(id);
            public async Task AddAsync(Transaction transaction) => await _context.Transactions.AddAsync(transaction);
            // التحديث والحذف غالباً لا يُسمح بهما في المعاملات المالية للحفاظ على النزاهة (Data Integrity)
            public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

            // دوال مخصصة للمشروع
            public async Task<IEnumerable<Transaction>> GetRecentTransactionsForChildAsync(int childId)
            {
                return await _context.Transactions
                    .Where(t => t.ChildId == childId)
                    .OrderByDescending(t => t.Date)
                    .Take(10) // جلب آخر 10 حركات فقط
                    .ToListAsync();
            }
        }
    }
