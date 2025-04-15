using Newtonsoft.Json.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace KCCINC.Exaone.Activities.Helpers
{
    internal static class PromptFormatter
    {
        // 벡터 JSON에서 context 정보를 추출해 LLM용 프롬프트로 정제

        public static string CleanContextForPrompt(string vectorJson)
        {
            try
            {
                var obj = JObject.Parse(vectorJson);
                var contextToken = obj["context"];

                // case 1: context가 string인 경우 (SearchQuery 등)
                if (contextToken.Type == JTokenType.String)
                {
                    var content = contextToken.ToString();
                    return string.IsNullOrWhiteSpace(content) ? "" : "● " + FormatText(content);
                }

                // case 2: context가 배열인 경우 (FileResource 등)
                if (contextToken is JArray contextArray && contextArray.Count > 0)
                {
                    var sb = new StringBuilder();

                    foreach (var item in contextArray)
                    {
                        var content = item["page_content"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            sb.AppendLine("● " + FormatText(content));
                            sb.AppendLine();
                        }
                    }

                    return sb.ToString();
                }

                return "";
            }
            catch
            {
                return vectorJson; // fallback
            }
        }


        /// 개행 문자, 탭, 중복 공백 등을 정리한 단일 라인 텍스트로 변환합니다.
        private static string FormatText(string content)
        {
            return Regex.Replace(
                content
                    .Replace("\n", " ")
                    .Replace("\r", " ")
                    .Replace("\t", " ")
                    .Replace("\"", "'"),
                @"\s{2,}", " "
            ).Trim();
        }
    }
}
