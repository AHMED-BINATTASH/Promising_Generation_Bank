namespace Promising_Generation_Bank_API.Models
{
    public class Parent
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal TotalFamilyBalance { get; set; } = 0m;
        public decimal EarnedThisWeek { get; set; } = 0m;
        public int PendingApprovals { get; set; } = 0;
        public int ActiveChildren { get; set; } = 0;


        // Navigation Property
        public ICollection<Child> Children { get; set; } = new List<Child>();
    }
}
