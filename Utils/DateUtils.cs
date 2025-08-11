using System.Globalization;

namespace FORO_UTTN_API.Utils
{
    public class DateUtils
    {
        public static string DateMX(DateTime date)
        {
            var cultureInfo = new CultureInfo("es-MX");
            return date.ToString("dd/MM/yyyy", cultureInfo); // Formato día/mes/año
        }

        public static string TimeMX(DateTime date)
        {
            var cultureInfo = new CultureInfo("es-ES");
            return date.ToString("hh:mm tt", cultureInfo); // Formato de 12 horas
        }
    }
}
