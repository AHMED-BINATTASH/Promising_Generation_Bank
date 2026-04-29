namespace Promising_Generation_Bank_API.DTOs
{
    public class QuestRequestDto
    {
        public string? SessionId { get; set; } // اختياري (يمكن أن يكون ID الأبمن قاعدة البيانات)
        public string Message { get; set; }
    }
}
