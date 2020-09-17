using LiveCharts;
using LiveCharts.Wpf;
using OpusMTInterface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FiskmoMTEngine
{
    /// <summary>
    /// Interaction logic for TranslateWindow.xaml
    /// </summary>
    public partial class CustomizationProgressWindow : Window
    {
        
        private MTModel model;

        private LineSeries ScoresToSeries(IEnumerable<FileInfo> scoreFiles, string title)
        {
            var series = new LineSeries {
                Title = title,
                Values = new ChartValues<double>()
            };

            foreach (FileInfo file in scoreFiles)
            {
                try
                {
                    using (var reader = new StreamReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.None)))
                    {
                        var allText = reader.ReadToEnd();
                        var trimmed = allText.TrimEnd('\r', '\n');
                        var score = double.Parse(trimmed, CultureInfo.InvariantCulture);
                        series.Values.Add(score);
                    }
                }
                catch (IOException)
                {
                    //the file is unavailable because it is:
                    //still being written to
                    //or being processed by another thread
                    //or does not exist (has already been processed1,2,3,4,5)
                }
            }

           
            return series;
        }

        public CustomizationProgressWindow(MTModel selectedModel)
        {
            this.DataContext = this;
            

            this.Model = selectedModel;
            this.Title = $"Customization progress for model {Model.Name}";

            this.SeriesCollection = new SeriesCollection();

            var inDomainFiles = Directory.GetFiles(this.Model.InstallDir, "valid*_1.score.txt").Select(x => new FileInfo(x)).OrderBy(x => x.CreationTime);
            var inDomainSeries = this.ScoresToSeries(inDomainFiles, "In-domain");
            this.SeriesCollection.Add(inDomainSeries);

            var outOfDomainFiles = Directory.GetFiles(this.Model.InstallDir, "valid*_0.score.txt").Select(x => new FileInfo(x)).OrderBy(x => x.CreationTime); ;
            var outOfDomainSeries = this.ScoresToSeries(outOfDomainFiles, "Out-of-domain");
            this.SeriesCollection.Add(outOfDomainSeries);

            InitializeComponent();

        }

        public MTModel Model { get => model; set => model = value; }

        public SeriesCollection SeriesCollection { get; set; }
        public string[] Labels { get; set; }
        public Func<double, string> YFormatter { get; set; }

    }
}
