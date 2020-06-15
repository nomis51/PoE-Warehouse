using PoEW.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoEW.API.Models {
    public class TabPrice : Model {
        public int ThreadId { get; set; }
        public int TabIndex { get; set; }
        public PoEW.Data.Price Price { get; set; }

        public TabPrice(int threadId, int tabIndex, PoEW.Data.Price price) {
            ThreadId = threadId;
            TabIndex = tabIndex;
            Price = price;
        }
    }
}
