using System.Collections.Generic;

namespace KCCINC.Exaone.Activities.Models
{
    // 🔹 Exaone API 응답 모델
    public class ExaoneResponse
    {
        public string Id { get; set; }                  // 응답 ID
        public string Object { get; set; }              // 객체 타입
        public int Created { get; set; }                // 생성 시각 (Unix Timestamp)
        public string Model { get; set; }               // 사용된 모델
        public List<Choice> Choices { get; set; }       // 생성된 응답 목록
        public Usage Usage { get; set; }                // 토큰 사용량 정보
    }

    // 🔹 생성된 응답 선택지
    public class Choice
    {
        public int Index { get; set; }                  // 선택지 인덱스
        public Message Message { get; set; }            // 응답 메시지
    }

    // 🔹 응답 메시지
    public class Message
    {
        public string Role { get; set; }                // 역할 (assistant 등)
        public string Content { get; set; }             // 실제 응답 텍스트
        public string ReasoningContent { get; set; }    // 추론 내용 (nullable)
        public List<object> ToolCalls { get; set; }     // 툴 호출 정보 (미사용)
    }

    // 🔹 토큰 사용 정보
    public class Usage
    {
        public int PromptTokens { get; set; }           // 입력 토큰 수
        public int CompletionTokens { get; set; }       // 생성 토큰 수
        public int TotalTokens { get; set; }            // 전체 토큰 수
    }
}
    