using System;
using System.Activities.DesignViewModels;

namespace UiPath.Activities.Exaone.Extensions
{
    public static class TrackValueExtension
    {
        public static void TrackValue<T>(this DesignProperty<T> prop, Action<DesignProperty<T>> onChange)
        {
            prop.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(prop.Value))
                {
                    onChange(prop);
                }
            };

            // 초기 값에도 반응
            onChange(prop);
        }
    }
}
