using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoEW.Data.Models {
    public class ItemRequirement : Model {
        public string Name { get; set; }
        public List<ItemRequirementValue> Values { get; set; } = new List<ItemRequirementValue>();
        public int DisplayMode { get; set; }
        public string Suffix { get; set; }

        public ItemRequirement() { }

        public ItemRequirement(Dictionary<string, object> data) {
            if (data.ContainsKey("name")) {
                Name = data["name"].ToString();
            }

            if (data.ContainsKey("values")) {
                var values = JsonConvert.DeserializeObject<List<List<object>>>(data["values"].ToString());
                Values = values.Select(v => new ItemRequirementValue(v)).ToList();
            }

            if (data.ContainsKey("displayMode")) {
                DisplayMode = Convert.ToInt32(data["displayMode"]);
            }

            if (data.ContainsKey("suffix")) {
                Suffix = data["suffix"].ToString();
            }
        }
    }
}
