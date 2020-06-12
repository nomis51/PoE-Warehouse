using System;
using System.Collections.Generic;
using System.Text;

namespace PoEW.Data.Models {
    public class Color {
        public int R { get; set; }
        public int G { get; set; }
        public int B { get; set; }

        public Color() { }

        public Color(Dictionary<string, object> data) {
            R = Convert.ToInt32(data["r"]);
            G = Convert.ToInt32(data["g"]);
            B = Convert.ToInt32(data["b"]);
        }

        public Color(string rgb) {
            var splits = rgb.Split(';');
            R = Convert.ToInt32(splits[0]);
            G = Convert.ToInt32(splits[1]);
            B = Convert.ToInt32(splits[2]);
        }
    }
}
