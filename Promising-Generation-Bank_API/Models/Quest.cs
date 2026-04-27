using Promising_Generation_Bank_API.Enums;

namespace Promising_Generation_Bank_API.Models
{
    public class Quest
    {
        public int Id { get; set; }
        public int ChildId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int RewardPoints { get; set; }
        public QuestStatus Status { get; set; } = QuestStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public Child Child { get; set; }
    }
}
