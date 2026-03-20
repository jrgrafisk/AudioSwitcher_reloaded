using System;
using System.Collections.Generic;
using AudioSwitcher.AudioApi;

namespace FortyOne.AudioSwitcher
{
    public static class DeviceNameManager
    {
        private static readonly Dictionary<Guid, string> _customNames = new Dictionary<Guid, string>();

        public static void LoadCustomNames(string raw)
        {
            _customNames.Clear();
            if (string.IsNullOrEmpty(raw))
                return;

            // Format: [guid|name],[guid|name]
            var entries = raw.Split(new[] { "],[" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var entry in entries)
            {
                var clean = entry.Trim('[', ']');
                var pipe = clean.IndexOf('|');
                if (pipe < 0) continue;

                var guidStr = clean.Substring(0, pipe);
                var name = clean.Substring(pipe + 1);

                if (Guid.TryParse(guidStr, out Guid id) && !string.IsNullOrWhiteSpace(name))
                    _customNames[id] = name;
            }
        }

        public static string SaveCustomNames()
        {
            var parts = new List<string>();
            foreach (var kv in _customNames)
                parts.Add("[" + kv.Key + "|" + kv.Value + "]");
            return string.Join(",", parts);
        }

        public static string GetDisplayName(IDevice device)
        {
            if (device == null) return null;
            if (_customNames.TryGetValue(device.Id, out string custom))
                return custom;
            return device.FullName;
        }

        public static string GetCustomName(Guid id)
        {
            _customNames.TryGetValue(id, out string name);
            return name;
        }

        public static void SetCustomName(Guid id, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                _customNames.Remove(id);
            else
                _customNames[id] = name.Trim();
        }

        public static void ClearCustomName(Guid id)
        {
            _customNames.Remove(id);
        }
    }
}
