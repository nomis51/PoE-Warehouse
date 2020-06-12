using System;
using System.Collections.Generic;
using System.Text;

namespace PoEW.Data.Models {
    public class Position {
        public int X { get; set; }
        public int Y { get; set; }

        public Position() { }

        public Position(int x, int y) {
            X = x;
            Y = y;
        }

        public Position(string xy) {
            var splits = xy.Split(';');
            X = Convert.ToInt32(splits[0]);
            Y = Convert.ToInt32(splits[1]);
        }
    }
}
