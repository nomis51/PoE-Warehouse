using PoEW.Data.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoEW.Data.Models {
    public class Model : IModel {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
    }
}
