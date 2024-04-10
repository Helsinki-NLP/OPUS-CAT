using Avalonia.Controls;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using Serilog;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using System.Linq;
using LiveChartsCore.Measure;
using System.Timers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LiveChartsCore.SkiaSharpView.Painting;
using Avalonia.Media;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.VisualElements;

namespace OpusCatMtEngine
{
    public partial class CustomizationProgressView : UserControl, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

        }

        private MTModel model;

        private IEnumerable<LineSeries<double, SVGPathGeometry>> ScoresToSeries(
            IEnumerable<FileInfo> scoreFiles,
            string title, string geometry=null)
        {

            Dictionary<string, List<double>> scoresValues = new Dictionary<string, List<double>>();

            foreach (FileInfo file in scoreFiles)
            {
                try
                {
                    using (var reader = new StreamReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.Write)))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var lineSplit = line.Split(':');
                            var metricName = lineSplit[0].Trim();
                            var metricScore = double.Parse(lineSplit[1].Trim(), CultureInfo.InvariantCulture);
                            if (scoresValues.ContainsKey(metricName))
                            {
                                scoresValues[metricName].Add(metricScore);
                            }
                            else
                            {
                                scoresValues[metricName] = new List<double>
                                {
                                    metricScore
                                };
                            }
                        }
                    }
                }
                catch (IOException ex)
                {
                    //the file is unavailable because it is:
                    //still being written to
                    //or being processed by another thread
                    //or does not exist (has already been processed1,2,3,4,5)
                    Log.Information($"Error in reading score file {file.Name}: {ex.Message}");
                }
                catch (FormatException ex)
                {
                    //Parsing the score file content as double may fail (possibly some problem with
                    //sacrebleu output or execution)
                    Log.Information($"Error in reading score file {file.Name}: {ex.Message}");
                }
            }

            List<LineSeries<double, SVGPathGeometry>> scoresSeries = new List<LineSeries<double, SVGPathGeometry>>();
            foreach (var scoreKey in scoresValues.Keys)
            {
                scoresSeries.Add(new LineSeries<double, SVGPathGeometry>()
                {
                    Values = scoresValues[scoreKey],
                    Name = $"{title} {scoreKey}",
                    GeometrySvg = geometry,
                    Fill = null
                });
            }

            

            return scoresSeries;
        }

        public void UpdateChart()
        {
            //The update may happen at the same time as the scores as being written, in that case
            //simply skip the update, it will run again soon
            try
            {
                this.SeriesCollection.Clear();
                var inDomainFiles = Directory.GetFiles(this.Model.InstallDir, "valid*_1.score.txt").Select(x => new FileInfo(x)).OrderBy(x => x.CreationTime);
                var inDomainSeries =
                    this.ScoresToSeries(
                        inDomainFiles,
                        Properties.Resources.Progress_InDomainSeriesName, SVGPoints.Square);
                this.SeriesCollection.AddRange(inDomainSeries);

                if (this.model.HasOODValidSet)
                {
                    var outOfDomainFiles = Directory.GetFiles(this.Model.InstallDir, "valid*_0.score.txt").Select(x => new FileInfo(x)).OrderBy(x => x.CreationTime); ;
                    var outOfDomainSeries =
                        this.ScoresToSeries(
                            outOfDomainFiles,
                            Properties.Resources.Progress_OutOfDomainSeriesName, SVGPoints.Circle);
                    this.SeriesCollection.AddRange(outOfDomainSeries);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public CustomizationProgressView()
        {

        }

        public CustomizationProgressView(MTModel selectedModel)
        {
            this.DataContext = this;

            this.Model = selectedModel;
            this.SeriesCollection = new List<LineSeries<double, SVGPathGeometry>>();

            if (this.Model.FinetuneProcess != null)
            {
                this.SetTimer();
            }
            this.Title = $"Fine-tuning progress for model {Model.Name}";

            this.UpdateChart();
            InitializeComponent();

            this.ProgressChart.YAxes = new List<Axis>
            {
                new Axis
                {
                    MinLimit = 0,
                    MaxLimit = 100,
                }
            };
            this.ProgressChart.XAxes = new List<Axis>
            {
                new Axis
                {
                    MinStep = 1,
                }
            };


        }

        private System.Timers.Timer aTimer;

        private void SetTimer()
        {
            // Create a timer with 60 second interval.
            aTimer = new System.Timers.Timer(60000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            this.UpdateChart();
            if (this.Model.ModelConfig.FinetuningComplete)
            {
                aTimer.Stop();
            }
        }

        private List<LineSeries<double, SVGPathGeometry>> seriesCollection;
        public MTModel Model { get => model; set => model = value; }
        public string Title { get; private set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public List<LineSeries<double, SVGPathGeometry>> SeriesCollection
        { 
            get => seriesCollection;
            set
            {
                seriesCollection = value;
                NotifyPropertyChanged();
            }
        }
    }
}

