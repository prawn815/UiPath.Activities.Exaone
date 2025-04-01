using System;
using System.Activities;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UiPath.Activities.Exaone
{
    public class ClearDatabase : CodeActivity<string> // result: 전체 json 문자열
    {
        // 🔹 상태 코드 (200, 400 등)
        public OutArgument<int> StatusCode { get; set; }

        // 🔹 메시지 요약
        public OutArgument<string> Message { get; set; }

        protected override string Execute(CodeActivityContext context)
        {
            var resultJson = Task.Run(() => ClearDBAsync(context)).Result;
            return resultJson;
        }

        private async Task<string> ClearDBAsync(CodeActivityContext context)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.DeleteAsync("http://exaone.myrobots.co.kr/db/");
                string content = await response.Content.ReadAsStringAsync();

                context.SetValue(StatusCode, (int)response.StatusCode);

                if (response.IsSuccessStatusCode)
                    context.SetValue(Message, "✔ DB 초기화 성공");
                else
                    context.SetValue(Message, $"❌ DB 초기화 실패: {response.StatusCode}");

                return content; // 전체 json 문자열 리턴
            }
        }
    }
}
