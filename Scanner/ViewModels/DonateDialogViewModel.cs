﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.DependencyInjection;
using Microsoft.Toolkit.Mvvm.Input;
using Scanner.Services;
using System;
using System.Threading.Tasks;
using Windows.System;

using static Scanner.Helpers.AppConstants;

namespace Scanner.ViewModels
{
    public class DonateDialogViewModel : ObservableRecipient, IDisposable
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // DECLARATIONS /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Services
        private readonly IAppCenterService AppCenterService = Ioc.Default.GetService<IAppCenterService>();
        public readonly IAccessibilityService AccessibilityService = Ioc.Default.GetService<IAccessibilityService>();
        #endregion

        #region Commands
        public RelayCommand DisposeCommand;
        public AsyncRelayCommand DonateCommand;
        #endregion


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS / FACTORIES /////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public DonateDialogViewModel()
        {
            DonateCommand = new AsyncRelayCommand(Donate);

            AppCenterService.TrackEvent(AppCenterEvent.DonationDialogOpened);
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // METHODS //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void Dispose()
        {
            // clean up messenger
            Messenger.UnregisterAll(this);
        }

        /// <summary>
        ///     Opens the donation webpage specified by <see cref="UriDonation"/>.
        /// </summary>
        private async Task Donate()
        {
            AppCenterService?.TrackEvent(AppCenterEvent.DonationLinkClicked);
            await Launcher.LaunchUriAsync(UriDonation);
        }
    }
}
