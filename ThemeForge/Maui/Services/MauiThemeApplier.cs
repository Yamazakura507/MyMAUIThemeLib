using ThemeForge.Abstractions.Interfaces;
using ThemeForge.Abstractions.Records.UseOfTheme;
using ThemeForge.Maui.Styles;

namespace ThemeForge.Maui.Services
{
    /// <summary>
    /// Применяет тему к ресурсам подключенного MAUI-приложения.
    /// </summary>
    public sealed class MauiThemeApplier : IThemeApplier
    {
        private readonly ThemeResourceBuilder builder;
        private ResourceDictionary? runtimeDictionary;

        /// <summary>
        /// Создает применитель темы.
        /// </summary>
        /// <param name="builder">Строитель ресурсов темы.</param>
        public MauiThemeApplier(ThemeResourceBuilder builder)
        {
            this.builder = builder;
        }

        /// <inheritdoc />
        public void Apply(ThemeDefinition theme)
        {
            ArgumentNullException.ThrowIfNull(theme);

            MainThread.BeginInvokeOnMainThread(() => ApplyCore(theme));
        }

        /// <inheritdoc />
        public void Reset() => MainThread.BeginInvokeOnMainThread(ResetCore);

        private void ApplyCore(ThemeDefinition theme)
        {
            Application? application = Application.Current;

            if (application is null) return;

            EnsureDefaults(application);

            ResourceDictionary runtime = builder.Build(theme);

            if (runtimeDictionary is not null) application.Resources.MergedDictionaries.Remove(runtimeDictionary);

            application.Resources.MergedDictionaries.Add(runtime);

            runtimeDictionary = runtime;
        }

        private void ResetCore()
        {
            Application? application = Application.Current;

            if (application is null || runtimeDictionary is null) return;

            application.Resources.MergedDictionaries.Remove(runtimeDictionary);

            runtimeDictionary = null;
        }

        private static void EnsureDefaults(Application application)
        {
            foreach (ResourceDictionary dictionary in application.Resources.MergedDictionaries)
            {
                if (dictionary is ThemeForgeDefaults) return;
            }

            ((System.Collections.IList)application.Resources.MergedDictionaries).Insert(0, new ThemeForgeDefaults());
        }
    }
}
