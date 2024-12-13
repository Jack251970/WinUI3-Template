using System.Numerics;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using Windows.Foundation;
using Windows.UI;

namespace WinUI3Demo.UserControls;

/// <summary>
/// An editable line chart with tooltip and without axis labels.
/// Codes are edited from: https://github.com/mitch344/WinUi3Charts.
/// </summary>
public sealed partial class LineChart : UserControl
{
    #region Properties

    public List<Vector2> ItemsSource
    {
        get => (List<Vector2>)GetValue(ItemsSourceProperty) ?? [];
        set => SetValue(ItemsSourceProperty, value);
    }

    public float? XAxisMin
    {
        get => (float?)GetValue(XAxisMinProperty);
        set => SetValue(XAxisMinProperty, value);
    }

    public float? XAxisMax
    {
        get => (float?)GetValue(XAxisMaxProperty);
        set => SetValue(XAxisMaxProperty, value);
    }

    public float? YAxisMin
    {
        get => (float?)GetValue(YAxisMinProperty);
        set => SetValue(YAxisMinProperty, value);
    }

    public float? YAxisMax
    {
        get => (float?)GetValue(YAxisMaxProperty);
        set => SetValue(YAxisMaxProperty, value);
    }

    #endregion

    #region Dependency Properties

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(List<Vector2>), typeof(LineChart), new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty XAxisMinProperty =
        DependencyProperty.Register(nameof(XAxisMin), typeof(float?), typeof(LineChart), new PropertyMetadata(null, OnAppearanceChanged));

    public static readonly DependencyProperty XAxisMaxProperty =
        DependencyProperty.Register(nameof(XAxisMax), typeof(float?), typeof(LineChart), new PropertyMetadata(null, OnAppearanceChanged));

    public static readonly DependencyProperty YAxisMinProperty =
        DependencyProperty.Register(nameof(YAxisMin), typeof(float?), typeof(LineChart), new PropertyMetadata(null, OnAppearanceChanged));

    public static readonly DependencyProperty YAxisMaxProperty =
        DependencyProperty.Register(nameof(YAxisMax), typeof(float?), typeof(LineChart), new PropertyMetadata(null, OnAppearanceChanged));

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var chart = d as LineChart;
        chart?.UpdateChart();
    }

    private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var chart = d as LineChart;
        chart?.UpdateChart();
    }

    #endregion

    private readonly Canvas _canvas;
    private readonly CanvasControl _canvasControl;

    public LineChart()
    {
        // theme resources
        ToolTipContentThemeFontSize = (double)Application.Current.Resources["ToolTipContentThemeFontSize"];
        ToolTipBorderPadding = (Thickness)Application.Current.Resources["ToolTipBorderPadding"];

        InitializeComponent();

        // ui elemtns
        _canvas = ChartCanvas;
        _canvasControl = ChartCanvasControl;

        // events
        ActualThemeChanged += LineChart_ActualThemeChanged;
        Loaded += LineChart_Loaded;
        Unloaded += LineChart_Unloaded;
        SizeChanged += LineChart_SizeChanged;
    }

    #region Resources

    // TODO: Use SurfaceStrokeColorFlyoutBrush.

    private SolidColorBrush TextFillColorPrimaryBrush = null!;

    private SolidColorBrush GridLineBrush = null!;

    private const float DataLineThickness = 3.5f;

    private Color DataLineColor;

    private const float DataPointEdgeSize = 12f;

    private const float DataPointOutterSize = 10.5f;

    private const float DataPointInnerSize = 5.5f;

    private const float DataPointInnerSizeDragged = 4f;

    private Color DataPointEdgeColor;

    private Color DataPointOutterColor;

    private Color DataPointInnerColor;

    private readonly double ToolTipContentThemeFontSize;

    private readonly float ToolTipRectangleCornerRadius = 4f;

    private readonly Thickness ToolTipBorderPadding;

    private readonly float ToolTipRectangleBorderThickness = 1f;

    private SolidColorBrush ToolTipRectangleBrush = null!;

    private SolidColorBrush ToolTipRectangleBorderBrush = null!;

    private void InitializeResource(ElementTheme theme)
    {
        if (theme == ElementTheme.Light)
        {
            TextFillColorPrimaryBrush = new SolidColorBrush(Color.FromArgb(255, 26, 26, 26));
            GridLineBrush = new SolidColorBrush(Color.FromArgb(255, 126, 126, 126));
            DataLineColor = Color.FromArgb(255, 14, 106, 186);
            DataPointEdgeColor = Color.FromArgb(255, 232, 232, 232);
            DataPointOutterColor = Color.FromArgb(255, 255, 255, 255);
            DataPointInnerColor = Color.FromArgb(255, 26, 118, 198);
            ToolTipRectangleBrush = new SolidColorBrush(Color.FromArgb(255, 246, 246, 246));
            ToolTipRectangleBorderBrush = new SolidColorBrush(Color.FromArgb(255, 214, 214, 214));
        }
        else
        {
            TextFillColorPrimaryBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            GridLineBrush = new SolidColorBrush(Color.FromArgb(255, 155, 155, 155));
            DataLineColor = Color.FromArgb(255, 84, 190, 245);
            DataPointEdgeColor = Color.FromArgb(255, 52, 52, 52);
            DataPointOutterColor = Color.FromArgb(255, 71, 71, 71);
            DataPointInnerColor = Color.FromArgb(255, 75, 180, 236);
            ToolTipRectangleBrush = new SolidColorBrush(Color.FromArgb(255, 50, 50, 50));
            ToolTipRectangleBorderBrush = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30));
        }
    }

    #endregion

    #region UserControl Event

    private void LineChart_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateChart();
        _canvasControl.PointerEntered += ChartCanvasControl_PointerEntered;
        _canvasControl.PointerExited += ChartCanvasControl_PointerExited;
        _canvasControl.PointerPressed += ChartCanvasControl_PointerPressed;
        _canvasControl.PointerMoved += ChartCanvasControl_PointerMoved;
        _canvasControl.PointerReleased += ChartCanvasControl_PointerReleased;
    }

    private void LineChart_Unloaded(object sender, RoutedEventArgs e)
    {
        _canvasControl.PointerEntered -= ChartCanvasControl_PointerEntered;
        _canvasControl.PointerExited -= ChartCanvasControl_PointerExited;
        _canvasControl.PointerPressed -= ChartCanvasControl_PointerPressed;
        _canvasControl.PointerMoved -= ChartCanvasControl_PointerMoved;
        _canvasControl.PointerReleased -= ChartCanvasControl_PointerReleased;
    }

    private void LineChart_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        UpdateChart();
    }

    private void LineChart_ActualThemeChanged(FrameworkElement sender, object args)
    {
        UpdateChart();
    }

    #endregion

    #region Pointer Events

    private List<Vector2> _itemSource = null!;
    private double _dataPointDragTolerance;

    private bool _isDragging = false;
    private int _draggingPointIndex = -1;
    private double _divX;
    private double _divY;

    private int _enteringPointIndex = -1;
    private double _enteringPointerX;
    private double _enteringPointerY;

    private void ChartCanvasControl_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        _enteringPointIndex = -1;
    }

    private void ChartCanvasControl_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        var pointerPos = e.GetCurrentPoint(_canvasControl).Position;
        var x = pointerPos.X;
        var y = pointerPos.Y;

        for (var i = 0; i < _itemSource.Count; i++)
        {
            var itemX = ScaleX(_itemSource[i].X);
            var itemY = ScaleY(_itemSource[i].Y);
            var divX = itemX - x;
            var divY = itemY - y;
            if (divX * divX + divY * divY < _dataPointDragTolerance)
            {
                _enteringPointIndex = i;
                _enteringPointerX = x;
                _enteringPointerY = y;
                _canvasControl.Invalidate();
                return;
            }
        }

        _enteringPointIndex = -1;
    }

    private void ChartCanvasControl_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var pointerPos = e.GetCurrentPoint(_canvasControl).Position;
        var x = pointerPos.X;
        var y = pointerPos.Y;

        for (var i = 0; i < _itemSource.Count; i++)
        {
            var itemX = ScaleX(_itemSource[i].X);
            var itemY = ScaleY(_itemSource[i].Y);
            var divX = itemX - x;
            var divY = itemY - y;
            if (divX * divX + divY * divY < _dataPointDragTolerance)
            {
                _divX = divX;
                _divY = divY;
                _isDragging = true;
                _draggingPointIndex = i;
                break;
            }
        }
    }

    private void ChartCanvasControl_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        var pointerPos = e.GetCurrentPoint(_canvasControl).Position;
        var x = pointerPos.X;
        var y = pointerPos.Y;

        var needRedraw = false;

        // check entering point for tooltip
        for (var i = 0; i < _itemSource.Count; i++)
        {
            var itemX = ScaleX(_itemSource[i].X);
            var itemY = ScaleY(_itemSource[i].Y);
            var divX = itemX - x;
            var divY = itemY - y;
            if (divX * divX + divY * divY < _dataPointDragTolerance)
            {
                _enteringPointIndex = i;
                _enteringPointerX = x;
                _enteringPointerY = y;
                needRedraw = true;
            }
        }

        if ((!needRedraw) && _enteringPointIndex != -1)
        {
            _enteringPointIndex = -1;
            needRedraw = true;
        }

        // check dragging pointer for dragging data points
        if (_isDragging)
        {
            // handle x
            var iX = InverseScaleX(x + _divX);
            if (iX < _xMin)
            {
                iX = _xMin;
            }
            else if (iX > _xMax)
            {
                iX = _xMax;
            }

            // handle y
            var iY = InverseScaleY(y + _divY);
            if (iY < _yMin)
            {
                iY = _yMin;
            }
            else if (iY > _yMax)
            {
                iY = _yMax;
            }

            // handle list
            var newVector = new Vector2((float)iX, (float)iY);
            _itemSource[_draggingPointIndex] = newVector;
            for (var i = _draggingPointIndex - 1; i >= 0; i--)
            {
                var needBreak = true;
                if (_itemSource[i].X > newVector.X)
                {
                    _itemSource[i] = new Vector2(newVector.X, _itemSource[i].Y);
                    needBreak = false;
                }
                if (_itemSource[i].Y > newVector.Y)
                {
                    _itemSource[i] = new Vector2(_itemSource[i].X, newVector.Y);
                    needBreak = false;
                }
                if (needBreak)
                {
                    break;
                }
            }

            for (var i = _draggingPointIndex + 1; i < _itemSource.Count; i++)
            {
                var needBreak = true;
                if (_itemSource[i].X < newVector.X)
                {
                    _itemSource[i] = new Vector2(newVector.X, _itemSource[i].Y);
                    needBreak = false;
                }
                if (_itemSource[i].Y < newVector.Y)
                {
                    _itemSource[i] = new Vector2(_itemSource[i].X, newVector.Y);
                    needBreak = false;
                }
                if (needBreak)
                {
                    break;
                }
            }

            // redraw
            needRedraw = true;
        }

        if (needRedraw)
        {
            _canvasControl.Invalidate();
        }
    }

    private void ChartCanvasControl_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_isDragging)
        {
            _isDragging = false;
            _draggingPointIndex = -1;
            _canvasControl.Invalidate();
            SetValue(ItemsSourceProperty, _itemSource);
        }
    }

    #endregion

    #region Draw Chart

    private const float ToolTipMargin = 40;

    private bool _isInitialized;
    private bool _axisInitialized;

    public void ForceUpdateChart()
    {
        UpdateChart(true);
    }

    private void UpdateChart(bool force = false)
    {
        if (!force)
        {
            if (!_isInitialized)
            {
                if (IsLoaded)
                {
                    _isInitialized = true;
                }
                else
                {
                    return;
                }
            }
        }

        if (ItemsSource != null)
        {
            // Cache data
            _itemSource = [.. ItemsSource];
            _xMin = XAxisMin ?? _itemSource.Min(v => v.X);
            _xMax = XAxisMax ?? _itemSource.Max(v => v.X);
            _yMin = YAxisMin ?? _itemSource.Min(v => v.Y);
            _yMax = YAxisMax ?? _itemSource.Max(v => v.Y);
            _dataPointDragTolerance = (DataPointOutterSize + 3) * (DataPointOutterSize + 3);
            
            // Initialize resources
            InitializeResource(ActualTheme);

            // Draw chart
            _canvasControl.Invalidate();
        }
    }

    private void ChartCanvasControl_Draw(CanvasControl sender, CanvasDrawEventArgs args)
    {
        DrawChart(args.DrawingSession);
    }

    private void DrawChart(CanvasDrawingSession session)
    {
        // Get chart size
        _chartWidth = _canvasControl.ActualWidth;
        _chartHeight = _canvasControl.ActualHeight;

        // Draw axis
        // Note: We should only draw axises once so that they will not be blurred.
        if (!_axisInitialized)
        {
            DrawAxis();

            _axisInitialized = true;
        }

        // Draw data
        DrawData(session);
    }

    private void DrawAxis()
    {
        DrawXAxis();
        DrawYAxis();
    }

    private void DrawData(CanvasDrawingSession session)
    {
        // Draw data lines
        for (var i = 0; i < _itemSource.Count - 1; i++)
        {
            var ele1 = _itemSource[i];
            var x1 = ScaleX(ele1.X);
            var y1 = ScaleY(ele1.Y);
            var vector1 = new Vector2((float)x1, (float)y1);
            var ele2 = _itemSource[i + 1];
            var x2 = ScaleX(ele2.X);
            var y2 = ScaleY(ele2.Y);
            var vector2 = new Vector2((float)x2, (float)y2);
            session.DrawLine(vector1, vector2, DataLineColor, DataLineThickness);
        }

        // Drag data points
        for (var i = 0; i < _itemSource.Count; i++)
        {
            var ele = _itemSource[i];
            var x = ScaleX(ele.X);
            var y = ScaleY(ele.Y);
            var vector = new Vector2((float)x, (float)y);
            session.FillCircle(vector, DataPointEdgeSize, DataPointEdgeColor);
            session.FillCircle(vector, DataPointOutterSize, DataPointOutterColor);
            session.FillCircle(vector, _isDragging && i == _draggingPointIndex ? DataPointInnerSizeDragged : DataPointInnerSize, DataPointInnerColor);
        }

        // Draw tooltip
        if (_enteringPointIndex != -1)
        {
            // Create tooltip
            var tooltipItem = _itemSource[_enteringPointIndex];
            var tooltip = $"{tooltipItem.X}, {tooltipItem.Y}";

            // Calculate position: center in the entering pointer
            var textFormat = new CanvasTextFormat
            {
                FontSize = (float)ToolTipContentThemeFontSize
            };

            // Measure text layout
            using var textLayout = new CanvasTextLayout(session, tooltip, textFormat, (float)_chartWidth, (float)_chartHeight);
            var textWidth = textLayout.LayoutBounds.Width;
            var textHeight = textLayout.LayoutBounds.Height;
            var textX = _enteringPointerX - textWidth / 2;
            var textY = _enteringPointerY - ToolTipMargin;
            
            // Calculate rectangle bounds with padding
            var rectX = textX - ToolTipBorderPadding.Left;
            var rectY = textY - ToolTipBorderPadding.Top;
            var rectWidth = textWidth + ToolTipBorderPadding.Left + ToolTipBorderPadding.Right;
            var rectHeight = textHeight + ToolTipBorderPadding.Top + ToolTipBorderPadding.Bottom;

            // Calculate border rectangle bounds
            var borderRectX = rectX - ToolTipRectangleBorderThickness;
            var borderRectY = rectY - ToolTipRectangleBorderThickness;
            var borderRectWidth = rectWidth + 2 * ToolTipRectangleBorderThickness;
            var borderRectHeight = rectHeight + 2 * ToolTipRectangleBorderThickness;

            // Adjust bounds
            var top = borderRectY;
            var bottom = borderRectY + borderRectHeight;
            var left = borderRectX;
            var right = borderRectX + borderRectWidth;
            if (right > _chartWidth)
            {
                var div = right - _chartWidth;
                textX -= div;
                rectX -= div;
                borderRectX -= div;
                left -= div;
            }
            if (left < 0)
            {
                var div = -left;
                textX += div;
                rectX += div;
                borderRectX += div;
            }
            if (bottom > _chartHeight)
            {
                var div = bottom - _chartHeight;
                textY -= div;
                rectY -= div;
                borderRectY -= div;
                top -= div;
            }
            if (top < 0)
            {
                var div = -top;
                textY += div;
                rectY += div;
                borderRectY += div;
            }

            // Draw border rectangle
            var borderRect = new Rect(borderRectX, borderRectY, borderRectWidth, borderRectHeight);
            session.FillRoundedRectangle(borderRect, ToolTipRectangleCornerRadius, ToolTipRectangleCornerRadius, ToolTipRectangleBorderBrush.Color);

            // Draw rectangle
            var rect = new Rect(rectX, rectY, rectWidth, rectHeight);
            session.FillRoundedRectangle(rect, ToolTipRectangleCornerRadius, ToolTipRectangleCornerRadius, ToolTipRectangleBrush.Color);

            // Draw text
            var tooltipVector = new Vector2((float)textX, (float)textY);
            session.DrawText(tooltip, tooltipVector, TextFillColorPrimaryBrush.Color, textFormat);
        }
    }

    #endregion

    #region Axis

    private double _chartWidth;
    private double _chartHeight;

    private float _xMin;
    private float _xMax;
    private float _yMin;
    private float _yMax;

    private const double _leftPadding = 40;
    private const double _rightPadding = 40;
    private const double _topPadding = 30;
    private const double _bottomPadding = 30;

    private const double LineThickness = 2.0;
    private const double GridLineThinkness = 1.0;
    private const double TickLength = 5.0;
    private const double XTickLabelInterval = 2.0;
    private const double YTickLabelInterval = 10.0;

    #region Draw Axis

    private void DrawXAxis()
    {
        // get steps
        var xSteps = CalculateAxisSteps(_xMin, _xMax, _chartWidth - _leftPadding - _rightPadding);
        if (xSteps[^1] != _xMax)
        {
            xSteps[^1] = _xMax;
        }

        // draw grid line
        foreach (var step in xSteps.Skip(1))
        {
            DrawGridLine(ScaleX(step), _topPadding, ScaleX(step), _chartHeight - _bottomPadding);
        }

        // draw axis line
        DrawAxisLine(_leftPadding, _chartHeight - _bottomPadding, _chartWidth - _rightPadding, _chartHeight - _bottomPadding);

        // draw axis ticks & labels
        DrawXAxisTicksAndLabels(xSteps);
    }

    private void DrawYAxis()
    {
        // get steps
        var ySteps = CalculateAxisSteps(_yMin, _yMax, _chartHeight - _topPadding - _bottomPadding);
        if (ySteps[^1] != _yMax)
        {
            ySteps[^1] = _yMax;
        }

        // draw grid line
        foreach (var step in ySteps.Skip(1))
        {
            DrawGridLine(_leftPadding, ScaleY(step), _chartWidth - _rightPadding, ScaleY(step));
        }

        // draw axis line
        DrawAxisLine(_leftPadding, _topPadding, _leftPadding, _chartHeight - _bottomPadding);

        // draw axis ticks & labels
        DrawYAxisTicksAndLabels(ySteps);
    }

    private void DrawAxisLine(double x1, double y1, double x2, double y2)
    {
        var axis = new Line
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Stroke = TextFillColorPrimaryBrush,
            StrokeThickness = LineThickness
        };
        _canvas.Children.Add(axis);
    }

    private void DrawGridLine(double x1, double y1, double x2, double y2)
    {
        var grid = new Line
        {
            X1 = x1,
            Y1 = y1,
            X2 = x2,
            Y2 = y2,
            Stroke = GridLineBrush,
            StrokeThickness = GridLineThinkness
        };
        _canvas.Children.Add(grid);
    }

    private void DrawXAxisTicksAndLabels(List<float> steps)
    {
        foreach (var step in steps)
        {
            var pos = ScaleX(step);
            if (pos >= _leftPadding && pos <= _chartWidth - _rightPadding)
            {
                // draw tick line
                var tickLine = new Line
                {
                    X1 = pos,
                    Y1 = _chartHeight - _bottomPadding,
                    X2 = pos,
                    Y2 = _chartHeight - _bottomPadding + TickLength,
                    Stroke = TextFillColorPrimaryBrush,
                    StrokeThickness = LineThickness
                };
                _canvas.Children.Add(tickLine);

                // draw tick label
                var label = new TextBlock
                {
                    Text = FormatAxisLabel(step),
                    FontSize = 10
                };

                label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var labelDesiredWidth = label.DesiredSize.Width;
                var labelLeft = Math.Max(0, Math.Min(_chartWidth - labelDesiredWidth, pos - labelDesiredWidth / 2));

                Canvas.SetLeft(label, labelLeft);
                Canvas.SetTop(label, _chartHeight - _bottomPadding + TickLength + XTickLabelInterval);

                _canvas.Children.Add(label);
            }
        }
    }

    private void DrawYAxisTicksAndLabels(List<float> steps)
    {
        foreach (var step in steps)
        {
            var pos = ScaleY(step);
            if (pos >= 0 && pos <= _chartHeight - _bottomPadding)
            {
                // draw tick line
                var tickLine = new Line
                {
                    X1 = _leftPadding,
                    Y1 = pos,
                    X2 = _leftPadding - TickLength,
                    Y2 = pos,
                    Stroke = TextFillColorPrimaryBrush,
                    StrokeThickness = LineThickness
                };
                _canvas.Children.Add(tickLine);

                // draw tick label
                var label = new TextBlock
                {
                    Text = FormatAxisLabel(step),
                    FontSize = 10
                };

                label.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                var labelDesiredHeight = label.DesiredSize.Height;
                var labelTop = Math.Max(0, Math.Min(_chartHeight - labelDesiredHeight, pos - labelDesiredHeight / 2));

                Canvas.SetLeft(label, _leftPadding - TickLength - label.DesiredSize.Width - YTickLabelInterval);
                Canvas.SetTop(label, labelTop);

                _canvas.Children.Add(label);
            }
        }
    }

    private static List<float> CalculateAxisSteps(float min, float max, double chartSize)
    {
        var range = (double)max - (double)min;
        var pixelsPerStep = 30;
        var numberOfSteps = Math.Max(2, Math.Min(10, (int)(chartSize / pixelsPerStep)));

        if (range == 0)
        {
            return [min - 1, min, min + 1];
        }

        var isIntegerRange = Math.Abs((double)min - Math.Round((double)min)) < 1e-10 &&
                              Math.Abs((double)max - Math.Round((double)max)) < 1e-10;

        if (isIntegerRange && range <= 10)
        {
            return Enumerable.Range((int)Math.Floor(min), (int)Math.Ceiling(max - min) + 1)
                .Select(x => (float)x)
                .ToList();
        }

        var stepSize = NiceNumber(range / (numberOfSteps - 1), true);
        var niceMin = Math.Floor((double)min / stepSize) * stepSize;
        var niceMax = Math.Ceiling((double)max / stepSize) * stepSize;

        var steps = new List<float>();
        for (var step = niceMin; step <= niceMax + stepSize / 2; step += stepSize)
        {
            steps.Add((float)step);
        }

        if (steps.Count < 2)
        {
            steps.Insert(0, (float)(niceMin - stepSize));
            steps.Add((float)(niceMax + stepSize));
        }

        if (steps.All(s => Math.Abs((double)s - Math.Round((double)s)) < 1e-10))
        {
            return steps.Select(s => (float)Math.Round(s)).Distinct().ToList();
        }

        return steps;
    }

    private static double NiceNumber(double range, bool round)
    {
        if (range == 0)
        {
            return 1;
        }

        var exponent = Math.Floor(Math.Log10(range));
        var fraction = range / Math.Pow(10, exponent);
        double niceFraction;

        if (round)
        {
            if (fraction < 1.5)
            {
                niceFraction = 1;
            }
            else if (fraction < 3)
            {
                niceFraction = 2;
            }
            else if (fraction < 7)
            {
                niceFraction = 5;
            }
            else
            {
                niceFraction = 10;
            }
        }
        else
        {
            if (fraction <= 1)
            {
                niceFraction = 1;
            }
            else if (fraction <= 2)
            {
                niceFraction = 2;
            }
            else if (fraction <= 5)
            {
                niceFraction = 5;
            }
            else
            {
                niceFraction = 10;
            }
        }

        return niceFraction * Math.Pow(10, exponent);
    }

    private static string FormatAxisLabel(float value)
    {
        if (Math.Abs((double)value) < 1e-15)
        {
            return "0";
        }
        else if (Math.Abs((double)value) < 0.01 || Math.Abs((double)value) >= 1e6)
        {
            return ((double)value).ToString("E2");
        }
        else if (Math.Abs((double)value - Math.Round((double)value)) < 1e-10)
        {
            return Math.Round((double)value).ToString("F0");
        }
        else
        {
            return ((double)value).ToString("G4");
        }
    }

    #endregion

    #region Convert Methods

    private double ScaleX(float x)
    {
        return _leftPadding + ((double)x - _xMin) / ((double)_xMax - _xMin) * (_chartWidth - _leftPadding - _rightPadding);
    }

    private double ScaleY(float y)
    {
        return _chartHeight - _bottomPadding - ((double)y - _yMin) / ((double)_yMax - _yMin) * (_chartHeight - _topPadding - _bottomPadding);
    }

    private double InverseScaleX(double x)
    {
        return (x - _leftPadding) / (_chartWidth - _leftPadding - _rightPadding) * ((double)_xMax - _xMin) + _xMin;
    }

    private double InverseScaleY(double y)
    {
        return (_chartHeight - _bottomPadding - y) / (_chartHeight - _topPadding - _bottomPadding) * ((double)_yMax - _yMin) + _yMin;
    }

    #endregion

    #endregion
}
