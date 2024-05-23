using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using DnsClient;

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

        public static async Task<string> GetDnsTxtRecordAsync(string domainName)
        {
            try
            {
                var lookupClient = new LookupClient(); // Create a DNS lookup client
                var result = await lookupClient.QueryAsync(domainName, QueryType.TXT); // Query for TXT records

                // Check if the query was successful and if any TXT records were found
                if (result.HasError || !result.Answers.Any())
                {
                    return null;
                }

                // Combine TXT record values
                var txtRecords = result.Answers
                    .OfType<DnsClient.Protocol.TxtRecord>()
                    .SelectMany(r => r.Text); 

                return string.Join("", txtRecords);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static string GetApplicationDataPath()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+ "\\VSDebugPro\\";
        }
    }
}