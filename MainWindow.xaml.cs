

using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LiveChartsCore.SkiaSharpView.WPF;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Input;
// no additional defaults used

namespace MonProjetGraphiques
{
    public class FinancialPoint
    {
        public DateTime Date { get; set; }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }

        public FinancialPoint(DateTime date, double open, double high, double low, double close, double volume = 0)
        {
            Date = date;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
        }
    }

    public partial class MainWindow : Window
    {
        // Simple Gantt task model
        public class GanttTask
        {
            public string Label { get; set; }
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
            public Brush Fill { get; set; }
            public GanttTask(string label, DateTime start, DateTime end, Brush fill = null)
            {
                Label = label; Start = start; End = end; Fill = fill ?? Brushes.SteelBlue;
            }
        }

        private List<GanttTask> GanttTasks = new List<GanttTask>();
        public List<string> FinancialDates { get; set; }
        public Func<double, string> DateFormatter { get; set; }

        public IEnumerable<ISeries> LineSeries { get; set; } = Array.Empty<ISeries>();
        public IEnumerable<ISeries> ColumnSeries { get; set; } = Array.Empty<ISeries>();
        public IEnumerable<ISeries> BarSeries { get; set; } = Array.Empty<ISeries>();
        public IEnumerable<ISeries> StepLineSeries { get; set; } = Array.Empty<ISeries>();
        public IEnumerable<ISeries> ScatterSeries { get; set; } = Array.Empty<ISeries>();
        public IEnumerable<ISeries> PieSeries { get; set; } = Array.Empty<ISeries>();
        public IEnumerable<ISeries> FinancialSeries { get; set; } = Array.Empty<ISeries>();
        public IEnumerable<ISeries> FinancialCloseSeries { get; set; } = Array.Empty<ISeries>();
        public string FinancialDebugText { get; set; } = string.Empty;
        public IEnumerable<ISeries> RadarSeries { get; set; } = Array.Empty<ISeries>();
        public IEnumerable<ISeries> HeatmapSeries { get; set; } = Array.Empty<ISeries>();
        public IEnumerable<ISeries> AreaSeries { get; set; } = Array.Empty<ISeries>();

        // Candle styling: make candles look like typical Forex candles
        // Filled green for bullish, filled red for bearish, thin black wicks and borders
        private readonly Brush CandleUpFill = new SolidColorBrush(Color.FromArgb(0xFF, 0x00, 0xB0, 0x50)); // forex green
        private readonly Brush CandleDownFill = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x33, 0x33)); // forex red
        private readonly Brush CandleUpStroke = Brushes.Black;
        private readonly Brush CandleDownStroke = Brushes.Black;
        private readonly Brush WickBrush = Brushes.Black;
        private bool HollowBullish = false; // keep bullish filled (not hollow)

        // Interaction state for zoom/pan
        private double scaleX = 1.0; // horizontal zoom
        private double panOffset = 0.0; // horizontal pan in pixels
        private bool isPanning = false;
        private double lastPanX = 0.0;
        // UI toggles
        private bool showCloseLine = false;
        // Manual bar values for the horizontal bar chart
        private double[] BarValues;

        public MainWindow()
        {
            InitializeComponent();
            

            LineSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = new double[] { 3, 5, 7, 4, 6, 9, 2 },
                    Fill = null,
                    Stroke = new SolidColorPaint(SKColors.Blue),
                    GeometrySize = 8
                }
            };

            ColumnSeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = new double[] { 3, 5, 7, 4, 6, 9, 2 },
                    Fill = new SolidColorPaint(SKColors.Green)
                }
            };

            // Keep a valid BarSeries (fallback) for bindings; we'll draw a horizontal bar chart manually on BarCanvas.
            BarSeries = new ISeries[]
            {
                new ColumnSeries<double>
                {
                    Values = new double[] { 4, 6, 3, 8, 5, 7, 2 },
                    Fill = new SolidColorPaint(SKColors.OrangeRed)
                }
            };

            // Values used for the manual horizontal bar chart drawing
            BarValues = new double[] { 4, 6, 3, 8, 5, 7, 2 };

            StepLineSeries = new ISeries[]
            {
                new StepLineSeries<double>
                {
                    Values = new double[] { 3, 5, 7, 4, 6, 9, 2 },
                    Stroke = new SolidColorPaint(SKColors.Purple),
                    Fill = null
                }
            };

            ScatterSeries = new ISeries[]
            {
                new ScatterSeries<double>
                {
                    Values = new double[] { 3, 5, 7, 4, 6, 9, 2 },
                    Fill = new SolidColorPaint(SKColors.Orange),
                    GeometrySize = 10
                }
            };

            PieSeries = new ISeries[]
            {
                new PieSeries<double>
                {
                    Values = new double[] { 3 },
                    Fill = new SolidColorPaint(SKColors.Yellow),
                    Stroke = new SolidColorPaint(SKColors.Black)
                    // StrokeThickness n'existe pas, donc retirée
                },
                new PieSeries<double>
                {
                    Values = new double[] { 5 },
                    Fill = new SolidColorPaint(SKColors.Red),
                    Stroke = new SolidColorPaint(SKColors.Black)
                },
                new PieSeries<double>
                {
                    Values = new double[] { 7 },
                    Fill = new SolidColorPaint(SKColors.Blue),
                    Stroke = new SolidColorPaint(SKColors.Black)
                },
                new PieSeries<double>
                {
                    Values = new double[] { 4 },
                    Fill = new SolidColorPaint(SKColors.Green),
                    Stroke = new SolidColorPaint(SKColors.Black)
                },
                new PieSeries<double>
                {
                    Values = new double[] { 6 },
                    Fill = new SolidColorPaint(SKColors.Pink),
                    Stroke = new SolidColorPaint(SKColors.Black)
                },
                new PieSeries<double>
                {
                    Values = new double[] { 9 },
                    Fill = new SolidColorPaint(SKColors.Cyan),
                    Stroke = new SolidColorPaint(SKColors.Black)
                },
            };

            var candlesticksSeries = new CandlesticksSeries<FinancialPoint>
            {
                Values = new List<FinancialPoint>
                {
                    new FinancialPoint(DateTime.Now.AddDays(-6), 100, 110, 90, 105, 1200),
                    new FinancialPoint(DateTime.Now.AddDays(-5), 105, 115, 95, 110, 800),
                    new FinancialPoint(DateTime.Now.AddDays(-4), 110, 120, 100, 115, 1500),
                    // Make this point bearish (close < open) so we see a red candle
                    new FinancialPoint(DateTime.Now.AddDays(-3), 125, 130, 105, 110, 600),
                    new FinancialPoint(DateTime.Now.AddDays(-2), 120, 130, 110, 125, 2000),
                    new FinancialPoint(DateTime.Now.AddDays(-1), 125, 135, 115, 130, 1700),
                    new FinancialPoint(DateTime.Now, 130, 140, 120, 135, 900)
                }
            };

            // Build a simple list of date strings for the X axis labels from the FinancialPoint.Date values.
            var values = candlesticksSeries.Values ?? Enumerable.Empty<FinancialPoint>();
            FinancialDates = values.Select(p => p.Date.ToString("dd/MM")).ToList();

            // Also add a simple line series (middle of candle) so we can visually confirm data renders.
            var avgValues = values.Select(p => (p.Open + p.Close) / 2.0).ToArray();
            var closeValues = values.Select(p => p.Close).ToArray();

            FinancialSeries = new ISeries[] { candlesticksSeries };

            // A simpler close-price line series to guarantee something renders
            FinancialCloseSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = closeValues,
                    Stroke = new SolidColorPaint(SKColors.DarkBlue),
                    GeometrySize = 6,
                    Fill = null
                }
            };

            FinancialDebugText = $"Points: {values.Count()}  — Close sample: {string.Join(", ", closeValues.Take(5))}{(closeValues.Length>5?", ...":"")}";

            // DateFormatter maps the X axis value (index) to the corresponding date string.
            DateFormatter = value =>
            {
                var idx = (int)Math.Round(value);
                if (idx < 0) idx = 0;
                if (idx >= FinancialDates.Count) idx = FinancialDates.Count - 1;
                return FinancialDates.Count > 0 ? FinancialDates[idx] : string.Empty;
            };

            RadarSeries = new ISeries[]
            {
                new PolarLineSeries<double>
                {
                    Values = new double[] { 3, 5, 7, 4, 6, 9, 2 },
                    Fill = new SolidColorPaint(SKColors.Cyan),
                    Stroke = new SolidColorPaint(SKColors.DarkBlue)
                }
            };

            // Pour HeatSeries, aplatis le tableau 2D en liste 1D
            var heatValues2D = new double[,]
            {
                { 1, 2, 3, 4 },
                { 5, 6, 7, 8 },
                { 9, 10, 11, 12 }
            };
            var heatValuesFlattened = heatValues2D.Cast<double>().ToList();

            HeatmapSeries = new ISeries[]
            {
                new HeatSeries<double>
                {
                    Values = heatValuesFlattened
                }
            };

            AreaSeries = new ISeries[]
            {
                new LineSeries<double>
                {
                    Values = new double[] { 3, 5, 7, 4, 6, 9, 2 },
                    Fill = new SolidColorPaint(SKColors.LightBlue),
                    Stroke = new SolidColorPaint(SKColors.Blue)
                }
            };

            DataContext = this;

            // Configure manual drawing and interactions on the FinancialCanvas and FinancialVolumeCanvas
            if (FinancialCanvas != null)
            {
                FinancialCanvas.SizeChanged += (s, e) => DrawCandles();
                FinancialCanvas.MouseWheel += FinancialCanvas_MouseWheel;
                FinancialCanvas.MouseLeftButtonDown += FinancialCanvas_MouseLeftButtonDown;
                FinancialCanvas.MouseLeftButtonUp += FinancialCanvas_MouseLeftButtonUp;
                FinancialCanvas.MouseMove += FinancialCanvas_MouseMove;
                FinancialCanvas.Focusable = true;
            }

            if (BarCanvas != null)
            {
                BarCanvas.SizeChanged += (s, e) => DrawBarChart();
            }

            // Wire the checkboxes that toggle rendering modes (if present)
            if (ShowCloseCheckBox != null)
            {
                ShowCloseCheckBox.Checked += (s, e) => { showCloseLine = true; DrawCandles(); };
                ShowCloseCheckBox.Unchecked += (s, e) => { showCloseLine = false; DrawCandles(); };
                ShowCloseCheckBox.IsChecked = showCloseLine;
            }

            // ShowVolumesCheckBox removed: volumes are not drawn by default and feature removed

            Loaded += (s, e) => { DrawCandles(); DrawBarChart(); };

            // Prepare simple sample Gantt tasks
            GanttTasks = new List<GanttTask>
            {
                new GanttTask("Planification", DateTime.Today.AddDays(-10), DateTime.Today.AddDays(-6), Brushes.MediumSeaGreen),
                new GanttTask("Développement", DateTime.Today.AddDays(-5), DateTime.Today.AddDays(2), Brushes.SteelBlue),
                new GanttTask("Tests", DateTime.Today.AddDays(3), DateTime.Today.AddDays(7), Brushes.OrangeRed),
                new GanttTask("Déploiement", DateTime.Today.AddDays(8), DateTime.Today.AddDays(9), Brushes.Purple)
            };

            // Wire Gantt canvas events if present
            if (GanttCanvas != null)
            {
                GanttCanvas.SizeChanged += (s, e) => DrawGantt();
                Loaded += (s, e) => DrawGantt();
            }
        }

        private void DrawGantt()
        {
            if (GanttCanvas == null) return;
            GanttCanvas.Children.Clear();

            if (GanttTasks == null || GanttTasks.Count == 0) return;

            DateTime min = GanttTasks.Min(t => t.Start);
            DateTime max = GanttTasks.Max(t => t.End);
            double totalDays = Math.Max(1, (max - min).TotalDays + 1);

            double leftLabelWidth = 140;
            double pxPerDay = 28; // pixels per day
            double canvasWidth = leftLabelWidth + totalDays * pxPerDay + 40;
            GanttCanvas.Width = Math.Max(GanttCanvas.ActualWidth, canvasWidth);

            double y = 10;
            double rowHeight = 32;

            // Draw time axis (top)
            for (int d = 0; d <= totalDays; d++)
            {
                DateTime day = min.AddDays(d);
                double x = leftLabelWidth + d * pxPerDay;
                var tick = new Line { X1 = x, X2 = x, Y1 = 0, Y2 = GanttCanvas.Height, Stroke = new SolidColorBrush(Color.FromArgb(0x22,0,0,0)), StrokeThickness = 0.5 };
                GanttCanvas.Children.Add(tick);

                if (d % Math.Max(1, (int)Math.Ceiling(totalDays / 8.0)) == 0)
                {
                    var lbl = new TextBlock { Text = day.ToString("dd/MM"), FontSize = 11 };
                    Canvas.SetLeft(lbl, x + 2);
                    Canvas.SetTop(lbl, 2);
                    GanttCanvas.Children.Add(lbl);
                }
            }

            // Draw tasks
            for (int i = 0; i < GanttTasks.Count; i++)
            {
                var t = GanttTasks[i];
                double startOffset = (t.Start - min).TotalDays;
                double dur = Math.Max(0.5, (t.End - t.Start).TotalDays + 1);
                double x = leftLabelWidth + startOffset * pxPerDay;
                double width = dur * pxPerDay;

                var rect = new Rectangle
                {
                    Width = Math.Max(4, width),
                    Height = rowHeight * 0.6,
                    Fill = t.Fill,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.6,
                    RadiusX = 3,
                    RadiusY = 3
                };
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y + i * rowHeight + 8);
                rect.ToolTip = $"{t.Label}\n{t.Start:dd/MM} → {t.End:dd/MM}";
                GanttCanvas.Children.Add(rect);

                var lbl = new TextBlock { Text = t.Label, FontSize = 12 };
                Canvas.SetLeft(lbl, 6);
                Canvas.SetTop(lbl, y + i * rowHeight + 6);
                GanttCanvas.Children.Add(lbl);
            }
        }

        private void DrawBarChart()
        {
            if (BarCanvas == null) return;
            BarCanvas.Children.Clear();

            if (BarValues == null || BarValues.Length == 0) return;

            double w = BarCanvas.ActualWidth;
            double h = BarCanvas.ActualHeight;
            double leftPadding = 80;
            double top = 8;
            double rowHeight = Math.Max(18, h / (double)BarValues.Length);
            double maxVal = BarValues.Max();

            for (int i = 0; i < BarValues.Length; i++)
            {
                double val = BarValues[i];
                double barWidth = (w - leftPadding - 16) * (val / Math.Max(1.0, maxVal));

                var rect = new Rectangle
                {
                    Width = Math.Max(2, barWidth),
                    Height = rowHeight * 0.6,
                    Fill = Brushes.OrangeRed,
                    Stroke = Brushes.Black,
                    StrokeThickness = 0.6
                };
                Canvas.SetLeft(rect, leftPadding);
                Canvas.SetTop(rect, top + i * rowHeight + (rowHeight * 0.2));
                BarCanvas.Children.Add(rect);

                var lbl = new TextBlock { Text = val.ToString(), FontSize = 12 };
                Canvas.SetLeft(lbl, leftPadding + barWidth + 6);
                Canvas.SetTop(lbl, top + i * rowHeight + (rowHeight * 0.15));
                BarCanvas.Children.Add(lbl);

                var name = new TextBlock { Text = "Item " + (i + 1), FontSize = 12 };
                Canvas.SetLeft(name, 6);
                Canvas.SetTop(name, top + i * rowHeight + (rowHeight * 0.15));
                BarCanvas.Children.Add(name);
            }
        }

        private void FinancialCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            // Zoom horizontally around mouse position
            var pos = e.GetPosition(FinancialCanvas);
            double oldScale = scaleX;
            double factor = e.Delta > 0 ? 1.12 : 1 / 1.12;
            scaleX = Math.Clamp(scaleX * factor, 0.4, 6.0);

            // Adjust panOffset so that the point under the cursor stays approximately the same
            double mouseX = pos.X;
            panOffset = mouseX - (mouseX - panOffset) * (scaleX / oldScale);
            DrawCandles();
        }

        private void FinancialCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isPanning = true;
            lastPanX = e.GetPosition(FinancialCanvas).X;
            FinancialCanvas.CaptureMouse();
        }

        private void FinancialCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isPanning = false;
            FinancialCanvas.ReleaseMouseCapture();
        }

        private void FinancialCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isPanning) return;
            var p = e.GetPosition(FinancialCanvas);
            double dx = p.X - lastPanX;
            lastPanX = p.X;
            panOffset += dx;
            DrawCandles();
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            scaleX = 1.0;
            panOffset = 0.0;
            DrawCandles();
        }

        private void DrawCandles()
        {
            if (FinancialCanvas == null) return;
            FinancialCanvas.Children.Clear();

            var candlesticksSeries = FinancialSeries.FirstOrDefault() as CandlesticksSeries<FinancialPoint>;
            var values = candlesticksSeries?.Values ?? Enumerable.Empty<FinancialPoint>();
            var pts = values.ToList();
            if (!pts.Any()) return;

            double w = FinancialCanvas.ActualWidth;
            double h = FinancialCanvas.ActualHeight;
            // Reserve left padding for Y axis labels and bottom padding for date labels
            double leftPadding = 60;
            double bottomPadding = 18;
            double usableWidth = Math.Max(10, w - leftPadding - 10);
            double usableHeight = Math.Max(10, h - bottomPadding - 10);
            if (w <= 0 || h <= 0) return;

            double xmax = pts.Count - 1;
            double ymin = pts.Min(p => p.Low);
            double ymax = pts.Max(p => p.High);
            double padding = (ymax - ymin) * 0.1;
            ymin -= padding; ymax += padding;

            // Candle width scales with available space and number of points (account for zoom)
            double candleWidth = Math.Max(4, Math.Min(usableWidth / (pts.Count * 1.5), 40));
            // Apply horizontal scale
            candleWidth = Math.Max(2, candleWidth * scaleX);

            for (int i = 0; i < pts.Count; i++)
            {
                var p = pts[i];
                // Map index to X inside usableWidth, offset by leftPadding
                double x = leftPadding + (usableWidth * scaleX) * (i / (double)Math.Max(1, pts.Count - 1)) + panOffset;
                double yHigh = 5 + ((ymax - p.High) / (ymax - ymin) * usableHeight);
                double yLow = 5 + ((ymax - p.Low) / (ymax - ymin) * usableHeight);
                double yOpen = 5 + ((ymax - p.Open) / (ymax - ymin) * usableHeight);
                double yClose = 5 + ((ymax - p.Close) / (ymax - ymin) * usableHeight);

                var isUp = p.Close >= p.Open;

                // Wick
                var wick = new Line
                {
                    X1 = x,
                    X2 = x,
                    Y1 = yHigh,
                    Y2 = yLow,
                    Stroke = isUp ? CandleUpStroke : CandleDownStroke,
                    StrokeThickness = 1
                };
                // Add tooltip on wick
                wick.ToolTip = $"{p.Date:dd/MM/yyyy}\nO:{p.Open} H:{p.High} L:{p.Low} C:{p.Close}";
                FinancialCanvas.Children.Add(wick);

                // Body (narrower body with small gap — typical forex candle look)
                double top = Math.Min(yOpen, yClose);
                double bottom = Math.Max(yOpen, yClose);
                double bodyWidth = Math.Max(1, candleWidth * 0.7);
                var rect = new Rectangle
                {
                    Width = bodyWidth,
                    Height = Math.Max(1, bottom - top),
                    Fill = (isUp && HollowBullish) ? Brushes.Transparent : (isUp ? CandleUpFill : CandleDownFill),
                    Stroke = CandleUpStroke,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(rect, x - bodyWidth / 2);
                Canvas.SetTop(rect, top);
                // Attach tooltip to body as well
                rect.ToolTip = $"{p.Date:dd/MM/yyyy}\nO:{p.Open} H:{p.High} L:{p.Low} C:{p.Close}";
                FinancialCanvas.Children.Add(rect);

                // Date labels are drawn at the bottom of the chart if needed.
            }

            // Draw Y axis ticks and horizontal grid lines
            int yTicks = 5;
            for (int t = 0; t <= yTicks; t++)
            {
                double v = ymin + (ymax - ymin) * (t / (double)yTicks);
                double yy = 5 + ((ymax - v) / (ymax - ymin) * usableHeight);
                var line = new Line
                {
                    X1 = leftPadding,
                    X2 = leftPadding + usableWidth,
                    Y1 = yy,
                    Y2 = yy,
                    Stroke = new SolidColorBrush(Color.FromArgb(0x44, 0, 0, 0)),
                    StrokeThickness = 0.6
                };
                FinancialCanvas.Children.Add(line);
                var lbl = new TextBlock { Text = FormatPrice(v), FontSize = 11 };
                Canvas.SetLeft(lbl, 4);
                Canvas.SetTop(lbl, yy - 8);
                FinancialCanvas.Children.Add(lbl);
            }

            // Draw close-price polyline for confirmation (toggleable)
            if (showCloseLine)
            {
                var closePoints = new PointCollection();
                for (int i = 0; i < pts.Count; i++)
                {
                    var p = pts[i];
                    double x = leftPadding + (usableWidth * scaleX) * (i / (double)Math.Max(1, pts.Count - 1)) + panOffset;
                    double yClose = 5 + ((ymax - p.Close) / (ymax - ymin) * usableHeight);
                    closePoints.Add(new System.Windows.Point(x, yClose));
                }
                var poly = new Polyline
                {
                    Stroke = Brushes.Navy,
                    StrokeThickness = 1.2,
                    Points = closePoints
                };
                FinancialCanvas.Children.Add(poly);
            }

            // Volumes drawing removed per request.
        }

        private string FormatPrice(double v)
        {
            double av = Math.Abs(v);
            if (av >= 1_000_000) return (v / 1_000_000d).ToString("0.##") + "M";
            if (av >= 1000) return (v / 1000d).ToString("0.#") + "K";
            return v.ToString("0.##");
        }
    }

    // Suppression des classes inutiles pour BarSeries générique
}
