using System;
using System.Reflection;
using SkiaSharp.Extended.Svg;
using SkiaSharp.Views.Forms;
using Xamarin.Forms;

namespace SkiaSharpSvgImage.Controls
{
    public class SvgImage : SKCanvasView
    {
        public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(string), typeof(SvgImage), default(string), propertyChanged: OnSourceChanged);

        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        static void OnSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var skCanvasView = bindable as SKCanvasView;
            skCanvasView?.InvalidateSurface();
        }

        public static readonly BindableProperty AssemblyProperty = BindableProperty.Create(nameof(AssemblyLocation), typeof(string), typeof(SvgImage), default(string), propertyChanged: OnAssemblyChanged);

        public string AssemblyLocation
        {
            get => (string)GetValue(AssemblyProperty);
            set => SetValue(AssemblyProperty, value);
        }

        static void OnAssemblyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var skCanvasView = bindable as SKCanvasView;
            skCanvasView?.InvalidateSurface();
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            InvalidateSurface();
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            InvalidateSurface();
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            try
            {
                var surface = e.Surface;
                var canvas = surface.Canvas;

                canvas.Clear();

                if (string.IsNullOrEmpty(Source) || string.IsNullOrEmpty(AssemblyLocation))
                    return;

                var currentAssembly = Assembly.Load(AssemblyLocation);
                using (var stream = currentAssembly.GetManifestResourceStream(AssemblyLocation + "." + Source))
                {
                    var skSvg = new SKSvg();
                    skSvg.Load(stream);

                    var skImageInfo = e.Info;
                    canvas.Translate(skImageInfo.Width / 2f, skImageInfo.Height / 2f);

                    var skRect = skSvg.ViewBox;
                    float xRatio = skImageInfo.Width / skRect.Width;
                    float yRatio = skImageInfo.Height / skRect.Height;

                    float ratio = Math.Min(xRatio, yRatio);

                    canvas.Scale(ratio);
                    canvas.Translate(-skRect.MidX, -skRect.MidY);

                    canvas.DrawPicture(skSvg.Picture);
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine("OnPaintSurface Exception: " + exc);
            }
        }
    }
}