using System;
using System.Globalization;

namespace FORO_UTTN_API.Utils
{
    public static class DateUtils
    {
        private static readonly TimeZoneInfo MxTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Matamoros");

        public static string DateMX(DateTime date)
        {
            var converted = TimeZoneInfo.ConvertTimeFromUtc(date, MxTimeZone);
            return converted.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("es-MX"));
        }

        public static string TimeMX(DateTime date)
        {
            var converted = TimeZoneInfo.ConvertTimeFromUtc(date, MxTimeZone);
            return converted.ToString("hh:mm tt", CultureInfo.GetCultureInfo("es-MX"));
        }
    }
}

