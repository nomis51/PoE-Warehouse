using PoEW.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoEW.API.Models {
    public class League : Model {
        public string Name { get; set; }
        public string Realm { get; set; }
        public string Url { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool DelveEvent { get; set; }

        public League() { }

        public League(Dictionary<string, object> data) {
            if (data.ContainsKey("id")) {
                Name = data["id"].ToString();
            }

            if (data.ContainsKey("realm")) {
                Realm = data["realm"].ToString();
            }

            if (data.ContainsKey("url")) {
                Url = data["url"].ToString();
            }

            if (data.ContainsKey("startAt")) {
                DateTime time;

                if (DateTime.TryParse(data["startAt"].ToString(), out time)) {
                    StartTime = time;
                }
            }

            if (data.ContainsKey("endAt") && data["endAt"] != null) {
                DateTime time;

                if (DateTime.TryParse(data["endAt"].ToString(), out time)) {
                    EndTime = time;
                }
            }
        }
    }
}
