﻿using WinUI = Microsoft.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using static Enums;

namespace Scanner.Views
{
    public sealed partial class HelpView : Page
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // DECLARATIONS /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private TaskCompletionSource<bool> PageLoaded = new TaskCompletionSource<bool>();


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS / FACTORIES /////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public HelpView()
        {
            this.InitializeComponent();

            ViewModel.HelpTopicRequested += ViewModel_HelpTopicRequested;
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // METHODS //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private async void ViewModel_HelpTopicRequested(object sender, HelpTopic topic)
        {
            WinUI.Expander requestedExpander = ConvertHelpTopic(topic);
            if (requestedExpander != null)
            {
                requestedExpander.IsExpanded = true;
                PageLoaded = new TaskCompletionSource<bool>();
                await PageLoaded.Task;
                requestedExpander.StartBringIntoView();
            }
        }

        /// <summary>
        ///     Maps a <see cref="HelpTopic"/> to the corresponding
        ///     <see cref="WinUI.Expander"/>.
        /// </summary>
        public WinUI.Expander ConvertHelpTopic(HelpTopic topic)
        {
            switch (topic)
            {
                case HelpTopic.ScannerDiscovery:
                    return ExpanderScannerDiscovery;
                case HelpTopic.ScannerNotWorking:
                    return ExpanderScannerNotWorking;
                case HelpTopic.ChooseResolution:
                    return ExpanderChooseResolution;
                case HelpTopic.BrightnessContrast:
                    return ExpanderBrightnessContrast;
                case HelpTopic.AutoCrop:
                    return ExpanderAutoCrop;
                case HelpTopic.SaveChanges:
                    return ExpanderSaveChanges;
                case HelpTopic.ChangeScanFolder:
                    return ExpanderChangeScanFolder;
                case HelpTopic.ChooseFileFormat:
                    return ExpanderChooseFileFormat;
                case HelpTopic.StartNewPdf:
                    return ExpanderStartNewPdf;
                case HelpTopic.ReorderPdfPages:
                    return ExpanderReorderPdfPages;
                default:
                    break;
            }
            return null;
        }

        private void Page_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            PageLoaded.TrySetResult(true);
        }
    }
}
