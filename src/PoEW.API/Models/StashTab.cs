using Newtonsoft.Json;
using PoEW.API.Models;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;

namespace PoEW.Data.Models {
    public class StashTab : Model {
        public string Name { get; set; }
        public int Index { get; set; }
        public string Type { get; set; }
        public bool Hidden { get; set; } = false;
        public bool Selected { get; set; } = false;
        public Color Colour { get; set; }
        public string League { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();

        public StashTab() { }

        public StashTab(Dictionary<string, object> data, string league) {
            Name = data["n"].ToString();
            Index = Convert.ToInt32(data["i"]);
            Id = data["id"].ToString();
            Type = data["type"].ToString();
            Hidden = Convert.ToBoolean(data["hidden"]);
            Selected = Convert.ToBoolean(data["selected"]);
            Colour = new Color(JsonConvert.DeserializeObject<Dictionary<string, object>>(data["colour"].ToString()));
            League = league;
        }

    }
}
