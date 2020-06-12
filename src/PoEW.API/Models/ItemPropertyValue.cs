using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoEW.Data.Models {
    public class ItemPropertyValue : Model {
        public string Value { get; set; }
        public bool DisplayOnly { get; set; }

        public ItemPropertyValue() { }

        public ItemPropertyValue(List<object> data) {
            if (data.Count >= 2) {
                Value = data[0].ToString();
                DisplayOnly = !Convert.ToBoolean(data[1]);
            }
        }
    }
}
