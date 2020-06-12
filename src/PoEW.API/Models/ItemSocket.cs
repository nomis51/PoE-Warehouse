using System;
using System.Collections.Generic;
using System.Text;

namespace PoEW.Data.Models {
    public class ItemSocket : Model {
        public int Group { get; set; } = 0;
        public string Attribute { get; set; }
        public string Colour { get; set; }

        ItemSocket() { }

        public ItemSocket(Dictionary<string, object> data) {
            if (data != null) {
                if (data.ContainsKey("group")) {
                    Group = Convert.ToInt32(data["group"]);
                }

                if (data.ContainsKey("attr")) {
                    Attribute = data["attr"].ToString();
                }

                if (data.ContainsKey("sColour")) {
                    Colour = data["sColour"].ToString();
                }
            }
        }
    }
}
