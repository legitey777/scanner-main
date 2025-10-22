﻿using Microsoft.Toolkit.Mvvm.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Scanners;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

using static Utilities;

namespace Scanner.Services
{
    internal class AutoRotatorService : IAutoRotatorService
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // DECLARATIONS /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private readonly IAppCenterService AppCenterService = Ioc.Default.GetService<IAppCenterService>();
        private readonly ILogService LogService = Ioc.Default.GetService<ILogService>();
        private readonly IHelperService HelperService = Ioc.Default.GetRequiredService<IHelperService>();
        private readonly ISettingsService SettingsService = Ioc.Default.GetRequiredService<ISettingsService>();

        private const int MinimumNumberOfWords = 50;

        private OcrEngine OcrEngine;

        public IReadOnlyList<Language> AvailableLanguages => OcrEngine.AvailableRecognizerLanguages;
        public Language DefaultLanguage => OcrEngine.TryCreateFromUserProfileLanguages()?.RecognizerLanguage;


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS / FACTORIES /////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public AutoRotatorService()
        {
            Initialize();

            SettingsService.SettingChanged += SettingsService_SettingChanged;
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // METHODS //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        ///     Initializes the service by getting the desired language and trying to use it to create an
        ///     <see cref="OcrEngine"/>. If that fails, a default one is created instead.
        /// </summary>
        public void Initialize()
        {
            LogService?.Log.Information("AutoRotatorService: Initializing");
            string desiredLanguageScript = (string)SettingsService.GetSetting(AppSetting.SettingAutoRotateLanguage);

            Language desiredLanguage;
            if (desiredLanguageScript != "")
            {
                desiredLanguage = new Language(desiredLanguageScript);

                try
                {
                    OcrEngine = OcrEngine.TryCreateFromLanguage(desiredLanguage);
                }
                catch (Exception)
                {

                }
            }

            if (OcrEngine == null)
            {
                // language unavailable, try to reset to default
                OcrEngine = OcrEngine.TryCreateFromUserProfileLanguages();

                if (OcrEngine == null)
                {
                    // can not reset to default, try choosing a language
                    foreach (Language language in OcrEngine.AvailableRecognizerLanguages)
                    {
                        OcrEngine = OcrEngine.TryCreateFromLanguage(language);
                        if (OcrEngine != null)
                        {
                            break;
                        }
                    }
                }

                // language changed, update settings
                if (OcrEngine != null)
                {
                    SettingsService.SetSetting(AppSetting.SettingAutoRotateLanguage, OcrEngine.RecognizerLanguage.LanguageTag);
                }
                else
                {
                    SettingsService.SetSetting(AppSetting.SettingAutoRotateLanguage, "");
                }
            }
        }
        
        /// <summary>
        ///     Attempts to determine the <see cref="BitmapRotation"/> needed to fix the orientation of the given
        ///     <paramref name="imageFile"/>.
        /// </summary>
        public async Task<BitmapRotation> TryGetRecommendedRotationAsync(StorageFile imageFile, ImageScannerFormat format)
        {
            try
            {
                if (OcrEngine != null)
                {
                    // get separate stream
                    using (IRandomAccessStream sourceStream = await imageFile.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapDecoder decoder = await BitmapDecoder.CreateAsync(sourceStream);
                        Tuple<BitmapRotation, int> bestRotation;

                        // create rotated 0°
                        SoftwareBitmap bitmap = await decoder.GetSoftwareBitmapAsync();
                        OcrResult ocrResult = await OcrEngine.RecognizeAsync(bitmap);
                        bestRotation = new Tuple<BitmapRotation, int>(BitmapRotation.None, ocrResult.Text.Length);

                        using (InMemoryRandomAccessStream targetStream = new InMemoryRandomAccessStream())
                        {
                            // create rotated 90°
                            BitmapEncoder encoder = await HelperService.CreateOptimizedBitmapEncoderAsync(format, targetStream);
                            encoder.SetSoftwareBitmap(bitmap);
                            encoder.BitmapTransform.Rotation = BitmapRotation.Clockwise90Degrees;
                            await encoder.FlushAsync();
                            decoder = await BitmapDecoder.CreateAsync(targetStream);
                            bitmap = await decoder.GetSoftwareBitmapAsync();
                            ocrResult = await OcrEngine.RecognizeAsync(bitmap);
                            if (ocrResult.Text.Length > bestRotation.Item2)
                            {
                                bestRotation = new Tuple<BitmapRotation, int>(BitmapRotation.Clockwise90Degrees, ocrResult.Text.Length);
                            }
                        }

                        using (InMemoryRandomAccessStream targetStream = new InMemoryRandomAccessStream())
                        {
                            // create rotated 180°
                            BitmapEncoder encoder = await HelperService.CreateOptimizedBitmapEncoderAsync(format, targetStream);
                            encoder.SetSoftwareBitmap(bitmap);
                            encoder.BitmapTransform.Rotation = BitmapRotation.Clockwise90Degrees;
                            await encoder.FlushAsync();
                            decoder = await BitmapDecoder.CreateAsync(targetStream);
                            bitmap = await decoder.GetSoftwareBitmapAsync();
                            ocrResult = await OcrEngine.RecognizeAsync(bitmap);
                            if (ocrResult.Text.Length > bestRotation.Item2)
                            {
                                bestRotation = new Tuple<BitmapRotation, int>(BitmapRotation.Clockwise180Degrees, ocrResult.Text.Length);
                            }
                        }

                        using (InMemoryRandomAccessStream targetStream = new InMemoryRandomAccessStream())
                        {
                            // create rotated 270°
                            BitmapEncoder encoder = await HelperService.CreateOptimizedBitmapEncoderAsync(format, targetStream);
                            encoder.SetSoftwareBitmap(bitmap);
                            encoder.BitmapTransform.Rotation = BitmapRotation.Clockwise90Degrees;
                            await encoder.FlushAsync();
                            decoder = await BitmapDecoder.CreateAsync(targetStream);
                            bitmap = await decoder.GetSoftwareBitmapAsync();
                            ocrResult = await OcrEngine.RecognizeAsync(bitmap);
                            if (ocrResult.Text.Length > bestRotation.Item2)
                            {
                                bestRotation = new Tuple<BitmapRotation, int>(BitmapRotation.Clockwise270Degrees, ocrResult.Text.Length);
                            }
                        }

                        bitmap.Dispose();

                        if (bestRotation.Item2 < MinimumNumberOfWords)
                        {
                            // very low confidence, could just be random patterns
                            return BitmapRotation.None;
                        }
                        else
                        {
                            return bestRotation.Item1;
                        }
                    }
                }
                else
                {
                    return BitmapRotation.None;
                }
            }
            catch (Exception exc)
            {
                LogService?.Log.Error(exc, "Determining the recommended rotation failed.");
                AppCenterService?.TrackError(exc);
                return BitmapRotation.None;
            }            
        }

        private void SettingsService_SettingChanged(object sender, AppSetting e)
        {
            if (e == AppSetting.SettingAutoRotateLanguage)
            {
                // desired language changed, reinitialize recognizer
                Initialize();
            }
        }
    }
}
