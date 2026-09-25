using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using ThemeForge.Maui.Controls.Helpers;

namespace ThemeForge.Maui.Controls.ComponentModels.Skia
{
    /// <summary>
    /// Canvas для размытой подложки под кнопками overlay.
    /// Размывает исходный display brush, а не произвольный системный фон.
    /// </summary>
    public class BackdropBlurCanvas : SKCanvasView
    {
        /// <summary>
        /// Bindable-свойство исходного brush.
        /// </summary>
        public static readonly BindableProperty SourceBrushProperty = BindableProperty.Create(
                                                                            nameof(SourceBrush),
                                                                            typeof(Brush),
                                                                            typeof(BackdropBlurCanvas),
                                                                            null,
                                                                            propertyChanged: OnVisualPropertyChanged);

        /// <summary>
        /// Bindable-свойство радиуса размытия.
        /// </summary>
        public static readonly BindableProperty BlurRadiusProperty = BindableProperty.Create(
                                                                            nameof(BlurRadius),
                                                                            typeof(double),
                                                                            typeof(BackdropBlurCanvas),
                                                                            18.0,
                                                                            propertyChanged: OnVisualPropertyChanged);

        /// <summary>
        /// Bindable-свойство затемнения поверх blur.
        /// </summary>
        public static readonly BindableProperty ScrimOpacityProperty = BindableProperty.Create(
                                                                            nameof(ScrimOpacity),
                                                                            typeof(double),
                                                                            typeof(BackdropBlurCanvas),
                                                                            0.55,
                                                                            propertyChanged: OnVisualPropertyChanged);

        /// <summary>
        /// Исходный brush для размытия.
        /// </summary>
        public Brush? SourceBrush
        {
            get => (Brush?)GetValue(SourceBrushProperty);
            set => SetValue(SourceBrushProperty, value);
        }

        /// <summary>
        /// Радиус размытия.
        /// </summary>
        public double BlurRadius
        {
            get => (double)GetValue(BlurRadiusProperty);
            set => SetValue(BlurRadiusProperty, value);
        }

        /// <summary>
        /// Прозрачность чёрного scrim поверх blur.
        /// </summary>
        public double ScrimOpacity
        {
            get => (double)GetValue(ScrimOpacityProperty);
            set => SetValue(ScrimOpacityProperty, value);
        }

        /// <summary>
        /// Создает backdrop blur canvas.
        /// </summary>
        public BackdropBlurCanvas()
        {
            BackgroundColor = Colors.Transparent;
            PaintSurface += OnPaintSurface;
        }

        private static void OnVisualPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BackdropBlurCanvas canvas)
            {
                canvas.InvalidateSurface();
            }
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            SKCanvas canvas = e.Surface.Canvas;
            SKImageInfo info = e.Info;

            canvas.Clear(SKColors.Transparent);

            if (SourceBrush is null || info.Width <= 0 || info.Height <= 0) return;

            SKSize size = new (info.Width, info.Height);
            SKRect rect = new (0, 0, size.Width, size.Height);

            using SKShader? shader = SkiaBrushHelper.CreateShader(SourceBrush, size);

            if (shader is null) return;

            float sigma = (float)Math.Clamp(BlurRadius, 0.0, 80.0);

            using SKPaint blurPaint = new ()
            {
                IsAntialias = true,
                Shader = shader
            };

            if (sigma > 0.1f)
            {
                blurPaint.ImageFilter = SKImageFilter.CreateBlur(sigma, sigma);
            }

            canvas.DrawRect(rect, blurPaint);

            double scrim = Math.Clamp(ScrimOpacity, 0.0, 1.0);

            if (scrim > 0.0)
            {
                using SKPaint scrimPaint = new ()
                {
                    Color = new SKColor(0, 0, 0, (byte)(scrim * 255.0))
                };

                canvas.DrawRect(rect, scrimPaint);
            }
        }
    }
}
