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

namespace OpusCatMtEngine
{
    public partial class CustomizationProgressView : UserControl
    {

        private MTModel model;

        private IEnumerable<LineSeries<double>> ScoresToSeries(
            IEnumerable<FileInfo> scoreFiles,
            string title,
            Geometry geometry)
        {

            Dictionary<string, List<double>> scoresValues = new Dictionary<string, List<double>>();

            foreach (FileInfo file in scoreFiles)
            {
                try
                {
                    using (var reader = new StreamReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.None)))
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

            List<LineSeries<double>> scoresSeries = new List<LineSeries<double>>();
            foreach (var scoreKey in scoresValues.Keys)
            {
                scoresSeries.Add(new LineSeries<double>()
                {
                    Values = scoresValues[scoreKey],
                    Name = $"{title} {scoreKey}"
                });
            }

            

            return scoresSeries;
        }

        public CustomizationProgressView()
        {

        }

        public CustomizationProgressView(MTModel selectedModel)
        {
            this.DataContext = this;

            this.SeriesCollection = new List<LineSeries<double>>();
            this.Model = selectedModel;
            this.Title = $"Fine-tuning progress for model {Model.Name}";

            var inDomainFiles = Directory.GetFiles(this.Model.InstallDir, "valid*_1.score.txt").Select(x => new FileInfo(x)).OrderBy(x => x.CreationTime);
            var inDomainSeries =
                this.ScoresToSeries(
                    inDomainFiles,
                    Properties.Resources.Progress_InDomainSeriesName, null);
            this.SeriesCollection.AddRange(inDomainSeries);

            if (this.model.HasOODValidSet)
            {
                var outOfDomainFiles = Directory.GetFiles(this.Model.InstallDir, "valid*_0.score.txt").Select(x => new FileInfo(x)).OrderBy(x => x.CreationTime); ;
                var outOfDomainSeries =
                    this.ScoresToSeries(
                        outOfDomainFiles,
                        Properties.Resources.Progress_OutOfDomainSeriesName,
                        null);
                this.SeriesCollection.AddRange(outOfDomainSeries);
            }
            
            InitializeComponent();
            
            this.ProgressChart.XAxes = new List<Axis>
            {
                new Axis
                {
                    MinStep = 1
                }
            };

        }

        private List<LineSeries<double>> seriesCollection;
        public MTModel Model { get => model; set => model = value; }
        public string Title { get; private set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }
        public List<LineSeries<double>> SeriesCollection { get => seriesCollection; set => seriesCollection = value; }
    }
}

