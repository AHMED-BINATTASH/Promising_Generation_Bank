using Promising_Generation_Bank_API.Enums;

namespace Promising_Generation_Bank_API.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int ChildId { get; set; }
        public TransactionType Type { get; set; }
        public int PointsAmount { get; set; } // قد يكون موجب أو سالب
        public decimal CashAmount { get; set; } // قد يكون موجب أو سالب
        public decimal BalanceAfter { get; set; } // الرصيد النقدي بعد العملية
        public DateTime Date { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public Child Child { get; set; }
    }
}
