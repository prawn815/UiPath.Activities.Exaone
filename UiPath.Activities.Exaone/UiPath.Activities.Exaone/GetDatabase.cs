using System;
using System.Activities;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace UiPath.Activities.Exaone
{
    public class GetDatabase : CodeActivity<string> // result: 전체 json 문자열
    {
        // 🔹 Collection 입력값
        public InArgument<string> CollectionName { get; set; }

        // 🔹 상태 코드 (200, 400 등)
        public OutArgument<int> StatusCode { get; set; }

        // 🔹 성공 메시지 요약
        public OutArgument<string> Message { get; set; }

        protected override string Execute(CodeActivityContext context)
        {
            var resultJson = Task.Run(() => GetDBAsync(context)).Result;
            return resultJson;
        }

        private async Task<string> GetDBAsync(CodeActivityContext context)
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
                HttpResponseMessage response = await client.GetAsync(requestUrl);
                string content = await response.Content.ReadAsStringAsync();

                context.SetValue(StatusCode, (int)response.StatusCode);

                if (response.IsSuccessStatusCode)
                    context.SetValue(Message, "✔ 전체 DB 조회 성공");
                else
                    context.SetValue(Message, $"❌ 전체 DB 조회 실패: {response.StatusCode}");

                return content;
            }
        }
    }
}
