using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AudioSwitcher.AudioApi;

namespace FortyOne.AudioSwitcher
{
    public static class HiddenDeviceManager
    {
        private static List<Guid> HiddenDeviceIDs = new List<Guid>();

        public static int HiddenDeviceCount => HiddenDeviceIDs.Count;

        public static ReadOnlyCollection<Guid> HiddenDevices => new ReadOnlyCollection<Guid>(HiddenDeviceIDs);

        public static bool LoadHiddenDevices(Guid[] hiddenIDs)
        {
            HiddenDeviceIDs = new List<Guid>(hiddenIDs);
            return true;
        }

        public static bool IsHiddenDevice(IDevice device) => IsHiddenDevice(device.Id);

        public static bool IsHiddenDevice(Guid id) => HiddenDeviceIDs.Contains(id);

        public static void HideDevice(Guid id)
        {
            if (!HiddenDeviceIDs.Contains(id))
                HiddenDeviceIDs.Add(id);
        }

        public static void UnhideDevice(Guid id)
        {
            HiddenDeviceIDs.Remove(id);
        }
    }
}
