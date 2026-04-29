using System.Collections;
using System.Text.Json.Serialization;
using System.Transactions;

namespace Promising_Generation_Bank_API.Models
{
    public class Child
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public int Level { get; set; } = 1;
        public string AvatarUrl { get; set; } = string.Empty;
        public decimal SavingsBalance { get; set; } = 0m;

        // Navigation Properties
        [JsonIgnore]
        public Parent? Parent { get; set; }
        [JsonIgnore]
        public ICollection<Quest> Quests { get; set; } = new List<Quest>();
        [JsonIgnore]
        public ICollection<SavingsGoal> SavingsGoals { get; set; } = new List<SavingsGoal>();
        [JsonIgnore]
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }

}
