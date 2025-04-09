using System.Activities;
using System.Activities.DesignViewModels;   

namespace UiPath.Activities.Exaone.ViewModels
{
    public class GetDatabaseViewModel : DesignPropertiesViewModel
    {
        // 🔹 Collection 입력값
        public DesignInArgument<string> CollectionName { get; set; }

        // 🔹 API StatusCode
        public DesignOutArgument<int> StatusCode { get; set; }

        // 🔹 성공 여부 Message 
        public DesignOutArgument<string> Message { get; set; }

        // 🔹 전체 결과 json 문자열
        public DesignOutArgument<string> Result { get; set; }

        public GetDatabaseViewModel(IDesignServices services) : base(services)
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

            CollectionName.DisplayName = Resources.CollectionName_DisplayName;
            CollectionName.Tooltip = Resources.CollectionName_Tooltip;
            CollectionName.Placeholder = Resources.CollectionName_Placeholder;
            CollectionName.IsRequired = false;
            CollectionName.IsPrincipal = true;
            CollectionName.OrderIndex = orderIndex++;

            StatusCode.DisplayName = Resources.StatusCode_DisplayName;
            StatusCode.Tooltip = Resources.StatusCode_Tooltip;
            StatusCode.OrderIndex = orderIndex++;

            Message.DisplayName = Resources.Message_DisplayName;
            Message.Tooltip = Resources.Message_Tooltip;
            Message.OrderIndex = orderIndex++;

            Result.DisplayName = Resources.GetDBResult_DisplayName;
            Result.Tooltip = Resources.GetDBResult_Tooltip;
            Result.OrderIndex = orderIndex;


        }
    }
}
