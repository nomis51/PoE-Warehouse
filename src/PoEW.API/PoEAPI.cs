using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using PoEW.API.Logging;
using PoEW.API.Models;
using PoEW.Data;
using PoEW.Data.Abstractions;
using PoEW.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PoEW.API {
    public class PoEAPI {
        private string BaseUrlTemplate_EditThread = "forum/edit-thread/$threadId$";
        private string BaseUrlTemplate_VerifyPoETrade = "$threadId$/aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
        private string Url_VerifyPoETrade = "http://verify.poe.trade/";
        private string Url_MainPoEPage = "https://www.pathofexile.com/";
        private string Url_ApiPoE = "http://api.pathofexile.com/";
        private string BaseUrl_Characters = "character-window/get-characters";
        private string Url_CharactersItems = "https://www.pathofexile.com/character-window/get-items";
        private string BaseUrl_StashItems = "character-window/get-stash-items";
        private string BaseUrl_Account = "my-account";
        private string BaseUrl_Leagues = "leagues?type=main&compact=1";
        private string Url_OnlineControl = "http://control.poe.trade/";
        private string BaseUrl_ViewThread = "forum/view-thread/$threadId$";
        private string BaseUrl_Forum = "/forum";
        private string BaseUrlTemplate_NewThread = "forum/new-thread/$forumId$";

        private Regex RegFindForumShops = new Regex("<a href=\"\\/forum\\/view-forum\\/([0-9]+|((standard|hardcore)-trading-shops))\">(Hardcore )*[a-z]+ (League|Trading) - Shops<\\/a>", RegexOptions.IgnoreCase);

        private string PoESessionIdCookieName = "POESESSID";

        public PoEAPI() {
        }

        private async Task<string> GetForumPage() {
            if (Settings.Data().Get<bool>("Debug")) {
                MessageController.Instance().Log("[Debug][API] Retrieving forum page...");
            }

            var uri = new Uri(Url_MainPoEPage);
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = uri }) {
                try {
                    return await client.GetStringAsync(BaseUrl_Forum);
                } catch (HttpRequestException e) {
                    MessageController.Instance().Log($"[Error][API] Error while retrieving forum page: {e.Message}");

                    return null;
                }
            }
        }

        private string GetForumId(string forumHtml, string league) {
            var matches = RegFindForumShops.Matches(forumHtml);

            foreach (Match match in matches) {
                if (match.Success) {
                    string forumPath = Utils.FindTextBetween(match.Value, "<a href=\"", "\">");
                    string forumLeague = Utils.FindTextBetween(match.Value, "\">", " - Shops</a>");
                    forumLeague = forumLeague.Replace(" Trading", "").Replace(" League", "");

                    if (forumLeague == league) {
                        return forumPath.Substring(forumPath.LastIndexOf("/") + 1);
                    }
                }
            }

            return null;
        }

        private async Task<string> GetNewThreadPage(string forumId, string sessionId) {
            if (Settings.Data().Get<bool>("Debug")) {
                MessageController.Instance().Log($"[Debug][API] Retrieving the new-thread page for forum section {forumId}...");
            }

            var uri = new Uri(Url_MainPoEPage);
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = uri }) {
                cookieContainer.Add(uri, new Cookie(PoESessionIdCookieName, sessionId));
                try {
                    return await client.GetStringAsync(BaseUrlTemplate_NewThread.Replace("$forumId$", forumId));
                } catch (HttpRequestException e) {
                    MessageController.Instance().Log($"[Error][API] Error while retrieving the new-thread page for forum section {forumId}: {e.Message}");

                    return null;
                }
            }
        }

        public async Task<int> GenerateShopThread(string league, Player player) {
            string html = await GetForumPage();
            string forumId = GetForumId(html, league);

            if (!string.IsNullOrEmpty(forumId)) {
                var newThreadPage = await GetNewThreadPage(forumId, player.SessionId);

                string csrfToken = Utils.GetCsrfToken(newThreadPage, "hash");

                UrlQuery data = new UrlQuery();
                data.Add("hash", csrfToken);
                data.Add("title", $"{player.AccountName}'s {league} Shop");
                data.Add("content", "reserved");
                data.Add("submit", "Submit");

                var uri = new Uri(Url_MainPoEPage);
                var cookieContainer = new CookieContainer();
                using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                using (var client = new HttpClient(handler) { BaseAddress = uri }) {
                    cookieContainer.Add(uri, new Cookie(PoESessionIdCookieName, player.SessionId));
                    var baseUrl = BaseUrlTemplate_NewThread.Replace("$forumId$", forumId);
                    try {
                        var response = await client.PostAsync(baseUrl, new StringContent(data.Build(), Encoding.UTF8, "application/x-www-form-urlencoded"));

                        if (response.IsSuccessStatusCode) {
                            return Convert.ToInt32(response.RequestMessage.RequestUri.Segments.Last());
                        }

                        MessageController.Instance().Log($"[Error][API] Error while generating a new forum thread: HTTP{response.StatusCode} {response.ReasonPhrase}");
                    } catch (HttpRequestException e) {
                        MessageController.Instance().Log($"[Error][API] Error while generating a new forum thread: {e.Message}");

                        return -1;
                    }
                }
            }

            return -1;
        }

        public async Task<string> GetShopThreadTitle(int threadId) {
            if (Settings.Data().Get<bool>("Debug")) {
                MessageController.Instance().Log($"[Debug][API] Retrieving thread title of thread {threadId}...");
            }

            var uri = new Uri(Url_MainPoEPage);
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = uri }) {
                try {
                    var html = await client.GetStringAsync(BaseUrl_ViewThread.Replace("$threadId$", threadId.ToString()));

                    string title = Utils.FindTextBetween(html, "<h1 class=\"topBar last layoutBoxTitle\">", "</h1>");
                    return title;
                } catch (HttpRequestException e) {
                    MessageController.Instance().Log($"[Error][API] Error while retrieving thread title of thread {threadId}: {e.Message}");

                    return null;
                }
            }
        }

        public async Task RefreshOnlineStatus(Player player) {
            if (!string.IsNullOrEmpty(player.OnlineCode)) {
                if (!await VerifyOnlineStatus(player.OnlineCode)) {
                    await BecomeOnline(player.OnlineCode);
                    return;
                }

                var uri = new Uri(Url_OnlineControl);
                var cookieContainer = new CookieContainer();
                using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
                using (var client = new HttpClient(handler) { BaseAddress = uri }) {
                    var response = await client.PostAsync($"{player.OnlineCode}/online-league", null);

                    if (!response.IsSuccessStatusCode) {
                        MessageController.Instance().Log($"[Error][API] Error while refreshing online status: HTTP{response.StatusCode} {response.ReasonPhrase}");
                    }
                }
            }
        }



        private async Task<bool> BecomeOnline(string onlineCode) {
            if (Settings.Data().Get<bool>("Debug")) {
                MessageController.Instance().Log("[Debug][API] Auto-Online Status: Going from Offline to Online");
            }

            var uri = new Uri(Url_OnlineControl);
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = uri }) {
                var response = await client.PostAsync(onlineCode, null);

                if (!response.IsSuccessStatusCode) {
                    MessageController.Instance().Log($"[Error][API] Error while going from Offline to Online: HTTP{response.StatusCode} {response.ReasonPhrase}");
                }

                return response.IsSuccessStatusCode;
            }
        }

        private async Task<bool> VerifyOnlineStatus(string onlineCode) {
            if (Settings.Data().Get<bool>("Debug")) {
                MessageController.Instance().Log("[Debug][API] Verifying Online status...");
            }

            var uri = new Uri(Url_OnlineControl);
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = uri }) {
                try {
                    var response = await client.GetStringAsync(onlineCode);
                    return response.IndexOf("You are not online. Or maybe you are. Who knows.") == -1;
                } catch (HttpRequestException e) {
                    MessageController.Instance().Log($"[Error][API] Error while verifying online status: {e.Message}");
                    return false;
                }
            }
        }

        public async Task NotifyPoETrade(int threadId, string sessionId) {
            if (Settings.Data().Get<bool>("Debug")) {
                MessageController.Instance().Log("[Debug][API] Notifying poe.trade for thread indexing...");
            }

            var uri = new Uri(Url_VerifyPoETrade);
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = uri }) {
                cookieContainer.Add(uri, new Cookie(PoESessionIdCookieName, sessionId));

                try {
                    var response = await client.GetStringAsync(BaseUrlTemplate_VerifyPoETrade.Replace("$threadId$", threadId.ToString()));
                } catch (HttpRequestException e) {
                    MessageController.Instance().Log($"[Error][API] Error while notifying poe.trade for thread indexing: {e.Message}");
                }
            }
        }

        public async Task<List<League>> GetLeagues() {
            List<League> leagues = new List<League>();

            string raw = await GetLeaguesPage();
            var data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(raw);

            foreach (var d in data) {
                if (d.ContainsKey("id") && d["id"].ToString().IndexOf("SSF") == -1) {
                    leagues.Add(new League(d));
                }
            }

            return leagues;
        }

        private async Task<string> GetLeaguesPage() {
            if (Settings.Data().Get<bool>("Debug")) {
                MessageController.Instance().Log("[Debug][API] Retrieving leagues...");
            }

            var uri = new Uri(Url_ApiPoE);
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = uri }) {
                try {
                    return await client.GetStringAsync(BaseUrl_Leagues);
                } catch (HttpRequestException e) {
                    MessageController.Instance().Log($"[Error][API] Error while retrieving leagues: {e.Message}");
                    return null;
                }
            }
        }

        public async Task<string> GetAccountName(string sessionId) {
            if (!string.IsNullOrEmpty(sessionId)) {
                string html = await GetAccountPage(sessionId);

                if (html == null) {
                    return null;
                }

                return Utils.FindTextBetween(html, "<a href=\"/account/view-profile/", "\">");
            }

            return null;
        }

        private async Task<string> GetAccountPage(string sessionId) {
            if (Settings.Data().Get<bool>("Debug")) {
                MessageController.Instance().Log("[Debug][API] Retrieving account page...");
            }

            var uri = new Uri(Url_MainPoEPage);
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = uri }) {
                cookieContainer.Add(uri, new Cookie(PoESessionIdCookieName, sessionId));

                try {
                    return await client.GetStringAsync(BaseUrl_Account);
                } catch (HttpRequestException e) {
                    MessageController.Instance().Log($"[Error][API] Error while retieving account page: {e.Message}");
                    return null;
                }
            }
        }

        public async Task UpdateShopThread(int threadId, Player player, string content) {
            string editThreadPage = await GetEditThreadPage(threadId, player.SessionId);

            if (editThreadPage == null) {
                return;
            }

            string csrfToken = Utils.GetCsrfToken(editThreadPage, "hash");

            if (string.IsNullOrEmpty(csrfToken)) {
                return;
            }

            string title = Utils.FindTextBetween(editThreadPage, "<input type=\"text\" name=\"title\" id=\"title\" onkeypress=\"return&#x20;event.keyCode&#x21;&#x3D;13\" value=\"", "\">");

            UrlQuery data = new UrlQuery();
            data.Add("hash", csrfToken);
            data.Add("title", title);
            data.Add("content", content);
            data.Add("submit", "Submit");

            var uri = new Uri(Url_MainPoEPage);
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = uri }) {
                cookieContainer.Add(uri, new Cookie(PoESessionIdCookieName, player.SessionId));
                var baseUrl = BaseUrlTemplate_EditThread.Replace("$threadId$", threadId.ToString());
                try {
                    var response = await client.PostAsync(baseUrl, new StringContent(data.Build(), Encoding.UTF8, "application/x-www-form-urlencoded"));

                    if (response.IsSuccessStatusCode) {
                        await NotifyPoETrade(threadId, player.SessionId);
                    }
                } catch (HttpRequestException e) {
                    MessageController.Instance().Log($"[Error][API] Error while updating shop thread {threadId}: {e.Message}");
                    return;
                }
            }
        }

        public async Task<string> GetEditThreadPage(int threadId, string sessionId) {
            if (Settings.Data().Get<bool>("Debug")) {
                MessageController.Instance().Log($"[Debug][API] Retrieving edit-thread page for thread {threadId}...");
            }

            var uri = new Uri(Url_MainPoEPage);
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = uri }) {
                cookieContainer.Add(uri, new Cookie(PoESessionIdCookieName, sessionId));
                try {
                    return await client.GetStringAsync(BaseUrlTemplate_EditThread.Replace("$threadId$", threadId.ToString()));
                } catch (HttpRequestException e) {
                    MessageController.Instance().Log($"[Error][API]  retrieving edit-thread page for thread {threadId}: {e.Message}");
                    return null;
                }
            }
        }

        public async Task<List<StashTab>> UpdateLocalStash(Player player, League league) {
            var stashTabs = await GetStashTabs(league.Name, player);
            return stashTabs;
        }

        public async Task<List<StashTab>> GetStashTabs(string league, Player player) {
            if (Settings.Data().Get<bool>("Debug")) {
                MessageController.Instance().Log($"[Debug][API] Retrieving stash tabs for league {league}...");
            }

            List<StashTab> stashTabs = new List<StashTab>();

            var uri = new Uri(Url_MainPoEPage);
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = uri }) {
                cookieContainer.Add(uri, new Cookie(PoESessionIdCookieName, player.SessionId));

                UrlQuery query = new UrlQuery();
                query.Add("league", league);
                query.Add("tabs", "1");
                query.Add("tabIndex", "0");
                query.Add("accountName", player.AccountName);
                try {
                    var data = await client.GetStringAsync(BaseUrl_StashItems + query.Build(true));
                    var parsedData = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
                    var tabsStr = parsedData["tabs"].ToString();
                    var parsedTabs = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(tabsStr);
                    stashTabs = parsedTabs.Select(t => new StashTab(t, league)).ToList();

                    var itemsStr = parsedData["items"].ToString();
                    var parsedItems = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(itemsStr);
                    List<Item> items = ParseItems(parsedItems);

                    stashTabs.Find(t => t.Index == 0).Items = items;
                } catch (HttpRequestException e) {
                    MessageController.Instance().Log($"[Error][API] Error while retrieving stash tabs for league {league}: {e.Message}");
                    return new List<StashTab>();
                }
            }

            foreach (var tab in stashTabs) {
                if (tab.Index > 0) {
                    tab.Items = await GetItems(tab.Index, league, player);
                }
            }

            return stashTabs;
        }

        public async Task<List<Item>> GetItems(int tabIndex, string league, Player player) {
            if (Settings.Data().Get<bool>("Debug")) {
                MessageController.Instance().Log($"[Debug][API] Retrieving items of tab #{tabIndex} of {league} league...");
            }

            var uri = new Uri(Url_MainPoEPage);
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = uri }) {
                cookieContainer.Add(uri, new Cookie(PoESessionIdCookieName, player.SessionId));

                UrlQuery query = new UrlQuery();
                query.Add("league", league);
                query.Add("tabs", "0");
                query.Add("tabIndex", tabIndex.ToString());
                query.Add("accountName", player.AccountName);

                try {
                    var data = await client.GetStringAsync(BaseUrl_StashItems + query.Build(true));
                    var parsedData = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);

                    var itemsStr = parsedData["items"].ToString();
                    var parsedItems = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(itemsStr);
                    List<Item> items = ParseItems(parsedItems);

                    return items;
                } catch (HttpRequestException e) {
                    MessageController.Instance().Log($"[Error][API] Error while retrieving items of tab #{tabIndex} of {league} league: {e.Message}");
                    return new List<Item>();
                }
            }
        }

        public List<Item> ParseItems(List<Dictionary<string, object>> data) {
            List<Item> items = new List<Item>();

            foreach (var d in data) {
                items.Add(new Item(d));
            }

            return items;
        }

        public async Task<List<Character>> GetCharacters(string sessionId) {
            if (Settings.Data().Get<bool>("Debug")) {
                MessageController.Instance().Log("[Debug][API] Retrieving characters...");
            }

            var uri = new Uri(Url_MainPoEPage);
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = uri }) {
                cookieContainer.Add(uri, new Cookie(PoESessionIdCookieName, sessionId));

                try {
                    var data = await client.GetStringAsync(BaseUrl_Characters);

                    if (data.Length == 0) {
                        return new List<Character>();
                    }

                    var parsedData = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(data);

                    return parsedData.Select(d => new Character(d)).ToList();
                } catch (HttpRequestException e) {
                    MessageController.Instance().Log($"[Error][API] Error while retrieving characters: {e.Message}");
                    return new List<Character>();
                }
            }
        }
    }
}
