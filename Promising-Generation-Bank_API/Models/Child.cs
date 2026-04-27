using System.Collections;
using System.Transactions;

namespace Promising_Generation_Bank_API.Models
{
    public class Child
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string AvatarUrl { get; set; } = string.Empty;

        // الأرصدة وإعدادات التحويل
        public int TotalPoints { get; set; } = 0;
        public decimal SavingsBalance { get; set; } = 0m;
        public decimal PointExchangeRate { get; set; } = 0.5m; // مثال: النقطة = 0.5 ريال

        // Navigation Properties
        public Parent Parent { get; set; }
        public ICollection<Quest> Quests { get; set; } = new List<Quest>();
        public ICollection<SavingsGoal> SavingsGoals { get; set; } = new List<SavingsGoal>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
