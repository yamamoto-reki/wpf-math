using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WpfMath.Exceptions;

namespace WpfMath.Controls
{
    /// <summary>
    /// Interaction logic for FormulaControl.xaml
    /// </summary>
    public partial class FormulaControl : UserControl
    {
        private static TexFormulaParser formulaParser = new TexFormulaParser();
        private TexFormula texFormula;

        public string Formula
        {
            get { return (string)GetValue(FormulaProperty); }
            set { SetValue(FormulaProperty, value); }
        }

        public double Scale
        {
            get { return (double)GetValue(ScaleProperty); }
            set { SetValue(ScaleProperty, value); }
        }

        public string SystemTextFontName
        {
            get => (string)GetValue(SystemTextFontNameProperty);
            set => SetValue(SystemTextFontNameProperty, value);
        }

        public bool HasError
        {
            get { return (bool)GetValue(HasErrorProperty); }
            private set { SetValue(HasErrorProperty, value); }
        }

        public ObservableCollection<Exception> Errors
        {
            get { return (ObservableCollection<Exception>)GetValue(ErrorsProperty); }
            private set { SetValue(ErrorsProperty, value); }
        }

        public ControlTemplate ErrorTemplate
        {
            get { return (ControlTemplate)GetValue(ErrorTemplateProperty); }
            set { SetValue(ErrorTemplateProperty, value); }
        }

        public int SelectionStart
        {
            get { return (int)GetValue(SelectionStartProperty); }
            set { SetValue(SelectionStartProperty, value); }
        }

        public int SelectionLength
        {
            get { return (int)GetValue(SelectionLengthProperty); }
            set { SetValue(SelectionLengthProperty, value); }
        }

        public Brush SelectionBrush
        {
            get { return (Brush)GetValue(SelectionBrushProperty); }
            set { SetValue(SelectionBrushProperty, value); }
        }

        public static readonly DependencyProperty FormulaProperty = DependencyProperty.Register(
            "Formula", typeof(string), typeof(FormulaControl),
            new PropertyMetadata("", OnRenderSettingsChanged, CoerceFormula));

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale", typeof(double), typeof(FormulaControl),
            new PropertyMetadata(20d, OnRenderSettingsChanged, CoerceScaleValue));

        public static readonly DependencyProperty SystemTextFontNameProperty = DependencyProperty.Register(
            nameof(SystemTextFontName), typeof(string), typeof(FormulaControl),
            new PropertyMetadata("Arial", OnRenderSettingsChanged, CoerceScaleValue));

        public static readonly DependencyProperty HasErrorProperty = DependencyProperty.Register(
            "HasError", typeof(bool), typeof(FormulaControl),
            new PropertyMetadata(false));

        public static readonly DependencyProperty ErrorsProperty = DependencyProperty.Register(
            "Errors", typeof(ObservableCollection<Exception>), typeof(FormulaControl),
            new PropertyMetadata(new ObservableCollection<Exception>()));

        public static readonly DependencyProperty ErrorTemplateProperty = DependencyProperty.Register(
            "ErrorTemplate", typeof(ControlTemplate), typeof(FormulaControl),
            new PropertyMetadata(new ControlTemplate()));

        public static readonly DependencyProperty SelectionStartProperty = DependencyProperty.Register(
            "SelectionStart", typeof(int), typeof(FormulaControl),
            new PropertyMetadata(0, OnRenderSettingsChanged));

        public static readonly DependencyProperty SelectionLengthProperty = DependencyProperty.Register(
            "SelectionLength", typeof(int), typeof(FormulaControl),
            new PropertyMetadata(0, OnRenderSettingsChanged));

        public static readonly DependencyProperty SelectionBrushProperty = DependencyProperty.Register(
            "SelectionBrush", typeof(Brush), typeof(FormulaControl),
            new PropertyMetadata(null, OnRenderSettingsChanged));

        public FormulaControl()
        {
            InitializeComponent();
        }

        private void Render()
        {
            // Create formula object from input text.
            if (texFormula == null)
            {
                formulaContainerElement.Visual = null;
                return;
            }

            // Render formula to visual.
            var visual = new DrawingVisual();
            var renderer = texFormula.GetRenderer(TexStyle.Display, Scale, SystemTextFontName);

            // Render selection.
            var selectionBrush = SelectionBrush;
            if (selectionBrush != null)
            {
                var start = SelectionStart;
                var end = start + SelectionLength;
                var dic = FormulaUtils.GetVisibleBoxes(renderer.Box, Scale, 0, 0,
                    box => box.Source != null && box.Source.Start < end && start < box.Source.Start + box.Source.Length);
                foreach (var box in dic.Keys)
                {
                    box.Background = Brushes.Aquamarine;
                }
            }

            using (var drawingContext = visual.RenderOpen())
            {
                renderer.Render(drawingContext, 0, 0);
            }
            formulaContainerElement.Visual = visual;
        }

        private static object CoerceFormula(DependencyObject d, object baseValue)
        {
            var control = (FormulaControl)d;
            var formula = (string)baseValue;
            try
            {
                control.HasError = false;
                control.Errors.Clear();
                control.texFormula = formulaParser.Parse(formula);
                return baseValue;
            }
            catch (TexException e)
            {
                control.SetError(e);
                return "";
            }
        }

        private static object CoerceScaleValue(DependencyObject d, object baseValue)
        {
            var val = (double)baseValue;
            return val < 1d ? 1d : val;
        }

        private static void OnRenderSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (FormulaControl)d;
            try
            {
                control.Render();
            }
            catch (TexException exception)
            {
                control.SetError(exception);
            }
        }

        private void SetError(TexException exception)
        {
            Errors.Add(exception);
            HasError = true;
            texFormula = null;
        }
    }
}
