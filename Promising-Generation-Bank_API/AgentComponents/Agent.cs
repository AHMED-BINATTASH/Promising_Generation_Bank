namespace Promising_Generation_Bank_API.AgentComponents
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.SemanticKernel;
    using Microsoft.SemanticKernel.ChatCompletion;
    using Microsoft.SemanticKernel.Connectors.OpenAI;
    using System.Collections.Concurrent;
    using System;
    using System.Threading.Tasks;

    public class AgentService
    {
        private readonly Kernel _kernel;
        private readonly IChatCompletionService _chatCompletion;
        private readonly ConcurrentDictionary<string, ChatHistory> _sessions = new();

        // 1. قمنا بإزالة حقن أداة Pinecone لأننا لا نحتاج للبحث في ملفات
        public AgentService(IConfiguration config)
        {
            var builder = Kernel.CreateBuilder();

            builder.AddOpenAIChatCompletion(
                modelId: "gpt-4o-mini",
                apiKey: config["OpenAiApiKey"] ?? config["OpenAI_ApiKey"]
            );

            _kernel = builder.Build();
            _chatCompletion = _kernel.GetRequiredService<IChatCompletionService>();
        }

        public async Task<string> RunAgentAsync(string sessionId, string message)
        {
            var history = _sessions.GetOrAdd(sessionId, _ =>
            new ChatHistory(@"أنت خبير تربوي ومستشار مالي للأطفال، متخصص في 'تلعيب المهام' (Gamification) ضمن تطبيق بنكي.
مهمتك هي مساعدة الآباء باقتراح مهام (Quests) ذكية وممتعة للأطفال من سن 5 إلى 12 سنة لتعزيز المسؤولية والوعي المالي.

⚠️ القواعد المقدسة للإخراج (Strict JSON Output):
1. يجب أن يكون الرد حصرياً عبارة عن كائن JSON يحتوي على مصفوفة باسم 'quests'.
2. إياك أن تكتب أي حرف أو مقدمة أو خاتمة خارج هيكل الـ JSON.
3. كل مقترح (Object) داخل المصفوفة يجب أن يلتزم بهذا الهيكل (Keys) باللغة الإنجليزية، بينما القيم (Values) باللغة العربية:
   - 'title': عنوان المهمة (جذاب ومحفز للطفل).
   - 'description': وصف المهمة التي سيقوم بها الطفل.
   - 'targetAge': الفئة العمرية (اختر من: '5-7', '8-10', '11-12').
   - 'category': نوع المهمة (اختر من: 'مهارات مالية', 'مساعدة منزلية', 'تطوير ذاتي')..

مثال لشكل الإخراج المطلوب:
{
  ""quests"": [
    {
      ""title"": ""حارس الحصالة"",
      ""description"": ""وفر 10 ريالات من مصروفك هذا الأسبوع ولا تنفقها على الحلوى."",
      ""targetAge"": ""8-10"",
      ""category"": ""مهارات مالية""
    }
  ]
}"));

            history.AddUserMessage(message);

            // 2. إعدادات التنفيذ الجديدة الخاصة بالـ Hackathon
            var executionSettings = new OpenAIPromptExecutionSettings
            {
                ResponseFormat = "json_object", // 🚨 هذا السطر السحري يجبر الذكاء الاصطناعي على إرجاع JSON فقط
                Temperature = 0.7 // رفعنا درجة الإبداع لكي يبتكر أفكار ألعاب ومهام ممتعة وغير مكررة
            };

            try
            {
                // 3. التنفيذ المباشر (لا يوجد استدعاء أدوات)
                var response = await _chatCompletion.GetChatMessageContentAsync(history, executionSettings, _kernel);

                history.AddAssistantMessage(response.Content ?? string.Empty);

                string safeContent = response.Content ?? "";
                Console.WriteLine($"\n✅ إجابة الوكيل: {safeContent.Substring(0, Math.Min(safeContent.Length, 150))}...");

                return safeContent; // سيعود كنص JSON جاهز ليتم عمل Deserialize له في الـ Frontend
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in runAgent: {ex.Message}");
                throw;
            }
        }
    }
}