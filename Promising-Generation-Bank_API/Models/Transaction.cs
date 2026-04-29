using Promising_Generation_Bank_API.Enums;
using System.Text.Json.Serialization;

namespace Promising_Generation_Bank_API.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int ChildId { get; set; }
        public int? QuestId { get; set; }
        public string ActivityName { get; set; }
        public decimal CashAmount { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        // Navigation Property
        [JsonIgnore]
        public Child Child { get; set; }
        [JsonIgnore]
        public Quest Quest { get; set; }
    }
}
