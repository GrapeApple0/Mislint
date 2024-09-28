using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mislint.Core
{
    public class Settings
    {
        public static Settings Instance { get; } = new Settings();
        private readonly string _settingsDir = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\mislint\";
        public class SettingJsonClass
        {
            public string Host { get; set; }
            public string Token { get; set; }
        }

        public SettingJsonClass Setting { get; }

        private Settings()
        {
            if (!System.IO.Directory.Exists(_settingsDir)) System.IO.Directory.CreateDirectory(_settingsDir);
            if (!System.IO.File.Exists(_settingsDir + "settings.json")) System.IO.File.WriteAllText(_settingsDir + "settings.json", JsonSerializer.Serialize(new SettingJsonClass()));
            this.Setting = JsonSerializer.Deserialize<SettingJsonClass>(System.IO.File.ReadAllText(_settingsDir + "settings.json"));
        }

        public void Save()
        {
            System.IO.File.WriteAllText(_settingsDir + "settings.json", JsonSerializer.Serialize(this.Setting));
        }
    }
}
