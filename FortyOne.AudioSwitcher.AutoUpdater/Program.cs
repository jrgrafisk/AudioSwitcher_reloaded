using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace FortyOne.AudioSwitcher.AutoUpdater
{
    internal class Program
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(60)
        };

        private const string ServiceUrl = "http://services.audioswit.ch/AudioSwitcher.asmx";

        private static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                return -1;
            }

            var pid = int.Parse(args[0]);
            var audioSwitcherPath = args[1];
            var audioSwitcherOldPath = audioSwitcherPath + "_old";

            var x = 0;

            Console.Write("Waiting for Audio Switcher to exit");

            try
            {
                while (Process.GetProcessById(pid) != null)
                {
                    Console.Write(".");
                    Thread.Sleep(500);

                    if (x > 30)
                    {
                        Console.WriteLine();
                        Console.WriteLine();
                        Console.WriteLine("Audio Switcher process has not ended");
                        return Exit();
                    }
                }
            }
            catch
            {
            }

            try
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Updating...");

                if (File.Exists(audioSwitcherOldPath))
                    File.Delete(audioSwitcherOldPath);

                File.Move(audioSwitcherPath, audioSwitcherOldPath);

                var url = CheckForUpdate("0.0.0.0").Replace(".zip", ".exe");

                var bytes = _httpClient.GetByteArrayAsync(url).GetAwaiter().GetResult();
                File.WriteAllBytes(audioSwitcherPath, bytes);
            }
            catch
            {
                if (File.Exists(audioSwitcherPath))
                    File.Delete(audioSwitcherPath);
                File.Move(audioSwitcherOldPath, audioSwitcherPath);
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Error while updating");
            }

            Process.Start(audioSwitcherPath);

            if (File.Exists(audioSwitcherOldPath))
                File.Delete(audioSwitcherOldPath);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Updating Complete");

            return Exit();
        }

        private static string CheckForUpdate(string assemblyVersion)
        {
            var envelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
               xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
               xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <CheckForUpdate xmlns=""http://tempuri.org/"">
      <assemblyVersion>{assemblyVersion}</assemblyVersion>
    </CheckForUpdate>
  </soap:Body>
</soap:Envelope>";

            var content = new StringContent(envelope, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "\"http://tempuri.org/CheckForUpdate\"");

            var response = _httpClient.PostAsync(ServiceUrl, content).GetAwaiter().GetResult();
            response.EnsureSuccessStatusCode();

            var xml = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://tempuri.org/";
            return doc.Descendants(ns + "CheckForUpdateResult").FirstOrDefault()?.Value ?? string.Empty;
        }

        private static int Exit()
        {
            Console.WriteLine("Exiting...");
            return 1;
        }
    }
}
