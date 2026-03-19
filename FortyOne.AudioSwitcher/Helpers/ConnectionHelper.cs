using System;
using System.Net.Http;
using FortyOne.AudioSwitcher.AudioSwitcherService;
using FortyOne.AudioSwitcher.Properties;

namespace FortyOne.AudioSwitcher.Helpers
{
    public static class ConnectionHelper
    {
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        public static bool IsServerOnline
        {
            get
            {
                try
                {
                    var response = _httpClient.GetAsync(Resources.WebServiceURL).GetAwaiter().GetResult();
                    return response.IsSuccessStatusCode;
                }
                catch
                {
                    return false;
                }
            }
        }

        public static AudioSwitcherClient GetAudioSwitcherProxy()
        {
            if (IsServerOnline)
                return new AudioSwitcherClient(Resources.WebServiceURL);
            return null;
        }
    }
}
