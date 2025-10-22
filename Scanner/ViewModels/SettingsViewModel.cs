﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Toolkit.Mvvm.Messaging;
using Scanner.Models;
using Scanner.Models.FileNaming;
using Scanner.Services;
using Scanner.Services.Messenger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Globalization;
using Windows.Storage;
using Windows.System;
using static Enums;
using static Utilities;

namespace Scanner.ViewModels
{
    public class SettingsViewModel : ObservableRecipient, IDisposable
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // DECLARATIONS /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Services
        public readonly IAccessibilityService AccessibilityService = Ioc.Default.GetService<IAccessibilityService>();
        public readonly ISettingsService SettingsService = Ioc.Default.GetRequiredService<ISettingsService>();
        private readonly IScanService ScanService = Ioc.Default.GetRequiredService<IScanService>();
        private readonly IHelperService HelperService = Ioc.Default.GetRequiredService<IHelperService>();
        public readonly IAppCenterService AppCenterService = Ioc.Default.GetService<IAppCenterService>();
        public readonly ILogService LogService = Ioc.Default.GetService<ILogService>();
        public readonly IAutoRotatorService AutoRotatorService = Ioc.Default.GetService<IAutoRotatorService>();
        #endregion

        #region Commands
        public RelayCommand DisposeCommand;
        public RelayCommand DisplayLogExportDialogCommand;
        public RelayCommand DisplayLicensesDialogCommand;
        public AsyncRelayCommand StoreRatingCommand;
        public AsyncRelayCommand ChooseSaveLocationCommand;
        public AsyncRelayCommand ResetSaveLocationCommand;
        public RelayCommand DisplayChangelogCommand;
        public RelayCommand ShowDonateDialogCommand;
        public RelayCommand<string> SetAutoRotateLanguageCommand;
        public AsyncRelayCommand LaunchLanguageSettingsCommand;
        public RelayCommand DisplayCustomFileNamingDialogCommand;
        #endregion

        #region Events
        public event EventHandler ChangelogRequested;
        public event EventHandler<SettingsSection> SettingsSectionRequested;
        public event EventHandler LogExportDialogRequested;
        public event EventHandler LicensesDialogRequested;
        public event EventHandler CustomFileNamingDialogRequested;
        #endregion

        private string _SaveLocationPath;
        public string SaveLocationPath
        {
            get => _SaveLocationPath;
            set => SetProperty(ref _SaveLocationPath, value);
        }

        private bool? _IsDefaultSaveLocation;
        public bool? IsDefaultSaveLocation
        {
            get => _IsDefaultSaveLocation;
            set => SetProperty(ref _IsDefaultSaveLocation, value);
        }

        public int SettingSaveLocationType
        {
            get => (int)SettingsService.GetSetting(AppSetting.SettingSaveLocationType);
            set => SettingsService.SetSetting(AppSetting.SettingSaveLocationType, value);
        }

        public int SettingAppTheme
        {
            get => (int)SettingsService.GetSetting(AppSetting.SettingAppTheme);
            set => SettingsService.SetSetting(AppSetting.SettingAppTheme, value);
        }

        public bool SettingAutoRotate
        {
            get => (bool)SettingsService.GetSetting(AppSetting.SettingAutoRotate);
            set => SettingsService.SetSetting(AppSetting.SettingAutoRotate, value);
        }

        public string SettingAutoRotateLanguage
        {
            get => (string)SettingsService.GetSetting(AppSetting.SettingAutoRotateLanguage);
            set => SettingsService.SetSetting(AppSetting.SettingAutoRotateLanguage, value);
        }

        public bool SettingAppendTime
        {
            get => (bool)SettingsService.GetSetting(AppSetting.SettingAppendTime);
            set => SettingsService.SetSetting(AppSetting.SettingAppendTime, value);
        }

        public int SettingScanAction
        {
            get => (int)SettingsService.GetSetting(AppSetting.SettingScanAction);
            set => SettingsService.SetSetting(AppSetting.SettingScanAction, value);
        }

        public int SettingEditorOrientation
        {
            get => (int)SettingsService.GetSetting(AppSetting.SettingEditorOrientation);
            set => SettingsService.SetSetting(AppSetting.SettingEditorOrientation, value);
        }

        public bool SettingRememberScanOptions
        {
            get => (bool)SettingsService.GetSetting(AppSetting.SettingRememberScanOptions);
            set => SettingsService.SetSetting(AppSetting.SettingRememberScanOptions, value);
        }

        public bool SettingShowAdvancedScanOptions
        {
            get => (bool)SettingsService.GetSetting(AppSetting.SettingShowAdvancedScanOptions);
            set => SettingsService.SetSetting(AppSetting.SettingShowAdvancedScanOptions, value);
        }

        public bool SettingErrorStatistics
        {
            get => (bool)SettingsService.GetSetting(AppSetting.SettingErrorStatistics);
            set => SettingsService.SetSetting(AppSetting.SettingErrorStatistics, value);
        }

        public bool SettingShowSurveys
        {
            get => (bool)SettingsService.GetSetting(AppSetting.SettingShowSurveys);
            set => SettingsService.SetSetting(AppSetting.SettingShowSurveys, value);
        }

        public bool SettingAnimations
        {
            get => (bool)SettingsService.GetSetting(AppSetting.SettingAnimations);
            set => SettingsService.SetSetting(AppSetting.SettingAnimations, value);
        }

        public int SettingMeasurementUnits
        {
            get => (int)SettingsService.GetSetting(AppSetting.SettingMeasurementUnits);
            set => SettingsService.SetSetting(AppSetting.SettingMeasurementUnits, value);
        }

        public string SettingAppLanguage
        {
            get => (string)SettingsService.GetSetting(AppSetting.SettingAppLanguage);
            set => SettingsService.SetSetting(AppSetting.SettingAppLanguage, value);
        }

        public int SettingFileNamingPattern
        {
            get => (int)SettingsService.GetSetting(AppSetting.SettingFileNamingPattern);
            set
            {
                SettingsService.SetSetting(AppSetting.SettingFileNamingPattern, value);
                RefreshFileNamingPatternPreviewResult();
            }
        }

        private string _FileNamingPatternPreviewResult;
        public string FileNamingPatternPreviewResult
        {
            get => _FileNamingPatternPreviewResult;
            set => SetProperty(ref _FileNamingPatternPreviewResult, value);
        }

        private bool _IsScanInProgress;
        public bool IsScanInProgress
        {
            get => _IsScanInProgress;
            set => SetProperty(ref _IsScanInProgress, value);
        }

        public string CurrentVersion => GetCurrentVersion();

        public List<string> AvailableAppLanguages = new List<string>();


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS / FACTORIES /////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public SettingsViewModel()
        {
            SettingsService.ScanSaveLocationChanged += SettingsService_ScanSaveLocationChanged;
            SettingsService.SettingChanged += SettingsService_SettingChanged;
            SaveLocationPath = SettingsService.ScanSaveLocation?.Path;
            IsDefaultSaveLocation = SettingsService.IsScanSaveLocationDefault;
            ScanService.ScanStarted += ScanService_ScanStartedOrCompleted;
            ScanService.ScanEnded += ScanService_ScanStartedOrCompleted;
            IsScanInProgress = ScanService.IsScanInProgress;
            WeakReferenceMessenger.Default.Register<SettingsRequestMessage>(this, (r, m) => SettingsRequestMessage_Received(r, m));

            DisposeCommand = new RelayCommand(Dispose);
            DisplayLogExportDialogCommand = new RelayCommand(DisplayLogExportDialog);
            DisplayLicensesDialogCommand = new RelayCommand(DisplayLicensesDialog);
            DisplayChangelogCommand = new RelayCommand(DisplayChangelog);
            StoreRatingCommand = new AsyncRelayCommand(DisplayStoreRatingDialogAsync);
            ChooseSaveLocationCommand = new AsyncRelayCommand(ChooseSaveLocation);
            ResetSaveLocationCommand = new AsyncRelayCommand(ResetSaveLocationAsync);
            ShowDonateDialogCommand = new RelayCommand(() => Messenger.Send(new DonateDialogRequestMessage()));
            SetAutoRotateLanguageCommand = new RelayCommand<string>((x) => SetAutoRotateLanguage(int.Parse(x)));
            LaunchLanguageSettingsCommand = new AsyncRelayCommand(LaunchLanguageSettings);
            DisplayCustomFileNamingDialogCommand = new RelayCommand(DisplayCustomFileNamingDialog);

            // prepare language list
            foreach (string languageString in ApplicationLanguages.ManifestLanguages)
            {
                AvailableAppLanguages.Add(languageString);
            }
            AvailableAppLanguages = AvailableAppLanguages.OrderBy((x) => new Language(x).DisplayName).ToList();
            AvailableAppLanguages.Insert(0, "SYSTEM");

            // prepare file naming preview
            RefreshFileNamingPatternPreviewResult();
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // METHODS //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void Dispose()
        {
            // clean up messenger
            Messenger.UnregisterAll(this);

            // clean up event handlers
            SettingsService.ScanSaveLocationChanged -= SettingsService_ScanSaveLocationChanged;
            ScanService.ScanStarted -= ScanService_ScanStartedOrCompleted;
            ScanService.ScanEnded -= ScanService_ScanStartedOrCompleted;
        }

        private void SettingsService_SettingChanged(object sender, AppSetting e)
        {
            switch (e)
            {
                case AppSetting.CustomFileNamingPattern:
                    RefreshFileNamingPatternPreviewResult();
                    break;
                default:
                    break;
            }
        }

        private void SettingsRequestMessage_Received(object r, SettingsRequestMessage m)
        {
            SettingsSectionRequested?.Invoke(this, m.SettingsSection);
        }

        private void DisplayLogExportDialog()
        {
            LogService?.Log.Information("DisplayLogExportDialog");
            LogExportDialogRequested?.Invoke(this, EventArgs.Empty);
        }

        private void DisplayLicensesDialog()
        {
            LogService?.Log.Information("DisplayLicensesDialog");
            LicensesDialogRequested?.Invoke(this, EventArgs.Empty);
        }

        private void DisplayCustomFileNamingDialog()
        {
            LogService?.Log.Information("DisplayCustomFileNamingDialog");
            CustomFileNamingDialogRequested?.Invoke(this, EventArgs.Empty);
        }

        private async Task DisplayStoreRatingDialogAsync()
        {
            await RunOnUIThreadAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                await HelperService.ShowRatingDialogAsync();
            });
        }

        private async Task ChooseSaveLocation()
        {
            if (ChooseSaveLocationCommand.IsRunning) return;    // already running?
            LogService?.Log.Information("ChooseSaveLocation");

            // prepare folder picker
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            folderPicker.FileTypeFilter.Add("*");

            // pick folder and check it
            StorageFolder folder;
            try
            {
                folder = await folderPicker.PickSingleFolderAsync();
                if (folder == null) return;
            }
            catch (Exception exc)
            {
                LogService?.Log.Warning(exc, "Picking a new save location failed.");
                Messenger.Send(new AppWideStatusMessage
                {
                    Title = LocalizedString("ErrorMessagePickFolderHeading"),
                    MessageText = LocalizedString("ErrorMessagePickFolderBody"),
                    AdditionalText = exc.Message,
                    Severity = AppWideStatusMessageSeverity.Error
                });
                return;
            }

            // check same folder as before
            if (folder.Path == SettingsService.ScanSaveLocation.Path) return;

            await SettingsService.SetScanSaveLocationAsync(folder);

            LogService?.Log.Information("Successfully selected new save location.");
        }

        private async Task ResetSaveLocationAsync()
        {
            if (ResetSaveLocationCommand.IsRunning) return;     // already running?
            LogService?.Log.Information("ResetSaveLocationAsync");

            try
            {
                await SettingsService.ResetScanSaveLocationAsync();
            }
            catch (UnauthorizedAccessException exc)
            {
                Messenger.Send(new AppWideStatusMessage
                {
                    Title = LocalizedString("ErrorMessageResetFolderUnauthorizedHeading"),
                    MessageText = LocalizedString("ErrorMessageResetFolderUnauthorizedBody"),
                    AdditionalText = exc.Message,
                    Severity = AppWideStatusMessageSeverity.Error
                });
                return;
            }
            catch (Exception exc)
            {
                Messenger.Send(new AppWideStatusMessage
                {
                    Title = LocalizedString("ErrorMessageResetFolderHeading"),
                    MessageText = LocalizedString("ErrorMessageResetFolderBody"),
                    AdditionalText = exc.Message,
                    Severity = AppWideStatusMessageSeverity.Error
                });
                return;
            }
        }

        private void SettingsService_ScanSaveLocationChanged(object sender, EventArgs e)
        {
            SaveLocationPath = SettingsService.ScanSaveLocation?.Path;
            IsDefaultSaveLocation = SettingsService.IsScanSaveLocationDefault;
        }

        private void ScanService_ScanStartedOrCompleted(object sender, object e)
        {
            IsScanInProgress = ScanService.IsScanInProgress;
        }

        private void DisplayChangelog()
        {
            AppCenterService?.TrackEvent(AppCenterEvent.ChangelogOpened, new Dictionary<string, string> {
                            { "Source", "Settings" },
                        });
            ChangelogRequested?.Invoke(this, EventArgs.Empty);
        }

        private async Task LaunchLanguageSettings()
        {
            await Launcher.LaunchUriAsync(new Uri("ms-settings:regionlanguage"));
        }

        private void SetAutoRotateLanguage(int language)
        {
            if (language < AutoRotatorService.AvailableLanguages.Count)
            {
                SettingAutoRotateLanguage = AutoRotatorService.AvailableLanguages[language].LanguageTag;
            }
        }

        private void RefreshFileNamingPatternPreviewResult()
        {
            // get pattern
            FileNamingPattern previewPattern;
            switch ((SettingFileNamingPattern)SettingFileNamingPattern)
            {
                default:
                case Services.SettingFileNamingPattern.DateTime:
                    previewPattern = FileNamingStatics.DateTimePattern;
                    break;
                case Services.SettingFileNamingPattern.Date:
                    previewPattern = FileNamingStatics.DatePattern;
                    break;
                case Services.SettingFileNamingPattern.Custom:
                    previewPattern = new FileNamingPattern((string)SettingsService.GetSetting(AppSetting.CustomFileNamingPattern));
                    break;
            }

            // create preview DiscoveredScanner
            DiscoveredScanner previewScanner;
            string currentScannerName = Messenger.Send(new SelectedScannerRequestMessage()).Response?.Name;
            if (String.IsNullOrEmpty(currentScannerName))
            {
                previewScanner = new DiscoveredScanner("IntelliQ TX3000-S");
            }
            else
            {
                previewScanner = new DiscoveredScanner(currentScannerName);
            }
            previewScanner.FlatbedBrightnessConfig = new BrightnessConfig
            {
                DefaultBrightness = 0
            };
            previewScanner.FlatbedContrastConfig = new ContrastConfig
            {
                DefaultContrast = 0
            };

            // generate preview
            FileNamingPatternPreviewResult = previewPattern.GenerateResult(FileNamingStatics.PreviewScanOptions, previewScanner);
        }
    }
}
