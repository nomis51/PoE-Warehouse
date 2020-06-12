using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace PoEW.Data.Models {
    public class Item : Model {
        public bool Verified { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string IconUrl { get; set; }
        public string League { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public bool Identified { get; set; } = true;
        public int ItemLevel { get; set; }
        public List<ItemProperty> Properties { get; set; } = new List<ItemProperty>();
        public string Description { get; set; }
        public int FrameType { get; set; }
        public Position Position { get; set; }
        public string InventoryId { get; set; }
        public List<string> ExplicitMods { get; set; } = new List<string>();
        public List<string> ImplicitMods { get; set; } = new List<string>();
        public List<string> EnchantMods { get; set; } = new List<string>();
        public List<string> FlavourText { get; set; }
        public List<ItemSocket> Sockets { get; set; } = new List<ItemSocket>();
        public bool Corrupted { get; set; } = false;
        public int Size { get; set; }
        public int MaxSize { get; set; }
        public List<ItemRequirement> Requirements { get; set; } = new List<ItemRequirement>();
        public List<ItemRequirement> NextLevelRequirements { get; set; } = new List<ItemRequirement>();
        public List<Item> SocketedItems { get; set; } = new List<Item>();
        public bool IsSupportGem { get; set; } = false;
        public List<ItemProperty> AddtionalProperties { get; set; } = new List<ItemProperty>();
        public int TalismanTier { get; set; } = 0;
        public string ArtFileName { get; set; }

        public Item() { }

        public Item(Dictionary<string, object> data) {
            if (data.ContainsKey("verified")) {
                Verified = Convert.ToBoolean(data["verified"]);
            }

            if (data.ContainsKey("w")) {
                Width = Convert.ToInt32(data["w"]);
            }

            if (data.ContainsKey("h")) {
                Height = Convert.ToInt32(data["h"]);
            }

            if (data.ContainsKey("icon")) {
                IconUrl = data["icon"].ToString();
            }

            if (data.ContainsKey("league")) {
                League = data["league"].ToString();
            }

            if (data.ContainsKey("id")) {
                Id = data["id"].ToString();
            }

            if (data.ContainsKey("typeLine")) {
                Type = data["typeLine"].ToString();
            }

            if (data.ContainsKey("name")) {
                Name = data["name"].ToString();
            }

            if (data.ContainsKey("identified")) {
                Identified = Convert.ToBoolean(data["identified"]);
            }

            if (data.ContainsKey("ilvl")) {
                ItemLevel = Convert.ToInt32(data["ilvl"]);
            }

            if (data.ContainsKey("properties")) {
                var properties = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(data["properties"].ToString());
                Properties = properties.Select(p => new ItemProperty(p)).ToList();
            }

            if (data.ContainsKey("descrText")) {
                Description = data["descrText"].ToString() + (data.ContainsKey("secDescrText") ? " " + data["secDescrText"].ToString() : "");
            }

            if (data.ContainsKey("frameType")) {
                FrameType = Convert.ToInt32(data["frameType"]);
            }

            if (data.ContainsKey("x") && data.ContainsKey("y")) {
                Position = new Position(Convert.ToInt32(data["x"]), Convert.ToInt32(data["y"]));
            }

            if (data.ContainsKey("inventoryId")) {
                InventoryId = data["inventoryId"].ToString();
            }

            if (data.ContainsKey("explicitMods") && data["explicitMods"] != null) {
                ExplicitMods = JsonConvert.DeserializeObject<List<string>>(data["explicitMods"].ToString());
            }

            if (data.ContainsKey("implicitMods") && data["implicitMods"] != null) {
                ImplicitMods = JsonConvert.DeserializeObject<List<string>>(data["implicitMods"].ToString());
            }

            if (data.ContainsKey("flavourText") && data["flavourText"] != null) {
                FlavourText = JsonConvert.DeserializeObject<List<string>>(data["flavourText"].ToString());
            }

            if (data.ContainsKey("sockets") && data["sockets"] != null) {
                var sockets = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(data["sockets"].ToString());
                Sockets = sockets.Select(s => new ItemSocket(s)).ToList();
            }

            if (data.ContainsKey("corrupted")) {
                Corrupted = Convert.ToBoolean(data["corrupted"]);
            }

            if (data.ContainsKey("stackSize")) {
                Size = Convert.ToInt32(data["stackSize"]);
            }

            if (data.ContainsKey("maxStackSize")) {
                MaxSize = Convert.ToInt32(data["maxStackSize"]);
            }

            if (data.ContainsKey("requirements")) {
                var requirements = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(data["requirements"].ToString());
                Requirements = requirements.Select(r => new ItemRequirement(r)).ToList();
            }

            if (data.ContainsKey("nextLevelRequirements")) {
                var nextLevelRequirements = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(data["nextLevelRequirements"].ToString());
                NextLevelRequirements = nextLevelRequirements.Select(r => new ItemRequirement(r)).ToList();
            }

            if (data.ContainsKey("socketedItems")) {
                var socketedItems = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(data["socketedItems"].ToString());
                SocketedItems = socketedItems.Select(i => new Item(i)).ToList();
            }

            if (data.ContainsKey("enchantMods")) {
                EnchantMods = JsonConvert.DeserializeObject<List<string>>(data["enchantMods"].ToString());
            }

            if (data.ContainsKey("support")) {
                IsSupportGem = Convert.ToBoolean(data["support"]);
            }

            if (data.ContainsKey("additionalProperties")) {
                var additionalProperties = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(data["additionalProperties"].ToString());
                AddtionalProperties = additionalProperties.Select(p => new ItemProperty(p)).ToList();
            }

            if (data.ContainsKey("talismanTier")) {
                TalismanTier = Convert.ToInt32(data["talismanTier"]);
            }

            if (data.ContainsKey("artFilename")) {
                ArtFileName = data["artFilename"].ToString();
            }
        }
    }
}
