using System.Activities;
using UiPath.Robot.Activities.Api;

namespace UiPath.Activities.Exaone.Helpers
{
    public static class ActivityContextExtensions
    {
        public static IExecutorRuntime GetExecutorRuntime(this ActivityContext context) => context.GetExtension<IExecutorRuntime>();
    }
}
