using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace AISIGA.UI
{
    

    public partial class DashboardWindow : Window
    {
        public ObservableCollection<ISeries> LargeSeries { get; set; }
        public ObservableCollection<ISeries> SmallSeries1 { get; set; }
        public ObservableCollection<ISeries> SmallSeries2 { get; set; }
        public ObservableCollection<ISeries> SmallSeries3 { get; set; }
        public ObservableCollection<ISeries> SmallSeries4 { get; set; }

        public DashboardWindow()
        {
            InitializeComponent();

            // Setup series collections
            LargeSeries = CreateSeries(true);
            SmallSeries1 = CreateSeries(false);
            SmallSeries2 = CreateSeries(false);
            SmallSeries3 = CreateSeries(false);
            SmallSeries4 = CreateSeries(false);

            DataContext = this;
        }

        private readonly (string Name, Color Color)[] MetricDefs = new[]
        {
            ("Total Fitness", Colors.Blue),
            ("Correctness", Colors.Orange),
            ("Coverage", Colors.Purple),
            ("Uniqueness", Colors.Brown),
            ("Valid Avidity", Colors.Yellow),
            ("Invalid Avidity", Colors.Orange)
        };


        private ObservableCollection<ISeries> CreateSeries(bool main)
        {
            var seriesCollection = new ObservableCollection<ISeries>();

            foreach (var (name, color) in MetricDefs)
            {
                seriesCollection.Add(new LineSeries<double>
                {
                    Values = new ObservableCollection<double> { 0 },
                    Name = name,
                    Stroke = new SolidColorPaint(new SKColor(color.R, color.G, color.B)),
                    GeometryFill = new SolidColorPaint(new SKColor(color.R, color.G, color.B)), 
                    GeometrySize = 6,
                    GeometryStroke = new SolidColorPaint(new SKColor(color.R, color.G, color.B)), // Match border color to the line color
                    Fill = null
                });
            }

            if (main)
            {
                seriesCollection.Add(new LineSeries<double>
                {
                    Values = new ObservableCollection<double> { 0 },
                    Name = "Train Accuracy %",
                    Stroke = new SolidColorPaint(new SKColor(0, 255, 0)),
                    
                    GeometryFill = new SolidColorPaint(new SKColor(0, 255, 0)),
                    GeometrySize = 10,
                    GeometryStroke = new SolidColorPaint(new SKColor(0, 255, 0)),
                    Fill = null
                });
                seriesCollection.Add(new LineSeries<double>
                {
                    Values = new ObservableCollection<double> { 0 },
                    Name = "Test Accuracy %",
                    Stroke = new SolidColorPaint(new SKColor(255, 0, 0)),

                    GeometryFill = new SolidColorPaint(new SKColor(255, 0, 0)),
                    GeometrySize = 10,
                    GeometryStroke = new SolidColorPaint(new SKColor(255, 0, 0)),
                    Fill = null
                });
            }

            return seriesCollection;
        }


        public void AddToSeries(ObservableCollection<ISeries> seriesCollection, double[] newValues)
        {
            for (int i = 0; i < newValues.Length && i < seriesCollection.Count; i++)
            {
                if (seriesCollection[i] is LineSeries<double> line &&
                    line.Values is ObservableCollection<double> values)
                {
                    values.Add(newValues[i]);
                    if (values.Count > 50) values.RemoveAt(0); // Optional: trim old points
                }
            }
        }

    }
}
