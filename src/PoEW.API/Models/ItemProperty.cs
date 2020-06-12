using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoEW.Data.Models {
    public class ItemProperty : Model {
        public string Name { get; set; }
        public List<ItemPropertyValue> Values { get; set; }
        public int DisplayMode { get; set; }
        public int Type { get; set; }
        public double Progress { get; set; }

        public ItemProperty() { }

        public ItemProperty(Dictionary<string, object> data) {
            if (data.ContainsKey("name")) {
                Name = data["name"].ToString();
            }

            if (data.ContainsKey("values")) {
                var values = JsonConvert.DeserializeObject<List<List<object>>>(data["values"].ToString());
                Values = values.Select(v => new ItemPropertyValue(v)).ToList();
            }

            if (data.ContainsKey("displayMode")) {
                DisplayMode = Convert.ToInt32(data["displayMode"]);
            }

            if (data.ContainsKey("type")) {
                Type = data.ContainsKey("type") ? Convert.ToInt32(data["type"]) : -1;
            }

            if (data.ContainsKey("progress")) {
                Progress = Convert.ToDouble(data["progress"]);
            }
        }

        public override string ToString() {
            string content = Name + (Values.Count == 0 ? "" : ": ");
            bool calculatedName = Name.IndexOf("%0") != -1;

            for (int i = 0; i < Values.Count; ++i) {
                if (!calculatedName) {
                    content += Values[i].Value;
                } else {
                    content = content.Replace($"%{i}", Values[i].Value);
                }
            }

            return content;
        }
    }
}
