using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using AISIGA.Program.AIS;
using LiveChartsCore;
using System.Collections.ObjectModel;
using LiveChartsCore.Defaults;

namespace AISIGA.UI
{
    /// <summary>
    /// Interaction logic for ResultsWindow.xaml
    /// </summary>
    public partial class ResultsWindow : Window
    {
        public ResultsWindow()
        {
            InitializeComponent();
        }

        public void ShowClassificationResults(List<Antigen> classifiedData, double accuracyPercentage)
        {
            // Set accuracy percentage at the top
            AccuracyPercentageText.Text = $"Accuracy: {accuracyPercentage}%";

            // Prepare the data for the plot
            var correctPoints = classifiedData.Where(dp => dp.GetActualClass() == dp.GetAssignedClass()).ToList();
            var incorrectPoints = classifiedData.Where(dp => dp.GetActualClass() != dp.GetAssignedClass()).ToList();


            // Calculate the min and max values for X and Y
            double minX = classifiedData.Min(dp => dp.GetFeatureValueAt(0));
            double maxX = classifiedData.Max(dp => dp.GetFeatureValueAt(0));
            double minY = classifiedData.Min(dp => dp.GetFeatureValueAt(1));
            double maxY = classifiedData.Max(dp => dp.GetFeatureValueAt(1));

            // Set dynamic axis limits with padding for better visibility
            Chart.XAxes = new List<Axis>
            {
                new Axis
                {
                    MinLimit = minX - 1,  // Padding
                    MaxLimit = maxX + 1,
                }
            };

            Chart.YAxes = new List<Axis>
            {
                new Axis
                {
                    MinLimit = minY - 1,  // Padding
                    MaxLimit = maxY + 1,
                }
            };



            //Create the series for correct and incorrect points

            var correctSeries = new ScatterSeries<ObservablePoint>
            {
                Values = GetObservablePoints(correctPoints),
                Fill = new SolidColorPaint(SKColors.Green),
                Stroke = new SolidColorPaint(SKColors.Green),
                GeometrySize = 5
            };

            var incorrectSeries = new ScatterSeries<ObservablePoint>
            {
                Values = GetObservablePoints(incorrectPoints),
                Fill = new SolidColorPaint(SKColors.Red),
                Stroke = new SolidColorPaint(SKColors.Red),
                GeometrySize = 5
            };


            // Ensure that Series is an ObservableCollection
            Chart.Series = new ObservableCollection<ISeries>
            {
                correctSeries,
                incorrectSeries
            };

            // Show the plot window
            Show();
        }

        private ObservableCollection<ObservablePoint> GetObservablePoints(List<Antigen> data)
        {
            return new ObservableCollection<ObservablePoint>(data.Select(dp => new ObservablePoint(dp.GetFeatureValueAt(0), dp.GetFeatureValueAt(1))));
        }
    }
}
