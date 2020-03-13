using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace VSDebugCoreLib.Utils
{
    public class MiscHelpers
    {
        public static string GetClickableFileName(string strFile)
        {
            return "<file://" + strFile + ">.";
        }

        public static void LaunchLink(string link)
        {
            try
            {
                Process.Start(link);
            }
            catch (Exception)
            {
                // Do nothing if default application handler is not associated.
            }
        }

        public static T[] ShiftArray<T>(T[] arr, uint count = 1)
        {
            if (arr.Length <= count)
            {
                return new T[0];
            }

            var res = new T[arr.Length - count];
            Array.Copy(arr, count, res, 0, res.Length);
            return res;
        }

        public static string[] ParseCommand(string text)
        {
            char[] spaces = { ' ', '\t' };
            char[] stringDelimiter = { '\'', '"' };
            char[] escapeCharacters = { '\\' };

            var args = new List<string>();

            var currentArg = "";
            char lastDelimiter = ' ';
            bool inString = false;
            bool isEscaped = false;
            bool endOfString = false;

            foreach (var c in text)
            {
                if (isEscaped)
                {
                    // escaped, do not do any processing of this character
                    currentArg += c;
                    isEscaped = false;
                    continue;
                }
                else if (Array.Exists(escapeCharacters, e => e == c))
                {
                    // escape character, skip next
                    isEscaped = true;
                    continue;
                }

                // in a string, we are only interested in end characerts
                if (inString)
                {
                    if (c == lastDelimiter)
                    {
                        // end of string
                        inString = false;
                        args.Add(currentArg);
                        currentArg = "";
                        endOfString = true;
                    }
                    else
                    {
                        currentArg += c;
                    }
                    continue;
                }

                if (Array.Exists(spaces, e => e == c))
                {
                    args.Add(currentArg);
                    currentArg = "";
                    endOfString = false;
                    continue;
                }

                if (endOfString)
                {
                    throw new InvalidOperationException("The given command contains invalid string delimiter.");
                }

                if (currentArg.Length == 0 && Array.Exists(stringDelimiter, e => e == c))
                {
                    // start of new group and a string delimter => handle as a string
                    inString = true;
                    lastDelimiter = c;
                }
                else
                {
                    currentArg += c;
                }
            }

            if (isEscaped)
            {
                throw new InvalidOperationException("The given command ends with an escape character.");
            }
            else if (inString)
            {
                throw new InvalidOperationException("The given command contains invalid string delimiter.");
            }

            args.Add(currentArg);

            return args.FindAll(e => e.Length > 0).ToArray();
        }
    }
}
