namespace Promising_Generation_Bank_API.Enums
{
    public enum QuestStatus
    {
        Pending,    // قيد الانتظار/التنفيذ
        Completed,  // مكتملة بانتظار موافقة الأب
        Approved    // تمت الموافقة وتم تحويل النقاط
    }

    public enum TransactionType
    {
        PointsEarned,      // كسب نقاط من مهمة
        PointsConverted,   // تحويل نقاط إلى كاش
        GoalFunded         // إيداع كاش في هدف ادخار
    }
}
