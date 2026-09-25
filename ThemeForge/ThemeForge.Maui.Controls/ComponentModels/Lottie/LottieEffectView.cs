using System.Globalization;
using System.Reflection;
using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Helpers;
using ThemeForge.Abstractions.Records.UseOfEffects;
using ThemeForge.Maui.Helpers;

namespace ThemeForge.Maui.Controls.ComponentModels.Lottie
{
    /// <summary>
    /// Кроссплатформенный адаптер для отображения Lottie-эффекта.
    /// Использует reflection, чтобы не зависеть от конкретной версии/расположения LottieView.
    /// </summary>
    public class LottieEffectView : ContentView
    {
        /// <summary>
        /// Bindable-свойство настроек эффекта.
        /// </summary>
        public static readonly BindableProperty EffectProperty = BindableProperty.Create(
                                                                            nameof(Effect),
                                                                            typeof(EffectSettings),
                                                                            typeof(LottieEffectView),
                                                                            null,
                                                                            propertyChanged: OnEffectChanged);

        /// <summary>
        /// Bindable-свойство запуска/остановки анимации.
        /// </summary>
        public static readonly BindableProperty IsRunningProperty = BindableProperty.Create(
                                                                            nameof(IsRunning),
                                                                            typeof(bool),
                                                                            typeof(LottieEffectView),
                                                                            true,
                                                                            propertyChanged: OnIsRunningChanged);

        private static readonly Lazy<Type?> NativeLottieViewType = new(FindNativeLottieViewType);

        private View? nativeView;
        private bool hasSource;

        /// <summary>
        /// Настройки эффекта.
        /// </summary>
        public EffectSettings? Effect
        {
            get => (EffectSettings?)GetValue(EffectProperty);
            set => SetValue(EffectProperty, value);
        }

        /// <summary>
        /// Включена ли анимация.
        /// </summary>
        public bool IsRunning
        {
            get => (bool)GetValue(IsRunningProperty);
            set => SetValue(IsRunningProperty, value);
        }

        /// <summary>
        /// Удалось ли установить источник Lottie.
        /// </summary>
        public bool HasSource => hasSource;

        /// <summary>
        /// Создает Lottie-хост.
        /// </summary>
        public LottieEffectView()
        {
            BackgroundColor = Colors.Transparent;
            IsVisible = false;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        /// <inheritdoc />
        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();

            EnsureNativeView();
            ApplyEffect();
        }

        private static void OnEffectChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LottieEffectView view)
            {
                view.ApplyEffect();
            }
        }

        private static void OnIsRunningChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is LottieEffectView view)
            {
                view.ApplyEffect();
            }
        }

        private void OnLoaded(object? sender, EventArgs e)
        {
            EnsureNativeView();
            ApplyEffect();
        }

        private void OnUnloaded(object? sender, EventArgs e)
        {
            // Нативный контрол может сам управлять жизненным циклом анимации.
            // Здесь достаточно скрыть хост, если эффект больше не нужен.
            if (!hasSource)
            {
                IsVisible = false;
            }
        }

        private void EnsureNativeView()
        {
            if (nativeView is not null) return;

            Type? type = NativeLottieViewType.Value;

            if (type is null) return;

            try
            {
                object? instance = Activator.CreateInstance(type);

                if (instance is View view)
                {
                    nativeView = view;
                    Content = view;
                }
            }
            catch
            {
                nativeView = null;
                Content = null;
            }
        }

        private void ApplyEffect()
        {
            hasSource = false;

            if (nativeView is null || Effect is not { IsEnabled: true, Kind: EffectKind.Lottie } effect)
            {
                IsVisible = false;
                return;
            }

            string? source = ResolveSource(effect);

            if (string.IsNullOrWhiteSpace(source) || !TrySetSource(nativeView, source))
            {
                IsVisible = false;
                return;
            }

            hasSource = true;
            IsVisible = true;

            ApplyLoop(effect);
            ApplyAutoPlay(effect);
            ApplySpeed(effect);
            ApplyTintColor(effect);
            ApplyScaleMode(effect);
        }

        private static Type? FindNativeLottieViewType()
        {
            string[] candidateTypeNames =
            [
                "CommunityToolkit.Maui.Views.LottieView",
                "CommunityToolkit.Maui.LottieView",
                "CommunityToolkit.Maui.Controls.LottieView"
            ];

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var typeName in candidateTypeNames)
                {
                    Type? type = assembly.GetType(typeName, throwOnError: false);

                    if (type is not null && typeof(View).IsAssignableFrom(type))
                    {
                        return type;
                    }
                }
            }

            return null;
        }

        private static string? ResolveSource(EffectSettings effect)
        {
            if (TryGetString(effect.Parameters, LottieEffectParameters.Url, out var url) && Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                return url;
            }

            if (TryGetString(effect.Parameters, LottieEffectParameters.AssetName, out var asset) && !string.IsNullOrWhiteSpace(asset))
            {
                return asset;
            }

            if (TryGetString(effect.Parameters, "Lottie.Source", out var genericSource) && !string.IsNullOrWhiteSpace(genericSource))
            {
                return genericSource;
            }

            return null;
        }

        private static bool TrySetSource(View view, string source)
        {
            Type viewType = view.GetType();
            PropertyInfo? sourceProperty = viewType.GetProperty("Source", BindingFlags.Public | BindingFlags.Instance);

            if (sourceProperty is null || !sourceProperty.CanWrite) return false;

            if (sourceProperty.PropertyType == typeof(string))
            {
                try
                {
                    sourceProperty.SetValue(view, source);
                    return true;
                }
                catch
                {
                    return false;
                }
            }

            Type? sourceType = sourceProperty.PropertyType;
            Assembly hintAssembly = viewType.Assembly;
            object? sourceObject = CreateSourceObject(sourceType, source, hintAssembly);

            if (sourceObject is not null)
            {
                try
                {
                    sourceProperty.SetValue(view, sourceObject);
                    return true;
                }
                catch
                {
                    // Продолжаем fallback-попытки.
                }
            }

            try
            {
                sourceProperty.SetValue(view, source);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static object? CreateSourceObject(Type sourceType, string source, Assembly hintAssembly)
        {
            if (Uri.TryCreate(source, UriKind.Absolute, out var uri))
            {
                return
                    TryCallStatic(
                        sourceType,
                        ["FromUri", "FromUrl", "FromAbsoluteUri"],
                        [new object?[] { uri }],
                        [new object?[] { source }])
                    ?? TryCreateInstance(
                        sourceType,
                        [new object?[] { uri }],
                        [new object?[] { source }]);
            }

            if (source.StartsWith("resource:", StringComparison.OrdinalIgnoreCase))
            {
                string resourceName = source["resource:".Length..].TrimStart('/');

                return
                    TryCallStatic(
                        sourceType,
                        ["FromResource"],
                        [new object?[] { resourceName, hintAssembly }],
                        [new object?[] { resourceName }])
                    ?? TryCreateInstance(
                        sourceType,
                        [new object?[] { resourceName, hintAssembly }],
                        [new object?[] { resourceName }]);
            }

            return
                TryCallStatic(
                    sourceType,
                    ["FromFile", "FromAsset", "FromFilename", "FromResource"],
                    [new object?[] { source, hintAssembly }],
                    [new object?[] { source }])
                ?? TryCreateInstance(
                    sourceType,
                    [new object?[] { source, hintAssembly }],
                    [new object?[] { source }]);
        }

        private static object? TryCallStatic(Type type, string[] methodNames, params object?[][] argumentSets)
        {
            foreach (string methodName in methodNames)
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.Static))
                {
                    if (!string.Equals(method.Name, methodName, StringComparison.OrdinalIgnoreCase)) continue;

                    foreach (var args in argumentSets)
                    {
                        ParameterInfo[] parameters = method.GetParameters();

                        if (parameters.Length != args.Length) continue;

                        object?[]? converted = TryConvertArguments(parameters, args);

                        if (converted is null) continue;

                        try
                        {
                            return method.Invoke(null, converted);
                        }
                        catch
                        {
                            // Пробуем следующую сигнатуру.
                        }
                    }
                }
            }

            return null;
        }

        private static object? TryCreateInstance(Type type, params object?[][] argumentSets)
        {
            foreach (var constructor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                foreach (var args in argumentSets)
                {
                    ParameterInfo[] parameters = constructor.GetParameters();

                    if (parameters.Length != args.Length) continue;

                    object?[]? converted = TryConvertArguments(parameters, args);

                    if (converted is null) continue;

                    try
                    {
                        return constructor.Invoke(converted);
                    }
                    catch
                    {
                        // Пробуем следующий конструктор.
                    }
                }
            }

            return null;
        }

        private static object?[]? TryConvertArguments(ParameterInfo[] parameters, object?[] args)
        {
            object?[]? result = new object?[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                Type? parameterType = parameters[i].ParameterType;
                object? argument = args[i];

                if (argument is null)
                {
                    if (!parameters[i].IsOptional && parameterType.IsValueType) return null;

                    result[i] = null;
                    continue;
                }

                if (parameterType.IsInstanceOfType(argument))
                {
                    result[i] = argument;
                    continue;
                }

                if (parameterType == typeof(string))
                {
                    result[i] = argument.ToString();
                    continue;
                }

                if (parameterType == typeof(Uri) && argument is string uriText && Uri.TryCreate(uriText, UriKind.Absolute, out Uri? uri))
                {
                    result[i] = uri;
                    continue;
                }

                if (parameterType.IsEnum && argument is string enumText && Enum.TryParse(parameterType, enumText, ignoreCase: true, out object? enumValue))
                {
                    result[i] = enumValue;
                    continue;
                }

                if (parameterType == typeof(Assembly) && argument is Assembly assembly)
                {
                    result[i] = assembly;
                    continue;
                }

                return null;
            }

            return result;
        }

        private void ApplyLoop(EffectSettings effect)
        {
            if (nativeView is null) return;

            bool loop = GetBool(effect.Parameters, LottieEffectParameters.Loop, defaultValue: true);

            string[] names = loop ? ["Restart", "Infinite", "Loop", "Forever"] : ["Once", "One", "None"];

            SetEnumPropertyIfExists(nativeView, names, "RepeatMode", "LoopMode", "Repeat");
        }

        private void ApplyAutoPlay(EffectSettings effect)
        {
            if (nativeView is null) return;

            bool autoPlay = IsRunning && GetBool(effect.Parameters, LottieEffectParameters.AutoPlay, defaultValue: true);

            SetPropertyIfExists(nativeView, autoPlay, "AutoPlay", "IsAutoPlay", "PlayOnLoaded");
        }

        private void ApplySpeed(EffectSettings effect)
        {
            if (nativeView is null) return;

            float fallbackSpeed = (float)Math.Clamp(effect.Speed, 0.1, 10.0);
            float speed = GetFloat(effect.Parameters, LottieEffectParameters.Speed, fallbackSpeed);

            SetPropertyIfExists(nativeView, speed, "Speed", "PlaybackSpeed", "AnimationSpeed");
        }

        private void ApplyTintColor(EffectSettings effect)
        {
            if (nativeView is null || !TryGetString(effect.Parameters, LottieEffectParameters.TintColorHex, out var hex) || string.IsNullOrWhiteSpace(hex)) return;

            Color color = ColorConversion.ToColor(hex);

            SetPropertyIfExists(nativeView, color, "Color", "TintColor", "FillColor", "StrokeColor");
        }

        private void ApplyScaleMode(EffectSettings effect)
        {
            if (nativeView is null) return;

            string scaleMode = GetString(effect.Parameters, LottieEffectParameters.ScaleMode, "AspectFit");

            string[] names =
            [
                scaleMode,
                "AspectFit",
                "Uniform",
                "AspectFill",
                "Zoom"
            ];

            SetEnumPropertyIfExists(nativeView, names, "ScaleMode", "AnimationScaleMode", "FillMode");
        }

        private static void SetPropertyIfExists(View view, object? value, params string[] propertyNames)
        {
            if (view is null || value is null) return;

            Type? viewType = view.GetType();

            foreach (var propertyName in propertyNames)
            {
                PropertyInfo? property = viewType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

                if (property is null || !property.CanWrite) continue;

                object? converted = ConvertValue(property.PropertyType, value);

                if (converted is null) continue;

                try
                {
                    property.SetValue(view, converted);
                    return;
                }
                catch
                {
                    // Пробуем следующее имя свойства.
                }
            }
        }

        private static void SetEnumPropertyIfExists(View view, string[] names, params string[] propertyNames)
        {
            if (view is null || names.Length == 0) return;

            Type viewType = view.GetType();

            foreach (string propertyName in propertyNames)
            {
                PropertyInfo? property = viewType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);

                if (property is null || !property.CanWrite) continue;

                if (property.PropertyType.IsEnum)
                {
                    foreach (string name in names)
                    {
                        if (Enum.TryParse(property.PropertyType, name, ignoreCase: true, out object? value))
                        {
                            try
                            {
                                property.SetValue(view, value);
                                return;
                            }
                            catch
                            {
                                // Пробуем следующее имя.
                            }
                        }
                    }
                }
                else if (property.PropertyType == typeof(string))
                {
                    try
                    {
                        property.SetValue(view, names[0]);
                        return;
                    }
                    catch
                    {
                        // Пробуем следующее свойство.
                    }
                }
            }
        }

        private static object? ConvertValue(Type targetType, object value)
        {
            if (targetType.IsInstanceOfType(value)) return value;

            if (targetType == typeof(string)) return value.ToString();

            if (targetType == typeof(float) && value is IConvertible convertibleFloat)
            {
                return convertibleFloat.ToSingle(CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(double) && value is IConvertible convertibleDouble)
            {
                return convertibleDouble.ToDouble(CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(bool) && value is IConvertible convertibleBool)
            {
                return convertibleBool.ToBoolean(CultureInfo.InvariantCulture);
            }

            if (targetType.IsEnum && value is string enumText)
            {
                if (Enum.TryParse(targetType, enumText, ignoreCase: true, out object? enumValue))
                {
                    return enumValue;
                }
            }

            return null;
        }

        private static bool TryGetString(IReadOnlyDictionary<string, string> parameters, string key, out string? value)
        {
            value = null;

            if (parameters is null || !parameters.TryGetValue(key, out string? raw) || string.IsNullOrWhiteSpace(raw)) return false;

            value = raw;

            return true;
        }

        private static string GetString(IReadOnlyDictionary<string, string> parameters, string key, string defaultValue) => 
            TryGetString(parameters, key, out string? value) ? value! : defaultValue;

        private static bool GetBool(IReadOnlyDictionary<string, string> parameters, string key, bool defaultValue) => 
            TryGetString(parameters, key, out string? value) || !bool.TryParse(value, out bool parsed) ? defaultValue : parsed;

        private static float GetFloat(IReadOnlyDictionary<string, string> parameters, string key, float defaultValue) => 
            !TryGetString(parameters, key, out string? value) || !float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) 
                ? defaultValue 
                : Math.Clamp(parsed, 0.1f, 10f);
    }
}
