using FortyOne.AudioSwitcher.AudioSwitcherService;

namespace FortyOne.AudioSwitcher.Helpers
{
    public static class ConnectionHelper
    {
        public static AudioSwitcherClient GetAudioSwitcherProxy()
        {
            return new AudioSwitcherClient();
        }
    }
}
