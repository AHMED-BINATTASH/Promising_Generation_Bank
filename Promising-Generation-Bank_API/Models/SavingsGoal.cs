using System.Text.Json.Serialization;

namespace Promising_Generation_Bank_API.Models
{
    public class SavingsGoal
    {
        public int Id { get; set; }
        public int ChildId { get; set; }
        public string Name { get; set; } 
        public string ImageUrl { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; } = 0m;
        public bool IsCompleted { get; set; } = false;
        public double ProgressPercentage => TargetAmount > 0 ? (double)(CurrentAmount / TargetAmount) * 100 : 0;

        // Navigation Property
        [JsonIgnore]
        public Child Child { get; set; }
    }
}
