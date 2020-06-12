using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace PoEW.Data.Models {
    public class Character : Model {
        public string Name { get; set; }

        public string League { get; set; }

        public int ClassId { get; set; }

        public int AscendancyClass { get; set; }

        public string Class { get; set; }

        public int Level { get; set; }

        public long Experience { get; set; }

        public bool LastActive { get; set; } = false;

        public Character() { }

        public Character(Dictionary<string, object> data) {
            Name = data["name"].ToString();
            League = data["league"].ToString();
            ClassId = Convert.ToInt32(data["classId"]);
            AscendancyClass = Convert.ToInt32(data["ascendancyClass"]);
            Class = data["class"].ToString();
            Level = Convert.ToInt32(data["level"]);
            Experience = Convert.ToInt64(data["experience"]);
            LastActive = data.ContainsKey("lastActive") ? Convert.ToBoolean(data["lastActive"]) : false;
        }
    }
}
