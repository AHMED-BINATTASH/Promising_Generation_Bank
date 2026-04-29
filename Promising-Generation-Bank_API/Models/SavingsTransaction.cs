namespace Promising_Generation_Bank_API.Models
{
    public class SavingsTransaction
    {
        public int Id { get; set; }
        public int SavingsGoalId { get; set; }
     
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
        public string? Notes { get; set; }

        // Navigation Properties
        public SavingsGoal SavingsGoal { get; set; }
        public SavingsTransaction(int savingsGoalId, decimal amount, string? notes = null)
        {
            SavingsGoalId = savingsGoalId;
         
            Amount = amount;
            Notes = notes;
            TransactionDate = DateTime.Now;
        }

        // Empty constructor for EF Core
        private SavingsTransaction() { }


    }
}
