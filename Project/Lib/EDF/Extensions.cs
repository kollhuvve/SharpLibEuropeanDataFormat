using System;
using System.Globalization;

namespace SharpLib.EuropeanDataFormat.EDF
{
    public static class Extensions
    {
        public static string ToUTCDateTimeString(this DateTime dateTime)
        {
            //19.10.2020 1424.31i.48.3148
            return dateTime.ToString("dd.MM.yyyy HH:mm:ss.fff");
        }

        public static DateTime ToPosixDateTime(this int posixTimestamp)
        {
            var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(posixTimestamp);
            return DateTime.SpecifyKind(dateTimeOffset.DateTime, DateTimeKind.Utc);
        }

        public static string ToFloatString(this float value)
        {
            return value.ToString(FloatTwoDecimals, FloatNumberFormatInfo);
        }

        public static string ToDoubleString(this double value)
        {
            return value.ToString(FloatTwoDecimals, FloatNumberFormatInfo);
        }

        public static string FloatTwoDecimals
        {
            get => "0.00";
        }

        public static NumberFormatInfo FloatNumberFormatInfo
        {
            get
            {
                var numberFormatInfo = new CultureInfo("en-US", false).NumberFormat;
                numberFormatInfo.NumberDecimalSeparator = ".";
                return numberFormatInfo;
            }
        }
    }
}
