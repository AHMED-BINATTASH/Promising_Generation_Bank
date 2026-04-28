using Promising_Generation_Bank_API.Enums;
using System.Text.Json.Serialization;

namespace Promising_Generation_Bank_API.Models
{
    public class Quest
    {
        public int Id { get; set; }
        public int ChildId { get; set; }
        public int ParentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }

        public string emoji { get; set; }
        public QuestStatus Status { get; set; } = QuestStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Property
        [JsonIgnore]
        public Child? Child { get; set; }
        [JsonIgnore]
        public Parent? Parent { get; set; }
        [JsonIgnore]
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
