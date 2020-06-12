using PoEW.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoEW.API.Models {
    public class ShopThread : Model {
        public int ThreadId { get; set; }
        public League League { get; set; }

        public string Title { get; set; }

        public ShopThread() {
            Id = Guid.NewGuid().ToString();
        }
    }
}
