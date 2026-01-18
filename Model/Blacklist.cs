using System.IO;

namespace ClickLogger.Model
{
    public class Blacklist
    {
        // Each entry: (processName, windowTitle). windowTitle may be empty.
        private List<(string processName, string windowTitle)> _blacklistEntries = new List<(string, string)>();

        private bool _hasBlacklist = false;

        public void Initialize()
        {
            string blacklistFilePath = GetConfigFilePath();
            if (!File.Exists(blacklistFilePath))
            {
                _hasBlacklist = false;
                return;
            }

            try
            {
                using (var reader = new StreamReader(blacklistFilePath))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                            continue;

                        var parts = line.Split(',');
                        var process = parts.Length > 0 ? parts[0].Trim() : string.Empty;
                        var title = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                        if (!string.IsNullOrWhiteSpace(process))
                        {
                            _blacklistEntries.Add((process, title));
                        }
                    }
                }
                _hasBlacklist = _blacklistEntries.Count > 0;
            }
            catch
            {
                _hasBlacklist = false;
            }
        }

        public bool IsBlacklisted(string processName, string windowTitle = "")
        {
            if (!_hasBlacklist)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(processName))
            {
                return false;
            }

            foreach (var entry in _blacklistEntries)
            {
                // If entry has only processName, match any window with that processName
                if (string.IsNullOrWhiteSpace(entry.windowTitle))
                {
                    if (string.Equals(processName, entry.processName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                // If entry has both processName and windowTitle, require both to match
                else
                {
                    if (string.Equals(processName, entry.processName, StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrWhiteSpace(windowTitle)
                        && windowTitle.IndexOf(entry.windowTitle, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static string GetConfigFilePath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blacklist.csv");
        }
    }
}