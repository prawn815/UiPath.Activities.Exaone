using System.Activities;
using System.Activities.DesignViewModels;
using UiPath.Activities.Exaone.Extensions;

namespace UiPath.Activities.Exaone.ViewModels
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

        // 🔹 계수값
        public DesignInArgument<double> Temperature { get; set; }

        // 🔹 컨텍스트 그라운딩 방식 선택
        public DesignProperty<ContextGroundingType> ContextGrounding { get; set; }

        // 🔹 Query 기반 조회를 위한 속성 : 검색 쿼리
        public DesignInArgument<string> SearchQuery { get; set; }

        // 🔹 Query 기반 조회를 위한 속성 : 개수
        public DesignInArgument<int> Top_K { get; set; }

        // 🔹 Query 기반 조회를 위한 속성 : 스코어
        public DesignProperty<bool> Score { get; set; }

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
            Endpoint.IsRequired = true;
            Endpoint.IsPrincipal = true;
            Endpoint.OrderIndex = orderIndex++;

            ApiKey.DisplayName = Resources.ApiKey_DisplayName;
            ApiKey.Tooltip = Resources.ApiKey_Tooltip;
            ApiKey.IsRequired = false;
            ApiKey.IsPrincipal = true;
            ApiKey.OrderIndex = orderIndex++;

            Model.DisplayName = Resources.Model_DisplayName;
            Model.Tooltip = Resources.Model_Tooltip;
            Model.IsRequired = true;
            Model.IsPrincipal = true;
            Model.OrderIndex = orderIndex++;

            UserPrompt.DisplayName = Resources.UserPrompt_DisplayName;
            UserPrompt.Tooltip = Resources.UserPrompt_Tooltip;
            UserPrompt.IsRequired = true;
            UserPrompt.IsPrincipal = true;
            UserPrompt.OrderIndex = orderIndex++;

            SystemPrompt.DisplayName = Resources.SystemPrompt_DisplayName;
            SystemPrompt.Tooltip = Resources.SystemPrompt_Tooltip;
            SystemPrompt.IsRequired = false;
            SystemPrompt.IsPrincipal = true;
            SystemPrompt.OrderIndex = orderIndex++;

            Temperature.DisplayName = Resources.Temperature_DisplayName;
            Temperature.Tooltip = Resources.Temperature_Tooltip;
            Temperature.IsRequired = false;
            Temperature.IsPrincipal = true;
            Temperature.OrderIndex = orderIndex++;

            ContextGrounding.DisplayName = Resources.ContextGrounding_DisplayName;
            ContextGrounding.Tooltip = Resources.ContextGrounding_Tooltip;
            ContextGrounding.IsRequired = true;
            ContextGrounding.IsPrincipal = true;
            ContextGrounding.OrderIndex = orderIndex++;

            // 컨텍스트 그라운딩 하위 속성 visible false
            SearchQuery.IsVisible = false;
            Top_K.IsVisible = false;
            Score.IsVisible = false;
            FilePath.IsVisible = false;
            RawTextInput.IsVisible = false;
            Url.IsVisible = false;

            // 컨텍스트 그라운딩 드롭박스 선택
            // visible 활성화 및 이외 값 초기화
            ContextGrounding.TrackValue(prop =>
            {
                var selected = prop.Value;

                SearchQuery.IsVisible = selected == ContextGroundingType.SearchQuery;
                Top_K.IsVisible = selected == ContextGroundingType.SearchQuery;
                Score.IsVisible = selected == ContextGroundingType.SearchQuery;
                FilePath.IsVisible = selected == ContextGroundingType.FileResource;
                RawTextInput.IsVisible = selected == ContextGroundingType.RawText;
                Url.IsVisible = selected == ContextGroundingType.WebPage;

                if (selected != ContextGroundingType.SearchQuery)
                    SearchQuery.Value = string.Empty;
                    Top_K.Value = 0;
                    Score.Value = true;

                if (selected != ContextGroundingType.FileResource)
                    FilePath.Value = string.Empty;

                if (selected != ContextGroundingType.RawText)
                    RawTextInput.Value = string.Empty;

                if (selected != ContextGroundingType.WebPage)
                    Url.Value = string.Empty;

            });

            SearchQuery.DisplayName = Resources.SearchQuery_DisplayName;
            SearchQuery.Tooltip = Resources.SearchQuery_Tooltip;
            SearchQuery.IsRequired = false;
            SearchQuery.IsPrincipal = true;
            SearchQuery.OrderIndex = orderIndex++;

            Top_K.DisplayName = Resources.Top_K_DisplayName;
            Top_K.Tooltip = Resources.Top_K_Tooltip;
            Top_K.IsRequired = false;
            Top_K.IsPrincipal = true;
            Top_K.OrderIndex = orderIndex++;

            Score.DisplayName = Resources.Score_DisplayName;
            Score.Tooltip = Resources.Score_Tooltip;
            Score.IsRequired = false;
            Score.IsPrincipal = true;   
            Score.OrderIndex = orderIndex++;

            FilePath.DisplayName = Resources.FilePath_DisplayName;
            FilePath.Tooltip = Resources.FilePath_Tooltip;
            FilePath.IsRequired = false;
            FilePath.IsPrincipal = true;
            FilePath.OrderIndex = orderIndex++;

            RawTextInput.DisplayName = Resources.RawTextInput_DisplayName;
            RawTextInput.Tooltip = Resources.RawTextInput_Tooltip;
            RawTextInput.IsRequired = false;
            RawTextInput.IsPrincipal = true;
            RawTextInput.OrderIndex = orderIndex++;

            Url.DisplayName = Resources.Url_DisplayName;
            Url.Tooltip = Resources.Url_Tooltip;
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
            Result.OrderIndex = orderIndex;

        }
    }
}
