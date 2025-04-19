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
            LargeSeries = CreateSeries();
            SmallSeries1 = CreateSeries();
            SmallSeries2 = CreateSeries();
            SmallSeries3 = CreateSeries();
            SmallSeries4 = CreateSeries();

            DataContext = this;
        }

        private readonly (string Name, Color Color)[] MetricDefs = new[]
        {
            ("Total Fitness", Colors.Red),
            ("Correctness", Colors.Green),
            ("Coverage", Colors.Blue),
            ("Uniqueness", Colors.Purple),
            ("Valid Avidity", Colors.Yellow),
            ("Invalid Avidity", Colors.Orange)
        };


        private ObservableCollection<ISeries> CreateSeries()
        {
            var seriesCollection = new ObservableCollection<ISeries>();

            foreach (var (name, color) in MetricDefs)
            {
                seriesCollection.Add(new LineSeries<double>
                {
                    Values = new ObservableCollection<double> { 0 },
                    Name = name,
                    Stroke = new SolidColorPaint(new SKColor(color.R, color.G, color.B)),
                    Fill = null, 
                    GeometrySize = 4 
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
