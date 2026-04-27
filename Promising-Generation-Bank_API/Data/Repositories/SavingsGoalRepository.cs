namespace Promising_Generation_Bank_API.Data.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Promising_Generation_Bank_API.Models;

    namespace PromisingGenerationBank.Repositories
    {
        public class SavingsGoalRepository
        {
            private readonly AppDbContext _context;

            public SavingsGoalRepository(AppDbContext context)
            {
                _context = context;
            }

            public async Task<IEnumerable<SavingsGoal>> GetAllAsync() => await _context.SavingsGoals.ToListAsync();
            public async Task<SavingsGoal?> GetByIdAsync(int id) => await _context.SavingsGoals.FindAsync(id);
            public async Task AddAsync(SavingsGoal goal) => await _context.SavingsGoals.AddAsync(goal);
            public void Update(SavingsGoal goal) => _context.SavingsGoals.Update(goal);
            public void Delete(SavingsGoal goal) => _context.SavingsGoals.Remove(goal);
            public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
        }
    }
}
