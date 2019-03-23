using System.Globalization;
using System.Text.RegularExpressions;

namespace VSDebugCoreLib.Utils
{
    public class NumberHelpers
    {
        public static string ToHex(long value)
        {
            return string.Format("0x{0:X}", value);
        }

        public static bool IsHexNumber(string strValue)
        {
            return Regex.IsMatch(strValue, @"\A\b(0[xX])[0-9a-fA-F]+\b\Z");
        }

        public static bool TryParseLong(string stringToConvert, NumberStyles styles, out long number)
        {
            CultureInfo provider;

            // If currency symbol is allowed, use en-US culture.
            if ((styles & NumberStyles.AllowCurrencySymbol) > 0)
                provider = new CultureInfo("en-US");
            else
                provider = CultureInfo.InvariantCulture;

            var result = long.TryParse(stringToConvert, styles, provider, out number);

            if (false == result && (styles & NumberStyles.AllowHexSpecifier) != 0)
            {
                var substring = stringToConvert.Substring(2);

                result = long.TryParse(substring, styles, provider, out number);
            }

            return result;
        }

        public static int ParseInt32(string stringToConvert)
        {
            int number;
            var provider = CultureInfo.InvariantCulture;
            var style = IsHexNumber(stringToConvert) ? NumberStyles.HexNumber : NumberStyles.Integer;

            var result = int.TryParse(stringToConvert, style, provider, out number);

            if (false == result && (style & NumberStyles.AllowHexSpecifier) != 0)
            {
                var substring = stringToConvert.Substring(2);

                result = int.TryParse(substring, style, provider, out number);
            }

            return number;
        }

        public static uint ParseUInt32(string stringToConvert)
        {
            uint number;
            var provider = CultureInfo.InvariantCulture;
            var style = IsHexNumber(stringToConvert) ? NumberStyles.HexNumber : NumberStyles.Integer;

            var result = uint.TryParse(stringToConvert, style, provider, out number);

            if (false == result && (style & NumberStyles.AllowHexSpecifier) != 0)
            {
                var substring = stringToConvert.Substring(2);

                result = uint.TryParse(substring, style, provider, out number);
            }

            return number;
        }

        public static ulong ParseUInt64(string stringToConvert)
        {
            ulong number;
            var provider = CultureInfo.InvariantCulture;
            var style = IsHexNumber(stringToConvert) ? NumberStyles.HexNumber : NumberStyles.Integer;

            var result = ulong.TryParse(stringToConvert, style, provider, out number);

            if (false == result && (style & NumberStyles.AllowHexSpecifier) != 0)
            {
                var substring = stringToConvert.Substring(2);

                result = ulong.TryParse(substring, style, provider, out number);
            }

            return number;
        }

        public static long ParseInt64(string stringToConvert)
        {
            long number;
            var provider = CultureInfo.InvariantCulture;
            var style = IsHexNumber(stringToConvert) ? NumberStyles.HexNumber : NumberStyles.Integer;

            var result = long.TryParse(stringToConvert, style, provider, out number);

            if (false == result && (style & NumberStyles.AllowHexSpecifier) != 0)
            {
                var substring = stringToConvert.Substring(2);

                result = long.TryParse(substring, style, provider, out number);
            }

            return number;
        }

        public static float ParseFloat(string stringToConvert)
        {
            float number;

            var provider = CultureInfo.InvariantCulture;
            var style = IsHexNumber(stringToConvert) ? NumberStyles.HexNumber : NumberStyles.Integer;

            var result = float.TryParse(stringToConvert, style, provider, out number);

            if (false == result && (style & NumberStyles.AllowHexSpecifier) != 0)
            {
                var substring = stringToConvert.Substring(2);

                result = float.TryParse(substring, style, provider, out number);
            }

            return number;
        }
    }
}