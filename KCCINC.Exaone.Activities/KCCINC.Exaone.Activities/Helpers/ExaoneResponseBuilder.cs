using System;
using System.Collections.Generic;
using KCCINC.Exaone.Activities.Models;

namespace KCCINC.Exaone.Activities.Helpers
{
    // 🔹 누적된 텍스트를 ExaoneResponse 객체로 포장
    public static class ExaoneResponseBuilder
    {
        public static ExaoneResponse FromStreamedContent(string content, string model)
        {
            return new ExaoneResponse
            {
                Id = Guid.NewGuid().ToString(),
                Object = "chat.completion",
                Created = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Model = model,
                Choices = new List<Choice>
                {
                    new Choice
                    {
                        Index = 0,
                        Message = new Message
                        {
                            Role = "assistant",
                            Content = content,
                            ReasoningContent = null,
                            ToolCalls = null
                        }
                    }
                },
                Usage = null
            };
        }
    }
}
