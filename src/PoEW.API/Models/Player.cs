using System;
using System.Collections.Generic;
using System.Text;

namespace PoEW.Data.Models {
    public class Player : Model {
        public string SessionId { get; set; }
        public string AccountName { get; set; }
        public string OnlineCode { get; set; }

        public Player() { }
    }
}
