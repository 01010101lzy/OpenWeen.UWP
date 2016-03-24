﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace OpenWeen.UWP.Common.Converter
{
    public class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var date = DateTime.Parse(value.ToString());
            var time = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("China Standard Time")) - date;
            if (time.Days != 0)
            {
                if (time.Days > 7)
                {
                    return date.ToString("f");
                }
                else
                    return $"{time.Days}d";
            }
            if (time.Hours != 0)
            {
                return $"{time.Hours}h";
            }
            if (time.Minutes != 0)
            {
                return $"{time.Minutes}m";
            }
            if (time.Seconds != 0)
            {
                return $"{time.Seconds}s";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
