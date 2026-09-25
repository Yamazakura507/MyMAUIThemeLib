using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Globalization;
using ThemeForge.Abstractions.Enums;
using ThemeForge.Abstractions.Knowledge;
using ThemeForge.Abstractions.Records.UseOfEffects;
using ThemeForge.Maui.Controls.Helpers;

namespace ThemeForge.Maui.Controls.ComponentModels.Skia
{
    /// <summary>
    /// Кроссплатформенный canvas для отрисовки фона темы и анимированных эффектов.
    /// </summary>
    public class EffectCanvas : SKCanvasView
    {
        /// <summary>
        /// Bindable-свойство отображаемого brush.
        /// </summary>
        public static readonly BindableProperty DisplayBrushProperty = BindableProperty.Create(
                                                                            nameof(DisplayBrush),
                                                                            typeof(Brush),
                                                                            typeof(EffectCanvas),
                                                                            null,
                                                                            propertyChanged: OnVisualPropertyChanged);

        /// <summary>
        /// Bindable-свойство настроек эффекта.
        /// </summary>
        public static readonly BindableProperty EffectProperty = BindableProperty.Create(
                                                                            nameof(Effect),
                                                                            typeof(EffectSettings),
                                                                            typeof(EffectCanvas),
                                                                            null,
                                                                            propertyChanged: OnEffectChanged);

        /// <summary>
        /// Bindable-свойство запуска анимации.
        /// </summary>
        public static readonly BindableProperty IsRunningProperty = BindableProperty.Create(
                                                                            nameof(IsRunning),
                                                                            typeof(bool),
                                                                            typeof(EffectCanvas),
                                                                            true,
                                                                            propertyChanged: OnIsRunningChanged);

        private readonly object timerLock = new();
        private IDispatcherTimer? timer;
        private double progress;

        /// <summary>
        /// Отображаемый фон.
        /// </summary>
        public Brush? DisplayBrush
        {
            get => (Brush?)GetValue(DisplayBrushProperty);
            set => SetValue(DisplayBrushProperty, value);
        }

        /// <summary>
        /// Эффект фона.
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
        /// Создает effect canvas.
        /// </summary>
        public EffectCanvas()
        {
            BackgroundColor = Colors.Transparent;
            PaintSurface += OnPaintSurface;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is EffectCanvas canvas)
            {
                canvas.InvalidateSurface();
            }
        }

        private static void OnEffectChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is EffectCanvas canvas)
            {
                canvas.UpdateTimer();
                canvas.InvalidateSurface();
            }
        }

        private static void OnIsRunningChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is EffectCanvas canvas)
            {
                canvas.UpdateTimer();
            }
        }

        private void OnLoaded(object? sender, EventArgs e) => UpdateTimer();

        private void OnUnloaded(object? sender, EventArgs e) => StopTimer();

        private void UpdateTimer()
        {
            if (IsRunning && Effect is { IsEnabled: true })
            {
                StartTimer();
            }
            else
            {
                StopTimer();
            }
        }

        private void StartTimer()
        {
            lock (timerLock)
            {
                timer ??= Dispatcher.CreateTimer();
                timer.Interval = TimeSpan.FromMilliseconds(33);
                timer.Tick -= OnTimerTick;
                timer.Tick += OnTimerTick;

                if (!timer.IsRunning)
                {
                    timer.Start();
                }
            }
        }

        private void StopTimer()
        {
            lock (timerLock)
            {
                if (timer is not null)
                {
                    timer.Tick -= OnTimerTick;
                    timer.Stop();
                }
            }
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            double speed = Math.Clamp(Effect?.Speed ?? 1.0, 0.1, 10.0);
            progress = (progress + 0.016 * speed) % 1.0;

            MainThread.BeginInvokeOnMainThread(InvalidateSurface);
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;
            SKImageInfo info = e.Info;

            canvas.Clear(SKColors.Transparent);

            if (DisplayBrush is null || info.Width <= 0 || info.Height <= 0) return;

            SKSize size = new(info.Width, info.Height);
            SKRect rect = new(0, 0, size.Width, size.Height);
            EffectSettings? effect = Effect;
            SKShaderTileMode tileMode = effect is { IsEnabled: true, Kind: EffectKind.MovingColors } ? SKShaderTileMode.Repeat : SKShaderTileMode.Clamp;
            using SKShader? baseShader = SkiaBrushHelper.CreateShader(DisplayBrush, size, tileMode);

            if (baseShader is null) return;

            using SKPaint basePaint = new()
            {
                IsAntialias = true,
                Shader = baseShader
            };

            if (effect is { IsEnabled: true, Kind: EffectKind.MovingColors })
            {
                basePaint.Shader = ApplyMovingColors(baseShader, size, effect);
            }

            canvas.DrawRect(rect, basePaint);

            if (effect is not { IsEnabled: true }) return;

            switch (effect.Kind)
            {
                case EffectKind.Wave:
                    DrawWave(canvas, size, baseShader, effect);
                    break;

                case EffectKind.Shimmer:
                    DrawShimmer(canvas, rect, size, effect);
                    break;

                case EffectKind.Noise:
                    DrawNoise(canvas, rect, effect);
                    break;

                case EffectKind.MovingColors:
                    // Уже обработано выше через local matrix.
                    break;

                case EffectKind.Lottie:
                    // Lottie подключается отдельно как layered view.
                    break;
            }
        }

        private SKShader? ApplyMovingColors( SKShader shader, SKSize size, EffectSettings effect)
        {
            IReadOnlyDictionary<string,string> parameters = effect.Parameters;

            double speed = Math.Clamp(effect.Speed, 0.1, 10.0);
            double intensity = Math.Clamp(effect.Intensity, 0.0, 1.0);

            var directionX = GetFloat(parameters, KnownEffectParameters.MovingColorsDirectionX, 0.35f);
            var directionY = GetFloat(parameters, KnownEffectParameters.MovingColorsDirectionY, 0.18f);

            directionX = Math.Clamp(directionX, -10.0f, 10.0f);
            directionY = Math.Clamp(directionY, -10.0f, 10.0f);

            float dx = WrapFraction(progress * speed * directionX * intensity) * size.Width;
            float dy = WrapFraction(progress * speed * directionY * intensity) * size.Height;

            using SKShader? transformedShader = shader.WithLocalMatrix(SKMatrix.CreateTranslation(dx, dy));

            return transformedShader;
        }

        private void DrawWave(SKCanvas canvas, SKSize size, SKShader shader, EffectSettings effect)
        {
            IReadOnlyDictionary<string, string> parameters = effect.Parameters;

            double intensity = Math.Clamp(effect.Intensity, 0.0, 1.0);
            double speed = Math.Clamp(effect.Speed, 0.1, 10.0);

            int bands = Math.Clamp(GetInt(parameters, KnownEffectParameters.WaveBands, 4), 1, 12);
            float frequencyMultiplier = Math.Clamp(GetFloat(parameters, KnownEffectParameters.WaveFrequency, 1.0f), 0.1f, 10.0f);
            float amplitudeMultiplier = Math.Clamp(GetFloat(parameters, KnownEffectParameters.WaveAmplitude, 1.0f), 0.0f, 5.0f);

            using SKPaint paint = new ()
            {
                IsAntialias = true,
                Style = SKPaintStyle.Stroke,
                Shader = shader
            };

            paint.Color = paint.Color.WithAlpha((byte)Math.Clamp(90.0 * intensity, 0.0, 255.0));

            for (int band = 0; band < bands; band++)
            {
                using SKPathBuilder builder = new();

                float baseY = size.Height * (0.22f + band * (0.78f / Math.Max(1, bands - 1)));
                float amplitude = (float)(size.Height * 0.07f * intensity * amplitudeMultiplier * (1f + band * 0.12f));
                float frequency = (1.5f + band * 0.65f) * frequencyMultiplier;
                float phase = (float)(progress * Math.PI * 2.0 * speed + band * 0.8);

                paint.StrokeWidth = 2f + band * 1.5f;

                bool first = true;

                for (float x = 0; x <= size.Width; x += 4f)
                {
                    float normalized = x / size.Width;
                    float y = (float)(baseY + amplitude * Math.Sin(normalized * Math.PI * 2f * frequency + phase));

                    if (first)
                    {
                        builder.MoveTo(x, y);
                        first = false;
                    }
                    else
                    {
                        builder.LineTo(x, y);
                    }
                }

                using SKPath path = builder.Snapshot();
                canvas.DrawPath(path, paint);
            }
        }

        private void DrawShimmer(SKCanvas canvas, SKRect rect, SKSize size, EffectSettings effect)
        {
            IReadOnlyDictionary<string, string> parameters = effect.Parameters;

            double intensity = Math.Clamp(effect.Intensity, 0.0, 1.0);
            double speed = Math.Clamp(effect.Speed, 0.1, 10.0);

            float angleDegrees = GetFloat(parameters, KnownEffectParameters.ShimmerAngleDegrees, 45.0f);
            float widthRatio = Math.Clamp(GetFloat(parameters, KnownEffectParameters.ShimmerWidthRatio, 0.7f), 0.05f, 3.0f);
            float opacityMultiplier = Math.Clamp(GetFloat(parameters, KnownEffectParameters.ShimmerOpacityMultiplier, 1.0f), 0.0f, 5.0f);

            double radians = angleDegrees * Math.PI / 180.0;
            float dx = (float)Math.Cos(radians);
            float dy = (float)Math.Sin(radians);

            float maxLength = Math.Max(size.Width, size.Height);
            float halfLength = maxLength * widthRatio * 0.5f;

            SKPoint center = new (size.Width / 2f, size.Height / 2f);

            float shift = (float)((progress * speed) % 2.0) - 0.5f;
            float offset = shift * maxLength;

            SKPoint start = new (center.X + dx * (-halfLength + offset), center.Y + dy * (-halfLength + offset));
            SKPoint end = new (center.X + dx * (halfLength + offset), center.Y + dy * (halfLength + offset));

            byte peakAlpha = (byte)Math.Clamp(150.0 * intensity * opacityMultiplier, 0.0, 255.0);
            byte softAlpha = (byte)Math.Clamp(70.0 * intensity * opacityMultiplier, 0.0, 255.0);

            using SKShader shimmerShader = SKShader.CreateLinearGradient(
                start, end,
                [
                    new SKColor(255, 255, 255, 0),
                    new SKColor(255, 255, 255, softAlpha),
                    new SKColor(255, 255, 255, peakAlpha),
                    new SKColor(255, 255, 255, softAlpha),
                    new SKColor(255, 255, 255, 0)
                ],
                [0f, 0.35f, 0.5f, 0.65f, 1f], SKShaderTileMode.Clamp);

            using SKPaint paint = new ()
            {
                Shader = shimmerShader,
                BlendMode = SKBlendMode.Screen,
                IsAntialias = true
            };

            canvas.DrawRect(rect, paint);
        }

        private void DrawNoise(SKCanvas canvas, SKRect rect, EffectSettings effect)
        {
            IReadOnlyDictionary<string,string> parameters = effect.Parameters;

            double intensity = Math.Clamp(effect.Intensity, 0.0, 1.0);
            double speed = Math.Clamp(effect.Speed, 0.1, 10.0);

            int octaves = Math.Clamp(GetInt(parameters, KnownEffectParameters.NoiseOctaves, 3), 1, 8);
            float frequencyMultiplier = Math.Clamp(GetFloat(parameters, KnownEffectParameters.NoiseFrequency, 1.0f), 0.1f, 10.0f);
            float contrast = Math.Clamp(GetFloat(parameters, KnownEffectParameters.NoiseContrast, 1.0f), 0.0f, 5.0f);

            float frequency = (0.015f + (float)intensity * 0.08f) * frequencyMultiplier;
            float seed = (float)(progress * 120.0 * speed);

            using SKShader noiseShader = SKShader.CreatePerlinNoiseFractalNoise(frequency,frequency, octaves, seed);

            using SKPaint paint = new ()
            {
                Shader = noiseShader,
                BlendMode = SKBlendMode.Overlay,
                IsAntialias = true
            };

            paint.Color = paint.Color.WithAlpha((byte)Math.Clamp(70.0 * intensity * contrast, 0.0, 255.0));

            canvas.DrawRect(rect, paint);
        }

        private static float WrapFraction(double value)
        {
            double wrapped = value % 1.0;

            if (wrapped < 0)
            {
                wrapped += 1.0;
            }

            return (float)wrapped;
        }

        private static float GetFloat(IReadOnlyDictionary<string, string>? parameters, string key, float defaultValue)
        {
            if (parameters is null || !parameters.TryGetValue(key, out string? raw) || string.IsNullOrWhiteSpace(raw))
            {
                return defaultValue;
            }

            return float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value) ? value : defaultValue;
        }

        private static int GetInt(IReadOnlyDictionary<string, string>? parameters, string key, int defaultValue)
        {
            if (parameters is null || !parameters.TryGetValue(key, out string? raw) || string.IsNullOrWhiteSpace(raw))
            {
                return defaultValue;
            }

            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value) ? value : defaultValue;
        }
    }
}
