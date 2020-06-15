using Microsoft.EntityFrameworkCore.Internal;
using PoEW.API;
using PoEW.API.Logging;
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
        public int SelectedTab { get; private set; } = 0;
        private Dictionary<string, League> Leagues = new Dictionary<string, League>();

        private readonly PoEAPI _api = new PoEAPI();
        private readonly IDataStore _dataStore = new DataStore();

        private DateTime LastShopThreadUpdate = DateTime.Now;
        private int maxShopThreadUpdateRateInSeconds = 15;
        private Mutex Lock_ShopThreadUpdate = new Mutex();

        private DateTime LastLocalStashUpdate = DateTime.Now.AddDays(-1);
        private int maxLocalStashUpdateRateInSeconds = 30;
        private Mutex Lock_LocalStashUpdate = new Mutex();

        public bool IsLocalStashUpdaterPaused { get; set; } = false;
        private bool IsLocalStashUpdaterStarted = false;

        public bool IsShopThreadUpdaterStarted = false;

        public delegate void LocalStashTabsUpdated(Dictionary<int, StashTab> stashTabs);
        public static event LocalStashTabsUpdated OnLocalStashTabsUpdated;

        private Session() {
            Shop.OnRequestShopThreadUpdate += Shop_OnRequestShopThreadUpdate;
        }

        public Shop GetFirstShop() => AnyShops() ? ShopThreads.Values.FirstOrDefault() : null;

        public async Task<int> CreateShopThread(string league) {
            if (!string.IsNullOrEmpty(league)) {
                MessageController.Instance().Log($"Generating new shop forum thread for {league} league...");
                return await _api.GenerateShopThread(league, Player);

            }

            return -1;
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
                MessageController.Instance().Log("Auto online started.");
                while (true) {
                    MessageController.Instance().Log("Refreshing online status...");
                    if (await _api.RefreshOnlineStatus(Player)) {
                        MessageController.Instance().Log("Online status refreshed.");
                    } else {
                        MessageController.Instance().Log("[Warn][Session] Refreshing online status failed.");
                    }

                    // Every 4mins30s
                    Thread.Sleep((int)(4.5 * 60 * 1000));
                }
            });
        }

        public void RunLocalStashUpdater() {
            Task.Run(async () => {
                MessageController.Instance().Log("Local Stash Updater started.");
                while (true) {
                    int threadId = CurrentThreadId;

                    if (await UpdateLocalStash(threadId)) {
                        if (!IsLocalStashUpdaterPaused && GetShop(threadId).League.Name == GetShop(CurrentThreadId).League.Name) {
                            MessageController.Instance().Log($"{GetShop(threadId).League.Name} local stash updated.");
                            OnLocalStashTabsUpdated(ShopThreads[threadId].GetStashTabs());
                        }
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

        public async Task<bool> UpdateLocalStash(int threadId) {
            Lock_LocalStashUpdate.WaitOne(1, true);
            if ((DateTime.Now - LastLocalStashUpdate).TotalSeconds <= maxLocalStashUpdateRateInSeconds) {
                Lock_LocalStashUpdate.ReleaseMutex();
                return false;
            }

            MessageController.Instance().Log($"Updating {GetShop(threadId).League.Name} local stash...");

            var stashTabs = await _api.UpdateLocalStash(Player, ShopThreads[threadId].League);

            if (stashTabs == null) {
                MessageController.Instance().Log("[Warn][Session] Updating local stash failed.");
                Lock_LocalStashUpdate.ReleaseMutex();
                return false;
            }

            stashTabs = stashTabs.OrderBy(t => t.Index).ToList();

            MessageController.Instance().Log($"{stashTabs.Count} {GetShop(threadId).League.Name} stash tab{(stashTabs.Count > 1 ? "s" : "")} received.");

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

            ShopThreads[threadId].ApplyWholeTabPrices();

            LastLocalStashUpdate = DateTime.Now;

            Lock_LocalStashUpdate.ReleaseMutex();

            return true;
        }

        public async Task LoadLocalStash() {
            MessageController.Instance().Log("Loading local stash tabs...");
            var stashTabs = await _dataStore.Get<StashTab>(t => t.League == ShopThreads[CurrentThreadId].League.Name, new string[] { "Items" });
            stashTabs = stashTabs.OrderBy(t => t.Index).ToList();
            MessageController.Instance().Log($"{stashTabs.Count} local stash tab{(stashTabs.Count > 1 ? "s" : "")} loaded.");


            MessageController.Instance().Log("Loading prices...");
            var prices = await _dataStore.Get<API.Models.Price>(p => p.ThreadId == CurrentThreadId);
            MessageController.Instance().Log($"{prices.Count} price{(prices.Count > 1 ? "s" : "")} loaded.");

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
                ShopThreads[CurrentThreadId].SetPrice(price.ItemId, price.Value, true);
            }

            var wholeTabPrices = await _dataStore.Get<TabPrice>();

            foreach (var tabPrice in wholeTabPrices) {
                ShopThreads[tabPrice.ThreadId].SetWholeTabPrice(tabPrice.TabIndex, tabPrice.Price);
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

        public List<League> GetAvailableLeagues() {
            var usedLeagues = ShopThreads.Values.Select(s => s.League.Name);
            return Leagues.Values.ToList().FindAll(l => usedLeagues.IndexOf(l.Name) == -1);
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
            Lock_ShopThreadUpdate.WaitOne(1, true);
            if ((DateTime.Now - LastShopThreadUpdate).TotalSeconds <= maxShopThreadUpdateRateInSeconds) {
                Lock_ShopThreadUpdate.ReleaseMutex();
                return;
            }

            if (await _api.UpdateShopThread(CurrentThreadId, Player, shop.ToString())) {
                LastShopThreadUpdate = DateTime.Now;
                MessageController.Instance().Log($"Shop thread {shop.ThreadId} {shop.Title} updated.");
            } else {
                MessageController.Instance().Log($"[Warn][Session] Updating shop thread {shop.ThreadId} failed.");
            }

            Lock_ShopThreadUpdate.ReleaseMutex();
        }

        public void SetCurrentThreadId(int threadId) {
            CurrentThreadId = threadId;
        }

        private async Task<bool> SetLeagues() {
            MessageController.Instance().Log("Retrieving leagues...");
            var leagues = await _api.GetLeagues();

            if (leagues == null) {
                return false;
            }

            foreach (var l in leagues) {
                Leagues.Add(l.Name, l);
            }

            return true;
        }

        private async Task LoadPlayer() {
            MessageController.Instance().Log("Loading player data...");
            var players = (await _dataStore.Get<Player>());

            if (players != null && players.Count > 0) {
                Player = players.Last();
                MessageController.Instance().Log($"Player {Player.AccountName} loaded.");
            } else {
                MessageController.Instance().Log("No player data available.");
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

                    MessageController.Instance().Log($"{player.AccountName} authenticated.");

                    if (!(await SetLeagues())) {
                        MessageController.Instance().Log("[Warn][Session] Retrieving leagues failed.");
                    }
                } else {
                    MessageController.Instance().Log("[Warn][Session] Authentication failed.");
                }
            }
        }

        public List<Shop> GetShops() => ShopThreads.Values.ToList();

        public async Task LoadShops() {
            MessageController.Instance().Log("Loading shops...");
            var shops = await _dataStore.Get<ShopThread>();

            foreach (var shop in shops) {
                ShopThreads.Add(shop.ThreadId, new Shop(shop.League, shop.ThreadId, shop.Title));
                MessageController.Instance().Log($"Shop {shop.Title} of {shop.League.Name} league loaded.");
            }

            if (CurrentThreadId == 0 && AnyShops()) {
                CurrentThreadId = ShopThreads.Keys.FirstOrDefault();
                MessageController.Instance().Log($"Shop {GetShop().Title} of {GetShop().League.Name} league selected.");
            }
        }

        public async Task AddShop(int threadId, string leagueId, bool generateNewThread = false) {
            MessageController.Instance().Log("Adding new shop...");

            if (!ShopThreads.ContainsKey(threadId)) {
                if (generateNewThread) {
                    threadId = await CreateShopThread(leagueId);
                }

                string title = await _api.GetShopThreadTitle(threadId);

                ShopThreads.Add(threadId, new Shop(Leagues[leagueId], threadId, title));

                await _dataStore.Insert(new ShopThread() {
                    ThreadId = threadId,
                    League = Leagues[leagueId],
                    Title = title
                });

                MessageController.Instance().Log($"Shop {threadId} {title} of {leagueId} league created.");
            }
        }

        public Shop GetShop(int threadId) {
            if (ShopThreads.ContainsKey(threadId)) {
                return ShopThreads[threadId];
            }

            return null;
        }

        public Shop GetShop(string league) {
            return ShopThreads.Values.FirstOrDefault(s => s.League.Name == league);
        }

        public Shop GetShop() {
            if (ShopThreads.ContainsKey(CurrentThreadId)) {
                return ShopThreads[CurrentThreadId];
            }

            return null;
        }

        public async void SetWholeTabPrice(int tabIndex, Price price) {
            await _dataStore.Save(new TabPrice(CurrentThreadId, tabIndex, price));
            GetShop().SetWholeTabPrice(tabIndex, price);
            OnLocalStashTabsUpdated(GetShop().GetStashTabs());
        }

        public async void UnsetWholeTabPrice(int tabIndex) {
            await _dataStore.Delete<TabPrice>(GetShop().GetWholeTabPrice(tabIndex).Id);
            GetShop().UnsetWholeTabPrice(tabIndex);
            OnLocalStashTabsUpdated(GetShop().GetStashTabs());
        }
    }
}
