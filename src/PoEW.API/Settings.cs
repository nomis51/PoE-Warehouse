using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace PoEW.API {
    public class Settings {
        #region Singleton
        private static Settings _instance;
        public static Settings Data() {
            if (_instance == null) {
                _instance = new Settings();
            }

            return _instance;
        }
        #endregion

        private const string FilePath = ".\\settings.json";

        private Dictionary<string, object> Values = new Dictionary<string, object>();

        private Settings() {
            ReadSettingsFile();
        }

        private void ReadSettingsFile() {
            Values = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(FilePath));
        }

        public T Get<T>(string key) {
            if (Values.ContainsKey(key)) {
                return (T)Convert.ChangeType(Values[key], typeof(T));
            }

            return default(T);
        }
    }
}
