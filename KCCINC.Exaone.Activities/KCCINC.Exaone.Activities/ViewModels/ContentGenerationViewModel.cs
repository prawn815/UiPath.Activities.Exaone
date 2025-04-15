using KCCINC.Exaone.Activities;
using KCCINC.Exaone.Activities.Helpers;
using System.Activities;
using System.Activities.DesignViewModels;
using System.Collections;

namespace KCCINC.Exaone.Activities.ViewModels
{
    public class ContentGenerationViewModel : DesignPropertiesViewModel
    {
        // 🔹 Exaone API 엔드포인트 [필수값]
        public DesignInArgument<string> Endpoint { get; set; }

        // 🔹 API 키 (선택)
        public DesignInArgument<string> ApiKey { get; set; }

        // 🔹 모델 직접 입력 [필수값]
        public DesignInArgument<string> Model { get; set; }

        // 🔹 유저 프롬프트 [필수값]
        public DesignInArgument<string> UserPrompt { get; set; }

        // 🔹 시스템 프롬프트
        public DesignInArgument<string> SystemPrompt { get; set; }

        // 🔹 온도
        public DesignInArgument<double> Temperature { get; set; }

        // 🔹 컨텍스트 그라운딩 방식 선택
        public DesignProperty<ContextGroundingType> ContextGrounding { get; set; }

        // 🔹 Collection 입력값
        public DesignInArgument<string> CollectionName { get; set; }

        // 🔹 Query 기반 조회를 위한 속성 : 검색 쿼리
        public DesignInArgument<string> SearchQuery { get; set; }

        // 🔹 Query 기반 조회를 위한 속성 : 개수
        public DesignInArgument<int> Top_K { get; set; }

        // 🔹 Query 기반 조회를 위한 속성 : 스코어
        public DesignProperty<bool> Score { get; set; }

        // 🔹 컨텍스트 그라운딩 자료 인용 시 최소 스코어
        public DesignInArgument<double> MinimumScore {  get; set; }

        // 🔹 파일 기반 조회를 위한 속성
        public DesignInArgument<string> FilePath { get; set; }

        // 🔹 텍스트 기반 조회를 위한 속성
        public DesignInArgument<string> RawTextInput { get; set; }

        // 🔹 웹페이지 주소값 속성
        public DesignInArgument<string> Url { get; set; }

        // 🔹 컨텍스트 실패 시 무시 여부
        public DesignProperty<bool> FailOnGroundingError { get; set; }

        // 🔹 결과값 (가장 많이 생성된 텍스트)
        public DesignOutArgument<string> MainText { get; set; }
        // 🔹 결과값 (ExaoneResponse 타입 전체 결과)
        public DesignOutArgument<string> Result { get; set; }
        // 🔹 결과값 ( 컨텍스트 그라운딩 인용절 JSON 문자열)
        public DesignOutArgument<string> CitationText { get; set; }

        public ContentGenerationViewModel(IDesignServices services) : base(services)
        {
        }

        protected override void InitializeModel()
        {
            /*
             * The base call will initialize the properties of the view model with the values from the xaml or with the default values from the activity
             */
            base.InitializeModel();

            PersistValuesChangedDuringInit(); // mandatory call only when you change the values of properties during initialization

            var orderIndex = 0;

            Endpoint.DisplayName = Resources.Endpoint_DisplayName;
            Endpoint.Tooltip = Resources.Endpoint_Tooltip;
            Endpoint.Placeholder = Resources.Endpoint_Placeholder;
            Endpoint.IsRequired = true;
            Endpoint.IsPrincipal = true;
            Endpoint.OrderIndex = orderIndex++;

            ApiKey.DisplayName = Resources.ApiKey_DisplayName;
            ApiKey.Tooltip = Resources.ApiKey_Tooltip;
            ApiKey.Placeholder = Resources.ApiKey_Placeholder;
            ApiKey.IsRequired = false;
            ApiKey.IsPrincipal = true;
            ApiKey.OrderIndex = orderIndex++;

            Model.DisplayName = Resources.Model_DisplayName;
            Model.Tooltip = Resources.Model_Tooltip;
            Model.Placeholder = Resources.Model_Placeholder;
            Model.IsRequired = true;
            Model.IsPrincipal = true;
            Model.OrderIndex = orderIndex++;

            UserPrompt.DisplayName = Resources.UserPrompt_DisplayName;
            UserPrompt.Tooltip = Resources.UserPrompt_Tooltip;
            UserPrompt.Placeholder = Resources.UserPrompt_Placeholder;
            UserPrompt.IsRequired = true;
            UserPrompt.IsPrincipal = true;
            UserPrompt.OrderIndex = orderIndex++;

            SystemPrompt.DisplayName = Resources.SystemPrompt_DisplayName;
            SystemPrompt.Tooltip = Resources.SystemPrompt_Tooltip;
            SystemPrompt.Placeholder = Resources.SystemPrompt_Placeholder;
            SystemPrompt.IsRequired = false;
            SystemPrompt.IsPrincipal = true;
            SystemPrompt.OrderIndex = orderIndex++;

            Temperature.DisplayName = Resources.Temperature_DisplayName;
            Temperature.Tooltip = Resources.Temperature_Tooltip;
            Temperature.Placeholder = Resources.Temperature_Placeholder;
            Temperature.IsRequired = false;
            Temperature.IsPrincipal = true;
            Temperature.OrderIndex = orderIndex++;

            ContextGrounding.DisplayName = Resources.ContextGrounding_DisplayName;
            ContextGrounding.Tooltip = Resources.ContextGrounding_Tooltip;
            ContextGrounding.IsRequired = true;
            ContextGrounding.IsPrincipal = true;
            ContextGrounding.OrderIndex = orderIndex++;

            // 컨텍스트 그라운딩 하위 속성 visible false
            CollectionName.IsVisible = false;
            SearchQuery.IsVisible = false;
            Top_K.IsVisible = false;
            Score.IsVisible = false;
            MinimumScore.IsVisible = false;
            FilePath.IsVisible = false;
            RawTextInput.IsVisible = false;
            Url.IsVisible = false;

            // 컨텍스트 그라운딩 드롭박스 선택
            // visible 활성화 및 이외 값 초기화
            ContextGrounding.TrackValue(prop =>
            {
                var selected = prop.Value;

                // CollectionName, Top_K, Score는 SearchQuery, FileResource, Text, WebPage 사용
                bool showOptions = selected == ContextGroundingType.SearchQuery ||
                                         selected == ContextGroundingType.FileResource ||
                                         selected == ContextGroundingType.Text ||
                                         selected == ContextGroundingType.WebPage;

                CollectionName.IsVisible = showOptions;
                Top_K.IsVisible = showOptions;
                Score.IsVisible = showOptions;

                SearchQuery.IsVisible = selected == ContextGroundingType.SearchQuery;
                FilePath.IsVisible = selected == ContextGroundingType.FileResource;
                RawTextInput.IsVisible = selected == ContextGroundingType.Text;
                Url.IsVisible = selected == ContextGroundingType.WebPage;

                // 드롭박스 변경 시 기존 입력 값 초기화
                if (selected != ContextGroundingType.SearchQuery)
                    SearchQuery.Value = string.Empty;

                if (selected != ContextGroundingType.FileResource)
                    FilePath.Value = string.Empty;

                if (selected != ContextGroundingType.Text)
                    RawTextInput.Value = string.Empty;

                if (selected != ContextGroundingType.WebPage)
                    Url.Value = string.Empty;

                // 컨텍스트 그라운딩 옵션을 변경하면 기존 값 초기화
                if (!showOptions)
                {
                    CollectionName.Value = string.Empty;
                    Top_K.Value = 1;
                    Score.Value = true;
                    MinimumScore.Value = 1.0;
                }

                // Score 체크 여부에 따라 MinimumScore 표시 여부 갱신
                Score.TrackValue(prop =>
                {
                    // context grounding이 4가지 중 하나일 때만 반응
                    bool showMinimumScore = ContextGrounding.Value == ContextGroundingType.SearchQuery ||
                                                ContextGrounding.Value == ContextGroundingType.FileResource ||
                                                ContextGrounding.Value == ContextGroundingType.Text ||
                                                ContextGrounding.Value == ContextGroundingType.WebPage;

                    MinimumScore.IsVisible = prop.Value && showMinimumScore;

                    // 체크 해제되면 값 초기화
                    if (!prop.Value)
                        MinimumScore.Value = 1.0;
                });
            });

            CollectionName.DisplayName = Resources.CollectionName_DisplayName;
            CollectionName.Tooltip = Resources.CollectionName_Tooltip;
            CollectionName.Placeholder = Resources.CollectionName_Placeholder;
            CollectionName.IsRequired = false;
            CollectionName.IsPrincipal = true;
            CollectionName.OrderIndex = orderIndex++;

            SearchQuery.DisplayName = Resources.SearchQuery_DisplayName;
            SearchQuery.Tooltip = Resources.SearchQuery_Tooltip;
            SearchQuery.Placeholder = Resources.SearchQuery_Placeholder;
            SearchQuery.IsRequired = false;
            SearchQuery.IsPrincipal = true;
            SearchQuery.OrderIndex = orderIndex++;

            Top_K.DisplayName = Resources.Top_K_DisplayName;
            Top_K.Tooltip = Resources.Top_K_Tooltip;
            Top_K.Placeholder = Resources.Top_K_Placeholder;
            Top_K.IsRequired = false;
            Top_K.IsPrincipal = true;
            Top_K.OrderIndex = orderIndex++;

            Score.DisplayName = Resources.Score_DisplayName;
            Score.Tooltip = Resources.Score_Tooltip;
            Score.IsRequired = false;
            Score.IsPrincipal = true;   
            Score.OrderIndex = orderIndex++;

            MinimumScore.DisplayName = Resources.MinimumScore_DisplayName;
            MinimumScore.Tooltip = Resources.MinimumScore_Tooltip;
            MinimumScore.Placeholder = Resources.MinimumScore_Placeholder;
            MinimumScore.IsRequired = false;
            MinimumScore.IsPrincipal = true;
            MinimumScore.OrderIndex = orderIndex++;

            FilePath.DisplayName = Resources.FilePath_DisplayName;
            FilePath.Tooltip = Resources.FilePath_Tooltip;
            FilePath.Placeholder = Resources.FilePath_Placeholder;
            FilePath.IsRequired = false;
            FilePath.IsPrincipal = true;
            FilePath.OrderIndex = orderIndex++;

            RawTextInput.DisplayName = Resources.RawTextInput_DisplayName;
            RawTextInput.Tooltip = Resources.RawTextInput_Tooltip;
            RawTextInput.Placeholder = Resources.RawTextInput_Placeholder;
            RawTextInput.IsRequired = false;
            RawTextInput.IsPrincipal = true;
            RawTextInput.OrderIndex = orderIndex++;

            Url.DisplayName = Resources.Url_DisplayName;
            Url.Tooltip = Resources.Url_Tooltip;
            Url.Placeholder = Resources.Url_Placeholder;
            Url.IsRequired = false;
            Url.IsPrincipal = true;
            Url.OrderIndex = orderIndex++;

            FailOnGroundingError.DisplayName = Resources.FailOnGroundingError_DisplayName;
            FailOnGroundingError.Tooltip = Resources.FailOnGroundingError_Tooltip;
            FailOnGroundingError.OrderIndex = orderIndex++;
            FailOnGroundingError.Value = true;

            MainText.DisplayName = Resources.MainText_DisplayName;
            MainText.Tooltip = Resources.MainText_Tooltip;
            MainText.OrderIndex = orderIndex++;

            Result.DisplayName = Resources.Result_DisplayName;
            Result.Tooltip = Resources.Result_Tooltip;
            Result.OrderIndex = orderIndex++;

            CitationText.DisplayName = Resources.CitationText_DisplayName;
            CitationText.Tooltip = Resources.CitationText_Tooltip;
            CitationText.OrderIndex = orderIndex;

        }
    }
}
