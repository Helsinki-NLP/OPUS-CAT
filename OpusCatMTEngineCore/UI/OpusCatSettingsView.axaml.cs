using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using OpusCatMtEngine;
using OpusCatMtEngine.UI;
using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace OpusCatMtEngine;

public partial class OpusCatSettingsView : Avalonia.Controls.UserControl, INotifyPropertyChanged
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

    


    public OpusCatSettingsView()
    {
        InitializeComponent();
        this.Loaded += SettingsControl_Loaded;
    }


    private void SettingsControl_Loaded(object? sender, RoutedEventArgs e)
    {
        
        this.Loaded -= SettingsControl_Loaded;

        this.ServicePortBox = OpusCatMtEngineSettings.Default.MtServicePort;
        this.HttpServicePortBox = OpusCatMtEngineSettings.Default.HttpMtServicePort;
        this.StoreDataInAppdata = OpusCatMtEngineSettings.Default.StoreOpusCatDataInLocalAppdata;
        this.DatabaseRemovalInterval = OpusCatMtEngineSettings.Default.DatabaseRemovalInterval.ToString();
        this.UseDatabaseRemoval = OpusCatMtEngineSettings.Default.UseDatabaseRemoval;
        this.CacheMtInDatabase = OpusCatMtEngineSettings.Default.CacheMtInDatabase;
        this.DisplayOverlay = OpusCatMtEngineSettings.Default.DisplayOverlay;
        this.MaxLength = OpusCatMtEngineSettings.Default.MaxLength.ToString();
        NotifyPropertyChanged("SaveButtonEnabled");
    }

    public async void OpenCustomSettingsInEditor_Click(object sender, RoutedEventArgs e)
    {
        var customizeYml = HelperFunctions.GetOpusCatDataPath(OpusCatMtEngineSettings.Default.CustomizationBaseConfig);
        try
        {
#if WINDOWS
            Process.Start(new ProcessStartInfo("cmd", $"/c start {customizeYml}"));
#elif LINUX
            Process.Start("xdg-open", customizeYml);
#elif MACOS
            Process.Start("open", $"\"{ customizeYml}\"");
#endif
        }
        catch (Exception ex)
        {
            string messageBoxText = "Could not open customize.yml in a text editor.";
            var box = MessageBoxManager.GetMessageBoxStandard(
                "Cannot open editor",
                messageBoxText,
                ButtonEnum.Ok);
            var result = await box.ShowAsync();
        }
    }


    private void saveButton_Click(object sender, RoutedEventArgs e)
    {
        OpusCatMtEngineSettings.Default.MtServicePort = this.ServicePortBox;
        OpusCatMtEngineSettings.Default.HttpMtServicePort = this.HttpServicePortBox;
        OpusCatMtEngineSettings.Default.StoreOpusCatDataInLocalAppdata = this.StoreDataInAppdata;
        OpusCatMtEngineSettings.Default.DatabaseRemovalInterval = Int32.Parse(this.DatabaseRemovalInterval);
        OpusCatMtEngineSettings.Default.MaxLength = Int32.Parse(this.MaxLength);
        if (OpusCatMtEngineSettings.Default.CacheMtInDatabase != this.CacheMtInDatabase)
        {
            OpusCatMtEngineSettings.Default.CacheMtInDatabase = this.CacheMtInDatabase;
            //This checks whether the option can be enabled.
            TranslationDbHelper.SetupTranslationDb();
            this.CacheMtInDatabase = OpusCatMtEngineSettings.Default.CacheMtInDatabase;
        }
        OpusCatMtEngineSettings.Default.UseDatabaseRemoval = this.UseDatabaseRemoval;
        if (OpusCatMtEngineSettings.Default.DisplayOverlay != this.DisplayOverlay)
        {
            OpusCatMtEngineSettings.Default.DisplayOverlay = this.DisplayOverlay;
            if (this.DisplayOverlay)
            {
                App.OpenOverlay();
            }
            else
            {
                App.CloseOverlay();
            }
        }

        OpusCatMtEngineSettings.Default.Save();

        NotifyPropertyChanged("SaveButtonEnabled");
    }

    /*
    private void PreviewNumberInput(object? sender, TextCompositionEventArgs e)
    {
        Regex regex = new Regex("[^0-9]");
        e.Handled = regex.IsMatch(e.Text);
    }*/


    private string? httpServicePortBox;

    [Required]
    [StringRangeAttribute(1025, 65536, ErrorMessage="Port number should be 1025 - 65536")]
    public string HttpServicePortBox
    {
        get => httpServicePortBox;
        set
        {
            httpServicePortBox = value.Replace("_", "");
            NotifyPropertyChanged();
            NotifyPropertyChanged("SaveButtonEnabled");
        }
    }

    private string? databaseRemovalInterval;

    [StringRangeAttribute(1,365,ErrorMessage = "Value must be 1-365")]
    public string DatabaseRemovalInterval
    {
        get => databaseRemovalInterval;
        set
        {
            databaseRemovalInterval = value;
            NotifyPropertyChanged();
            NotifyPropertyChanged("SaveButtonEnabled");
        }
    }

    public bool StoreDataInAppdata
    {
        get => _storeDataInAppdata;
        set
        {
            _storeDataInAppdata = value;
            NotifyPropertyChanged();
            NotifyPropertyChanged("SaveButtonEnabled");
        }
    }

    public bool DisplayOverlay
    {
        get => _displayOverlay;
        set
        {
            _displayOverlay = value;
            NotifyPropertyChanged();
            NotifyPropertyChanged("SaveButtonEnabled");
        }
    }

    public bool CacheMtInDatabase
    {
        get => _cacheMtInDatabase;
        set
        {
            _cacheMtInDatabase = value;
            NotifyPropertyChanged();
            NotifyPropertyChanged("SaveButtonEnabled");
        }
    }

    public bool UseDatabaseRemoval
    {
        get => useDatabaseRemoval;
        set
        {
            useDatabaseRemoval = value;
            NotifyPropertyChanged();
            NotifyPropertyChanged("SaveButtonEnabled");
        }
    }
    private string servicePortBox;

    [Required]
    [StringRangeAttribute(1025,65536,ErrorMessage = "Port number should be 1025 - 65536")]
    public string ServicePortBox
    {
        get => servicePortBox;
        set
        {
            servicePortBox = value.Replace("_","");
            NotifyPropertyChanged();
            NotifyPropertyChanged("SaveButtonEnabled");
        }
    }


    public string MaxLength
    {
        get => maxLength;
        set
        {
            maxLength = value;
            NotifyPropertyChanged();
            NotifyPropertyChanged("SaveButtonEnabled");
        }
    }

    public bool SaveButtonEnabled
    {
        get
        {

            bool allSettingsDefault =
                this.ServicePortBox == OpusCatMtEngineSettings.Default.MtServicePort &&
                this.HttpServicePortBox == OpusCatMtEngineSettings.Default.HttpMtServicePort &&
                this.StoreDataInAppdata == OpusCatMtEngineSettings.Default.StoreOpusCatDataInLocalAppdata &&
                this.DatabaseRemovalInterval == OpusCatMtEngineSettings.Default.DatabaseRemovalInterval.ToString() &&
                this.UseDatabaseRemoval == OpusCatMtEngineSettings.Default.UseDatabaseRemoval &&
                this.CacheMtInDatabase == OpusCatMtEngineSettings.Default.CacheMtInDatabase &&
                this.DisplayOverlay == OpusCatMtEngineSettings.Default.DisplayOverlay &&
                this.MaxLength == OpusCatMtEngineSettings.Default.MaxLength.ToString();


            foreach (var tBox in this.GetVisualDescendants().OfType<TextBox>())
            {
                if (DataValidationErrors.GetHasErrors(tBox))
                {
                    return false;
                }
            }

            return !allSettingsDefault;
        }
    }

    private bool _storeDataInAppdata;
    private bool httpServicePortBoxIsValid;
    private bool servicePortBoxIsValid;
    private bool _cacheMtInDatabase;
    private bool useDatabaseRemoval;
    private bool _displayOverlay;
    private string? maxLength;


    private void revertToDefaultsButton_Click(object sender, RoutedEventArgs e)
    {
        OpusCatMtEngineSettings.Default.Reset();
        OpusCatMtEngineSettings.Default.Save();
        this.ServicePortBox = OpusCatMtEngineSettings.Default.MtServicePort;
        this.HttpServicePortBox = OpusCatMtEngineSettings.Default.HttpMtServicePort;
        this.StoreDataInAppdata = OpusCatMtEngineSettings.Default.StoreOpusCatDataInLocalAppdata;
        this.DatabaseRemovalInterval = OpusCatMtEngineSettings.Default.DatabaseRemovalInterval.ToString();
        this.UseDatabaseRemoval = OpusCatMtEngineSettings.Default.UseDatabaseRemoval;
        this.CacheMtInDatabase = OpusCatMtEngineSettings.Default.CacheMtInDatabase;
        this.MaxLength = OpusCatMtEngineSettings.Default.MaxLength.ToString();
    }

    
}
