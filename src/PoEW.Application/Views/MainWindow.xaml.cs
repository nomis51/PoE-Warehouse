using MahApps.Metro.Controls;
using PoEW.API;
using PoEW.API.Models;
using PoEW.Application.Views;
using PoEW.Data;
using PoEW.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Button = System.Windows.Controls.Button;
using Price = PoEW.Data.Price;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using ListViewItem = System.Windows.Controls.ListViewItem;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;
using CefSharp.Wpf;
using CefSharp;
using PoEW.API.Logging;
using System.Security.Policy;

namespace PoEW.Application {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow {
        private const string Url_PoETrade = "https://www.pathofexile.com/trade/search/$league$";
        private const string Url_PoENinja_Currency = "https://poe.ninja/$league$/currency";
        private const string Url_PlayerAccount = "https://www.pathofexile.com/my-account";
        private Regex regPoENinjaUrl = new Regex("https://poe.ninja/[a-z]+/(builds|currency)");
        private string LastUrlLoaded = "";

        ObservableCollection<HamburgerMenuItemBase> HamburderMenuItems = new ObservableCollection<HamburgerMenuItemBase>();
        HamburgerMenuHeaderItem HamburgerMenuHeader;
        HamburgerMenuSeparatorItem HamburgerMenuSeparator;

        public MainWindow() {
            InitializeComponent();
            Init();

            WindowController.Instance().LoginWin.ShowDialog();
        }

        private void Init() {
            MessageController.Instance().AddLogger(new FileLogger());
            MessageController.Instance().AddLogger(new ConsoleLogger(console));

            var lastPlayer = Session.Instance().GetLastPlayer().Result;

            if (lastPlayer != null) {
                WindowController.Instance().LoginWin.SetPlayer(lastPlayer);
            }

            InitHamburgerMenuItems();

            SetupEvents();
        }

        private async Task SetPoETradeBrowserCookie() {
            var cookieManager = Cef.GetGlobalCookieManager();
            Cookie cookie = new Cookie();
            cookie.Name = "POESESSID";
            cookie.Value = Session.Instance().Player.SessionId;
            await cookieManager.SetCookieAsync(Url_PoETrade.Replace("/search/$league$", ""), cookie);
            await cookieManager.SetCookieAsync(Url_PlayerAccount, cookie);
        }

        private void SetupEvents() {
            WindowController.Instance().LoginWin.Closed += LoginWin_Closed;
            WindowController.Instance().ShopFormWin.IsVisibleChanged += ShopFormWin_IsVisibleChanged;
            stashTabSelectorControl.OnStashTabSelected += StashTabSelectorControl_OnStashTabSelected;
            Session.OnLocalStashTabsUpdated += Session_OnLocalStashTabsUpdated;
            webBrowser_PoENinja_Builds.FrameLoadEnd += WebBrowser_FrameLoadEnd;
            webBrowser_PoENinja_Builds.FrameLoadStart += WebBrowser_FrameLoadStart;
            webBrowser_PoENinja_ChallengeCurrency.FrameLoadEnd += WebBrowser_FrameLoadEnd;
            webBrowser_PoENinja_ChallengeCurrency.FrameLoadStart += WebBrowser_FrameLoadStart;
            webBrowser_PoENinja_StandardCurrency.FrameLoadEnd += WebBrowser_FrameLoadEnd;
            webBrowser_PoENinja_StandardCurrency.FrameLoadStart += WebBrowser_FrameLoadStart;
            webBrowser_PoETrade.FrameLoadEnd += WebBrowser_FrameLoadEnd;
            webBrowser_PoETrade.FrameLoadStart += WebBrowser_FrameLoadStart;
            webBrowser_PoETrade.IsBrowserInitializedChanged += WebBrowser_PoETrade_IsBrowserInitializedChanged;
            console.OnErrorLogFound += Console_OnErrorLogFound;
        }

        private void Console_OnErrorLogFound(int nbErrors) {
            if (nbErrors > 0) {
                ShowConsoleErrorNotification(nbErrors);
            }
        }

        private void ShowConsoleErrorNotification(int nbErrors) {
            btnConsoleNbErrors.Content = nbErrors.ToString();
            btnConsoleNbErrors.ToolTip = $"There {(nbErrors > 1 ? "are" : "is")} {nbErrors} error{(nbErrors > 1 ? "s" :"")}! Open the console to see {(nbErrors > 1 ? "it" : "them")}.";
            btnConsoleNbErrors.Visibility = Visibility.Visible;
        }

        private void ShopFormWin_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if (WindowController.Instance().ShopFormWin.Visibility == Visibility.Hidden) {
                ShopFormWin_Closed();
            }
        }

        private void WebBrowser_PoETrade_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e) {
            _ = SetPoETradeBrowserCookie();
        }

        private void InitHamburgerMenuItems() {
            HamburgerMenuHeader = new HamburgerMenuHeaderItem();
            HamburgerMenuHeader.Label = "Shops";
            HamburgerMenuSeparator = new HamburgerMenuSeparatorItem();
        }

        private ChromiumWebBrowser GetVisibleBrowser() {
            if (webBrowser_PoETrade.Visibility == Visibility.Visible) {
                return webBrowser_PoETrade;
            } else if (webBrowser_PoENinja_StandardCurrency.Visibility == Visibility.Visible) {
                return webBrowser_PoENinja_StandardCurrency;
            } else if (webBrowser_PoENinja_ChallengeCurrency.Visibility == Visibility.Visible) {
                return webBrowser_PoENinja_ChallengeCurrency;
            } else if (webBrowser_PoENinja_Builds.Visibility == Visibility.Visible) {
                return webBrowser_PoENinja_Builds;
            } else if (webBrowser.Visibility == Visibility.Visible) {
                return webBrowser;
            }

            return null;
        }

        private void WebBrowser_FrameLoadStart(object sender, CefSharp.FrameLoadStartEventArgs e) {
            ChromiumWebBrowser webBrowser = (ChromiumWebBrowser)sender;
            this.Dispatcher.Invoke(() => {
                var visibleBrowser = GetVisibleBrowser();

                if (visibleBrowser != null && visibleBrowser.Name == webBrowser.Name) {
                    if (LastUrlLoaded != webBrowser.Address) {
                        loaderWebBrowser.IsActive = true;
                    }
                }
            });
        }

        private void WebBrowser_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e) {
            ChromiumWebBrowser webBrowser = (ChromiumWebBrowser)sender;
            this.Dispatcher.Invoke(() => {
                var visibleBrowser = GetVisibleBrowser();

                if (visibleBrowser != null && visibleBrowser.Name == webBrowser.Name) {
                    if (LastUrlLoaded != webBrowser.Address) {
                        LastUrlLoaded = webBrowser.Address;

                        if (regPoENinjaUrl.IsMatch(webBrowser.Address)) {
                            webBrowser.ZoomLevel = 5.46149645 * Math.Log(60) - 25.12;
                        } else {
                            webBrowser.ZoomLevel = 0;
                        }

                        loaderWebBrowser.IsActive = false;
                    }
                }
            });
        }

        private void Session_OnLocalStashTabsUpdated(Dictionary<int, StashTab> stashTabs) {
            stashTabSelectorControl.ClearTabs();

            foreach (var tab in stashTabs) {
                stashTabSelectorControl.AddTab(tab.Key, tab.Value.Name);
            }

            stashTabControl.SetStashTab(stashTabs.Values.ElementAt(Session.Instance().GetSelectedTabIndex()), Session.Instance().GetShop().GetPrices());
        }

        private void StashTabSelectorControl_OnStashTabSelected(int index) {
            Session.Instance().SetSelectedTab(index);
            stashTabControl.SetStashTab(Session.Instance().GetShop().GetStashTab(index), Session.Instance().GetShop().GetPrices());
        }

        private async void HandleShopWinClosed() {
            await Session.Instance().AddShop(WindowController.Instance().ShopFormWin.ThreadId, WindowController.Instance().ShopFormWin.League, WindowController.Instance().ShopFormWin.GenerateNewThread);
            Session.Instance().SetCurrentThreadId(WindowController.Instance().ShopFormWin.ThreadId);

            HamburgerMenuGlyphItem item = new HamburgerMenuGlyphItem();
            item.Label = Session.Instance().GetShop().Title;
            item.Glyph = WindowController.Instance().ShopFormWin.League;
            AddHamMenuItem(item);

            _ = InitUI(WindowController.Instance().ShopFormWin.ThreadId, WindowController.Instance().ShopFormWin.League);
        }

        private void SetHamMenuItems(List<HamburgerMenuItemBase> items) {
            HamburderMenuItems.Clear();
            HamburderMenuItems.Add(HamburgerMenuHeader);
            HamburderMenuItems.Add(HamburgerMenuSeparator);

            foreach (var item in items) {
                HamburderMenuItems.Add(item);
            }

            hamMenShopThreads.ItemsSource = HamburderMenuItems;
        }

        private void AddHamMenuItem(HamburgerMenuItemBase item) {
            HamburderMenuItems.Add(item);
            hamMenShopThreads.ItemsSource = HamburderMenuItems;
        }

        private void ShopFormWin_Closed() {
            if (WindowController.Instance().ShopFormWin.Success) {
                if (!Session.Instance().AnyShops(WindowController.Instance().ShopFormWin.ThreadId)) {
                    HandleShopWinClosed();
                }
            } else if (!Session.Instance().AnyShops()) {
                WindowController.Instance().ShopFormWin.SetLeagues(Session.Instance().GetAvailableLeagues());
                WindowController.Instance().ShopFormWin.ShowDialog();
            }
        }

        private async Task InitUI(int threadId, string league) {
            Session.Instance().IsLocalStashUpdaterPaused = true;

            txtbThreadId.Text = $"Shop {threadId}";
            txtbLeague.Text = league;

            await Session.Instance().LoadLocalStash();

            stashTabSelectorControl.ClearTabs();

            if (!Session.Instance().GetShop().GetStashTabs().Any()) {
                stashTabControl.ClearStashTab();
                await Session.Instance().UpdateLocalStash(threadId);
            }

            foreach (var tab in Session.Instance().GetShop().GetStashTabsName()) {
                stashTabSelectorControl.AddTab(tab.Value, tab.Key);
            }

            if (Session.Instance().GetShop().GetStashTabs().Any()) {
                Session.Instance().SetSelectedTab(0);
                stashTabControl.SetStashTab(Session.Instance().GetShop().GetStashTab(0), Session.Instance().GetShop().GetPrices());
                stashTabSelectorControl.SetActiveTab(0);
            }

            Session.Instance().IsLocalStashUpdaterPaused = false;

            loaderStash.IsActive = false;
        }

        private async Task HandleLogin() {
            MessageController.Instance().Log("Authenticating...");
            await Session.Instance().SetConnectedPlayer(new Player() {
                SessionId = WindowController.Instance().LoginWin.POESESSID
            });

            MessageController.Instance().Log($"{Session.Instance().Player.AccountName} authenticated.");
            btnAccount.Content = Session.Instance().Player.AccountName;
        }

        private void SetActiveHamMenuItem(int index) {
            hamMenShopThreads.SelectedOptionsIndex = index;
        }

        private void SetHamMenuItemsFromShops() {
            HamburderMenuItems.Add(HamburgerMenuHeader);
            HamburderMenuItems.Add(HamburgerMenuSeparator);

            foreach (var shop in Session.Instance().GetShops()) {
                HamburgerMenuGlyphItem item = new HamburgerMenuGlyphItem();
                item.Label = shop.Title;
                item.Glyph = shop.League.Name.ToString();
                HamburderMenuItems.Add(item);
            }

            hamMenShopThreads.ItemsSource = HamburderMenuItems;
        }

        private async void LoginWin_Closed(object sender, EventArgs e) {
            if (!WindowController.Instance().LoginWin.Success) {
                System.Windows.Application.Current.Shutdown();
                return;
            }

            await HandleLogin();

            Session.Instance().RunAutoOnlineUpdater();

            await Session.Instance().LoadShops();

            SetHamMenuItemsFromShops();

            if (!Session.Instance().AnyShops()) {
                MessageController.Instance().Log("No shop available, please create one to proceed.");
                WindowController.Instance().ShopFormWin.SetLeagues(Session.Instance().GetAvailableLeagues());
                WindowController.Instance().ShopFormWin.ShowDialog();
            } else {
                SetActiveHamMenuItem(0);
                webBrowser_PoETrade.Address = Url_PoETrade.Replace("$league$", Session.Instance().GetShop().League.Name);
                await InitUI(Session.Instance().CurrentThreadId, Session.Instance().GetShop().League.Name);
            }
        }

        private void btnAddShop_Click(object sender, RoutedEventArgs e) {
            WindowController.Instance().ShopFormWin.SetLeagues(Session.Instance().GetAvailableLeagues());
            WindowController.Instance().ShopFormWin.ShowDialog();
        }

        private void toggleResourceMenu_Click(object sender, RoutedEventArgs e) {
            popResourceMenu.IsOpen = !popResourceMenu.IsOpen;
        }

        private void menItQuit_Click(object sender, RoutedEventArgs e) {
            System.Windows.Application.Current.Shutdown();
        }

        private void menItPoETrade_Click(object sender, RoutedEventArgs e) {
            popResourceMenu.IsOpen = false;

            HideBrowsers();

            string url = Url_PoETrade.Replace("$league$", Session.Instance().GetShop().League.Name);

            if (webBrowser_PoETrade.Address != url) {
                webBrowser_PoETrade.Address = url;
            }

            if (webBrowser_PoETrade.Visibility != Visibility.Visible) {
                webBrowser_PoETrade.Visibility = Visibility.Visible;
            }
        }

        private void menuItPoENinjaBuilds_Click(object sender, RoutedEventArgs e) {
            popResourceMenu.IsOpen = false;

            HideBrowsers();

            if (webBrowser_PoENinja_Builds.Visibility != Visibility.Visible) {
                webBrowser_PoENinja_Builds.Visibility = Visibility.Visible;
            }
        }

        private void menuItPoENinjaCurrency_Click(object sender, RoutedEventArgs e) {
            popResourceMenu.IsOpen = false;

            HideBrowsers();

            if (Session.Instance().AnyShops()) {
                if (Session.Instance().GetShop().League.Name == "Standard") {
                    if (webBrowser_PoENinja_StandardCurrency.Visibility != Visibility.Visible) {
                        webBrowser_PoENinja_StandardCurrency.Visibility = Visibility.Visible;
                    }
                } else {
                    if (webBrowser_PoENinja_ChallengeCurrency.Visibility != Visibility.Visible) {
                        webBrowser_PoENinja_ChallengeCurrency.Visibility = Visibility.Visible;
                    }
                }

            } else {
                if (webBrowser_PoENinja_ChallengeCurrency.Visibility != Visibility.Visible) {
                    webBrowser_PoENinja_ChallengeCurrency.Visibility = Visibility.Visible;
                }
            }
        }

        private void HideBrowsers() {
            webBrowser_PoETrade.Visibility = Visibility.Hidden;
            webBrowser_PoENinja_StandardCurrency.Visibility = Visibility.Hidden;
            webBrowser_PoENinja_ChallengeCurrency.Visibility = Visibility.Hidden;
            webBrowser_PoENinja_Builds.Visibility = Visibility.Hidden;
            webBrowser.Visibility = Visibility.Hidden;
        }

        private async void hamMenShopThreads_ItemClick(object sender, ItemClickEventArgs args) {
            HamburgerMenuGlyphItem item = (HamburgerMenuGlyphItem)hamMenShopThreads.SelectedItem;
            string league = item.Glyph;
            var shop = Session.Instance().GetShop(league);
            Session.Instance().SetCurrentThreadId(shop.ThreadId);
            await InitUI(Session.Instance().CurrentThreadId, Session.Instance().GetShop().League.Name);
            AdjustLeagueInBrowsers();
        }

        private void AdjustLeagueInBrowsers() {
            string currentLeague = Session.Instance().GetShop().League.Name;
            string poeTradeUrl = Url_PoETrade.Replace("$league$", currentLeague);

            if (webBrowser_PoETrade.Address != poeTradeUrl) {
                webBrowser_PoETrade.Address = poeTradeUrl;
            }

            if (webBrowser_PoENinja_ChallengeCurrency.Visibility == Visibility.Visible && (currentLeague == "Standard" || currentLeague == "Hardcore")) {
                webBrowser_PoENinja_ChallengeCurrency.Visibility = Visibility.Hidden;
                webBrowser_PoENinja_StandardCurrency.Visibility = Visibility.Visible;
            } else if (webBrowser_PoENinja_StandardCurrency.Visibility == Visibility.Visible && currentLeague != "Standard" && currentLeague != "Hardcore") {
                webBrowser_PoENinja_ChallengeCurrency.Visibility = Visibility.Visible;
                webBrowser_PoENinja_StandardCurrency.Visibility = Visibility.Hidden;
            }
        }

        private void SetBrowsersHeight(bool collapsed = true) {
            webBrowser_PoENinja_Builds.SetValue(Grid.RowSpanProperty, collapsed ? 2 : 3);
            webBrowser_PoENinja_ChallengeCurrency.SetValue(Grid.RowSpanProperty, collapsed ? 2 : 3);
            webBrowser_PoENinja_StandardCurrency.SetValue(Grid.RowSpanProperty, collapsed ? 2 : 3);
            webBrowser_PoETrade.SetValue(Grid.RowSpanProperty, collapsed ? 2 : 3);
            webBrowser.SetValue(Grid.RowSpanProperty, collapsed ? 2 : 3);
        }

        private void btnShowHideConsole_Click(object sender, RoutedEventArgs e) {
            bool isVisible = console.Visibility == Visibility.Visible;
            console.Visibility = isVisible ? Visibility.Hidden : Visibility.Visible;

            btnShowHideConsole.HorizontalAlignment = isVisible ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            btnShowHideConsole.Margin = isVisible ? new Thickness(0, 0, 0, 5) : new Thickness(0, 0, 20, 5);

            btnConsoleNbErrors.HorizontalAlignment = isVisible ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            btnConsoleNbErrors.Margin = isVisible ? new Thickness(13, 0, 0, 20) : new Thickness(13, 0, 15, 20);

            SetBrowsersHeight(!isVisible);
        }

        private void btnAccount_Click(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start(Url_PlayerAccount);
        }

        private void btnPriceWholeTab_Click(object sender, RoutedEventArgs e) {
            WindowController.Instance().PricingFormWin.SetPricingWholeTab(Session.Instance().GetSelectedTabIndex(), true);
            WindowController.Instance().PricingFormWin.ShowDialog();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e) {
            if (txtSearch.Text != null && txtSearch.Text.Trim().Length > 0) {
                var g = 0;
            }
        }
    }
}
