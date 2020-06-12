using System;
using System.Collections.Generic;
using System.Text;
using PoEW.API;
using PoEW.Data;
using PoEW.Data.Models;

namespace PoEW.API.Models {
    public class Price : Model {
        public int ThreadId { get; set; }
        public PoEW.Data.Price Value { get; set; }
        public string ItemId { get; set; }

        public Price() {
            Id = Guid.NewGuid().ToString();
        }
    }
}
