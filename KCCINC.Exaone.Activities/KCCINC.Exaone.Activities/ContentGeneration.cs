using System.Activities;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UiPath.Platform.ResourceHandling;
using KCCINC.Exaone.Activities.Helpers;
using KCCINC.Exaone.Activities.Models;

namespace KCCINC.Exaone.Activities
{
    public class ContentGeneration : CodeActivity<string> // This base class exposes an OutArgument named Result
    {
        // 🔹 Exaone API 엔드포인트
        [RequiredArgument]
        public InArgument<string> Endpoint { get; set; }

        // 🔹 API 키 (선택)
        public InArgument<string> ApiKey { get; set; }

        // 🔹 유저 프롬프트
        [RequiredArgument]
        public InArgument<string> UserPrompt { get; set; }

        // 🔹 시스템 프롬프트
        public InArgument<string> SystemPrompt { get; set; } = new InArgument<string>("You are a helpful assistant");

        // 🔹 모델을 직접 입력
        [RequiredArgument]
        public InArgument<string> Model { get; set; }

        // 🔹 온도
        public InArgument<double> Temperature { get; set; } = 0.7;

        // 🔹 컨텍스트 그라운딩 방식 선택
        public ContextGroundingType ContextGrounding { get; set; } = ContextGroundingType.None;

        // 🔹 Collection 입력값 (선택, 기본값 : default)
        public InArgument<string> CollectionName { get; set; }

        // 🔹 Query 기반 조회를 위한 속성 : 검색 쿼리
        public InArgument<string> SearchQuery { get; set; } = "";

        // 🔹 Query 기반 조회를 위한 속성 : 개수
        public InArgument<int> Top_K { get; set; } = 1;

        // 🔹 Query 기반 조회를 위한 속성 : 스코어
        public bool Score { get; set; } = true;

        // 🔹 컨텍스트 그라운딩 자료 인용 시 최소 스코어
        public InArgument<double> MinimumScore { get; set; } = 1.0;

        // 🔹 파일 기반 조회를 위한 속성
        public InArgument<string> FilePath { get; set; } = "";

        // 🔹 텍스트 기반 조회를 위한 속성
        public InArgument<string> RawTextInput { get; set; }

        // 🔹 웹페이지 주소값 속성
        public InArgument<string> Url { get; set; }

        // 🔹 컨텍스트 그라운딩 에러 시 무시 유무
        public bool FailOnGroundingError { get; set; } = true;

        // 🔹 Out : 가장 많이 생성된 텍스트
        public OutArgument<string> MainText { get; set; }

        // 🔹 Out : 인용문자열 (컨텍스트 그라운딩 인용에 사용된 문구)
        public OutArgument<string> CitationText { get; set; }


        protected override string Execute(CodeActivityContext context)
        {
            string endpoint = Endpoint.Get(context);
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Endpoint는 필수입니다.");
            string apiKey = ApiKey.Get(context);
            string userPrompt = UserPrompt.Get(context) ?? "";
            string systemPrompt = SystemPrompt.Get(context) ?? "";
            string model = Model.Get(context);
            if (string.IsNullOrWhiteSpace(model))
                throw new ArgumentException("모델 이름은 필수입니다.");
            double temperature = Temperature.Get(context);
            ContextGroundingType groundingType = ContextGrounding;
            string collection = CollectionName.Get(context)?.Trim();
            if (string.IsNullOrWhiteSpace(collection))
                collection = "default";
            string searchQuery = SearchQuery.Get(context);
            int top_k = Math.Min(5, Math.Max(1, Top_K.Get(context)));   // 최소 1 , 최대 5개까지만 제한
            bool score = Score;
            double minimumScore = Math.Clamp(MinimumScore.Get(context), 0.0, 1.0);  // 최소 유사도 점수 0 ~ 1 제한
            string filePath = FilePath.Get(context);
            string rawTextInput = RawTextInput.Get(context) ?? "";
            string url = Url.Get(context)?.Trim() ?? "";
            bool failOnGroundingError = FailOnGroundingError;
            string citationJson = "";

            ExaoneResponse response = GenerateResponse(
                endpoint,
                apiKey,
                userPrompt,
                systemPrompt,
                model,
                temperature,
                groundingType,
                collection,
                searchQuery,
                top_k,
                score,
                minimumScore,
                filePath,
                rawTextInput,
                url,
                failOnGroundingError,
                out citationJson
                );

            // 결과값 설정
            MainText.Set(context, response?.Choices?[0]?.Message?.Content ?? "");
            CitationText.Set(context, citationJson);

            // 전체 응답 json 문자열 반환
            return JsonConvert.SerializeObject(response);
        }

        private ExaoneResponse GenerateResponse(
            string endpoint,
            string apiKey,
            string userPrompt,
            string systemPrompt,
            string model,
            double temperature,
            ContextGroundingType groundingType,
            string collection,
            string searchQuery,
            int top_k,
            bool score,
            double minimumScore,
            string filePath,
            string rawTextInput,
            string url,
            bool failOnGroundingError,
            out string citationText
            )
        {

            string vectorData = "";
            citationText = "";

            // 🔹 컨텍스트 그라운딩 수행

            try
            {
                switch (groundingType)
                {
                    case ContextGroundingType.SearchQuery:
                        vectorData = Task.Run(() => QueryChromaDB(searchQuery, top_k, score, minimumScore, collection)).Result;
                        citationText = vectorData;
                        break;

                    case ContextGroundingType.FileResource:
                        {
                            // 파일 업로드
                            string uploadResult = Task.Run(() => UploadFileToChromaDB(filePath, collection)).Result;
                            Console.WriteLine($"✔ 파일 텍스트 업로드 성공: {uploadResult}");

                            // 파일명 기반 쿼리 수행
                            string fileName = Path.GetFileName(filePath);   // 정확성을 위해 확장자 포함
                            vectorData = Task.Run(() => QueryChromaDB(fileName, top_k, score, minimumScore, collection)).Result;
                            citationText = vectorData;
                            break;
                        }

                    case ContextGroundingType.Text:
                        {
                            string rawText = rawTextInput;
                            string uploadResult = Task.Run(() => UploadRawTextToChromaDB(rawText, collection)).Result;
                            Console.WriteLine($"✔ 텍스트 업로드 성공: {uploadResult}");

                            // 입력 텍스트 앞부분으로 쿼리 수행 (텍스트 100자로 절삭)
                            string queryPreview = rawText.Length > 100 ? rawText.Substring(0, 100) : rawText;
                            vectorData = Task.Run(() => QueryChromaDB(queryPreview, top_k, score, minimumScore, collection)).Result;
                            citationText = vectorData;
                            break;
                        }

                    case ContextGroundingType.WebPage:
                        {
                            string uploadResult = Task.Run(() => LoadWebPage(url, collection)).Result;
                            Console.WriteLine($"✔ 웹페이지 업로드 성공: {uploadResult}");

                            // 🔹 전체 URL로 검색 수행 (정확 매칭을 위해)
                            vectorData = Task.Run(() => QueryChromaDB(url, top_k, score, minimumScore, collection)).Result;
                            citationText = vectorData;
                            break;
                        }

                    case ContextGroundingType.None:
                    default:
                        vectorData = ""; // 아무 컨텍스트도 사용 안 함
                        break;
                }
            }
            catch (Exception ex)
            {
                if (failOnGroundingError)
                {
                    vectorData = $"** Context grounding failed: {ex.Message}";
                    citationText = vectorData;
                }
                else
                {
                    throw;
                }
            }


            // 🔹 Exaone API 호출 및 결과 반환
            ExaoneResponse apiResponse = Task.Run(() => CallExaoneAPI(endpoint, apiKey, userPrompt, vectorData, systemPrompt, model, temperature)).Result;

            return apiResponse;
        }


        // 🔹 Exaone API 호출 메서드
        private async Task<ExaoneResponse> CallExaoneAPI(
            string endpoint,
            string apiKey,
            string userPrompt,
            string vectorData,
            string systemPrompt,
            string model,
            double temperature)
        {
            using (HttpClient client = new HttpClient())
            {
                // 🔹 Authorization 헤더 설정
                if (!string.IsNullOrWhiteSpace(apiKey))
                {
                    // ApiKey가 있으면 Bearer 토큰 형식으로 Authorization 헤더 추가
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                }
                // ApiKey가 없으면 Authorization 헤더를 생략 (추가하지 않음)

                // 요청 데이터 구성
                // 🔹 유저 프롬프트 + context 조합
                string combinedPrompt;

                if (!string.IsNullOrWhiteSpace(vectorData))
                {
                    string cleanVectorData = PromptFormatter.CleanContextForPrompt(vectorData);

                    combinedPrompt = $@"
                    {userPrompt}


                    위 질문에 대해 아래 CONTEXT를 참고해서 답변해줘. CONTEXT 외에 알 수 없는 내용은 모른다고 답변해줘.

                    ## CONTEXT ##
                    {cleanVectorData}
                    ";
                }
                else
                {
                    combinedPrompt = userPrompt;  // 컨텍스트가 없을 땐 프롬프트만 사용
                }

                var requestData = new
                {
                    model,
                    messages = new[]
                    {
                         new { role = "system", content = systemPrompt },  // 시스템 스타일
                          new { role = "user", content = combinedPrompt }   // 유저 입력 + 컨텍스트
                     },
                    temperature
                };

                string jsonData = JsonConvert.SerializeObject(requestData);
                HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                //PrintCurlCommand(endpoint, apiKey, jsonData); //*
                // API 호출
                HttpResponseMessage response = await client.PostAsync(endpoint, content);
                string result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"CallExaoneAPI failed: {response.StatusCode} - {result}");

                return JsonConvert.DeserializeObject<ExaoneResponse>(result);


            }
        }

        // 🔹 ChromaDB에서 검색 쿼리 기반 컨텍스트 검색
        private async Task<string> QueryChromaDB(string searchquery, int top_k, bool score, double minimumScore, string collection)
        {
            using (HttpClient client = new HttpClient())
            {
                string requestUrl = $"http://exaone.myrobots.co.kr/db/query?collection={Uri.EscapeDataString(collection)}";

                var requestData = new { query = searchquery, top_k, score };
                string jsonData = JsonConvert.SerializeObject(requestData);
                HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(requestUrl, content);
                string result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"QueryChromaDB failed: {response.StatusCode} - {result}");

                // 🔹 결과 파싱 후 필터링
                // 🔹 점수 포함 옵션 & 최소 점수 조건이 있을 때만 필터링
                if (score && minimumScore > 0)
                {
                    JObject responseObject = JObject.Parse(result);
                    JArray contextArray = (JArray)responseObject["context"];

                    contextArray = new JArray(
                        contextArray
                            .Where(item =>
                                item["score"] != null &&
                                double.TryParse(item["score"].ToString(), out double s) &&
                                s <= minimumScore
                            )
                    );

                    responseObject["context"] = contextArray;
                    result = responseObject.ToString(Formatting.None);  // result 값 갱신
                }

                return result;
            }
        }

        // 🔹 ChromaDB에 파일 업로드 (File 기반 컨텍스트 - txt 파일만)
        private async Task<string> UploadFileToChromaDB(string filePath, string collection)
        {
            // 파일 경로 빈값 예외처리
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("파일 경로가 비어 있습니다.");

            string fileName = Path.GetFileName(filePath);
            string fileText = "";

            // 환경 판별
            bool isWeb =
                Environment.GetEnvironmentVariable("UIPATH_SERVERLESS_CONTEXT") == "true" ||
                Environment.GetEnvironmentVariable("UIPATH_ENVIRONMENT_TAG") == "Serverless";

            if (isWeb)
            {
                // Studio Web, Storage Bucket에서 받은 파일 불러오기
                var resource = LocalResource.FromPath(filePath);
                fileText = await File.ReadAllTextAsync(resource.FullName, Encoding.UTF8);
            }
            else
            {
                // Studio (Desktop) 로컬 경로 직접 접근
                if (!File.Exists(filePath))
                    throw new FileNotFoundException($"파일을 찾을 수 없습니다: {filePath}");

                fileText = await File.ReadAllTextAsync(filePath, Encoding.UTF8);
            }

            using (HttpClient client = new HttpClient())
            {

                string requestUrl = $"http://exaone.myrobots.co.kr/db/text?collection={Uri.EscapeDataString(collection)}";

                var requestData = new
                {
                    title = fileName,
                    text = fileText
                };

                string jsonData = JsonConvert.SerializeObject(requestData);
                HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(requestUrl, content);
                string result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"UploadFileToChromaDB failed: {response.StatusCode} - {result}");

                return result;
            }
        }

        // 🔹 사용자가 입력한 텍스트를 ChromaDB에 업로드 (Text 기반 컨텍스트)
        private async Task<string> UploadRawTextToChromaDB(string rawText, string collection)
        {
            if (string.IsNullOrWhiteSpace(rawText))
                throw new ArgumentException("RawText input is empty.");

            using (HttpClient client = new HttpClient())
            {
                string requestUrl = $"http://exaone.myrobots.co.kr/db/text?collection={Uri.EscapeDataString(collection)}";

                var requestData = new
                {
                    title = "RawText Input", // 내부적으로 고정 (타이틀 반영 여부 확인 필요)
                    text = rawText
                };

                string jsonData = JsonConvert.SerializeObject(requestData);
                HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(requestUrl, content);
                string result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"UploadRawTextToChromaDB failed: {response.StatusCode} - {result}");

                return result;
            }
        }

        // 🔹 웹페이지 URL을 DB에 로드 (POST /webpage)
        private async Task<string> LoadWebPage(string url, string collection)
        {
            using (HttpClient client = new HttpClient())
            {
                string requestUrl = $"http://exaone.myrobots.co.kr/db/webpage?collection={Uri.EscapeDataString(collection)}";

                var requestData = new { url };
                string jsonData = JsonConvert.SerializeObject(requestData);
                HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(requestUrl, content);
                string result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception($"LoadWebPage failed: {response.StatusCode} - {result}");

                return result;
            }
        }

        
        /*
        //*
        private void PrintCurlCommand(string endpoint, string apiKey, string jsonData)
        {
            Console.WriteLine("📡 재현용 curl:");
            Console.WriteLine($"curl -X POST \"{endpoint}\" \\");
            Console.WriteLine("  -H \"Content-Type: application/json\" \\");
            if (!string.IsNullOrWhiteSpace(apiKey))
                Console.WriteLine($"  -H \"Authorization: Bearer {apiKey}\" \\");
            Console.WriteLine($"  -d '{jsonData.Replace("'", "\\'")}'");
        }*/

    }
    public enum ContextGroundingType
    {
        None,             // 컨텍스트 없이 실행
        SearchQuery,    // 검색쿼리(검색어)를 통한 검색
        FileResource,     // 로컬 파일 업로드 (경로)
        Text,          // 직접 텍스트 입력
        WebPage          // 웹 페이지 URL로부터 컨텍스트
    }

}