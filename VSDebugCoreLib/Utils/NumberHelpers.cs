using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace VSDebugCoreLib.Utils
{
    public class NumberHelpers
    {
        public static string ToHex(long value)
        {
            return String.Format("0x{0:X}", value);
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

            bool result = long.TryParse(stringToConvert, styles, provider, out number);

            if (false == result && (styles & NumberStyles.AllowHexSpecifier) != 0)
            {
                string substring = stringToConvert.Substring(2);

                result = long.TryParse(substring, styles, provider, out number);
            }

            return result;
        }

        public static Int32 ParseInt32(string stringToConvert)
        {
            Int32 number;
            CultureInfo provider = CultureInfo.InvariantCulture;
            NumberStyles style = IsHexNumber(stringToConvert) ? NumberStyles.HexNumber : NumberStyles.Integer;

            bool result = Int32.TryParse(stringToConvert, style, provider, out number);

            if (false == result && (style & NumberStyles.AllowHexSpecifier) != 0)
            {
                string substring = stringToConvert.Substring(2);

                result = Int32.TryParse(substring, style, provider, out number);
            }

            return number;
        }

        public static UInt32 ParseUInt32(string stringToConvert)
        {
            UInt32 number;
            CultureInfo provider = CultureInfo.InvariantCulture;
            NumberStyles style = IsHexNumber(stringToConvert) ? NumberStyles.HexNumber : NumberStyles.Integer;

            bool result = UInt32.TryParse(stringToConvert, style, provider, out number);

            if (false == result && (style & NumberStyles.AllowHexSpecifier) != 0)
            {
                string substring = stringToConvert.Substring(2);

                result = UInt32.TryParse(substring, style, provider, out number);
            }

            return number;
        }

        public static UInt64 ParseUInt64(string stringToConvert)
        {
            UInt64 number;
            CultureInfo provider = CultureInfo.InvariantCulture;
            NumberStyles style = IsHexNumber(stringToConvert) ? NumberStyles.HexNumber : NumberStyles.Integer;

            bool result = UInt64.TryParse(stringToConvert, style, provider, out number);

            if (false == result && (style & NumberStyles.AllowHexSpecifier) != 0)
            {
                string substring = stringToConvert.Substring(2);

                result = UInt64.TryParse(substring, style, provider, out number);
            }

            return number;
        }

        public static Int64 ParseInt64(string stringToConvert)
        {
            Int64 number;
            CultureInfo provider = CultureInfo.InvariantCulture;
            NumberStyles style = IsHexNumber(stringToConvert) ? NumberStyles.HexNumber : NumberStyles.Integer;

            bool result = Int64.TryParse(stringToConvert, style, provider, out number);

            if (false == result && (style & NumberStyles.AllowHexSpecifier) != 0)
            {
                string substring = stringToConvert.Substring(2);

                result = Int64.TryParse(substring, style, provider, out number);
            }

            return number;
        }

        public static float ParseFloat(string stringToConvert)
        {
            float number;

            CultureInfo provider = CultureInfo.InvariantCulture;
            NumberStyles style = IsHexNumber(stringToConvert) ? NumberStyles.HexNumber : NumberStyles.Integer;

            bool result = float.TryParse(stringToConvert, style, provider, out number);

            if (false == result && (style & NumberStyles.AllowHexSpecifier) != 0)
            {
                string substring = stringToConvert.Substring(2);

                result = float.TryParse(substring, style, provider, out number);
            }

            return number;
        }
    }
}