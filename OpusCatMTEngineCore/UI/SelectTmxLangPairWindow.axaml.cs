using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System;
using System.Linq;

namespace OpusCatMtEngine;


public partial class SelectTmxLangPairWindow : Window, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    private IEnumerable<KeyValuePair<Tuple<string, string>, int>> _eligiblePairs;
    private KeyValuePair<Tuple<string, string>, int> _selectedPair;

    public SelectTmxLangPairWindow()
    {

    }
    public SelectTmxLangPairWindow(IEnumerable<KeyValuePair<Tuple<string, string>, int>> eligibleLangPairs)
    {
        this.DataContext = this;
        this.EligiblePairs = eligibleLangPairs;
        this.SelectedPair = this.EligiblePairs.First();
        InitializeComponent();
    }

    public IEnumerable<KeyValuePair<Tuple<string, string>, int>> EligiblePairs
    {
        get => _eligiblePairs;
        set
        {
            _eligiblePairs = value;
            NotifyPropertyChanged();
        }
    }
    public KeyValuePair<Tuple<string, string>, int> SelectedPair
    {
        get => _selectedPair;
        set
        {
            _selectedPair = value;
            NotifyPropertyChanged();
        }
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        this.Close(false);
    }

    private void UseSelected_Click(object sender, RoutedEventArgs e)
    {
        this.Close(true);
    }
}