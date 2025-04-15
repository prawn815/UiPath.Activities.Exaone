using System;
using System.Activities;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace KCCINC.Exaone.Activities
{
    public class ClearDatabase : CodeActivity<string> // result: 전체 json 문자열
    {
        // 🔹 Collection 입력값
        public InArgument<string> CollectionName { get; set; }

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
            string collection = CollectionName.Get(context);
            // 사용자가 주입한 값이 없는 경우 : default
            if (string.IsNullOrWhiteSpace(collection))
            {
                collection = "default";
            }

            string requestUrl = $"http://exaone.myrobots.co.kr/db/?collection={Uri.EscapeDataString(collection)}";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = await client.DeleteAsync(requestUrl);
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
