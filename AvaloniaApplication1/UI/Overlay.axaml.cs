using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace OpusCatMtEngine;

public partial class Overlay : Window, INotifyPropertyChanged
{

    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public Overlay()
    {
        InitializeComponent();
        this.Show();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public string MtFontSize
    {
        get { return OpusCatMtEngineSettings.Default.OverlayFontsize.ToString(); }
        set
        {
            int intSize;
            var success = Int32.TryParse(value, out intSize);
            if (success)
            {
                OpusCatMtEngineSettings.Default.OverlayFontsize = Int32.Parse(value);
                OpusCatMtEngineSettings.Default.Save();
            }
            NotifyPropertyChanged();
        }
    }

    /*
    private void PreviewNumberInput(object sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]");
        e.Handled = regex.IsMatch(e.Text);
    }*/

    internal void ShowMessageInOverlay(string message)
    {
        /*TODO: implement with AvaloniaEdit?
        this.TranslationBox.Document.Blocks.Clear();
        this.TranslationBox.Document.Blocks.Add(new Paragraph(new Run(message)));
        */
    }

    internal void UpdateTranslation(TranslationPair result)
    {
        this.ShowMessageInOverlay(result.Translation);
    }

    internal void ClearTranslation()
    {
        //TODO
        //this.TranslationBox.Document.Blocks.Clear();
    }
}