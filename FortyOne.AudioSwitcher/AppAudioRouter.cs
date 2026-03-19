using System;
using System.Runtime.InteropServices;

namespace FortyOne.AudioSwitcher
{
    internal enum EDataFlow { eRender = 0, eCapture = 1, eAll = 2 }
    internal enum ERole { eConsole = 0, eCommunications = 1, eMultimedia = 2 }

    // COM class for AudioPolicyConfig (used internally by Windows Sound Settings)
    [ComImport, Guid("2a59116d-6c4f-45e0-a74f-707e3fef9258")]
    internal class AudioPolicyConfigClient { }

    // Modern vtable layout: Windows 10 21H1+ and Windows 11
    // Vtable slots 3-7: misc audio engine methods
    // Slots 8-9: GetProcessDevicePreferences / SetProcessDevicePreferences (added ~21H1)
    // Slots 10-12: persisted per-app endpoint control
    [ComImport, Guid("ab3d4648-e242-459f-b02f-541c70306324")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioPolicyConfig
    {
        void GetMixFormat();
        void GetDevicePeriod();
        void GetSharedModeEnginePeriod();
        void GetCurrentSharedModeEnginePeriod();
        void SetCurrentSharedModeEnginePeriod();
        void GetProcessDevicePreferences();
        void SetProcessDevicePreferences();

        [PreserveSig]
        int SetPersistedDefaultAudioEndpoint(
            uint processId, EDataFlow flow, ERole role,
            [MarshalAs(UnmanagedType.HString)] string deviceId);

        [PreserveSig]
        int GetPersistedDefaultAudioEndpoint(
            uint processId, EDataFlow flow, ERole role,
            [MarshalAs(UnmanagedType.HString)] out string deviceId);

        [PreserveSig]
        int ClearAllPersistedApplicationDefaultEndpoints();
    }

    // IMMDevice COM interfaces for resolving device endpoint ID strings
    [ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    internal class MMDeviceEnumeratorCoClass { }

    [ComImport, Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        [PreserveSig] int EnumAudioEndpoints(int dataFlow, int stateMask, out IMMDeviceCollection ppDevices);
        [PreserveSig] int GetDefaultAudioEndpoint(int dataFlow, int role, out IMMDevice ppEndpoint);
        [PreserveSig] int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IMMDevice ppDevice);
        [PreserveSig] int RegisterEndpointNotificationCallback(IntPtr pClient);
        [PreserveSig] int UnregisterEndpointNotificationCallback(IntPtr pClient);
    }

    [ComImport, Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceCollection
    {
        [PreserveSig] int GetCount(out uint pcDevices);
        [PreserveSig] int Item(uint nDevice, out IMMDevice ppDevice);
    }

    [ComImport, Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        [PreserveSig] int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);
        [PreserveSig] int OpenPropertyStore(int stgmAccess, out IntPtr ppProperties);
        [PreserveSig] int GetId([MarshalAs(UnmanagedType.LPWStr)] out string ppstrId);
        [PreserveSig] int GetState(out int pdwState);
    }

    /// <summary>
    /// Routes audio for all apps to follow the system default device by clearing
    /// per-app endpoint overrides set by apps like Discord or Qobuz.
    /// Uses the undocumented IAudioPolicyConfig COM interface (Windows 10 21H1+ / Windows 11).
    /// </summary>
    internal static class AppAudioRouter
    {
        private const int DEVICE_STATEMASK_ALL = 0x0000000F;

        /// <summary>
        /// Returns the Windows endpoint ID string (e.g. "{0.0.0.00000000}.{guid}") for a device GUID.
        /// </summary>
        public static string GetEndpointId(Guid deviceGuid)
        {
            try
            {
                var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorCoClass();
                enumerator.EnumAudioEndpoints(2 /* eAll */, DEVICE_STATEMASK_ALL, out var collection);
                collection.GetCount(out var count);

                for (uint i = 0; i < count; i++)
                {
                    collection.Item(i, out var device);
                    device.GetId(out var id);
                    if (id != null && id.IndexOf(deviceGuid.ToString("D"), StringComparison.OrdinalIgnoreCase) >= 0)
                        return id;
                }
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Clears all per-app audio endpoint overrides so every app falls back to
        /// the Windows system default device. Works on Windows 10 21H1+ and Windows 11.
        /// Silently no-ops on older systems.
        /// </summary>
        public static void ClearPersistedEndpoints()
        {
            try
            {
                var policyConfig = (IAudioPolicyConfig)new AudioPolicyConfigClient();
                int hr = policyConfig.ClearAllPersistedApplicationDefaultEndpoints();
                // hr != 0 means failure (e.g. wrong vtable on older Windows) – silently ignore
            }
            catch
            {
                // IAudioPolicyConfig not available on this Windows version; safe to ignore
            }
        }
    }
}
