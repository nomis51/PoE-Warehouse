using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoEW.API {
    public class UrlQuery {
        Dictionary<string, string> Params = new Dictionary<string, string>();

        public UrlQuery() { }

        public void Add(string key, string value) {
            if (Params.ContainsKey(key)) {
                Params[key] = value;
            } else {
                Params.Add(key, value);
            }
        }

        public void Remove(string key) {
            Params.Remove(key);
        }

        public string Build(bool inUrl = false) {
            var result = Params.Aggregate(inUrl ? "?" : "", (acc, f) => acc + $"{f.Key}={f.Value}&");
            return result.Substring(0, result.Length - 1);
        }
    }
}
