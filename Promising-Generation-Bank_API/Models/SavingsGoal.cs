namespace Promising_Generation_Bank_API.Models
{
    public class SavingsGoal
    {
        public int Id { get; set; }
        public int ChildId { get; set; }
        public string Name { get; set; } = string.Empty; // مثل: دراجة
        public decimal TargetAmount { get; set; }
        public decimal CurrentAmount { get; set; } = 0m;

        // Navigation Property
        public Child Child { get; set; }
    }
}
