using System.Collections.Generic;

namespace KCCINC.Exaone.Activities.Models
{
    // 🔹 Streaming 응답 조각(chunk) 구조 정의

    public class ExaoneStreamChunk
    {
        public string id { get; set; }
        public string @object { get; set; }
        public List<Choice> choices { get; set; }

        public class Choice
        {
            public Delta delta { get; set; }

            public class Delta
            {
                public string content { get; set; }
            }
        }
    }
}
