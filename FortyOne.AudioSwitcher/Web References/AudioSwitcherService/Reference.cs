// This file replaces the legacy auto-generated SOAP proxy.
// The service client now uses HttpClient to communicate with the SOAP endpoint.

using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FortyOne.AudioSwitcher.AudioSwitcherService
{
    public class AudioSwitcherVersionInfo
    {
        public string VersionInfo { get; set; }
        public string URL { get; set; }
        public string ChangeLog { get; set; }
    }

    public sealed class AudioSwitcherClient : IDisposable
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };

        private readonly string _serviceUrl;

        public AudioSwitcherClient(string serviceUrl)
        {
            _serviceUrl = serviceUrl;
        }

        public string CheckForUpdate(string assemblyVersion)
        {
            return CheckForUpdateAsync(assemblyVersion).GetAwaiter().GetResult();
        }

        public async Task<string> CheckForUpdateAsync(string assemblyVersion)
        {
            var envelope = BuildEnvelope("CheckForUpdate",
                $"<assemblyVersion>{assemblyVersion}</assemblyVersion>");

            var content = CreateSoapContent(envelope, "http://tempuri.org/CheckForUpdate");
            var response = await _httpClient.PostAsync(_serviceUrl, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var xml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://tempuri.org/";
            return doc.Descendants(ns + "CheckForUpdateResult").FirstOrDefault()?.Value;
        }

        public AudioSwitcherVersionInfo GetUpdateInfo(string assemblyVersion)
        {
            return GetUpdateInfoAsync(assemblyVersion).GetAwaiter().GetResult();
        }

        public async Task<AudioSwitcherVersionInfo> GetUpdateInfoAsync(string assemblyVersion)
        {
            var envelope = BuildEnvelope("GetUpdateInfo",
                $"<assemblyVersion>{assemblyVersion}</assemblyVersion>");

            var content = CreateSoapContent(envelope, "http://tempuri.org/GetUpdateInfo");
            var response = await _httpClient.PostAsync(_serviceUrl, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var xml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://tempuri.org/";
            var result = doc.Descendants(ns + "GetUpdateInfoResult").FirstOrDefault();
            if (result == null)
                return null;

            return new AudioSwitcherVersionInfo
            {
                VersionInfo = result.Element("VersionInfo")?.Value,
                URL = result.Element("URL")?.Value,
                ChangeLog = result.Element("ChangeLog")?.Value
            };
        }

        public string SendBugReport(string confirmation, string userComment, string details, string stackTrace)
        {
            return SendBugReportAsync(confirmation, userComment, details, stackTrace).GetAwaiter().GetResult();
        }

        public async Task<string> SendBugReportAsync(string confirmation, string userComment, string details, string stackTrace)
        {
            var body = $"<confirmation>{EscapeXml(confirmation)}</confirmation>" +
                       $"<userComment>{EscapeXml(userComment)}</userComment>" +
                       $"<details>{EscapeXml(details)}</details>" +
                       $"<stackTrace>{EscapeXml(stackTrace)}</stackTrace>";

            var envelope = BuildEnvelope("SendBugReport", body);
            var content = CreateSoapContent(envelope, "http://tempuri.org/SendBugReport");
            var response = await _httpClient.PostAsync(_serviceUrl, content).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var xml = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var doc = XDocument.Parse(xml);
            XNamespace ns = "http://tempuri.org/";
            return doc.Descendants(ns + "SendBugReportResult").FirstOrDefault()?.Value ?? "";
        }

        private static string BuildEnvelope(string operation, string innerXml)
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
               xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
               xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
  <soap:Body>
    <{operation} xmlns=""http://tempuri.org/"">
      {innerXml}
    </{operation}>
  </soap:Body>
</soap:Envelope>";
        }

        private static StringContent CreateSoapContent(string envelope, string soapAction)
        {
            var content = new StringContent(envelope, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", $"\"{soapAction}\"");
            return content;
        }

        private static string EscapeXml(string value)
        {
            if (value == null) return string.Empty;
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;");
        }

        public void Dispose()
        {
            // HttpClient is shared/static; nothing to dispose here.
        }
    }
}
