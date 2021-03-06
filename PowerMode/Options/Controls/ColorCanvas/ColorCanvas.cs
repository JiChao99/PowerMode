﻿namespace BigEgg.Tools.PowerMode.Options.Controls
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    using BigEgg.Tools.PowerMode.Utils;

    [TemplatePart(Name = PART_ColorShadingCanvas, Type = typeof(Canvas))]
    [TemplatePart(Name = PART_ColorShadeSelector, Type = typeof(Canvas))]
    [TemplatePart(Name = PART_SpectrumSlider, Type = typeof(ColorSpectrumSlider))]
    public class ColorCanvas : Control
    {
        private const string PART_ColorShadingCanvas = "PART_ColorShadingCanvas";
        private const string PART_ColorShadeSelector = "PART_ColorShadeSelector";
        private const string PART_SpectrumSlider = "PART_SpectrumSlider";

        private TranslateTransform colorShadeSelectorTransform = new TranslateTransform();
        private Canvas colorShadingCanvas;
        private Canvas colorShadeSelector;
        private ColorSpectrumSlider spectrumSlider;
        private bool updatingRGB;
        private bool currentColorPositionPropertyChanged;


        static ColorCanvas()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorCanvas), new FrameworkPropertyMetadata(typeof(ColorCanvas)));
        }


        #region Dependency Properties
        #region Selected Color
        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register("SelectedColor", typeof(Color?), typeof(ColorCanvas), new FrameworkPropertyMetadata(Colors.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(OnSelectedColorChanged)));

        public Color? SelectedColor
        {
            get { return (Color?)GetValue(SelectedColorProperty); }
            set { SetValue(SelectedColorProperty, value); }
        }

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorCanvas colorCanvas)
            {
                colorCanvas.OnSelectedColorChanged((Color?)e.OldValue, (Color?)e.NewValue);
            }
        }

        protected virtual void OnSelectedColorChanged(Color? oldValue, Color? newValue)
        {
            HexadecimalString = SelectedColor.Value.ToDrawingColor().ToHexString();
            UpdateRGBValues(SelectedColor);
            UpdateColorShadeSelectorPosition(SelectedColor);
        }
        #endregion

        #region RGB
        #region R
        public static readonly DependencyProperty RProperty = DependencyProperty.Register("R", typeof(byte), typeof(ColorCanvas), new UIPropertyMetadata((byte)0, OnRGBChanged));

        public byte R
        {
            get { return (byte)GetValue(RProperty); }
            set { SetValue(RProperty, value); }
        }
        #endregion

        #region G
        public static readonly DependencyProperty GProperty = DependencyProperty.Register("G", typeof(byte), typeof(ColorCanvas), new UIPropertyMetadata((byte)0, OnRGBChanged));

        public byte G
        {
            get { return (byte)GetValue(GProperty); }
            set { SetValue(GProperty, value); }
        }
        #endregion

        #region B
        public static readonly DependencyProperty BProperty = DependencyProperty.Register("B", typeof(byte), typeof(ColorCanvas), new UIPropertyMetadata((byte)0, OnRGBChanged));

        public byte B
        {
            get { return (byte)GetValue(BProperty); }
            set { SetValue(BProperty, value); }
        }
        #endregion

        private static void OnRGBChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ColorCanvas colorCanvas)
            {
                colorCanvas.OnRGBChanged((byte)e.OldValue, (byte)e.NewValue);
            }
        }

        protected virtual void OnRGBChanged(byte oldValue, byte newValue)
        {
            if (updatingRGB) { return; }

            updatingRGB = true;
            SelectedColor = Color.FromRgb(R, G, B);
            updatingRGB = false;
        }
        #endregion

        #region HexadecimalString
        public static readonly DependencyProperty HexadecimalStringProperty = DependencyProperty.Register("HexadecimalString", typeof(string), typeof(ColorCanvas), new UIPropertyMetadata(""));

        public string HexadecimalString
        {
            get { return (string)GetValue(HexadecimalStringProperty); }
            set { SetValue(HexadecimalStringProperty, value); }
        }
        #endregion
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (colorShadingCanvas != null)
            {
                colorShadingCanvas.MouseLeftButtonDown -= ColorShadingCanvas_MouseLeftButtonDown;
                colorShadingCanvas.MouseLeftButtonUp -= ColorShadingCanvas_MouseLeftButtonUp;
                colorShadingCanvas.MouseMove -= ColorShadingCanvas_MouseMove;
            }
            colorShadingCanvas = GetTemplateChild(PART_ColorShadingCanvas) as Canvas;
            if (colorShadingCanvas != null)
            {
                colorShadingCanvas.MouseLeftButtonDown += ColorShadingCanvas_MouseLeftButtonDown;
                colorShadingCanvas.MouseLeftButtonUp += ColorShadingCanvas_MouseLeftButtonUp;
                colorShadingCanvas.MouseMove += ColorShadingCanvas_MouseMove;
            }

            colorShadeSelector = GetTemplateChild(PART_ColorShadeSelector) as Canvas;
            if (colorShadeSelector != null) { colorShadeSelector.RenderTransform = colorShadeSelectorTransform; }

            if (spectrumSlider != null)
            {
                spectrumSlider.ValueChanged -= SpectrumSlider_ValueChanged;
                spectrumSlider.MouseLeftButtonDown -= SpectrumSlider_MouseLeftButtonDown;
                spectrumSlider.MouseLeftButtonUp -= SpectrumSlider_MouseLeftButtonUp; ;
            }
            spectrumSlider = GetTemplateChild(PART_SpectrumSlider) as ColorSpectrumSlider;
            if (spectrumSlider != null)
            {
                spectrumSlider.ValueChanged += SpectrumSlider_ValueChanged;
                spectrumSlider.MouseLeftButtonDown += SpectrumSlider_MouseLeftButtonDown;
                spectrumSlider.MouseLeftButtonUp += SpectrumSlider_MouseLeftButtonUp; ;
            }

            HexadecimalString = SelectedColor.Value.ToDrawingColor().ToHexString();
            UpdateRGBValues(SelectedColor);
            UpdateColorShadeSelectorPosition(SelectedColor);
        }

        #region Event Handlers
        private void ColorShadingCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(colorShadingCanvas);
            var colorShadeSelectorPosition = UpdateColorShadeSelectorPosition(p);
            CalculateColor(colorShadeSelectorPosition);

            colorShadingCanvas.CaptureMouse();
            e.Handled = true;
        }

        private void ColorShadingCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            colorShadingCanvas.ReleaseMouseCapture();
            e.Handled = true;
        }

        private void ColorShadingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point p = e.GetPosition(colorShadingCanvas);
                var colorShadeSelectorPosition = UpdateColorShadeSelectorPosition(p);
                CalculateColor(colorShadeSelectorPosition);

                Mouse.Synchronize();
            }
        }

        private void SpectrumSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SelectedColor.Value.ToDrawingColor().ColorToHSV(out double hue, out double saturation, out double value);
            var position = new Point(saturation, 1 - value);
            CalculateColor(position);
        }

        private void SpectrumSlider_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void SpectrumSlider_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
        #endregion


        private void UpdateRGBValues(Color? color)
        {
            if (!color.HasValue) { return; }
            if (updatingRGB) { return; }

            updatingRGB = true;

            R = color.Value.R;
            G = color.Value.G;
            B = color.Value.B;

            updatingRGB = false;
        }

        private Point UpdateColorShadeSelectorPosition(Point p)
        {
            if (colorShadeSelector == null || colorShadingCanvas == null) { return p; }

            if (p.Y < 0) { p.Y = 0; }
            if (p.X < 0) { p.X = 0; }
            if (p.X > colorShadingCanvas.ActualWidth) { p.X = colorShadingCanvas.ActualWidth; }
            if (p.Y > colorShadingCanvas.ActualHeight) { p.Y = colorShadingCanvas.ActualHeight; }

            colorShadeSelectorTransform.X = p.X - (colorShadeSelector.Width / 2);
            colorShadeSelectorTransform.Y = p.Y - (colorShadeSelector.Height / 2);

            p.X = p.X / colorShadingCanvas.ActualWidth;
            p.Y = p.Y / colorShadingCanvas.ActualHeight;

            return p;
        }

        private Point UpdateColorShadeSelectorPosition(Color? color)
        {
            if (!color.HasValue) { new Point(0, 0); }

            color.Value.ToDrawingColor().ColorToHSV(out double hue, out double saturation, out double value);
            Point p = new Point(saturation, 1 - value);
            if (currentColorPositionPropertyChanged || spectrumSlider == null || colorShadingCanvas == null)
            {
                return p;
            }

            spectrumSlider.Value = 360 - hue;

            colorShadeSelectorTransform.X = (p.X * colorShadingCanvas.Width) - 5;
            colorShadeSelectorTransform.Y = (p.Y * colorShadingCanvas.Height) - 5;

            return p;
        }

        private void CalculateColor(Point p)
        {
            var currentColor = ColorExtensions.ColorFromHSV(360 - spectrumSlider.Value, p.X, 1 - p.Y).ToMediaColor();

            currentColorPositionPropertyChanged = true;
            SelectedColor = currentColor;
            currentColorPositionPropertyChanged = false;
        }
    }
}
