using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using OpusCatMtEngine;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace OpusCatMtEngine;

public partial class OpusCatSettingsView : Avalonia.Controls.UserControl, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

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

    public void OpenCustomSettingsInEditor_Click(object sender, RoutedEventArgs e)
    {
        var customizeYml = HelperFunctions.GetLocalAppDataPath(OpusCatMtEngineSettings.Default.CustomizationBaseConfig);
        Process.Start("notepad.exe", customizeYml);
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

    public string this[string columnName]
    {
        get
        {
            return Validate(columnName);
        }
    }


    private string? httpServicePortBox;
    public string HttpServicePortBox
    {
        get => httpServicePortBox;
        set
        {
            httpServicePortBox = value;
            NotifyPropertyChanged();
            NotifyPropertyChanged("SaveButtonEnabled");
        }
    }

    private string? databaseRemovalInterval;
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

    private string? servicePortBox;
    public string ServicePortBox
    {
        get => servicePortBox;
        set
        {
            servicePortBox = value;
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

            return !allSettingsDefault && !this.validationErrors;
        }
    }

    private bool _storeDataInAppdata;
    private bool httpServicePortBoxIsValid;
    private bool servicePortBoxIsValid;
    private bool _cacheMtInDatabase;
    private bool validationErrors;
    private bool useDatabaseRemoval;
    private bool _displayOverlay;
    private string? maxLength;

    public string Error
    {
        get { return "...."; }
    }



    private string Validate(string propertyName)
    {
        // Return error message if there is error on else return empty or null string
        string validationMessage = string.Empty;
        this.validationErrors = false;
        switch (propertyName)
        {
            case "MaxLength":
                if (!String.IsNullOrEmpty(this.MaxLength))
                {
                    var length = Int32.Parse(this.MaxLength);
                    if (length == 0)
                    {
                        validationMessage = "Error";
                        this.validationErrors = true;
                    }
                }
                else
                {
                    validationMessage = "Error";
                    this.validationErrors = true;
                }
                break;
            case "DatabaseRemovalInterval":
                if (!String.IsNullOrEmpty(this.DatabaseRemovalInterval))
                {
                    var interval = Int32.Parse(this.DatabaseRemovalInterval);
                    if (interval == 0)
                    {
                        validationMessage = "Error";
                        this.validationErrors = true;
                    }
                }
                else
                {
                    validationMessage = "Error";
                    this.validationErrors = true;
                }
                break;
            case "ServicePortBox":
                if (this.ServicePortBox != null && this.ServicePortBox != "")
                {
                    var portNumber = Int32.Parse(this.ServicePortBox);
                    if (portNumber < 1024 || portNumber > 65535)
                    {
                        validationMessage = "Error";
                        this.validationErrors = true;
                    }
                }
                else
                {
                    validationMessage = "Error";
                    this.validationErrors = true;
                }

                break;
            case "HttpServicePortBox":
                if (this.HttpServicePortBox != null && this.HttpServicePortBox != "")
                {
                    var portNumber = Int32.Parse(this.HttpServicePortBox);
                    if (portNumber < 1024 || portNumber > 65535)
                    {
                        validationMessage = "Error";
                        this.validationErrors = true;
                    }
                    else
                    {
                        this.httpServicePortBoxIsValid = true;
                    }
                }
                else
                {
                    validationMessage = "Error";
                    this.validationErrors = true;
                }

                break;
        }

        NotifyPropertyChanged("SaveButtonEnabled");
        return validationMessage;
    }

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
