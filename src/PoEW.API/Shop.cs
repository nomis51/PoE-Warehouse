using Microsoft.Extensions.Primitives;
using PoEW.API.Models;
using PoEW.Data.Enums;
using PoEW.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PoEW.Data {
    public class Shop {
        #region CurrencyType/PriceType Mappings
        public static Dictionary<CurrencyType, string> CurrencyTypeToString = new Dictionary<CurrencyType, string>() {
            {CurrencyType.CURRENCY_NONE, ""},
            {CurrencyType.CURRENCY_ORB_OF_ALTERATION, "Orb of Alteration"},
            {CurrencyType.CURRENCY_ORB_OF_FUSING, "Orb of Fusing"},
            {CurrencyType.CURRENCY_ORB_OF_ALCHEMY, "Orb of Alchemy"},
            {CurrencyType.CURRENCY_CHAOS_ORB, "Chaos Orb"},
            {CurrencyType.CURRENCY_GCP, "Gemcutter's Prism"},
            {CurrencyType.CURRENCY_EXALTED_ORB, "Exalted Orb"},
            {CurrencyType.CURRENCY_CHROMATIC_ORB, "Chromatic Orb"},
            {CurrencyType.CURRENCY_JEWELLERS_ORB, "Jeweller's Orb"},
            {CurrencyType.CURRENCY_ORB_OF_CHANCE, "Orb of Chance"},
            {CurrencyType.CURRENCY_CARTOGRAPHERS_CHISEL, "Cartographer's Chisel"},
            {CurrencyType.CURRENCY_ORB_OF_SCOURING, "Orb of Scouring"},
            {CurrencyType.CURRENCY_BLESSED_ORB, "Blessed Orb"},
            {CurrencyType.CURRENCY_ORB_OF_REGRET, "Orb of Regret"},
            {CurrencyType.CURRENCY_REGAL_ORB, "Regal Orb"},
            {CurrencyType.CURRENCY_DIVINE_ORB, "Divine Orb"},
            {CurrencyType.CURRENCY_VAAL_ORB, "Vaal Orb"},
            {CurrencyType.CURRENCY_PERANDUS_COIN, "Perandus Coin"},
            {CurrencyType.CURRENCY_MIRROR_OF_KALANDRA, "Mirror of Kalandra"},
            {CurrencyType.CURRENCY_SILVER_COIN, "Silver Coin"},
        };
        public static Dictionary<string, CurrencyType> StringToCurrencyType = new Dictionary<string, CurrencyType>() {
            {"", CurrencyType.CURRENCY_NONE},
            {"Orb of Alteration", CurrencyType.CURRENCY_ORB_OF_ALTERATION},
            {"Orb of Fusing", CurrencyType.CURRENCY_ORB_OF_FUSING},
            {"Orb of Alchemy", CurrencyType.CURRENCY_ORB_OF_ALCHEMY},
            {"Chaos Orb", CurrencyType.CURRENCY_CHAOS_ORB},
            {"Gemcutter's Prism", CurrencyType.CURRENCY_GCP},
            {"Exalted Orb", CurrencyType.CURRENCY_EXALTED_ORB},
            {"Chromatic Orb", CurrencyType.CURRENCY_CHROMATIC_ORB},
            {"Jeweller's Orb", CurrencyType.CURRENCY_JEWELLERS_ORB},
            {"Orb of Chance", CurrencyType.CURRENCY_ORB_OF_CHANCE },
            {"Cartographer's Chisel", CurrencyType.CURRENCY_CARTOGRAPHERS_CHISEL },
            {"Orb of Scouring", CurrencyType.CURRENCY_ORB_OF_SCOURING},
            {"Blessed Orb", CurrencyType.CURRENCY_BLESSED_ORB},
            {"Orb of Regret", CurrencyType.CURRENCY_ORB_OF_REGRET},
            { "Regal Orb", CurrencyType.CURRENCY_REGAL_ORB},
            {"Divine Orb", CurrencyType.CURRENCY_DIVINE_ORB},
            {"Vaal Orb", CurrencyType.CURRENCY_VAAL_ORB },
            {"Perandus Coin", CurrencyType.CURRENCY_PERANDUS_COIN },
            {"Mirror of Kalandra", CurrencyType.CURRENCY_MIRROR_OF_KALANDRA},
            {"Silver Coin", CurrencyType.CURRENCY_SILVER_COIN},
        };
        public static Dictionary<CurrencyType, string> CurrencyTypeToTag = new Dictionary<CurrencyType, string>() {
            {CurrencyType.CURRENCY_NONE, ""},
            {CurrencyType.CURRENCY_ORB_OF_ALTERATION, "alt"},
            {CurrencyType.CURRENCY_ORB_OF_FUSING, "fuse"},
            {CurrencyType.CURRENCY_ORB_OF_ALCHEMY, "alch"},
            {CurrencyType.CURRENCY_CHAOS_ORB, "chaos"},
            {CurrencyType.CURRENCY_GCP, "gcp"},
            {CurrencyType.CURRENCY_EXALTED_ORB, "exa"},
            {CurrencyType.CURRENCY_CHROMATIC_ORB, "chrom"},
            {CurrencyType.CURRENCY_JEWELLERS_ORB, "jew"},
            {CurrencyType.CURRENCY_ORB_OF_CHANCE, "chance"},
            {CurrencyType.CURRENCY_CARTOGRAPHERS_CHISEL, "chisel"},
            {CurrencyType.CURRENCY_ORB_OF_SCOURING, "scour"},
            {CurrencyType.CURRENCY_BLESSED_ORB, "blessed"},
            {CurrencyType.CURRENCY_ORB_OF_REGRET, "regret"},
            {CurrencyType.CURRENCY_REGAL_ORB, "regal"},
            {CurrencyType.CURRENCY_DIVINE_ORB, "divine"},
            {CurrencyType.CURRENCY_VAAL_ORB, "vaal"},
            {CurrencyType.CURRENCY_PERANDUS_COIN, "coin"},
            {CurrencyType.CURRENCY_MIRROR_OF_KALANDRA, "mirror"},
            {CurrencyType.CURRENCY_SILVER_COIN, "silver"},
        };
        public static Dictionary<CurrencyType, string> CurrencyTypeToImageUrl = new Dictionary<CurrencyType, string>() {
        {CurrencyType.CURRENCY_NONE ,""},
{CurrencyType.CURRENCY_CHROMATIC_ORB ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyRerollSocketColours.png?v=9d377f2cf04a16a39aac7b14abc9d7c3"},
{CurrencyType.CURRENCY_ORB_OF_ALTERATION ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyRerollMagic.png?v=6d9520174f6643e502da336e76b730d3"},
{CurrencyType.CURRENCY_JEWELLERS_ORB ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyRerollSocketNumbers.png?v=2946b0825af70f796b8f15051d75164d"},
{CurrencyType.CURRENCY_ORB_OF_CHANCE ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyUpgradeRandomly.png?v=e4049939b9cd61291562f94364ee0f00"},
{CurrencyType.CURRENCY_CARTOGRAPHERS_CHISEL ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyMapQuality.png?v=f46e0a1af7223e2d4cae52bc3f9f7a1f"},
{CurrencyType.CURRENCY_PERANDUS_COIN ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyCoin.png?v=b971d7d9ea1ad32f16cce8ee99c897cf"},
{CurrencyType.CURRENCY_ORB_OF_FUSING ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyRerollSocketLinks.png?v=0ad7134a62e5c45e4f8bc8a44b95540f"},
{CurrencyType.CURRENCY_ORB_OF_ALCHEMY ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyUpgradeToRare.png?v=89c110be97333995522c7b2c29cae728"},
{CurrencyType.CURRENCY_BLESSED_ORB ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyImplicitMod.png?v=472eeef04846d8a25d65b3d4f9ceecc8"},
{CurrencyType.CURRENCY_ORB_OF_SCOURING ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyConvertToNormal.png?v=15e3ef97f04a39ae284359309697ef7d"},
{CurrencyType.CURRENCY_CHAOS_ORB ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyRerollRare.png?v=c60aa876dd6bab31174df91b1da1b4f9"},
{CurrencyType.CURRENCY_ORB_OF_REGRET ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyPassiveSkillRefund.png?v=1de687952ce56385b74ac450f97fcc33"},
{CurrencyType.CURRENCY_REGAL_ORB ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyUpgradeMagicToRare.png?v=1187a8511b47b35815bd75698de1fa2a"},
{CurrencyType.CURRENCY_VAAL_ORB ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyVaal.png?v=64114709d67069cd665f8f1a918cd12a"},
{CurrencyType.CURRENCY_GCP ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyGemQuality.png?v=f11792b6dbd2f5f869351151bc3a4539"},
{CurrencyType.CURRENCY_DIVINE_ORB ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyModValues.png?v=0ad99d4a2b0356a60fa8194910d80f6b"},
{CurrencyType.CURRENCY_EXALTED_ORB ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyAddModToRare.png?v=1745ebafbd533b6f91bccf588ab5efc5"},
{CurrencyType.CURRENCY_MIRROR_OF_KALANDRA ,"https://web.poecdn.com/image/Art/2DItems/Currency/CurrencyDuplicate.png?v=6fd68c1a5c4292c05b97770e83aa22bc"},
{CurrencyType.CURRENCY_SILVER_COIN ,"https://web.poecdn.com/image/Art/2DItems/Currency/SilverObol.png?v=93c1b204ec2736a2fe5aabbb99510bcf"},

        };
        public static Dictionary<PriceType, string> PriceTypeToTag = new Dictionary<PriceType, string>() {
            {PriceType.PRICE_TYPE_IGNORE, "[ignore]"},
            {PriceType.PRICE_TYPE_BUYOUT, "b/o"},
            {PriceType.PRICE_TYPE_FIXED, "price"},
            {PriceType.PRICE_TYPE_NO_PRICE, "no price"},
            {PriceType.PRICE_TYPE_CURRENT_OFFER, "c/o"},
            {PriceType.PRICE_TYPE_INHERIT, ""}
        };
        public static Dictionary<PriceType, string> PriceTypeToPrefix = new Dictionary<PriceType, string>() {
            {PriceType.PRICE_TYPE_IGNORE, ""},
            {PriceType.PRICE_TYPE_BUYOUT, "~b/o"},
            {PriceType.PRICE_TYPE_FIXED, "~price"},
            {PriceType.PRICE_TYPE_CURRENT_OFFER, "~c/o"},
            {PriceType.PRICE_TYPE_NO_PRICE, ""},
            {PriceType.PRICE_TYPE_INHERIT, ""}
        };
        public static Dictionary<string, PriceType> PrefixToPriceType = new Dictionary<string, PriceType>() {
            {"~b/o",PriceType.PRICE_TYPE_BUYOUT},
            {"~price",PriceType.PRICE_TYPE_FIXED},
            {"~c/o",PriceType.PRICE_TYPE_CURRENT_OFFER},
            {"",PriceType.PRICE_TYPE_NO_PRICE}
        };
        #endregion

        private Dictionary<int, StashTab> StashTabs = new Dictionary<int, StashTab>();
        private Dictionary<string, Item> Items = new Dictionary<string, Item>();
        private Dictionary<string, int> ItemIdToTabIndex = new Dictionary<string, int>();
        private Dictionary<string, Price> ItemIdToPrice = new Dictionary<string, Price>();
        private Dictionary<Price, List<string>> PriceToItems = new Dictionary<Price, List<string>>();
        private List<string> IgnoredItems = new List<string>();

        public League League { get; set; }
        public int ThreadId { get; set; }
        public string Title { get; set; }

        public delegate void RequestShopThreadUpdate(Shop shop);
        public static event RequestShopThreadUpdate OnRequestShopThreadUpdate;

        public Shop(League league, int threadId, string title) {
            League = league;
            ThreadId = threadId;
            Title = title;
        }

        public override string ToString() {
            string threadContent = "";

            foreach (var mapPriceItem in PriceToItems) {
                string spoiler = ToSpoiler(mapPriceItem.Key, mapPriceItem.Value.Select(itemId => Items[itemId]).ToList());

                if (spoiler.Length > 0) {
                    threadContent += spoiler;
                }
            }

            return threadContent.Length > 0 ? threadContent : "Reserved";
        }

        public Dictionary<string, Price> GetPrices() => ItemIdToPrice;

        private string ToSpoiler(Price price, List<Item> items) {
            string spoilerOpen = $"[spoiler=\"{PriceTypeToPrefix[price.Type]} {price.Value} {CurrencyTypeToTag[price.Currency]}\"]";

            string content = "";

            foreach (var item in items) {
                if (!IgnoredItems.Contains(item.Id)) {
                    int tabIndex = ItemIdToTabIndex[item.Id];
                    content += ToLinkItem(item, tabIndex);
                }
            }

            string spoilerClose = "[/spoiler]";

            return content.Length > 0 ? $"{spoilerOpen}{content}{spoilerClose}" : "";
        }

        private string ToLinkItem(Item item, int tabIndex) {
            return $"[linkItem location=\"Stash{tabIndex + 1}\" league=\"{League.Name}\" x=\"{item.Position.X}\" y=\"{item.Position.Y}\"]";
        }

        public void ClearStashTabs() {
            StashTabs.Clear();
            Items.Clear();
            ItemIdToTabIndex.Clear();
        }

        public void AddStashTab(StashTab stashTab) {
            StashTabs.Add(stashTab.Index, stashTab);

            foreach (var item in stashTab.Items) {
                Items.Add(item.Id, item);
                ItemIdToTabIndex.Add(item.Id, stashTab.Index);
            }
        }

        public void RemoveStashTab(int index) {
            if (StashTabs.ContainsKey(index)) {
                var stashTab = StashTabs[index];
                StashTabs.Remove(index);

                foreach (var item in stashTab.Items) {
                    Items.Remove(item.Id);
                    ItemIdToTabIndex.Remove(item.Id);
                }
            }
        }

        public void UpdateStashTab(StashTab stashTab) {
            if (StashTabs.ContainsKey(stashTab.Index)) {
                StashTabs[stashTab.Index] = stashTab;
            }
        }

        public Dictionary<int, StashTab> GetStashTabs() => StashTabs;

        public StashTab GetStashTab(int index) {
            if (StashTabs.ContainsKey(index)) {
                return StashTabs[index];
            }

            return null;
        }

        public Dictionary<string, int> GetStashTabsName() {
            Dictionary<string, int> names = new Dictionary<string, int>();

            foreach (var tab in StashTabs.Values) {
                names.Add(tab.Name, tab.Index);
            }

            return names;
        }

        public void AddItem(int stashTabIndex, Item item) {
            if (StashTabs.ContainsKey(stashTabIndex)) {
                StashTabs[stashTabIndex].Items.Add(item);
                Items.Add(item.Id, item);
                ItemIdToTabIndex.Add(item.Id, stashTabIndex);
            }
        }

        public void RemoveItem(string itemId) {
            if (Items.ContainsKey(itemId)) {
                Item item = Items[itemId];
                Items.Remove(itemId);
                int tabIndex = ItemIdToTabIndex[itemId];
                StashTabs[tabIndex].Items.Remove(item);
            }
        }

        public void UpdateItem(Item item) {
            if (Items.ContainsKey(item.Id)) {
                Items[item.Id] = item;
                int tabIndex = ItemIdToTabIndex[item.Id];
                int itemIndex = StashTabs[tabIndex].Items.IndexOf(item);
                StashTabs[tabIndex].Items[itemIndex] = item;
            }
        }

        public void ClearPrices() {
            ItemIdToPrice.Clear();
            PriceToItems.Clear();
        }

        public void SetPrice(string itemId, Price price, bool isActive) {
            Price oldPrice = null;
            if (ItemIdToPrice.ContainsKey(itemId)) {
                oldPrice = ItemIdToPrice[itemId];
                ItemIdToPrice[itemId] = price;
            } else {
                ItemIdToPrice.Add(itemId, price);
            }

            int index = -1;
            if (oldPrice != null && oldPrice != price) {
                if ((index = PriceToItems[oldPrice].FindIndex(id => id == itemId)) != -1) {
                    PriceToItems[oldPrice].Remove(itemId);

                    if (PriceToItems[oldPrice].Count == 0) {
                        PriceToItems.Remove(oldPrice);
                    }
                }
            }

            if (PriceToItems.ContainsKey(price)) {
                index = -1;
                if ((index = PriceToItems[price].FindIndex(id => id == itemId)) == -1) {
                    PriceToItems[price].Add(itemId);
                }
            } else {
                PriceToItems.Add(price, new List<string>() { itemId });
            }

            index = -1;
            if (!isActive && (index = IgnoredItems.IndexOf(itemId)) == -1) {
                IgnoredItems.Add(itemId);
            } else if (index != -1) {
                IgnoredItems.RemoveAt(index);
            }

            OnRequestShopThreadUpdate(this);
        }

        public void UnsetPrice(string itemId) {
            Price p = ItemIdToPrice[itemId];
            ItemIdToPrice.Remove(itemId);
            PriceToItems[p].Remove(itemId);
        }
    }
}
