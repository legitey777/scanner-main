﻿using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Windows.Devices.Scanners;
using static Utilities;

namespace Scanner.Models.FileNaming
{
    public class HourFileNamingBlock : ObservableObject, IFileNamingBlock
    {
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // DECLARATIONS /////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string Glyph => "\uE121";
        public string Name => "HOUR";

        public string DisplayName
        {
            get => LocalizedString("HeadingFileNamingBlockHour/Text");
        }

        private bool _Use24Hours;
        public bool Use24Hours
        {
            get => _Use24Hours;
            set => SetProperty(ref _Use24Hours, value);
        }

        private bool _Use2Digits = true;
        public bool Use2Digits
        {
            get => _Use2Digits;
            set => SetProperty(ref _Use2Digits, value);
        }

        public bool IsValid
        {
            get => true;
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // CONSTRUCTORS / FACTORIES /////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public HourFileNamingBlock()
        {
            if (CultureInfo.CurrentUICulture.DateTimeFormat.LongTimePattern.Contains("H"))
            {
                // assume 24-hour clock
                Use24Hours = true;
            }
        }

        public HourFileNamingBlock(string serialized)
        {
            string[] parts = serialized.TrimStart('*').Split('|', StringSplitOptions.RemoveEmptyEntries);
            Use24Hours = bool.Parse(parts[1]);
            Use2Digits = bool.Parse(parts[2]);
        }


        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // METHODS //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public string ToString(ScanOptions scanOptions, DiscoveredScanner scanner)
        {
            DateTime currentTime = DateTime.Now;
            string result;
            if (Use24Hours)
            {
                result = currentTime.Hour.ToString();
            }
            else
            {
                result = currentTime.ToString("%h");
            }

            if (Use2Digits)
            {
                return result.PadLeft(2, '0');
            }
            else
            {
                return result;
            }
        }

        public string GetSerialized(bool obfuscated)
        {
            return $"*{Name}|{Use24Hours}|{Use2Digits}";
        }
    }
}
