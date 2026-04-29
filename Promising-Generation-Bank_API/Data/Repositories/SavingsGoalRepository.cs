namespace Promising_Generation_Bank_API.Data.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using Promising_Generation_Bank_API.Models;

    namespace PromisingGenerationBank.Repositories
    {
        public class SavingsGoalRepository
        {
            private readonly AppDbContext _context;
            public SavingsGoalRepository(AppDbContext context) => _context = context;
            public async Task<IEnumerable<SavingsGoal>> GetGoalsByChildIdAsync(int childId)
            {
                return await _context.SavingsGoals
                    .Where(sg => sg.ChildId == childId)
                    .ToListAsync();
            }
            public async Task<SavingsGoal> AddAsync(SavingsGoal goal)
            {
                _context.SavingsGoals.Add(goal);
                await _context.SaveChangesAsync();
                return goal;
            }
            public async Task<SavingsGoal?> UpdateProgressAsync(int goalId, decimal amountToAdd)
            {
                var goal = await _context.SavingsGoals.FindAsync(goalId);
                if (goal == null) return null;

                goal.CurrentAmount += amountToAdd;

                // التحقق من اكتمال الهدف
                if (goal.CurrentAmount >= goal.TargetAmount)
                {
                    goal.IsCompleted = true;
                }

                await _context.SaveChangesAsync();
                return goal;
            }
        }
    }
}
