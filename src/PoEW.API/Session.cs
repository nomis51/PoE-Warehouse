using Microsoft.EntityFrameworkCore.Internal;
using PoEW.API;
using PoEW.API.Models;
using PoEW.Data.Abstractions;
using PoEW.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PoEW.Data {
    public class Session {
        #region Singleton
        private static Session _instance;
        public static Session Instance() {
            if (_instance == null) {
                _instance = new Session();
            }

            return _instance;
        }
        #endregion

        private Dictionary<int, Shop> ShopThreads = new Dictionary<int, Shop>();
        private Dictionary<string, List<StashTab>> StashTabs = new Dictionary<string, List<StashTab>>();
        public Player Player { get; private set; }
        public int CurrentThreadId { get; private set; }
        private int SelectedTab = 0;
        private Dictionary<string, League> Leagues = new Dictionary<string, League>();

        private readonly PoEAPI _api = new PoEAPI();
        private readonly IDataStore _dataStore = new DataStore();

        private DateTime LastShopThreadUpdate = DateTime.Now;
        private int maxShopThreadUpdateRateInSeconds = 15;

        public bool IsLocalStashUpdaterPaused { get; set; } = false;
        private bool IsLocalStashUpdaterStarted = false;

        public bool IsShopThreadUpdaterStarted = false;

        public delegate void LocalStashTabsUpdated(Dictionary<int, StashTab> stashTabs);
        public static event LocalStashTabsUpdated OnLocalStashTabsUpdated;

        private Session() {
            Shop.OnRequestShopThreadUpdate += Shop_OnRequestShopThreadUpdate;
        }

        public Shop GetFirstShop() => AnyShops() ? ShopThreads.Values.FirstOrDefault() : null;

        public async Task<bool> CreateShopThread(string league) {
            if (!string.IsNullOrEmpty(league)) {
                int threadId = await _api.GenerateShopThread(league, Player);

                if (threadId != -1) {
                    await AddShop(threadId, league);
                    return true;
                }
            }

            return false;
        }

        private async void Shop_OnRequestShopThreadUpdate(Shop shop) {
            await _dataStore.DeleteAll<API.Models.Price>();

            await _dataStore.Save(shop.GetPrices().Select(v => new API.Models.Price() {
                Value = v.Value,
                ItemId = v.Key,
                ThreadId = shop.ThreadId
            }).ToList());

            await UpdateShopThread(shop);
        }

        public void SetSelectedTab(int index) {
            SelectedTab = index;
        }

        public int GetSelectedTabIndex() => SelectedTab;

        public void RunAutoOnlineUpdater() {
            Task.Run(async () => {
                while (true) {
                    await _api.RefreshOnlineStatus(Player);

                    // Every 4mins30s
                    Thread.Sleep((int)(4.5 * 60 * 1000));
                }
            });
        }

        public void RunLocalStashUpdater() {
            Task.Run(async () => {
                while (true) {
                    int threadId = CurrentThreadId;
                    await UpdateLocalStash(threadId);

                    if (!IsLocalStashUpdaterPaused && GetShop(threadId).League.Name == GetShop(CurrentThreadId).League.Name) {
                        OnLocalStashTabsUpdated(ShopThreads[threadId].GetStashTabs());
                    }

                    Thread.Sleep(1 * 60 * 1000);
                }
            });
        }

        public void RunShopThreadUpdater() {
            Task.Run(async () => {
                while (true) {
                    await UpdateShopThread(GetShop());

                    Thread.Sleep(1 * 60 * 1000);
                }
            });
        }

        public async Task UpdateLocalStash(int threadId) {
            var stashTabs = await _api.UpdateLocalStash(Player, ShopThreads[threadId].League);
            stashTabs = stashTabs.OrderBy(t => t.Index).ToList();

            if (StashTabs.ContainsKey(ShopThreads[threadId].League.Name)) {
                StashTabs[ShopThreads[threadId].League.Name] = stashTabs;
            } else {
                StashTabs.Add(ShopThreads[threadId].League.Name, stashTabs);
            }

            await _dataStore.Save(stashTabs, new string[] { "Items" });

            ShopThreads[threadId].ClearStashTabs();

            foreach (var tab in stashTabs) {
                if (tab.Items.Count > 0) {
                    await _dataStore.Save(tab.Items);
                }

                ShopThreads[threadId].AddStashTab(tab);
            }
        }

        public async Task LoadLocalStash() {
            var stashTabs = await _dataStore.Get<StashTab>(t => t.League == ShopThreads[CurrentThreadId].League.Name, new string[] { "Items" });
            stashTabs = stashTabs.OrderBy(t => t.Index).ToList();

            var prices = await _dataStore.Get<API.Models.Price>(p => p.ThreadId == CurrentThreadId);

            if (StashTabs.ContainsKey(ShopThreads[CurrentThreadId].League.Name)) {
                StashTabs[ShopThreads[CurrentThreadId].League.Name] = stashTabs;
            } else {
                StashTabs.Add(ShopThreads[CurrentThreadId].League.Name, stashTabs);
            }

            ShopThreads[CurrentThreadId].ClearStashTabs();

            foreach (var tab in stashTabs) {
                ShopThreads[CurrentThreadId].AddStashTab(tab);
            }

            ShopThreads[CurrentThreadId].ClearPrices();

            foreach (var price in prices) {
                ShopThreads[CurrentThreadId].SetPrice(price.ItemId, price.Value);
            }

            if (!IsLocalStashUpdaterStarted) {
                RunLocalStashUpdater();
                IsLocalStashUpdaterStarted = true;
            }

            // EDIT: Not really necessary, because we already have the automatic event when a price is modified in the UI
            //if (!IsShopThreadUpdaterStarted) {
            //    RunShopThreadUpdater();
            //    IsShopThreadUpdaterStarted = true;
            //}
        }

        public List<League> GetLeagues() => Leagues.Select(l => l.Value).ToList();

        public League GetLeague(string id) {
            if (Leagues.ContainsKey(id)) {
                return Leagues[id];
            }

            return null;
        }

        public bool AnyShops() => ShopThreads.Any();

        public bool AnyShops(int threadId) => ShopThreads.Keys.IndexOf(threadId) != -1;

        public async Task UpdateShopThread(Shop shop) {
            if ((DateTime.Now - LastShopThreadUpdate).TotalSeconds <= maxShopThreadUpdateRateInSeconds) {
                return;
            }

            await _api.UpdateShopThread(CurrentThreadId, Player, shop.ToString());
            LastShopThreadUpdate = DateTime.Now;
        }

        public void SetCurrentThreadId(int threadId) {
            CurrentThreadId = threadId;
        }

        private async Task SetLeagues() {
            var leagues = await _api.GetLeagues();

            foreach (var l in leagues) {
                Leagues.Add(l.Name, l);
            }
        }

        private async Task LoadPlayer() {
            var players = (await _dataStore.Get<Player>());

            if (players != null && players.Count > 0) {
                Player = players.Last();
            }
        }

        public async Task<Player> GetLastPlayer() {
            await LoadPlayer();

            return Player;
        }

        public async Task SetConnectedPlayer(Player player) {
            Player = player;

            if (string.IsNullOrEmpty(Player.AccountName)) {
                string accountName = await _api.GetAccountName(player.SessionId);

                if (!string.IsNullOrEmpty(accountName)) {
                    Player.AccountName = accountName;

                    if (!(await _dataStore.Get<Player>(p => p.AccountName == Player.AccountName)).Any()) {
                        await _dataStore.Save(Player);
                    }

                    await SetLeagues();
                }
            }
        }

        public List<Shop> GetShops() => ShopThreads.Values.ToList();

        public async Task LoadShops() {
            var shops = await _dataStore.Get<ShopThread>();

            foreach (var shop in shops) {
                ShopThreads.Add(shop.ThreadId, new Shop(shop.League, shop.ThreadId, shop.Title));
            }

            if (CurrentThreadId == 0 && AnyShops()) {
                CurrentThreadId = ShopThreads.Keys.FirstOrDefault();
            }
        }

        public async Task AddShop(int threadId, string leagueId) {
            if (!ShopThreads.ContainsKey(threadId)) {
                string title = await _api.GetShopThreadTitle(threadId);

                ShopThreads.Add(threadId, new Shop(Leagues[leagueId], threadId, title));

                await _dataStore.Insert(new ShopThread() {
                    ThreadId = threadId,
                    League = Leagues[leagueId],
                    Title = title
                });
            }
        }

        public Shop GetShop(int threadId) {
            if (ShopThreads.ContainsKey(threadId)) {
                return ShopThreads[threadId];
            }

            return null;
        }

        public Shop GetShop() {
            if (ShopThreads.ContainsKey(CurrentThreadId)) {
                return ShopThreads[CurrentThreadId];
            }

            return null;
        }

    }
}
